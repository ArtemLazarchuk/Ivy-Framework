using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Recommendations", icon: Icons.Lightbulb, group: new[] { "Tools" }, order: 15)]
public class RecommendationsApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var jobService = UseService<JobService>();
        var refreshToken = UseRefreshToken();
        var selectedState = UseState<Recommendation?>(null);
        var projectFilter = UseState<string?>(null);
        var planStatusFilter = UseState<string?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var filtered = recommendations
            .Where(r => r.State == "Pending")
            .Where(r => projectFilter.Value == null || r.Project == projectFilter.Value)
            .Where(r => planStatusFilter.Value == null || r.SourcePlanStatus.ToString() == planStatusFilter.Value)
            .ToList();

        // If selected recommendation is no longer in filtered list, adjust selection
        if (selectedState.Value is { } selected && !filtered.Any(r => r.PlanId == selected.PlanId && r.Title == selected.Title))
        {
            selectedState.Set(filtered.Count > 0 ? filtered[0] : null);
        }

        void Refresh() => refreshToken.Refresh();

        var projectOptions = recommendations
            .Where(r => r.State == "Pending")
            .GroupBy(r => r.Project)
            .OrderByDescending(g => g.Count())
            .Select(g => new Option<string>($"{g.Key} ({g.Count()})", g.Key))
            .ToArray<IAnyOption>();

        var statusOptions = recommendations
            .Where(r => r.State == "Pending")
            .Select(r => r.SourcePlanStatus)
            .Distinct()
            .OrderBy(s => s)
            .Select(s => new Option<string>(s.ToString(), s.ToString()))
            .ToArray<IAnyOption>();

        var filterBar = new Expandable(
            header: "Filters",
            content: Layout.Vertical()
                | projectFilter.ToSelectInput(projectOptions).Placeholder("All Projects").Nullable().WithField().Label("Project")
                | planStatusFilter.ToSelectInput(statusOptions).Placeholder("All Statuses").Nullable().WithField().Label("Plan Status")
        ).Open(false).Ghost();

        var totalPendingCount = recommendations.Count(r => r.State == "Pending");
        var hasActiveFilters = projectFilter.Value != null || planStatusFilter.Value != null;

        var sidebar = new Recommendations.SidebarView(filtered, selectedState, totalPendingCount, hasActiveFilters);

        return new SidebarLayout(
            mainContent: new Recommendations.ContentView(selectedState.Value, filtered, selectedState, planService, jobService, Refresh),
            sidebarContent: Layout.Vertical()
                | filterBar
                | sidebar
        );
    }
}
