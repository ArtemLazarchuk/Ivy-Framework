using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Tendril.Services;

public static class BackgroundServiceActivator
{
    public static void Start(IServiceProvider services)
    {
        services.GetRequiredService<IPlanWatcherService>();
        services.GetRequiredService<IInboxWatcherService>();
        services.GetRequiredService<WorktreeCleanupService>().Start();

        var syncService = services.GetRequiredService<PlanDatabaseSyncService>();
        _ = Task.Run(syncService.PerformInitialSync);
    }
}
