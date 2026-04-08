using Ivy.Tendril.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ivy.Tendril.Test;

public class WorktreeCleanupServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _plansDir;
    private readonly WorktreeCleanupService _service;

    public WorktreeCleanupServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"tendril-cleanup-test-{Guid.NewGuid()}");
        _plansDir = Path.Combine(_tempDir, "plans");
        Directory.CreateDirectory(_plansDir);
        _service = new WorktreeCleanupService(_plansDir, NullLogger<WorktreeCleanupService>.Instance);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreatePlan(string folderName, string state, DateTime? updated = null)
    {
        var dir = Path.Combine(_plansDir, folderName);
        Directory.CreateDirectory(dir);
        var updatedStr = (updated ?? DateTime.UtcNow.AddHours(-2)).ToString("o");
        File.WriteAllText(Path.Combine(dir, "plan.yaml"),
            $"state: {state}\nproject: Test\ntitle: Test Plan\nupdated: {updatedStr}\n");
        return dir;
    }

    private void CreateWorktreeDir(string planDir, string repoName, bool withContent = true)
    {
        var worktreeDir = Path.Combine(planDir, "worktrees", repoName);
        Directory.CreateDirectory(worktreeDir);
        if (withContent)
            File.WriteAllText(Path.Combine(worktreeDir, "dummy.txt"), "test");
    }

    private static void CreateEmptyWorktreesDir(string planDir)
    {
        Directory.CreateDirectory(Path.Combine(planDir, "worktrees"));
    }

    [Fact]
    public void RunCleanup_Skips_Active_State_Plans()
    {
        var activeStates = new[] { "Draft", "Building", "Executing", "ReadyForReview", "Blocked" };
        foreach (var state in activeStates)
        {
            var dir = CreatePlan($"01{Array.IndexOf(activeStates, state):D3}-{state}Plan", state);
            CreateWorktreeDir(dir, "Repo");
        }

        _service.RunCleanup();

        foreach (var state in activeStates)
        {
            var worktreesDir = Path.Combine(_plansDir, $"01{Array.IndexOf(activeStates, state):D3}-{state}Plan", "worktrees");
            Assert.True(Directory.Exists(worktreesDir), $"Worktrees for {state} plan should not be removed");
        }
    }

    [Fact]
    public void RunCleanup_Cleans_Terminal_State_Plans_Past_Grace_Period()
    {
        var terminalStates = new[] { "Completed", "Failed", "Skipped", "Icebox" };
        foreach (var state in terminalStates)
        {
            var dir = CreatePlan($"02{Array.IndexOf(terminalStates, state):D3}-{state}Plan", state,
                DateTime.UtcNow.AddHours(-2));
            // Create empty worktrees dir (simulates post-RemoveWorktrees state where git cleaned subdirs)
            CreateEmptyWorktreesDir(dir);
        }

        _service.RunCleanup();

        foreach (var state in terminalStates)
        {
            var worktreesDir = Path.Combine(_plansDir, $"02{Array.IndexOf(terminalStates, state):D3}-{state}Plan", "worktrees");
            Assert.False(Directory.Exists(worktreesDir), $"Empty worktrees dir for {state} plan should be removed");
        }
    }

    [Fact]
    public void RunCleanup_Respects_Grace_Period()
    {
        // Plan just failed 10 minutes ago — should NOT be cleaned
        var dir = CreatePlan("03000-RecentFail", "Failed", DateTime.UtcNow.AddMinutes(-10));
        CreateWorktreeDir(dir, "Repo");

        _service.RunCleanup();

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.True(Directory.Exists(worktreesDir), "Recently failed plan should keep worktrees during grace period");
    }

    [Fact]
    public void RunCleanup_Skips_Plans_Without_Worktrees()
    {
        CreatePlan("04000-NoWorktree", "Failed", DateTime.UtcNow.AddHours(-2));

        // Should not throw
        _service.RunCleanup();
    }

    [Fact]
    public void RunCleanup_Skips_Plans_Without_PlanYaml()
    {
        var dir = Path.Combine(_plansDir, "05000-NoPlanYaml");
        Directory.CreateDirectory(dir);
        var worktreeDir = Path.Combine(dir, "worktrees", "Repo");
        Directory.CreateDirectory(worktreeDir);

        // Should not throw
        _service.RunCleanup();

        Assert.True(Directory.Exists(worktreeDir), "Worktree should not be removed when plan.yaml is missing");
    }

    [Fact]
    public void CleanupPlanWorktrees_Static_Cleans_Terminal_Plan()
    {
        var dir = CreatePlan("06000-StaticTest", "Completed", DateTime.UtcNow.AddHours(-2));
        CreateEmptyWorktreesDir(dir);

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.False(Directory.Exists(worktreesDir));
    }

    [Fact]
    public void CleanupPlanWorktrees_Static_Skips_Active_Plan()
    {
        var dir = CreatePlan("07000-ActivePlan", "Executing", DateTime.UtcNow.AddHours(-2));
        CreateWorktreeDir(dir, "Repo");

        WorktreeCleanupService.CleanupPlanWorktrees(dir);

        var worktreesDir = Path.Combine(dir, "worktrees");
        Assert.True(Directory.Exists(worktreesDir));
    }

    [Fact]
    public void RemoveWorktrees_Remains_Functional_After_Extraction()
    {
        // Verify RemoveWorktrees is accessible as internal static
        var dir = CreatePlan("08000-ExtractTest", "Failed", DateTime.UtcNow.AddHours(-2));
        var worktreeDir = Path.Combine(dir, "worktrees", "TestRepo");
        Directory.CreateDirectory(worktreeDir);
        // No .git file — RemoveWorktrees should skip this entry gracefully
        File.WriteAllText(Path.Combine(worktreeDir, "file.txt"), "test");

        PlanReaderService.RemoveWorktrees(dir);

        // The directory still exists because there's no .git file for git worktree remove,
        // but the method should not throw
        Assert.True(Directory.Exists(worktreeDir));
    }
}
