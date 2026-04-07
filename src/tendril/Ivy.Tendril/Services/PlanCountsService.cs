using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public record PlanCounts(int Drafts, int ActiveJobs, int Reviews, int Icebox, int Recommendations);

public class PlanCountsService : IPlanCountsService, IDisposable
{
    private readonly IPlanReaderService _planReaderService;
    private readonly IJobService _jobService;
    private readonly IPlanWatcherService _planWatcher;
    private PlanCounts _current;

    public event Action? CountsChanged;

    public PlanCounts Current => _current;

    public PlanCountsService(IPlanReaderService planReaderService, IJobService jobService, IPlanWatcherService planWatcher)
    {
        _planReaderService = planReaderService;
        _jobService = jobService;
        _planWatcher = planWatcher;
        _current = ComputeCounts();
        _planWatcher.PlansChanged += OnPlansSourceChanged;
        _jobService.JobsChanged += OnSourceChanged;
    }

    private void OnPlansSourceChanged(string? _) => OnSourceChanged();

    private void OnSourceChanged()
    {
        _planReaderService.InvalidateCaches();
        Refresh();
    }

    private void Refresh()
    {
        var updated = ComputeCounts();
        if (updated != _current)
        {
            _current = updated;
            CountsChanged?.Invoke();
        }
    }

    private PlanCounts ComputeCounts()
    {
        var snapshot = _planReaderService.ComputePlanCounts();
        var jobs = _jobService.GetJobs();

        return new PlanCounts(
            Drafts: snapshot.Drafts,
            ActiveJobs: jobs.Count(j => j.Status is JobStatus.Running or JobStatus.Queued or JobStatus.Blocked),
            Reviews: snapshot.ReadyForReview + snapshot.Failed,
            Icebox: snapshot.Icebox,
            Recommendations: snapshot.PendingRecommendations
        );
    }

    public void Dispose()
    {
        _planWatcher.PlansChanged -= OnPlansSourceChanged;
        _jobService.JobsChanged -= OnSourceChanged;
    }
}
