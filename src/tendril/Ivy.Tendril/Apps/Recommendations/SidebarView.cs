using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class SidebarView(
    List<Recommendation> recommendations,
    IState<Recommendation?> selectedState,
    IState<string?> textFilter) : ViewBase
{
    private readonly List<Recommendation> _recommendations = recommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly IState<string?> _textFilter = textFilter;

    public object BuildHeader()
    {
        return Layout.Vertical()
            | _textFilter.ToSearchInput().Placeholder("Search recommendations...");
    }

    public override object Build()
    {
        var filtered = _recommendations.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(_textFilter.Value))
        {
            var search = _textFilter.Value.Trim();
            filtered = filtered.Where(r =>
                r.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return new List(filtered.Select(rec =>
        {
            var clickableRec = rec;
            var stateBadgeVariant = rec.State switch
            {
                "Accepted" => BadgeVariant.Success,
                "Declined" => BadgeVariant.Destructive,
                _ => BadgeVariant.Outline
            };

            return new ListItem(rec.Title)
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge(rec.State).Variant(stateBadgeVariant).Small()
                    | new Badge($"#{rec.PlanId}").Variant(BadgeVariant.Outline).Small()
                )
                .OnClick(() => _selectedState.Set(clickableRec));
        }));
    }
}
