using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application;

public record AzureDevopsProjectDataContext(
    AzureDevopsProject Project,
    Lazy<VssConnection> VssConnection,
    Lazy<AzureDevopsApiProjectClient> HttpClient,
    Lazy<AzureDevopsApiProjectQueries> Queries,
    Func<ILogger> GetLogger);

public record AzureDevopsOrganisationDataContext(
    Lazy<VssConnection> VssConnection,
    Lazy<AzureDevopsApiOrgClient> HttpClient,
    Lazy<AzureDevopsApiOrgQueries> Queries,
    Func<ILogger> GetLogger);
