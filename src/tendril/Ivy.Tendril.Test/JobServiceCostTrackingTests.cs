using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class JobServiceCostTrackingTests
{
    private static JobService CreateService()
    {
        return new JobService(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void LaunchJob_SetsSessionIdToValidGuid()
    {
        var service = CreateService();

        var id = service.StartJob("ExecutePlan", Path.GetTempPath());
        var job = service.GetJob(id);

        Assert.NotNull(job);
        Assert.NotNull(job.SessionId);
        Assert.NotEmpty(job.SessionId);
        Assert.True(Guid.TryParse(job.SessionId, out _), "SessionId should be a valid GUID");
    }

    [Fact]
    public void LaunchJob_SessionIdIsUniquePerJob()
    {
        var service = CreateService();

        var id1 = service.StartJob("ExecutePlan", Path.GetTempPath());
        var id2 = service.StartJob("ExecutePlan", Path.GetTempPath());

        var job1 = service.GetJob(id1);
        var job2 = service.GetJob(id2);

        Assert.NotNull(job1?.SessionId);
        Assert.NotNull(job2?.SessionId);
        Assert.NotEqual(job1.SessionId, job2.SessionId);
    }
}
