namespace AzureDevopsExplorer.Database.Commands;

public class FindNonExistingBuildIds
{
    public List<int> Run(List<int> buildIdsFromApi)
    {
        using (var db = new DataContext())
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
