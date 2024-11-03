using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application;

public record AzureDevopsProjectDataContext(
    AzureDevopsApiOrganisationClientFactory OrgClientFactory,
    AzureDevopsApiOrgQueries OrgQueries,
    AzureDevopsProject Project,
    AzureDevopsApiProjectClientFactory ProjectClientFactory,
    AzureDevopsApiProjectQueries Queries,
    ILoggerFactory LoggerFactory,
    ICreateDataContexts DataContextFactory,
    CancellationToken CancellationToken);

public record AzureDevopsOrganisationDataContext(
    AzureDevopsApiOrganisationClientFactory HttpClient,
    AzureDevopsApiOrgQueries Queries,
    ILoggerFactory LoggerFactory,
    ICreateDataContexts DataContextFactory,
    CancellationToken CancellationToken);
