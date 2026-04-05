using System.Collections.Concurrent;
using System.Reflection;
using Ivy.Tendril.Apps.Jobs;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class PlanCountsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _plansDir;
    private readonly PlanReaderService _planReader;
    private readonly JobService _jobService;
    private readonly PlanWatcherService _planWatcher;

    public PlanCountsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-counts-test-{Guid.NewGuid()}");
        _plansDir = Path.Combine(_tempDir, "Plans");
        Directory.CreateDirectory(_plansDir);

        var settings = new TendrilSettings();
        var configService = new ConfigService(settings, _tempDir);
        _planReader = new PlanReaderService(configService);
        _jobService = new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
        _planWatcher = new PlanWatcherService(configService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void CreatePlan(string folderName, string state)
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plan.yaml"),
            $"state: {state}\nproject: Tendril\ntitle: Test\nrepos: []\ncommits: []\nprs: []\nverifications: []\nrelatedPlans: []\ndependsOn: []\ncreated: 2026-01-01T00:00:00Z\nupdated: 2026-01-01T00:00:00Z\n");

        var revisionsDir = Path.Combine(dir, "revisions");
        Directory.CreateDirectory(revisionsDir);
        File.WriteAllText(Path.Combine(revisionsDir, "001.md"), "# Test");
    }

    private void CreateRecommendations(string folderName, string yaml)
    {
        var artifactsDir = Path.Combine(_plansDir, folderName, "artifacts");
        Directory.CreateDirectory(artifactsDir);
        File.WriteAllText(Path.Combine(artifactsDir, "recommendations.yaml"), yaml);
    }

    private void AddJob(string id, string status)
    {
        var jobsField = typeof(JobService).GetField("_jobs", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var jobs = (ConcurrentDictionary<string, JobItem>)jobsField.GetValue(_jobService)!;
        jobs[id] = new JobItem { Id = id, Status = status };
    }

    private PlanCountsService CreateService()
    {
        return new PlanCountsService(_planReader, _jobService, _planWatcher);
    }

    [Fact]
    public void ComputeCounts_WithNoPlans_ReturnsAllZeros()
    {
        using var service = CreateService();

        Assert.Equal(0, service.Current.Drafts);
        Assert.Equal(0, service.Current.ActiveJobs);
        Assert.Equal(0, service.Current.Reviews);
        Assert.Equal(0, service.Current.Icebox);
        Assert.Equal(0, service.Current.Recommendations);
    }

    [Fact]
    public void ComputeCounts_WithFailedPlans_CountsOnlyInReviews()
    {
        // This is the CRITICAL regression test:
        // Failed plans must be counted ONLY in Reviews, NOT in Drafts.
        CreatePlan("00001-FailedPlanA", "Failed");
        CreatePlan("00002-FailedPlanB", "Failed");

        using var service = CreateService();

        Assert.Equal(0, service.Current.Drafts);
        Assert.Equal(2, service.Current.Reviews);
    }

    [Fact]
    public void ComputeCounts_WithVariousStates_AggregatesCorrectly()
    {
        // Drafts
        CreatePlan("00001-DraftPlan", "Draft");
        CreatePlan("00002-AnotherDraft", "Draft");

        // Reviews (ReadyForReview + Failed)
        CreatePlan("00003-ReviewPlan", "ReadyForReview");
        CreatePlan("00004-FailedPlan", "Failed");

        // Icebox
        CreatePlan("00005-IceboxPlan", "Icebox");

        // Completed (should not appear in any badge)
        CreatePlan("00006-CompletedPlan", "Completed");

        // Recommendations (pending on a completed plan)
        CreatePlan("00007-WithRecs", "Completed");
        CreateRecommendations("00007-WithRecs", "- title: Fix something\n  description: |\n    Details here.\n  state: Pending\n");

        // Jobs
        AddJob("job-1", "Running");
        AddJob("job-2", "Queued");
        AddJob("job-3", "Completed"); // should not count as active

        using var service = CreateService();

        Assert.Equal(2, service.Current.Drafts);
        Assert.Equal(2, service.Current.ActiveJobs);
        Assert.Equal(2, service.Current.Reviews); // 1 ReadyForReview + 1 Failed
        Assert.Equal(1, service.Current.Icebox);
        Assert.Equal(1, service.Current.Recommendations);
    }

    [Fact]
    public void ComputeCounts_WithRunningAndQueuedJobs_CountsActiveJobs()
    {
        AddJob("job-running-1", "Running");
        AddJob("job-running-2", "Running");
        AddJob("job-queued-1", "Queued");
        AddJob("job-completed-1", "Completed");
        AddJob("job-failed-1", "Failed");
        AddJob("job-pending-1", "Pending");

        using var service = CreateService();

        // Only Running + Queued count as active
        Assert.Equal(3, service.Current.ActiveJobs);
    }

    [Fact]
    public void ComputeCounts_RefreshUpdatesCountsWhenPlansChange()
    {
        using var service = CreateService();

        // Initially no plans
        Assert.Equal(0, service.Current.Drafts);

        // Add a draft plan and trigger refresh via the CountsChanged event flow
        CreatePlan("00001-NewDraft", "Draft");

        // Trigger the PlansChanged event through the watcher's debounce timer
        // Since PlanWatcherService uses a debounce timer, we invoke the event via reflection
        var plansChangedField = typeof(PlanWatcherService).GetField("PlansChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        // The event is public, so we can get the backing field or invoke via reflection
        // PlanWatcherService.PlansChanged is a public event - invoke it
        var eventDelegate = typeof(PlanWatcherService)
            .GetField("PlansChanged", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_planWatcher) as Action;

        if (eventDelegate != null)
        {
            eventDelegate.Invoke();
        }
        else
        {
            // Fallback: use the event's raise method through the service's subscription
            // The PlanCountsService subscribes to PlansChanged, so we need to trigger it
            // Try accessing through the debounce timer elapsed event
            var debounceTimer = typeof(PlanWatcherService)
                .GetField("_debounceTimer", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(_planWatcher) as System.Timers.Timer;

            // Simulate the timer elapsed by waiting
            debounceTimer?.Stop();
            debounceTimer?.Start();
            Thread.Sleep(600); // wait for debounce (500ms)
        }

        Assert.Equal(1, service.Current.Drafts);
    }
}
