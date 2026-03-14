namespace Ivy.Samples.Shared.Apps.Widgets.Charts;

[App(icon: Icons.GitFork, searchHints: ["visualization", "flow", "analytics", "data", "diagram", "network"])]
public class SankeyChartApp : SampleBase
{
    protected override object? BuildSample()
    {
        return Layout.Grid().Columns(2)
            | new SankeyChart0View()
            | new SankeyChart1View()
            | new SankeyChart2View()
            | new SankeyChart3View();
    }
}

public class SankeyChart0View : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Visit"),
                new SankeyNode("Add to Cart"),
                new SankeyNode("Checkout"),
                new SankeyNode("Purchase"),
                new SankeyNode("Bounce"),
            },
            Links: new[]
            {
                new SankeyLink(0, 1, 3500),   // Visit -> Cart
                new SankeyLink(0, 4, 1500),   // Visit -> Bounce
                new SankeyLink(1, 2, 2800),   // Cart -> Checkout
                new SankeyLink(1, 4, 700),    // Cart -> Bounce
                new SankeyLink(2, 3, 2200),   // Checkout -> Purchase
                new SankeyLink(2, 4, 600),    // Checkout -> Bounce
            }
        );

        return new Card().Title("User Funnel")
            | new SankeyChart(data).Tooltip().Toolbox();
    }
}

public class SankeyChart1View : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Revenue"),
                new SankeyNode("Marketing"),
                new SankeyNode("Operations"),
                new SankeyNode("R&D"),
                new SankeyNode("Admin"),
                new SankeyNode("Digital Ads"),
                new SankeyNode("Content"),
                new SankeyNode("Salaries"),
                new SankeyNode("Equipment"),
            },
            Links: new[]
            {
                new SankeyLink(0, 1, 450000),  // Revenue -> Marketing
                new SankeyLink(0, 2, 320000),  // Revenue -> Operations
                new SankeyLink(0, 3, 280000),  // Revenue -> R&D
                new SankeyLink(0, 4, 150000),  // Revenue -> Admin
                new SankeyLink(1, 5, 250000),  // Marketing -> Digital Ads
                new SankeyLink(1, 6, 200000),  // Marketing -> Content
                new SankeyLink(2, 7, 200000),  // Operations -> Salaries
                new SankeyLink(3, 8, 150000),  // R&D -> Equipment
            }
        );

        return new Card().Title("Budget Flow")
            | new SankeyChart(data)
                .Tooltip()
                .Toolbox()
                .ColorScheme(ColorScheme.Rainbow);
    }
}

public class SankeyChart2View : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Coal"),
                new SankeyNode("Natural Gas"),
                new SankeyNode("Solar"),
                new SankeyNode("Wind"),
                new SankeyNode("Electricity"),
                new SankeyNode("Heating"),
                new SankeyNode("Residential"),
                new SankeyNode("Industrial"),
                new SankeyNode("Commercial"),
            },
            Links: new[]
            {
                new SankeyLink(0, 4, 300),   // Coal -> Electricity
                new SankeyLink(0, 5, 150),   // Coal -> Heating
                new SankeyLink(1, 4, 400),   // Gas -> Electricity
                new SankeyLink(1, 5, 250),   // Gas -> Heating
                new SankeyLink(2, 4, 200),   // Solar -> Electricity
                new SankeyLink(3, 4, 180),   // Wind -> Electricity
                new SankeyLink(4, 6, 450),   // Electricity -> Residential
                new SankeyLink(4, 7, 350),   // Electricity -> Industrial
                new SankeyLink(4, 8, 280),   // Electricity -> Commercial
                new SankeyLink(5, 6, 250),   // Heating -> Residential
                new SankeyLink(5, 7, 150),   // Heating -> Industrial
            }
        );

        return new Card().Title("Energy Flow")
            | new SankeyChart(data)
                .Tooltip()
                .NodeWidth(15)
                .NodeGap(10)
                .Curvature(0.6);
    }
}

public class SankeyChart3View : ViewBase
{
    public override object? Build()
    {
        var data = new SankeyData(
            Nodes: new[]
            {
                new SankeyNode("Home"),
                new SankeyNode("Products"),
                new SankeyNode("Product A"),
                new SankeyNode("Product B"),
                new SankeyNode("Cart"),
                new SankeyNode("Checkout"),
                new SankeyNode("Exit"),
            },
            Links: new[]
            {
                new SankeyLink(0, 1, 5000),   // Home -> Products
                new SankeyLink(0, 6, 2000),   // Home -> Exit
                new SankeyLink(1, 2, 2000),   // Products -> Product A
                new SankeyLink(1, 3, 1500),   // Products -> Product B
                new SankeyLink(1, 6, 1500),   // Products -> Exit
                new SankeyLink(2, 4, 1200),   // Product A -> Cart
                new SankeyLink(2, 6, 800),    // Product A -> Exit
                new SankeyLink(3, 4, 900),    // Product B -> Cart
                new SankeyLink(3, 6, 600),    // Product B -> Exit
                new SankeyLink(4, 5, 1500),   // Cart -> Checkout
                new SankeyLink(4, 6, 600),    // Cart -> Exit
                new SankeyLink(5, 6, 300),    // Checkout -> Exit
            }
        );

        return new Card().Title("Website Navigation Flow")
            | new SankeyChart(data)
                .Tooltip()
                .Toolbox()
                .NodeAlign(SankeyAlign.Left);
    }
}
