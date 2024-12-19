using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Security;

namespace AzureDevopsExplorer.Web.Domain
{
    public record PermissionsReport(
        // id to show in list of reports and url, must be unique!
        string Id,
        // name to show in list of reports
        string Name,
        // description to show of chosen report
        string Description,
        Func<DataContext, IQueryable<SecurityNamespacePermissionReport>> getRows);

    public class PermissionsReports
    {
        public static readonly List<PermissionsReport> All = new List<PermissionsReport>
        {
            new PermissionsReport(
                "all",
                "All permissions",
                "All of the permissions in the access control lists for all projects",
                x => x.SecurityNamespacePermissionReport),

            new PermissionsReport(
                "projectleveladminuser",
                "Project level admin user permissions",
                "Project level admin permissions of descriptors which include the @ symbol",
                x => {
                    // it's tempting to try and refactor the predicates, but it can easily mess up the query performance
                    return x.SecurityNamespacePermissionReport
                        .Where(y => y.PermissionScope == AccessControlTokenPermissionScope.Project)
                        // descriptors with @ in them will be service accounts and AD users
                        .Where(y => y.IdentityDescriptor.Contains("@"))
                        // look for permission actions with dmin as this will be Admin or Administrator
                        .Where(y => y.ActionName.Contains("dmin"));
                }),
        };
    }
}
