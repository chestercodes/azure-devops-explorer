namespace AzureDevopsExplorer.Database;

public class DatabaseConnection
{
    public static string SqlServer
    {
        get
        {
            DbType = DatabaseType.SqlServer;
            var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var userId = Environment.GetEnvironmentVariable("DB_USERID") ?? "sa";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "p@55word";
            return $"Server={server};Database=AzureDevopsExplorer;User Id={userId};Password={password};Encrypt=False;";
        }
    }

    public enum DatabaseType
    {
        SqlServer,
        Sqlite
    }

    public static string SqlServerLocal
    {
        get
        {
            DbType = DatabaseType.SqlServer;
            return $"Server=(local)\\SQLExpress;Database=AzureDevopsExplorer;Integrated Security=True;Encrypt=False;";
        }
    }

    public static DatabaseType DbType { get; private set; } = DatabaseType.SqlServer;

    public static string Sqlite
    {
        get
        {
            DbType = DatabaseType.Sqlite;
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            var dbPath = Path.Join(path, "AzureDevopsExplorer.db");
            return $"Data Source={dbPath}";
        }
    }

}
