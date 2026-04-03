using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class ContentView(
    Recommendation? selectedRecommendation,
    List<Recommendation> allRecommendations,
    IState<Recommendation?> selectedState,
    PlanReaderService planService,
    Action refresh) : ViewBase
{
    private readonly Recommendation? _selected = selectedRecommendation;
    private readonly List<Recommendation> _all = allRecommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;
    private readonly PlanReaderService _planService = planService;
    private readonly Action _refresh = refresh;

    public override object? Build()
    {
        var nav = this.UseNavigation();

        if (_selected is null)
        {
            return Layout.Vertical().AlignContent(Align.Center).Height(Size.Full())
                | Text.Muted("Select a recommendation from the sidebar");
        }

        var currentIndex = _all.FindIndex(r => r.PlanId == _selected.PlanId && r.Title == _selected.Title);

        var stateBadgeVariant = _selected.State switch
        {
            "Accepted" => BadgeVariant.Success,
            "Declined" => BadgeVariant.Destructive,
            _ => BadgeVariant.Outline
        };

        // Header
        var header = Layout.Horizontal().Width(Size.Full()).Padding(1).Gap(2)
            | Text.Block(_selected.Title).Bold()
            | new Badge(_selected.State).Variant(stateBadgeVariant)
            | new Badge($"#{_selected.PlanId}").Variant(BadgeVariant.Outline)
            | new Spacer().Width(Size.Grow())
            | Text.Rich()
                .Bold($"{currentIndex + 1}/{_all.Count}", word: true)
                .Muted("recommendations", word: true);

        // Content
        var scrollableContent = Layout.Vertical().Width(Size.Auto().Max(Size.Units(200))).Gap(4).Padding(2);

        // Source plan info
        scrollableContent |= Layout.Vertical().Gap(1)
            | Text.Block("Source Plan").Bold()
            | Layout.Horizontal().Gap(2)
                | Text.Muted($"Plan #{_selected.PlanId}: {_selected.PlanTitle}")
                | Text.Muted($"Date: {_selected.Date:yyyy-MM-dd HH:mm}");

        // Description
        scrollableContent |= new Separator();
        scrollableContent |= new Markdown(_selected.Description);

        // Action bar
        var actionBar = Layout.Horizontal().AlignContent(Align.Center).Gap(2).Padding(1)
            | new Button("Accept").Icon(Icons.Check).Primary().ShortcutKey("a").OnClick(() =>
            {
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Accepted");
                _refresh();
                GoToNext();
            })
            | new Button("Decline").Icon(Icons.X).Outline().ShortcutKey("x").OnClick(() =>
            {
                _planService.UpdateRecommendationState(_selected.PlanFolderName, _selected.Title, "Declined");
                _refresh();
                GoToNext();
            })
            | new Button("View Plan").Icon(Icons.ExternalLink).Outline().ShortcutKey("d").OnClick(() =>
            {
                var fullPath = Path.Combine(_planService.PlansDirectory, _selected.PlanFolderName);
                if (Directory.Exists(fullPath))
                    nav.Navigate<PlanViewerApp>(new PlanViewerAppArgs(fullPath));
            })
            | new Button("Previous").Icon(Icons.ChevronLeft).Outline().ShortcutKey("p").OnClick(() => GoToPrevious())
            | new Button("Next").Icon(Icons.ChevronRight, Align.Right).Outline().ShortcutKey("n").OnClick(() => GoToNext());

        return new HeaderLayout(
            header: header,
            content: new FooterLayout(
                footer: actionBar,
                content: scrollableContent
            ).Size(Size.Full())
        ).Scroll(Scroll.None).Size(Size.Full());
    }

    private void GoToNext()
    {
        if (_all.Count == 0) return;
        var currentIndex = _all.FindIndex(r => r.PlanId == _selected?.PlanId && r.Title == _selected?.Title);
        var nextIndex = (currentIndex + 1) % _all.Count;
        _selectedState.Set(_all[nextIndex]);
    }

    private void GoToPrevious()
    {
        if (_all.Count == 0) return;
        var currentIndex = _all.FindIndex(r => r.PlanId == _selected?.PlanId && r.Title == _selected?.Title);
        var prevIndex = (currentIndex - 1 + _all.Count) % _all.Count;
        _selectedState.Set(_all[prevIndex]);
    }
}
