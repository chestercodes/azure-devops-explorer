namespace AzureDevopsExplorer.Database;

public enum DatabaseType
{
    SqlServer,
    Sqlite
}

public class DatabaseConnection
{
    private static string EnsureConsoleDataFolderExists()
    {
        var basePath = Path.GetFullPath("../../..");
        if (new DirectoryInfo(basePath).Name != "AzureDevopsExplorer.Console")
        {
            throw new Exception("Directory is not AzureDevopsExplorer.Console");
        }
        var dataDirPath = Path.Combine(basePath, "data");
        Directory.CreateDirectory(dataDirPath);
        return dataDirPath;
    }

    public static string DebugSqliteFileConnectionString
    {
        get
        {
            var dbPath = Path.Join(EnsureConsoleDataFolderExists(), "AzureDevopsExplorer.db");
            return $"Data Source={dbPath}";
        }
    }

}
