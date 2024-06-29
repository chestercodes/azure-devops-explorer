using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class SecureFileImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public SecureFileImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.httpClient = dataContext.HttpClient.Value;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.SecureFileImport)
        {
            await ImportSecureFiles();
        }
    }

    public async Task ImportSecureFiles()
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        var secureFilesResult = await queries.GetSecureFiles();
        if (secureFilesResult.IsT1)
        {
            Console.WriteLine(secureFilesResult.AsT1.AsError);
            return;
        }

        var existingIds = new List<Guid>();
        using (var db = new DataContext())
        {
            existingIds = db.SecureFile.Where(x => x.ProjectId == httpClient.Info.ProjectId).Select(x => x.Id).ToList();
        }

        var secureFiles = secureFilesResult.AsT0;
        var importTime = DateTime.UtcNow;

        foreach (var secureFile in secureFiles.Value)
        {
            await AddSecureFile(secureFile, importTime);
        }

        await RemoveExistingNotPresentInApiResponse(existingIds, secureFiles, importTime);
    }

    private async Task AddSecureFile(AzureDevopsApi.Dtos.SecureFile secureFile, DateTime importTime)
    {
        using var db = new DataContext();

        var secureFileFromApi = mapper.MapSecureFile(secureFile);
        secureFileFromApi.ProjectId = httpClient.Info.ProjectId;
        var secureFilePropertiesFromApi = secureFile.Properties.Select(x =>
            {
                return new SecureFileProperty
                {
                    SecureFileId = secureFileFromApi.Id,
                    Name = x.Key,
                    Value = x.Value,
                };
            })
            .ToList();
        secureFileFromApi.LastImport = importTime;
        secureFileFromApi.Properties = secureFilePropertiesFromApi;

        var currentValue = db.SecureFile.Include(x => x.Properties).Where(x => x.Id == secureFile.Id).SingleOrDefault();
        if (currentValue != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                        nameof(Database.Model.Data.SecureFileProperty.Id),
                        nameof(Database.Model.Data.SecureFile.LastImport),
                    },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(currentValue, secureFileFromApi);
            if (comparison.AreEqual)
            {
                return;
            }
            else
            {
                db.SecureFile.Remove(currentValue);
                db.SecureFile.Add(secureFileFromApi);

                db.SecureFileChange.Add(new SecureFileChange
                {
                    SecureFileId = secureFile.Id,
                    PreviousImport = currentValue.LastImport,
                    NextImport = importTime,
                    Difference = comparison.DifferencesString,
                });

                db.SaveChanges();
                return;
            }
        }

        db.SecureFile.Add(secureFileFromApi);

        db.SecureFileChange.Add(new SecureFileChange
        {
            SecureFileId = secureFile.Id,
            PreviousImport = null,
            NextImport = importTime,
            Difference = $"First time or added secure file {secureFile.Id}"
        });

        db.SaveChanges();
        return;
    }

    private static async Task RemoveExistingNotPresentInApiResponse(List<Guid> existingIds, ListResponse<AzureDevopsApi.Dtos.SecureFile> secureFiles, DateTime importTime)
    {
        var idsFromApi = secureFiles.Value.Select(x => x.Id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = new DataContext())
            {
                var toRemove = db.SecureFile.Where(x => removed.Contains(x.Id)).ToList();
                db.SecureFileChange.AddRange(toRemove.Select(x =>
                {
                    return new SecureFileChange
                    {
                        SecureFileId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = importTime
                    };
                }));
                db.SecureFile.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }

        }
    }
}
