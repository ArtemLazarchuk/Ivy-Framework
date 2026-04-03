using Ivy;
using Ivy.Tendril.Apps.Recommendations;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

[App(title: "Recommendations", icon: Icons.Lightbulb, group: new[] { "Tools" }, order: 15)]
public class RecommendationsApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var refreshToken = UseRefreshToken();
        var stateFilter = UseState<string?>(null);
        var selectedState = UseState<Recommendation?>(null);
        var textFilter = UseState<string?>("");

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var filtered = stateFilter.Value is { } filter
            ? recommendations.Where(r => r.State == filter).ToList()
            : recommendations;

        // If selected recommendation is no longer in filtered list, adjust selection
        if (selectedState.Value is { } selected && !filtered.Any(r => r.PlanId == selected.PlanId && r.Title == selected.Title))
        {
            selectedState.Set(filtered.Count > 0 ? filtered[0] : null);
        }

        void Refresh() => refreshToken.Refresh();

        var pendingCount = recommendations.Count(r => r.State == "Pending");
        var acceptedCount = recommendations.Count(r => r.State == "Accepted");
        var declinedCount = recommendations.Count(r => r.State == "Declined");

        var filterBar = Layout.Horizontal().Gap(2) | new object?[]
        {
            stateFilter.Value == null
                ? (object)new Button($"All ({recommendations.Count})").OnClick(() => stateFilter.Set(null))
                : new Button($"All ({recommendations.Count})").Ghost().OnClick(() => stateFilter.Set(null)),
            stateFilter.Value == "Pending"
                ? (object)new Button($"Pending ({pendingCount})").OnClick(() => stateFilter.Set("Pending"))
                : new Button($"Pending ({pendingCount})").Ghost().OnClick(() => stateFilter.Set("Pending")),
            stateFilter.Value == "Accepted"
                ? (object)new Button($"Accepted ({acceptedCount})").OnClick(() => stateFilter.Set("Accepted"))
                : new Button($"Accepted ({acceptedCount})").Ghost().OnClick(() => stateFilter.Set("Accepted")),
            stateFilter.Value == "Declined"
                ? (object)new Button($"Declined ({declinedCount})").OnClick(() => stateFilter.Set("Declined"))
                : new Button($"Declined ({declinedCount})").Ghost().OnClick(() => stateFilter.Set("Declined"))
        };

        var sidebar = new Recommendations.SidebarView(filtered, selectedState, textFilter);

        var sidebarHeader = Layout.Vertical().Gap(2)
            | sidebar.BuildHeader()
            | filterBar;

        return new SidebarLayout(
            mainContent: new Recommendations.ContentView(selectedState.Value, filtered, selectedState, planService, Refresh),
            sidebarContent: sidebar,
            sidebarHeader: sidebarHeader
        );
    }
}
