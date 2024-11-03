namespace AzureDevopsExplorer.Database.Commands;

public class FindNonExistingBuildIds
{
    private readonly ICreateDataContexts dataContext;

    public FindNonExistingBuildIds(ICreateDataContexts dataContext)
    {
        this.dataContext = dataContext;
    }

    public List<int> Run(List<int> buildIdsFromApi)
    {
        using (var db = dataContext.Create())
        {
            var buildIdsAlreadyPresent = db.Build
                .Where(x => buildIdsFromApi.Contains(x.Id))
                .Select(x => x.Id)
                .ToList();

            var toAdd = buildIdsFromApi.Except(buildIdsAlreadyPresent).ToList();

            return toAdd;
        }
    }
}
