using System.Security.Cryptography;
using System.Text;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextImportErrorExtensions
{
    public static string AddImportError(this DataContext db, string error)
    {
        var errorHash = GetMd5Hash(error);
        if (db.ImportError.Any(x => x.Hash == errorHash) == false)
        {
            db.ImportError.Add(new Model.Pipelines.ImportError
            {
                Hash = errorHash,
                Error = error
            });
        }
        return errorHash;
    }

    private static string GetMd5Hash(string errorString)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(errorString));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
