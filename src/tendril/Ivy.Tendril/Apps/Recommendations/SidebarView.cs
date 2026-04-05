using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class SidebarView(
    List<Recommendation> recommendations,
    IState<Recommendation?> selectedState,
    int totalCount,
    bool hasActiveFilters) : ViewBase
{
    private readonly List<Recommendation> _recommendations = recommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly int _totalCount = totalCount;
    private readonly bool _hasActiveFilters = hasActiveFilters;

    public override object Build()
    {
        if (_recommendations.Count == 0 && _hasActiveFilters && _totalCount > 0)
        {
            return Layout.Vertical().AlignContent(Align.Center).Gap(2).Padding(4)
                | new Icon(Icons.ListFilterPlus).Size(Size.Units(6)).Color(Colors.Gray)
                | Text.Muted("No matching recommendations")
                | Text.Muted("Try adjusting your filters").Small();
        }

        return new List(_recommendations.Select(rec =>
        {
            var clickableRec = rec;

            var preview = rec.Description.Length > 120
                ? rec.Description[..120] + "…"
                : rec.Description;

            return new ListItem($"#{rec.PlanId} {rec.Title}", subtitle: preview)
                .OnClick(() => _selectedState.Set(clickableRec));
        }));
    }
}
