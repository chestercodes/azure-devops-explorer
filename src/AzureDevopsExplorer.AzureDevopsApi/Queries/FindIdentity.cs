//namespace AzureDevopsExplorer.AzureDevopsApi;

//using Microsoft.VisualStudio.Services.Identity;
//using Microsoft.VisualStudio.Services.Identity.Client;
//using Microsoft.VisualStudio.Services.WebApi;

//public class FindIdentity
//{
//    private string projectName;
//    private VssConnection connection;

//    public FindIdentity(VssConnection connection, string projectName)
//    {
//        this.connection = connection;
//        this.projectName = projectName;
//    }

//    public async Task<Identity> GetId(Guid id)
//    {
//        var client = connection.GetClient<IdentityHttpClient>();
//        var identity = await client.ReadIdentityAsync(id);
//        return identity;
//    }

//    public async Task<Identity> GetId(string puid)
//    {
//        var client = connection.GetClient<IdentityHttpClient>();
//        var identity = await client.ReadIdentityAsync(puid);
//        return identity;
//    }

//    public async Task<PagedIdentities> GetAll(string? continuationToken = null)
//    {
//        var client = connection.GetClient<IdentityHttpClient>();
//        if (continuationToken == null)
//        {
//            return await client.ListUsersAsync();
//        }

//        return await client.ListUsersAsync(continuationToken: continuationToken);
//    }
//}
