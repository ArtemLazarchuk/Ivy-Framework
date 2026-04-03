using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps;

public class RecommendationRow
{
    public string Id { get; set; } = "";
    public string Date { get; set; } = "";
    public string State { get; set; } = "";
    public string PlanId { get; set; } = "";
    public string PlanFolderName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
}

[App(title: "Recommendations", icon: Icons.Lightbulb, group: new[] { "Tools" }, order: 15)]
public class RecommendationsApp : ViewBase
{
    public override object? Build()
    {
        var planService = UseService<PlanReaderService>();
        var nav = this.UseNavigation();
        var refreshToken = UseRefreshToken();
        var stateFilter = UseState<string?>(null);

        UseInterval(() => refreshToken.Refresh(), TimeSpan.FromMinutes(1));

        var recommendations = planService.GetRecommendations();

        var filtered = stateFilter.Value is { } filter
            ? recommendations.Where(r => r.State == filter).ToList()
            : recommendations;

        var rows = filtered.Select(r => new RecommendationRow
        {
            Id = $"{r.PlanId}-{r.Title.GetHashCode()}",
            Date = r.Date.ToString("yyyy-MM-dd HH:mm"),
            State = r.State,
            PlanId = r.PlanId,
            PlanFolderName = r.PlanFolderName,
            Title = r.Title,
            Description = r.Description.Length > 200 ? r.Description.Substring(0, 200) + "..." : r.Description
        }).ToList();

        var dataTable = rows.AsQueryable()
            .ToDataTable(idSelector: r => r.Id)
            .RefreshToken(refreshToken)
            .Width(Size.Full())
            .Height(Size.Full())
            .Header(r => r.State, "State")
            .Header(r => r.Date, "Date")
            .Header(r => r.PlanId, "Plan")
            .Header(r => r.Title, "Title")
            .Header(r => r.Description, "Description")
            .Hidden(r => r.Id)
            .Hidden(r => r.PlanFolderName)
            .Config(c =>
            {
                c.AllowSorting = true;
                c.AllowFiltering = true;
                c.ShowSearch = true;
                c.SelectionMode = SelectionModes.None;
                c.ShowIndexColumn = true;
                c.BatchSize = 50;
                c.EnableCellClickEvents = true;
            })
            .Renderer(r => r.State, new LabelsDisplayRenderer())
            .Renderer(r => r.PlanId, new ButtonDisplayRenderer())
            .OnCellClick(e =>
            {
                if (e.Value.ColumnName == "Plan")
                {
                    var planId = e.Value.CellValue?.ToString();
                    if (!string.IsNullOrEmpty(planId))
                    {
                        var row = rows.FirstOrDefault(r => r.PlanId == planId);
                        if (row != null)
                        {
                            var fullPath = Path.Combine(planService.PlansDirectory, row.PlanFolderName);
                            if (Directory.Exists(fullPath))
                                nav.Navigate<PlanViewerApp>(new PlanViewerAppArgs(fullPath));
                        }
                    }
                }
                return ValueTask.CompletedTask;
            })
            .RowActions(
                new MenuItem(Label: "Accept", Icon: Icons.Check, Tag: "accept"),
                new MenuItem(Label: "Decline", Icon: Icons.X, Tag: "decline")
            )
            .OnRowAction(e =>
            {
                var tag = e.Value.Tag?.ToString();
                var id = e.Value.Id?.ToString();
                var row = rows.FirstOrDefault(r => r.Id == id);

                if (row != null && tag is "accept" or "decline")
                {
                    var newState = tag == "accept" ? "Accepted" : "Declined";
                    planService.UpdateRecommendationState(row.PlanFolderName, row.Title, newState);
                    refreshToken.Refresh();
                }
                return ValueTask.CompletedTask;
            });

        var pendingCount = recommendations.Count(r => r.State == "Pending");
        var acceptedCount = recommendations.Count(r => r.State == "Accepted");
        var declinedCount = recommendations.Count(r => r.State == "Declined");

        var header = Layout.Horizontal().Gap(2) | new object?[]
        {
            Text.Block($"Recommendations ({filtered.Count})").Bold(),
            stateFilter.Value == null
                ? (object)new Button("All").OnClick(() => stateFilter.Set(null))
                : new Button("All").Ghost().OnClick(() => stateFilter.Set(null)),
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

        return new HeaderLayout(
            header: header,
            content: dataTable
        );
    }
}
