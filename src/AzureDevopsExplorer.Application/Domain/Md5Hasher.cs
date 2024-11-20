using System.Security.Cryptography;
using System.Text;

namespace AzureDevopsExplorer.Application.Domain;

public class Md5Hasher
{
    public static string Hash(string? val)
    {
        var plain = val ?? "";
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(plain));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}