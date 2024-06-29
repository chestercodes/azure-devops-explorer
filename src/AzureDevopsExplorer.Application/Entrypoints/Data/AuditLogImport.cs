using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;

public record AuditLogImportTimePairing(DateTime StartTime, DateTime EndTime);

public class AuditLogImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsApiOrgQueries queries;

    public AuditLogImport(AzureDevopsOrganisationDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.queries = dataContext.Queries.Value;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.AuditLogImport)
        {
            DateTime? initialDate = null;
            if (DateTime.TryParse(config.AuditLogStartDate, out DateTime res))
            {
                initialDate = res;
            }
            await AddAuditLogDates(initialDate);

            await AddAuditLog();
        }
    }

    public List<AuditLogImportTimePairing> FindHourlyValuesToAdd(DateTime now, DateTime? initialDateFromCli, DateTime? upperLimitOrNull, List<AuditLogImportTimePairing> existing)
    {
        DateTime ToHour(DateTime x)
        {
            return new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0, DateTimeKind.Utc);
        }

        var initialDate = now.AddYears(-1);
        if (initialDateFromCli != null)
        {
            initialDate = initialDateFromCli.Value;
        }
        else
        {
            if (existing.Count > 0)
            {
                initialDate = existing.Select(x => x.EndTime).Max();
            }
        }
        initialDate = ToHour(initialDate);

        var upperLimit = ToHour(upperLimitOrNull ?? now);

        var toAdd = new List<AuditLogImportTimePairing>();
        var currentStartDate = initialDate;
        while (true)
        {
            if (currentStartDate < upperLimit)
            {
                var endTime = currentStartDate.AddHours(1);
                toAdd.Add(new AuditLogImportTimePairing(currentStartDate, endTime));
                currentStartDate = currentStartDate.AddHours(1);
            }
            else
            {
                break;
            }
        }

        var notAlreadyPresent = toAdd.Except(existing).ToList();
        return notAlreadyPresent;
    }

    public async Task AddAuditLogDates(DateTime? initialDateFromCli = null)
    {
        using var db = new DataContext();

        var existing = db.AuditLogImport.ToList()
                .Select(x => new AuditLogImportTimePairing(x.StartTime, x.EndTime)).ToList();
        var datesToAdd = FindHourlyValuesToAdd(DateTime.UtcNow, initialDateFromCli, null, existing);
        var rowsToAdd = datesToAdd.Select(x => new Database.Model.Data.AuditLogImport
        {
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            ImportError = null,
            Status = AuditLogImportStatus.Initial
        });
        db.AuditLogImport.AddRange(rowsToAdd);
        db.SaveChanges();
    }
    public async Task AddAuditLog()
    {
        var datesToRun = new List<AuditLogImportTimePairing>();
        using (var db = new DataContext())
        {
            datesToRun = db.AuditLogImport
                .Where(x => x.Status == AuditLogImportStatus.Initial)
                .OrderBy(x => x.StartTime)
                .ToList()
                .Select(x => new AuditLogImportTimePairing(x.StartTime, x.EndTime)).ToList();
        }

        foreach (var date in datesToRun)
        {
            await RunForDate(date);
        }
    }

    private async Task RunForDate(AuditLogImportTimePairing dates)
    {
        using var db = new DataContext();
        var importRow = db.AuditLogImport.Single(x => x.StartTime == dates.StartTime);
        try
        {
            var entries = await GetAuditLogsForDate(dates);
            var idsFromDate = entries.Select(x => x.id).ToList();
            var existingIds = db.AuditLog.Where(x => idsFromDate.Contains(x.Id)).Select(x => x.Id).ToList();
            var entriesToAdd = entries
                .Where(x => existingIds.Contains(x.id) == false)
                .ToList()
                .Select(x =>
                {
                    var m = mapper.MapAuditLogEntry(x);
                    return m;
                })
                .ToList();
            db.AuditLog.AddRange(entriesToAdd);
            importRow.Status = AuditLogImportStatus.Done;
        }
        catch (Exception ex)
        {
            importRow.ImportError = ex.ToString();
            importRow.Status = AuditLogImportStatus.ErrorFromApi;
        }

        db.SaveChanges();
    }

    private async Task<List<DecoratedAuditLogEntry>> GetAuditLogsForDate(AuditLogImportTimePairing date)
    {
        var toReturn = new List<DecoratedAuditLogEntry>();

        var startTime = date.StartTime.ToString("o");
        var endTime = date.EndTime.ToString("o");

        string? continuationToken = null;
        while (true)
        {
            var batch = await queries.GetAuditLog(startTime, endTime, continuationToken);
            if (batch.IsT1)
            {
                throw batch.AsT1.AsT1;
            }

            var values = batch.AsT0;
            toReturn.AddRange(values.decoratedAuditLogEntries);

            if (values.hasMore == false)
            {
                break;
            }
            continuationToken = values.continuationToken;
        }

        return toReturn;
    }
}
