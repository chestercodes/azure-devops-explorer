using AzureDevopsExplorer.Database.Model.Pipelines;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextImportStateExtensions
{
    private static Func<string, string> GetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompleted = projectName => $"BuildsQueueTimeOfLastCompletedBuildBeforeNonCompleted_{projectName}";
    public static DateTime? GetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompletedValue(this DataContext db, string projectName)
    {
        var id = GetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompleted(projectName);
        var v = db.ImportState.SingleOrDefault(x => x.Id == id);
        if (v == null)
        {
            return null;
        }
        return DateTime.Parse(v.Value!);
    }

    public static void SetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompletedValue(this DataContext db, string projectName, DateTime time)
    {
        var id = GetBuildsQueueTimeOfLastCompletedBuildBeforeNonCompleted(projectName);
        var v = time.ToString("o");
        var currentValue = db.ImportState.SingleOrDefault(x => x.Id == id);
        if (currentValue == null)
        {
            db.ImportState.Add(new ImportState
            {
                Id = id,
                Value = v
            });
        }
        else
        {
            currentValue.Value = v;
        }

        db.SaveChanges();
    }

    private static Func<Guid, string> GetServiceConnectionExecutionHistoryLatestId = serviceConnectionId => $"ServiceConnectionExecutionHistoryLatestId_{serviceConnectionId}";
    public static int? GetServiceConnectionExecutionHistoryLatestIdValue(this DataContext db, Guid serviceConnectionId)
    {
        var id = GetServiceConnectionExecutionHistoryLatestId(serviceConnectionId);
        var v = db.ImportState.SingleOrDefault(x => x.Id == id);
        if (v == null)
        {
            return null;
        }
        return int.Parse(v.Value!);
    }

    public static void SetServiceConnectionExecutionHistoryLatestIdValue(this DataContext db, Guid serviceConnectionId, int historyId)
    {
        var id = GetServiceConnectionExecutionHistoryLatestId(serviceConnectionId);
        var currentValue = db.ImportState.SingleOrDefault(x => x.Id == id);
        if (currentValue == null)
        {
            db.ImportState.Add(new ImportState
            {
                Id = id,
                Value = historyId.ToString()
            });
        }
        else
        {
            currentValue.Value = historyId.ToString();
        }

        db.SaveChanges();
    }

}
