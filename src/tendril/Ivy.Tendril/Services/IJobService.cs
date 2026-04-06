using System.Collections.Concurrent;
using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public interface IJobService
{
    event Action? JobsChanged;
    event Action<JobNotification>? NotificationReady;

    [Obsolete("Use NotificationReady event instead. Will be removed in a future version.")]
    ConcurrentQueue<JobNotification> PendingNotifications { get; }


    string StartJob(string type, string[] args, string? inboxFilePath);
    string StartJob(string type, params string[] args);
    void CompleteJob(string id, int? exitCode, bool timedOut = false, bool staleOutput = false);
    void StopJob(string id);
    void DeleteJob(string id);
    void ClearCompletedJobs();
    void ClearFailedJobs();
    List<JobItem> GetJobs();
    JobItem? GetJob(string id);
    bool IsInboxFileTracked(string filePath);
}
