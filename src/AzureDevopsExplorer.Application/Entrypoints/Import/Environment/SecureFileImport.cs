using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Environment;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
public class SecureFileImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public SecureFileImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.SecureFileImport)
        {
            await ImportSecureFiles();
        }
    }

    public async Task ImportSecureFiles()
    {
        logger.LogInformation($"Running secure file import");

        var secureFilesResult = await dataContext.Queries.DistributedTask.GetSecureFiles();
        if (secureFilesResult.IsT1)
        {
            logger.LogError(secureFilesResult.AsT1.AsError);
            return;
        }

        var existingIds = new List<Guid>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            existingIds = db.SecureFile.Where(x => x.ProjectId == dataContext.Project.ProjectId).Select(x => x.Id).ToList();
        }

        var secureFiles = secureFilesResult.AsT0;
        var importTime = DateTime.UtcNow;

        foreach (var secureFile in secureFiles.value)
        {
            await AddSecureFile(secureFile, importTime);
        }

        await RemoveExistingNotPresentInApiResponse(existingIds, secureFiles, importTime);
    }

    private async Task AddSecureFile(AzureDevopsApi.DistributedTask.SecureFile secureFile, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();

        var secureFileFromApi = mapper.MapSecureFile(secureFile);
        secureFileFromApi.ProjectId = dataContext.Project.ProjectId;
        var secureFilePropertiesFromApi = secureFile.properties.Select(x =>
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

        var currentValue = db.SecureFile.Include(x => x.Properties).Where(x => x.Id == secureFile.id).SingleOrDefault();
        if (currentValue != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                        nameof(SecureFileProperty.Id),
                        nameof(SecureFile.LastImport),
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
                    SecureFileId = secureFile.id,
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
            SecureFileId = secureFile.id,
            PreviousImport = null,
            NextImport = importTime,
            Difference = $"First time or added secure file {secureFile.id}"
        });

        db.SaveChanges();
        return;
    }

    private async Task RemoveExistingNotPresentInApiResponse(List<Guid> existingIds, ListResponse<AzureDevopsApi.DistributedTask.SecureFile> secureFiles, DateTime importTime)
    {
        var idsFromApi = secureFiles.value.Select(x => x.id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = dataContext.DataContextFactory.Create())
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
