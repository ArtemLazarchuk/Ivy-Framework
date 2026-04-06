using Ivy.Tendril.Apps.Jobs;

namespace Ivy.Tendril.Services;

public interface ITelemetryService
{
    void TrackAppStarted();
    void TrackPlanCreated();
    void TrackPrCreated();
    void TrackJobCompleted(string jobType, JobStatus status, int? durationSeconds);
    void TrackPlanStateTransition(int planId, string fromState, string toState);
    Task FlushAsync();
}
