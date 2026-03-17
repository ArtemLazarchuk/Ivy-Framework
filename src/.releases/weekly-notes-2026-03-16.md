# Ivy Framework Weekly Notes - Week of 2026-03-16

## New Features

### New Calendar Widget

Manage events and schedules with the new `Calendar` widget! Display events in four different views—month, week, day, and agenda—with full interactive capabilities including drag-and-drop, event clicks, and time slot selection.

**Basic calendar with events:**

```csharp
public class EventScheduler : ViewBase
{
    public override object? Build()
    {
        return new Calendar(
            new CalendarEvent()
                .EventId("meeting-1")
                .Title("Team Standup")
                .Start(new DateTime(2026, 3, 17, 9, 0, 0))
                .End(new DateTime(2026, 3, 17, 9, 30, 0))
                .Color("primary"),

            new CalendarEvent()
                .EventId("deadline-1")
                .Title("Project Deadline")
                .Start(new DateTime(2026, 3, 20, 0, 0, 0))
                .End(new DateTime(2026, 3, 20, 23, 59, 59))
                .AllDay(true)
                .Color("destructive")
        );
    }
}
```

**Configure view mode and date range:**

```csharp
new Calendar()
    .DefaultView(CalendarDisplayMode.Week)  // Month, Week, Day, or Agenda
    .Date(DateTime.Now)
    .MinDate(new DateTime(2026, 1, 1))
    .MaxDate(new DateTime(2026, 12, 31))
    .ShowToolbar(true)
```

**Handle user interactions:**

```csharp
new Calendar()
    .EnableDragDrop(true)
    .OnEventClick(eventId =>
    {
        Console.WriteLine($"Clicked event: {eventId}");
    })
    .OnEventMove((eventId, start, end) =>
    {
        Console.WriteLine($"Moved {eventId} to {start}");
    })
    .OnEventResize((eventId, start, end) =>
    {
        Console.WriteLine($"Resized {eventId}");
    })
    .OnSelectSlot((start, end) =>
    {
        Console.WriteLine($"Selected time slot: {start} to {end}");
    })
```

**Build from collections with `.ToCalendar()`:**

Similar to `.ToKanban()`, you can create calendars from any collection using the builder pattern:

```csharp
public class MeetingSchedule : ViewBase
{
    record Meeting(string Id, string Title, DateTime Start, DateTime End, string Category);

    public override object? Build()
    {
        var meetings = UseState(new[]
        {
            new Meeting("1", "Team Sync", DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "Work"),
            new Meeting("2", "Design Review", DateTime.Today.AddHours(14), DateTime.Today.AddHours(15), "Work"),
            new Meeting("3", "Gym", DateTime.Today.AddDays(1).AddHours(7), DateTime.Today.AddDays(1).AddHours(8), "Personal")
        });

        return meetings.Value
            .ToCalendar(
                startSelector: m => m.Start,
                endSelector: m => m.End,
                eventIdSelector: m => m.Id)
            .Title(m => m.Title)
            .Color(m => m.Category == "Work" ? "Blue" : "Green")
            .DefaultView(CalendarDisplayMode.Week)
            .EnableDragDrop()
            .OnMove(move =>
            {
                // Update meeting times when dragged
                var updated = meetings.Value.Select(m =>
                {
                    if (m.Id == move.EventId?.ToString())
                        return m with { Start = move.Start, End = move.End };
                    return m;
                }).ToArray();
                meetings.Set(updated);
            });
    }
}
```

The builder pattern makes it easy to map your domain objects to calendar events while maintaining type safety and enabling drag-and-drop updates.

**Add events dynamically:**

```csharp
var calendar = new Calendar();

// Add events using the | operator
calendar = calendar
    | new CalendarEvent()
        .Title("Morning Coffee")
        .Start(DateTime.Now.AddHours(1))
        .End(DateTime.Now.AddHours(1.5))
    | new CalendarEvent()
        .Title("Lunch Break")
        .Start(DateTime.Now.AddHours(4))
        .End(DateTime.Now.AddHours(5))
        .AllDay(false);
```

**Key features:**

- **Four view modes**: Month grid, week time grid, single day view, and agenda list
- **All-day events**: Set `.AllDay(true)` for events that span entire days
- **Color coding**: Use theme colors like "primary", "destructive", "warning", or "success"
- **Interactive toolbar**: Built-in navigation with Today/Previous/Next buttons and view switcher
- **Event interactions**: Click events, drag to move, resize handles, and click empty slots to create
- **Theme-aware**: Automatically adapts to your app's theme with proper contrast

The Calendar widget follows the same pattern as Kanban, where `CalendarEvent` children define the data while the parent `Calendar` manages the display and interactions.

### AudioInput Sample Rate Control

The `AudioInput` widget now supports configurable sample rates, giving you precise control over audio recording quality and file size. Set the target sample rate in Hz to optimize for your use case—lower rates for speech recognition efficiency, higher rates for music and high-fidelity recordings.

**Basic usage:**

```csharp
var upload = new UploadContext("/api/upload");

// Default browser sample rate (typically 48000 Hz)
new AudioInput(upload)

// Speech recognition (16kHz is optimal for most speech models)
new AudioInput(upload)
    .SampleRate(16000)

// High-fidelity audio recording
new AudioInput(upload)
    .SampleRate(48000)
```

**Supported sample rates:**

The widget supports standard audio sample rates: 8000, 11025, 16000, 22050, 24000, 32000, 44100, and 48000 Hz. When `SampleRate` is null or not specified, the browser uses its default (typically 48000 Hz).

**Common use cases:**

```csharp
// Voice notes and speech-to-text (smaller files, faster processing)
new AudioInput(upload, label: "Record Voice Note")
    .SampleRate(16000)

// Podcasts and interviews (balanced quality and size)
new AudioInput(upload, label: "Record Podcast")
    .SampleRate(32000)

// Music and high-quality recording
new AudioInput(upload, label: "Record Audio")
    .SampleRate(48000)
```

Lower sample rates produce smaller audio files and are more efficient for bandwidth-constrained scenarios, while higher sample rates preserve more audio detail for music and high-fidelity applications.

**Guaranteed sample rate accuracy:** The framework now uses Web Audio API resampling to ensure your specified sample rate is always applied, regardless of the microphone's native sample rate. Previously, browsers could ignore sample rate requests—now the audio is automatically resampled to match your exact specification.

### New NumberRangeInput Widget

Select numeric ranges with an intuitive dual-handle slider! The new `NumberRangeInput` widget provides a sleek interface for selecting minimum and maximum numeric values, perfect for filtering by price ranges, thresholds, or any min/max bounds.

**Basic usage:**

```csharp
public class PriceFilterDemo : ViewBase
{
    public override object? Build()
    {
        var priceRange = UseState<(int, int)>(() => (25, 75));

        return Layout.Vertical()
            | priceRange.ToNumberRangeInput()
                .Min(0)
                .Max(100)
            | Text.P($"Price: ${priceRange.Value.Item1} - ${priceRange.Value.Item2}");
    }
}
```

The widget uses a tuple `(min, max)` to represent the range, following the same pattern as `DateRangeInput`. Access values with `.Item1` (lower bound) and `.Item2` (upper bound).

**Currency and formatting:**

Like `NumberInput`, the `NumberRangeInput` supports all the same formatting options including currency, percentages, and custom precision:

```csharp
// Currency formatting
priceRange.ToNumberRangeInput()
    .Min(0)
    .Max(10000)
    .Currency("USD")
    .Precision(2)

// Percentage range
percentRange.ToNumberRangeInput()
    .Min(0)
    .Max(100)
    .Percent(true)

// Custom step intervals
quantityRange.ToNumberRangeInput()
    .Min(0)
    .Max(1000)
    .Step(50)
```

**Supported types:**

The widget supports all numeric tuple types including `(int, int)`, `(decimal, decimal)`, `(double, double)`, `(float, float)`, `(short, short)`, `(long, long)`, and their nullable variants.

### DateTimeInput and DateRangeInput Enhancements

The date input widgets now support custom placeholder text for date ranges and configurable calendar week start days, making them more flexible for international use and domain-specific scenarios.

**Custom placeholders for DateRangeInput:**

Set separate placeholders for the start and end date fields to provide context-specific hints to users:

```csharp
// Hotel booking with check-in/check-out placeholders
var bookingDates = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

bookingDates.ToDateRangeInput()
    .StartPlaceholder("Check-in")
    .EndPlaceholder("Check-out")
    .Format("MM/dd/yyyy")

// Event planning with start/end labels
var eventDates = UseState<(DateOnly?, DateOnly?)>(() => (null, null));

eventDates.ToDateRangeInput()
    .StartPlaceholder("Start Date")
    .EndPlaceholder("End Date")
```

When both placeholders are set, they replace the generic "Pick a date range" placeholder with your custom text separated by a dash (e.g., "Check-in - Check-out").

**Custom week start day:**

Customize which day the calendar week starts on for both `DateTimeInput` and `DateRangeInput` to match regional conventions or business requirements:

```csharp
// Monday-first calendar (common in Europe, ISO 8601)
var appointmentDate = UseState(DateOnly.FromDateTime(DateTime.Now));

appointmentDate.ToDateInput()
    .FirstDayOfWeek(DayOfWeek.Monday)

// Monday-first date range
var dateRange = UseState<(DateOnly, DateOnly)>((
    DateOnly.FromDateTime(DateTime.Now.AddDays(-7)),
    DateOnly.FromDateTime(DateTime.Now)
));

dateRange.ToDateRangeInput()
    .FirstDayOfWeek(DayOfWeek.Monday)

// Sunday-first calendar (default in US)
appointmentDate.ToDateInput()
    .FirstDayOfWeek(DayOfWeek.Sunday)
```

The `FirstDayOfWeek` property accepts any `DayOfWeek` enum value (Sunday through Saturday) and automatically adjusts the calendar display to start weeks on your specified day. This is particularly useful for:

- **International applications**: Match regional conventions (Monday-first for ISO 8601 / most of world, Sunday-first for US/Canada)
- **Business scheduling**: Align with your organization's workweek (e.g., Monday-Sunday for standard business weeks)
- **Domain-specific needs**: Custom week definitions for retail, healthcare, or other industries

### FileInput Default Variant

The `FileInput` widget now supports two display variants: the existing `Drop` variant (default) with its large drag-and-drop zone, and a new `Default` variant that provides a more compact, button-based interface. Both variants fully support drag-and-drop functionality.

**Using the Default variant:**

```csharp
public class FileUploadDemo : ViewBase
{
    public override object? Build()
    {
        var file = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(file));

        return Layout.Vertical()
            | file.ToFileInput(upload)
                .Variant(FileInputVariant.Default)
                .Placeholder("Select file");
    }
}
```

**When to use each variant:**

- **`FileInputVariant.Drop`** (default): Best for dedicated upload pages, large cards, or when you want to emphasize the file upload action with a prominent drag-and-drop zone.
- **`FileInputVariant.Default`**: Best for forms, sidebars, toolbars, or dense layouts where space is at a premium. The button-based interface is more compact while still supporting drag-and-drop directly onto the button.

**Comparing both variants:**

```csharp
public class VariantComparison : ViewBase
{
    public override object? Build()
    {
        var file1 = UseState<FileUpload<byte[]>?>();
        var upload1 = UseUpload(MemoryStreamUploadHandler.Create(file1));

        var file2 = UseState<FileUpload<byte[]>?>();
        var upload2 = UseUpload(MemoryStreamUploadHandler.Create(file2));

        return Layout.Vertical()
            | Text.H3("Drop Variant")
            | file1.ToFileInput(upload1)
            | Text.H3("Default Variant")
            | file2.ToFileInput(upload2).Variant(FileInputVariant.Default);
    }
}
```

The Default variant maintains all the same features as the Drop variant including validation, progress tracking, multiple file support, and file type restrictions—just in a more compact form factor.

### CameraInput Widget

Capture photos directly from the user's webcam or device camera with the new `CameraInput` widget! It provides a live video preview, one-click capture, and automatic upload—all using the same familiar upload pattern as `FileInput`.

**Basic usage:**

```csharp
public class BasicCameraDemo : ViewBase
{
    public override object? Build()
    {
        var photo = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

        return Layout.Vertical()
            | new CameraInput(upload.Value, "Take a photo")
            | (photo.Value != null
                ? Text.P($"Captured: {photo.Value.FileName} ({StringHelper.FormatBytes(photo.Value.Length)})")
                : Text.P("No photo captured yet."));
    }
}
```

**How it works:**

1. User clicks the widget to start the camera (browser prompts for permission)
2. Live video preview displays from the device camera
3. Clicking **Capture** takes a snapshot and uploads it as a PNG image
4. The captured image is shown with a **Retake** button to restart the camera

**Camera selection:**

Control which camera to use on devices with multiple cameras:

```csharp
// Front-facing camera (default)
new CameraInput(upload.Value).FacingMode("user")

// Rear-facing camera
new CameraInput(upload.Value).FacingMode("environment")
```

**Access captured photos:**

The widget uses the same upload pattern as FileInput, making it easy to work with captured photos:

```csharp
var photo = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

// Access captured photo data
if (photo.Value != null)
{
    var fileName = photo.Value.FileName;    // "capture.png"
    var fileSize = photo.Value.Length;       // Size in bytes
    var fileData = photo.Value.Content;      // byte[] containing PNG data
}
```

The CameraInput widget is perfect for profile photos, document scanning, ID verification, or any scenario where you need to capture images directly from the user's camera.

### TextInput Pattern Validation

The `TextInput` widget now supports regex pattern validation with the new `Pattern` property. Validate input formats like email addresses, phone numbers, URLs, postal codes, and custom patterns—all with built-in client-side validation and user-friendly error messages.

**Basic usage:**

```csharp
// Email validation
new TextInput()
    .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
    .Placeholder("Enter your email")

// Phone number (US format)
new TextInput()
    .Pattern(@"^\d{3}-\d{3}-\d{4}$")
    .Placeholder("xxx-xxx-xxxx")

// Postal code (US ZIP)
new TextInput()
    .Pattern(@"^\d{5}(-\d{4})?$")
    .Placeholder("12345 or 12345-6789")
```

**How validation works:**

- Validation triggers on **blur** (when the user leaves the input field)
- Error message displays if the input doesn't match the pattern
- Error clears automatically as soon as the input matches during typing
- Works alongside other validation like `MinLength` and server-side validation

**With fluent API:**

```csharp
var email = UseState<string>("");

email.ToTextInput()
    .Pattern(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
    .Placeholder("Email address")
    .Label("Email")
```

**Common patterns:**

```csharp
// URL validation
.Pattern(@"^https?://[^\s]+$")

// Alphanumeric only
.Pattern(@"^[a-zA-Z0-9]+$")

// Username (letters, numbers, underscore, 3-16 chars)
.Pattern(@"^[a-zA-Z0-9_]{3,16}$")

// Hexadecimal color code
.Pattern(@"^#[0-9A-Fa-f]{6}$")
```

The error message shown to users is "Please match the requested format". Pattern validation is applied **after** server-provided errors and `MinLength` validation, ensuring the most relevant error is always displayed first.

### New ScatterChart Widget

Scatter charts are now available in Ivy! This new widget supports standard scatter plots, bubble charts, and connected scatter plots with extensive customization options.

**Basic scatter plot:**

```csharp
ScatterChartView.Create(myData)
    .Dimension("temperature", d => d.Temperature)
    .Measure("pressure", d => d.Pressure)
    .Build()
```

**Bubble chart with size encoding:**

```csharp
ScatterChartView.Create(myData)
    .Dimension("x", d => d.XValue)
    .Measure("y", d => d.YValue)
    .Size("population", d => d.Population)
    .ZAxis(zAxis => zAxis
        .Min(0)
        .Max(1000000))
    .Build()
```

**Color-encoded scatter plot:**

```csharp
ScatterChartView.Create(myData)
    .Dimension("height", d => d.Height)
    .Measure("weight", d => d.Weight)
    .Color("category", d => d.Category)
    .Build()
```

**Customize point shapes:**

The widget supports 7 different point shapes: Circle (default), Square, Cross, Diamond, Star, Triangle, and Wye. Configure shapes through the Scatter series:

```csharp
new ScatterChart()
    .Series(new Scatter()
        .Shape(ScatterShape.Diamond)
        .Name("Data Points"))
```

**Connected scatter plots:**

Create connected scatter plots to show progression or relationships:

```csharp
new ScatterChart()
    .Series(new Scatter()
        .ShowLine(true)
        .LineType(ScatterLineType.Smooth))
```

Like all Ivy charts, ScatterChart supports tooltips, legends, toolbox controls, grid customization, and reference lines.

### New RadarChart Widget

Visualize multi-dimensional data across multiple quantitative variables with the new `RadarChart` widget! Display data on axes radiating from a center point to create spider web patterns—perfect for comparing profiles, skill assessments, and performance metrics.

**Basic radar chart:**

```csharp
var data = new[]
{
    new { name = "Product A", Sales = 85, Marketing = 72, Development = 90, Support = 78, Quality = 88 }
};

new RadarChart(data)
    .ColorScheme(ColorScheme.Default)
    .Indicator("Sales", 100)
    .Indicator("Marketing", 100)
    .Indicator("Development", 100)
    .Indicator("Support", 100)
    .Indicator("Quality", 100)
    .Tooltip()
    .Legend()
```

**Skill assessment with filled areas:**

```csharp
var data = new[]
{
    new { name = "Candidate A", Technical = 90, Communication = 75, Leadership = 80, ProblemSolving = 85, Teamwork = 88 }
};

new RadarChart(data)
    .ColorScheme(ColorScheme.Default)
    .Indicator("Technical", 100)
    .Indicator("Communication", 100)
    .Indicator("Leadership", 100)
    .Indicator("ProblemSolving", 100)
    .Indicator("Teamwork", 100)
    .Radar(new Radar("values").Filled())
    .Shape(RadarShape.Circle)
    .Tooltip()
```

**Key features:**

- **Two shape modes**: Polygon (angular spider web) or Circle (smooth circular pattern)
- **Indicators**: Define each axis with custom min/max values for each dimension
- **Filled areas**: Enable `.Filled()` on Radar series to highlight overall capability patterns
- **Visual customization**: Control split lines, split areas, axis lines, and start angle
- **Symbol markers**: Show or hide data point symbols with `.ShowSymbol()`
- **Multiple series**: Compare multiple datasets on the same radar chart

Each indicator represents an axis dimension. Configure them with custom ranges:

```csharp
new RadarChart(data)
    .Indicator("Performance", max: 100)
    .Indicator("Scalability", max: 100)
    .Indicator("Security", max: 100)
    .Radar(new Radar("values")
        .Fill(Colors.Primary)
        .Stroke(Colors.Primary)
        .Filled())
    .Shape(RadarShape.Polygon)
```

Like all Ivy charts, RadarChart supports tooltips, legends, and toolbox controls for a complete interactive experience.

**Ergonomic .ToRadarChart() extension methods:**

Create radar charts quickly using the `.ToRadarChart()` extension method, which follows the same ergonomic pattern as `.ToBarChart()`, `.ToPieChart()`, and `.ToScatterChart()`:

```csharp
var data = new[]
{
    new { Department = "Engineering", Speed = 85, Quality = 92, Innovation = 78, Collaboration = 88, Delivery = 80 },
    new { Department = "Marketing", Speed = 70, Quality = 75, Innovation = 90, Collaboration = 85, Delivery = 72 },
    new { Department = "Sales", Speed = 90, Quality = 68, Innovation = 65, Collaboration = 92, Delivery = 95 },
};

// Default style
data.ToRadarChart(
    x => x.Department,
    [
        q => q.Sum(x => x.Speed),
        q => q.Sum(x => x.Quality),
        q => q.Sum(x => x.Innovation),
        q => q.Sum(x => x.Collaboration),
        q => q.Sum(x => x.Delivery),
    ]
)

// Circle style with filled areas
data.ToRadarChart(
    x => x.Department,
    [
        q => q.Sum(x => x.Speed),
        q => q.Sum(x => x.Quality),
        q => q.Sum(x => x.Innovation),
        q => q.Sum(x => x.Collaboration),
        q => q.Sum(x => x.Delivery),
    ],
    RadarChartStyles.Circle
)

// Dashboard style with custom polish
data.ToRadarChart(
    x => x.Department,
    [
        q => q.Sum(x => x.Speed),
        q => q.Sum(x => x.Quality),
        q => q.Sum(x => x.Innovation),
        q => q.Sum(x => x.Collaboration),
        q => q.Sum(x => x.Delivery),
    ],
    RadarChartStyles.Dashboard,
    polish: chart => chart.Toolbox()
)
```

The extension method takes:

- **Category expression**: Groups your data (e.g., department names)
- **Measure expressions**: Array of aggregations for each radar axis
- **Style** (optional): Choose from `Default`, `Circle`, or `Dashboard`
- **Polish function** (optional): Further customize the generated chart

This pattern makes it easy to generate radar charts from collections without manually configuring indicators and series.

### New SankeyChart Widget

Visualize flows and transfers between nodes with the new `SankeyChart` widget! This diagram style shows relationships where the width of each connecting arrow represents the magnitude of flow—perfect for analyzing user funnels, budget allocation, energy flows, and supply chains.

**Basic Sankey chart:**

```csharp
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
        new SankeyLink(0, 1, 3500),   // Visit -> Add to Cart
        new SankeyLink(0, 4, 1500),   // Visit -> Bounce
        new SankeyLink(1, 2, 2800),   // Cart -> Checkout
        new SankeyLink(1, 4, 700),    // Cart -> Bounce
        new SankeyLink(2, 3, 2200),   // Checkout -> Purchase
        new SankeyLink(2, 4, 600),    // Checkout -> Bounce
    }
);

new SankeyChart(data)
    .Tooltip()
    .Toolbox()
```

Links reference nodes by their index in the Nodes array, with a value representing flow magnitude.

**Customize appearance:**

```csharp
new SankeyChart(data)
    .NodeWidth(25)                    // Width of node rectangles
    .NodeGap(15)                      // Vertical spacing between nodes
    .Curvature(0.7)                   // Link curve amount (0-1)
    .NodeAlign(SankeyAlign.Left)      // Align nodes to left
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Toolbox()
```

**Key features:**

- **Flow visualization**: Arrow widths scale proportionally to flow quantities
- **Node alignment**: Use `SankeyAlign.Justify` (default) or `SankeyAlign.Left` for layout control
- **Visual customization**: Configure node dimensions, spacing, and link curvature
- **Color schemes**: Apply any ColorScheme to differentiate nodes and links
- **Interactive**: Full support for tooltips, legends, and toolbox controls

The SankeyChart is ideal for conversion funnels, budget breakdowns, energy distribution, user journey flows, and any scenario where you need to visualize both relationships and their relative magnitudes.

**Ergonomic .ToSankeyChart() extension methods:**

Create Sankey charts quickly using the `.ToSankeyChart()` extension method, which follows the same ergonomic pattern as other chart builders:

```csharp
public record EnergyFlow(string Source, string Target, double Amount);

var flows = new[]
{
    new EnergyFlow("Coal", "Electricity", 300),
    new EnergyFlow("Coal", "Heating", 150),
    new EnergyFlow("Natural Gas", "Electricity", 400),
    new EnergyFlow("Natural Gas", "Heating", 250),
    new EnergyFlow("Solar", "Electricity", 200),
    new EnergyFlow("Wind", "Electricity", 180),
    new EnergyFlow("Electricity", "Residential", 450),
    new EnergyFlow("Electricity", "Industrial", 350),
};

// Default style
flows.ToSankeyChart(
    f => f.Source,
    f => f.Target,
    f => f.Amount
)

// Left-aligned style
flows.ToSankeyChart(
    f => f.Source,
    f => f.Target,
    f => f.Amount,
    SankeyChartStyles.LeftAligned
)

// Dashboard style
flows.ToSankeyChart(
    f => f.Source,
    f => f.Target,
    f => f.Amount,
    SankeyChartStyles.Dashboard
)
```

The extension method takes:

- **Source expression**: The origin node for each flow
- **Target expression**: The destination node for each flow
- **Value expression**: The magnitude of the flow
- **Style** (optional): Choose from `Default`, `LeftAligned`, or `Dashboard`
- **Polish function** (optional): Further customize the generated chart

This pattern makes it easy to generate Sankey charts from collections without manually creating `SankeyData`, `SankeyNode`, and `SankeyLink` instances.

### New ChordChart Widget

Visualize relationships and flows between entities with the new `ChordChart` widget! This circular diagram displays inter-relationships between nodes using connecting arcs—perfect for analyzing migration flows, trade relationships, network traffic, department collaboration, and any data where connections between entities matter.

**Basic chord chart:**

```csharp
var data = new ChordData(
    Nodes: new[]
    {
        new ChordNode("North America"),
        new ChordNode("Europe"),
        new ChordNode("Asia"),
        new ChordNode("South America"),
        new ChordNode("Africa"),
    },
    Links: new[]
    {
        new ChordLink(0, 1, 1200),  // North America → Europe: 1200
        new ChordLink(0, 2, 900),   // North America → Asia: 900
        new ChordLink(1, 2, 800),   // Europe → Asia: 800
        new ChordLink(2, 3, 300),   // Asia → South America: 300
        new ChordLink(3, 4, 150),   // South America → Africa: 150
    }
);

new ChordChart(data)
    .Tooltip()
    .Toolbox()
```

Links reference nodes by their index in the Nodes array, with a value representing the relationship strength or flow magnitude.

**Trade relationships with color schemes:**

```csharp
var data = new ChordData(
    Nodes: new[]
    {
        new ChordNode("USA"),
        new ChordNode("China"),
        new ChordNode("Germany"),
        new ChordNode("Japan"),
        new ChordNode("UK"),
    },
    Links: new[]
    {
        new ChordLink(0, 1, 5800),
        new ChordLink(0, 3, 3400),
        new ChordLink(1, 3, 3200),
        new ChordLink(2, 4, 1200),
    }
);

new ChordChart(data)
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Toolbox()
    .Legend()
```

**Key features:**

- **Circular layout**: Nodes arranged in a circle with arcs connecting related entities
- **Visual flow magnitude**: Connection widths represent relationship strength
- **Sorting**: Use `.Sort()` to arrange nodes by total connection value
- **Visual customization**: Configure padding angles, color schemes, and sub-group sorting
- **Interactive**: Full support for tooltips, legends, and toolbox controls

Customize the chart appearance with configuration options:

```csharp
new ChordChart(data)
    .Sort(true)              // Sort nodes by connection totals
    .SortSubGroups(true)     // Sort sub-groups within nodes
    .PadAngle(5)             // Spacing between nodes in degrees
    .ColorScheme(ColorScheme.Rainbow)
    .Tooltip()
    .Legend()
```

The ChordChart is ideal for visualizing any bidirectional or unidirectional relationships where both the connections and their magnitudes are important to understand.

**Ergonomic .ToChordChart() extension methods:**

Create chord charts quickly using the `.ToChordChart()` extension method, which follows the same ergonomic pattern as other chart builders:

```csharp
var trades = new[]
{
    new TradeFlow("USA", "China", 5800),
    new TradeFlow("USA", "Germany", 2100),
    new TradeFlow("USA", "Japan", 3400),
    new TradeFlow("China", "Germany", 1800),
    new TradeFlow("China", "Japan", 3200),
    new TradeFlow("Germany", "Japan", 800),
};

// Default style
trades.ToChordChart(
    t => t.Exporter,
    t => t.Importer,
    t => t.Volume
)

// Sorted style (sorts by connection totals)
trades.ToChordChart(
    t => t.Exporter,
    t => t.Importer,
    t => t.Volume,
    ChordChartStyles.Sorted
)

// Dashboard style with toolbox
trades.ToChordChart(
    t => t.Exporter,
    t => t.Importer,
    t => t.Volume,
    ChordChartStyles.Dashboard
)
```

The extension method takes:

- **Source expression**: The origin node for each connection
- **Target expression**: The destination node for each connection
- **Value expression**: The magnitude of the flow/relationship
- **Style** (optional): Choose from `Default`, `Sorted`, or `Dashboard`
- **Polish function** (optional): Further customize the generated chart

This pattern makes it easy to generate chord charts from any collection without manually creating `ChordData`, `ChordNode`, and `ChordLink` instances.

### New FunnelChart Widget

Visualize conversion rates and multi-stage processes with the new `FunnelChart` widget! Display data as progressively narrowing stages—perfect for sales pipelines, user onboarding flows, recruitment processes, and any workflow where values move through sequential steps.

**Basic funnel chart:**

```csharp
var data = new[]
{
    new PieChartData("Awareness", 5000),
    new PieChartData("Interest", 3500),
    new PieChartData("Decision", 2100),
    new PieChartData("Action", 1200),
};

new FunnelChart(data)
    .Funnel("Measure", "Dimension")
    .Tooltip()
    .Toolbox()
```

**Control sort order:**

By default, stages sort in descending order (largest at top). Change this with `.Sort()`:

```csharp
// Ascending order (smallest at top)
new FunnelChart(data)
    .Funnel("Measure", "Dimension")
    .Sort(FunnelSort.Ascending)

// No sorting (preserves data order)
new FunnelChart(data)
    .Funnel("Measure", "Dimension")
    .Sort(FunnelSort.None)
```

**Horizontal orientation:**

Display funnels horizontally instead of vertically:

```csharp
new FunnelChart(data)
    .Funnel("Measure", "Dimension")
    .Orientation(FunnelOrientation.Horizontal)
    .Tooltip()
```

**Key features:**

- **Stage visualization**: Width proportional to stage values shows drop-off at each step
- **Sort options**: Choose `Descending`, `Ascending`, or `None` to control stage ordering
- **Orientation**: Display as `Vertical` (default) or `Horizontal` funnels
- **Gap control**: Add spacing between stages with `.Gap(pixels)`
- **Color schemes**: Apply any ColorScheme to differentiate stages
- **Interactive**: Full support for tooltips, legends, and toolbox controls

The FunnelChart uses the same `PieChartData` record format (dimension/measure pairs) and supports the standard chart customization options. Ideal for analyzing conversion funnels, sales pipelines, recruitment workflows, and any process where understanding drop-off between stages is important.

**Ergonomic .ToFunnelChart() extension methods:**

Create funnel charts quickly using the `.ToFunnelChart()` extension method, which follows the same ergonomic pattern as `.ToBarChart()`, `.ToPieChart()`, and `.ToRadarChart()`:

```csharp
var data = new[]
{
    new SalesFunnelStage("Prospects", 8000),
    new SalesFunnelStage("Qualified", 5200),
    new SalesFunnelStage("Proposals", 3100),
    new SalesFunnelStage("Negotiations", 1800),
    new SalesFunnelStage("Closed", 950),
};

// Default style
data.ToFunnelChart(
    x => x.Stage,
    x => x.Sum(s => s.Count)
)

// Horizontal style
data.ToFunnelChart(
    x => x.Stage,
    x => x.Sum(s => s.Count),
    FunnelChartStyles.Horizontal
)

// Dashboard style with custom polish
data.ToFunnelChart(
    x => x.Stage,
    x => x.Sum(s => s.Count),
    FunnelChartStyles.Dashboard
)
```

The extension method takes:

- **Stage expression**: Labels for each funnel stage (e.g., stage names)
- **Value expression**: Aggregation for the value at each stage
- **Style** (optional): Choose from `Default`, `Horizontal`, or `Dashboard`
- **Polish function** (optional): Further customize the generated chart

This pattern makes it easy to generate funnel charts from collections without manually creating `FunnelChart` and `PieChartData` instances.

### Enhanced ScreenshotFeedback Widget with Annotation Tools

The `ScreenshotFeedback` widget now includes powerful annotation tools and structured data export! Add callout markers, arrows, blur censoring, and text annotations to screenshots with an Ivy-themed interface and keyboard shortcuts.

**New annotation tools:**

- **Callout**: Two-click numbered annotations perfect for step-by-step instructions
- **Arrow**: Point to specific UI elements with directional arrows
- **Censor**: Blur sensitive information with pixelation effect
- **Freehand, Line, Rectangle, Circle, Text**: All the essential drawing tools

**Structured annotation data export:**

The `OnSave` event now returns detailed annotation data including all shapes, positions, and metadata:

```csharp
new ScreenshotFeedback()
    .IsOpen(isOpen)
    .HandleSave(annotationData =>
    {
        // Access structured annotation data
        foreach (var shape in annotationData.Shapes)
        {
            switch (shape)
            {
                case CalloutAnnotation callout:
                    Console.WriteLine($"Callout #{callout.Number}: {callout.Text}");
                    Console.WriteLine($"  Position: ({callout.Anchor.X}, {callout.Anchor.Y})");
                    break;

                case ArrowAnnotation arrow:
                    Console.WriteLine($"Arrow from ({arrow.Start.X}, {arrow.Start.Y}) to ({arrow.End.X}, {arrow.End.Y})");
                    break;

                case CensorAnnotation censor:
                    Console.WriteLine($"Censored area: {censor.Start.X},{censor.Start.Y} to {censor.End.X},{censor.End.Y}");
                    break;
            }
        }

        // Screenshot dimensions available
        Console.WriteLine($"Screenshot size: {annotationData.ScreenshotWidth}x{annotationData.ScreenshotHeight}");
    })
```

**Convenient overloads:**

If you don't need the annotation data, use the simple callback:

```csharp
new ScreenshotFeedback()
    .IsOpen(isOpen)
    .HandleSave(() => Console.WriteLine("Screenshot saved!"))
```

**Keyboard shortcuts:**

- **Ctrl+S** (or Cmd+S): Submit annotated screenshot
- **Ctrl+Z** (or Cmd+Z): Undo last annotation
- **ESC**: Cancel (closes the widget)

**UI improvements:**

The widget now features an Ivy dark green theme with drop shadows, SVG toolbar icons, and CSS tooltips for a polished user experience. Callout numbers automatically increment, and all annotations support customizable colors and line widths.

The annotation system uses `JsonPolymorphic` attributes for clean deserialization, making it easy to process different annotation types in your C# code.

### Scroll Support for Layout and StackLayout

Layouts now support scrollable content! Add scroll behavior to any `Layout` or `StackLayout` with height or width constraints using the new `.Scroll()` method.

**Vertical scrolling:**

```csharp
Layout.Vertical()
    .Height(Size.Units(30))
    .Scroll(Scroll.Vertical)
    .Gap(2)
    | new Badge("Item 1") | new Badge("Item 2") | new Badge("Item 3")
    | new Badge("Item 4") | new Badge("Item 5") | new Badge("Item 6")
    | new Badge("Item 7") | new Badge("Item 8") | new Badge("Item 9")
    | new Badge("Item 10") | new Badge("Item 11") | new Badge("Item 12")
```

**Using with StackLayout:**

```csharp
new StackLayout([
    new Badge("Item 1"), new Badge("Item 2"), new Badge("Item 3"),
    new Badge("Item 4"), new Badge("Item 5"), new Badge("Item 6"),
    new Badge("Item 7"), new Badge("Item 8"), new Badge("Item 9"),
    new Badge("Item 10"), new Badge("Item 11"), new Badge("Item 12")
], gap: 2) { Scroll = Scroll.Vertical }
    .Height(Size.Units(30))
    .Width(Size.Full())
```

**Scroll modes:**

- `Scroll.Vertical` - Scrolls vertically, hides horizontal overflow
- `Scroll.Horizontal` - Scrolls horizontally, hides vertical overflow
- `Scroll.Both` - Scrolls in both directions
- `Scroll.Auto` - Browser determines scroll behavior
- `Scroll.None` - No scrolling (default)

Perfect for creating fixed-height containers with scrollable content like sidebar navigation, chat message lists, or data tables.

### Independent Row and Column Gap Control

Fine-tune spacing in your layouts with independent control over vertical (row) and horizontal (column) gaps. The `.Gap()` method now accepts two parameters for precise spacing control.

**Basic usage:**

```csharp
// Gap applies to both directions
Layout.Wrap().Gap(4)
    | new Badge("A") | new Badge("B") | new Badge("C")

// Independent row and column gap
Layout.Wrap().Gap(rowGap: 8, columnGap: 2)
    .Width(Size.Units(60))
    | new Badge("A") | new Badge("B") | new Badge("C")
    | new Badge("D") | new Badge("E") | new Badge("F")
    | new Badge("G") | new Badge("H") | new Badge("I")
```

**With StackLayout:**

```csharp
new StackLayout([
    new Badge("A"), new Badge("B"), new Badge("C"),
    new Badge("D"), new Badge("E"), new Badge("F"),
    new Badge("G"), new Badge("H")
], Orientation.Horizontal, wrap: true)
{
    RowGap = 8,      // Vertical spacing
    ColumnGap = 2    // Horizontal spacing
}.Width(Size.Units(60))
```

This is especially useful in wrapped layouts where you want tighter horizontal spacing between items but more vertical breathing room between rows.

### Space Distribution Alignment

Distribute space between elements in your layouts using `SpaceBetween`, `SpaceAround`, or `SpaceEvenly` alignment options. These work with both `Layout` methods and `StackLayout` widgets.

**Using with Layout methods:**

```csharp
Layout.Vertical().Gap(4)
    | Text.Label("SpaceBetween — items pushed to edges:")
    | (Layout.Horizontal().Align(Align.SpaceBetween).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceAround — equal space around each item:")
    | (Layout.Horizontal().Align(Align.SpaceAround).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
    | Text.Label("SpaceEvenly — equal space between all items:")
    | (Layout.Horizontal().Align(Align.SpaceEvenly).Width(Size.Full())
        | new Badge("A") | new Badge("B") | new Badge("C"))
```

**Using with StackLayout:**

```csharp
new StackLayout([
    Text.Label("SpaceBetween:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceBetween),
    Text.Label("SpaceAround:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceAround),
    Text.Label("SpaceEvenly:"),
    new StackLayout([new Badge("A"), new Badge("B"), new Badge("C")],
        Orientation.Horizontal, align: Align.SpaceEvenly)
], gap: 4).Width(Size.Full())
```

### AspectRatio Support for All Widgets

You can now maintain proportional dimensions on any widget using the new `AspectRatio` property. This is particularly useful for creating responsive layouts that preserve aspect ratios regardless of screen size.

Use the `.AspectRatio()` extension method with any widget:

```csharp
// Common aspect ratios
new Box(Text.Block("16:9 Video"))
    .Width(Size.Units(80))
    .AspectRatio(16f / 9f)
    .Background(Colors.Primary);

new Box(Text.Block("Square"))
    .Width(Size.Units(40))
    .AspectRatio(1f)
    .Background(Colors.Warning);

// Works with any widget inheriting from WidgetBase
new Card()
    .Width(Size.Units(100))
    .AspectRatio(4f / 3f);
```

The `AspectRatio` property is available on `WidgetBase`, so it works with all widgets including `Box`, `Card`, `StackLayout`, and more.

### SelectInput Radio and Slider Variants

The `SelectInput` widget now supports two powerful new variants: **Radio** for traditional radio button selection and **Slider** for selecting from ordered discrete options.

**Radio variant for single-select forms:**

The Radio variant renders familiar radio buttons, perfect for settings, configuration UIs, and forms where all options should be visible:

```csharp
public class NotificationSettings : ViewBase
{
    public override object? Build()
    {
        var frequency = UseState("Daily");

        return frequency.ToSelectInput(["Immediately", "Daily", "Weekly", "Never"])
            .Radio()
            .WithField()
            .Label("Notification frequency")
            .Width(Size.Full());
    }
}
```

**Slider variant for ordered options:**

The Slider variant is ideal for selecting from an ordered list like sizes, priority levels, or quality settings. It renders a range slider that snaps to each option:

```csharp
public class ProductSelector : ViewBase
{
    private enum Priority { Low, Medium, High, Critical }

    public override object? Build()
    {
        var size = UseState("M");
        var priority = UseState(Priority.Medium);

        return Layout.Vertical()
            | size.ToSelectInput(new[] { "XS", "S", "M", "L", "XL", "XXL" }.ToOptions())
                .Slider()
                .WithField()
                .Label("T-Shirt Size")
            | priority.ToSelectInput()
                .Slider()
                .WithField()
                .Label("Priority");
    }
}
```

The Slider variant shows option labels in a tooltip as you slide, displays dot indicators for each value, and provides intuitive feedback for ordered discrete selections. Both variants support all standard SelectInput features including validation, disabled state, and density controls.

### SelectInput Option Descriptions

Options in `SelectInput` widgets can now include descriptions that display as caption text below the label. This feature works with Toggle, Radio (List), and Checkbox (List) variants, making it easy to provide helpful context for each option.

**Add descriptions to options:**

```csharp
var genreOptions = new IAnyOption[]
{
    new Option<string>("Comedy", "Comedy")
        { Description = "Laugh out loud." },
    new Option<string>("Drama", "Drama")
        { Description = "Get the popcorn." },
    new Option<string>("Documentary", "Documentary")
        { Description = "Never stop learning." },
    new Option<string>("Action", "Action")
        { Description = "Edge of your seat thrills." }
};

// Works with Toggle variant
genreToggle.ToSelectInput(genreOptions)
    .Variant(SelectInputVariant.Toggle)

// Works with Radio variant (List, single-select)
genreRadio.ToSelectInput(genreOptions)
    .Variant(SelectInputVariant.List)

// Works with Checkbox variant (List, multi-select)
genreCheckbox.ToSelectInput(genreOptions)
    .Variant(SelectInputVariant.List)
```

Descriptions appear as smaller, muted text below the option label, providing a clean way to add explanatory text without cluttering the interface.

### Configurable Server Listening Interface

You can now configure which network interface your Ivy server listens on using the `--host` CLI argument or `HOST` environment variable. This is particularly useful when deploying to containerized or hosted environments.

**Using the CLI argument:**

```bash
ivy run --host 0.0.0.0 --port 8080
```

**Using environment variable:**

```bash
export HOST=0.0.0.0
ivy run
```

**Default behavior:**

- Local development: Binds to `localhost` by default
- Docker containers: Automatically binds to `*` (all interfaces)
- When `PORT` environment variable is set: Automatically binds to `*` (all interfaces)

The `--host` CLI argument takes precedence over the `HOST` environment variable, following standard CLI conventions.

### New Script Widget

Load external JavaScript files or execute inline JavaScript code with the new `Script` widget! This widget doesn't render any visible output—it simply injects `<script>` elements into the page head, making it perfect for adding analytics, third-party libraries, or custom JavaScript behavior to your apps.

**Load an external JavaScript library:**

```csharp
new Script("https://cdn.jsdelivr.net/npm/canvas-confetti@1.9.3/dist/confetti.browser.min.js")
```

**Execute inline JavaScript:**

```csharp
new Script().InlineCode("console.log('Hello from Ivy!');")
```

**Control script loading behavior:**

```csharp
// Load asynchronously (doesn't block page rendering)
new Script("https://example.com/analytics.js").Async()

// Defer execution until page is parsed
new Script("https://example.com/widget.js").Defer()
```

**Add security with Subresource Integrity:**

```csharp
new Script("https://cdn.example.com/lib.js")
    .Integrity("sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8w")
    .CrossOrigin("anonymous")
```

**Security considerations:**

- **Never pass user-supplied input** to `Src` or `InlineCode`—this widget executes arbitrary JavaScript
- Use `Integrity` for external scripts to prevent tampering
- Set `CrossOrigin` when using integrity checks

The Script widget is ideal for integrating analytics platforms, loading visualization libraries, adding custom tracking code, or injecting third-party widgets into your Ivy apps.

### TextBlock Anchor Navigation

Headings in `TextBlock` (H1-H6) now support anchor IDs for URL hash navigation! Add explicit anchor IDs or auto-generate them from heading text to enable deep linking and table-of-contents style navigation.

**Auto-generate anchor from heading text:**

```csharp
Text.H2("Getting Started").Anchor()
// Creates anchor: #getting-started
```

**Set explicit anchor ID:**

```csharp
Text.H2("API Reference").Anchor("api-docs")
// Creates anchor: #api-docs
```

**Build a navigation structure:**

```csharp
Layout.Vertical()
    | Text.H1("Documentation").Anchor("top")
    | Text.H2("Installation").Anchor()
    | Text.P("Follow these steps to install...")
    | Text.H2("Configuration").Anchor()
    | Text.P("Configure your app settings...")
    | Text.H2("Advanced Topics").Anchor("advanced")
    | Text.P("Deep dive into advanced features...")
```

When using `.Anchor()` without parameters, the framework automatically generates a URL-safe slug by converting to lowercase, replacing spaces with hyphens, and stripping special characters. For example, "Getting Started!" becomes "getting-started".

Anchors render as HTML `id` attributes on heading elements, allowing users to link directly to specific sections with URLs like `yourapp.com#getting-started`.

### VideoPlayer Volume Control

The `VideoPlayer` widget now supports fine-grained volume control! Set the playback volume from 0.0 (muted) to 1.0 (full volume) for precise audio level management.

**Set volume levels:**

```csharp
// Half volume
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Volume(0.5f)

// Quiet background video
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Volume(0.2f)
    .Autoplay(true)
    .Loop(true)

// Full volume (same as browser default)
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Volume(1.0f)
```

**Combine with other controls:**

```csharp
new VideoPlayer("https://www.w3schools.com/html/mov_bbb.mp4")
    .Volume(0.3f)
    .Autoplay(false)
    .Controls(true)
    .Loop(true)
    .Muted(false)
```

Volume values are automatically clamped between 0.0 and 1.0, so you don't need to worry about out-of-range values. The volume setting is applied when the video element loads in the browser.

This is especially useful for background videos, ambient soundscapes, tutorials with voiceover, or any scenario where you want to control the default audio level programmatically.

### Json and Xml Widget Enhancements

The `Json` and `Xml` widgets now support expansion control and simplified usage, making it easier to work with structured data visualization.

**Control initial tree expansion:**

Both widgets now have an `Expanded` property to control how deeply the tree is initially expanded:

```csharp
var data = new { name = "John", address = new { city = "NYC", zip = "10001" } };

// Collapsed (default)
new Json(data)

// Expand first level
new Json(data) { Expanded = 1 }

// Fully expanded
new Json(data) { Expanded = -1 }
```

**Direct object serialization for Json:**

The `Json` widget now accepts any object directly—no need to manually serialize:

```csharp
// Before: manual serialization required
var json = JsonSerializer.Serialize(myData);
new Json(json)

// Now: pass objects directly
new Json(myData)
```

The object is automatically serialized with proper indentation. You can still pass JSON strings directly for full control over formatting.

### Small Icon Buttons

Icon-only buttons now support the `.Small()` density option for a more compact appearance. When combined with icon buttons, this creates a smaller 6x6 unit button (compared to the default 9x9 units).

```csharp
// Small icon buttons
Layout.Horizontal(
    Icons.MessageSquareX.ToButton(eventHandler).Small(),
    Icons.Heart.ToButton(eventHandler, ButtonVariant.Destructive).Small(),
    Icons.Star.ToButton(eventHandler, ButtonVariant.Outline).Small()
)
```

This is particularly useful in toolbars, action panels, and other UI areas where space is at a premium.

### Ivy.Analyser: Detect Inline Hook Calls

A new analyzer rule **IVYHOOK007** helps you write cleaner, more maintainable code by detecting hooks that are called inline within expressions. While inline hooks may work at runtime, they obscure state management and make debugging harder.

**What triggers the warning:**

```csharp
// Hooks called inline in a pipe chain
public override object? Build()
{
    return new Card(
        Layout.Vertical().Gap(3)
            | UseState(true).ToBoolInput().Label("Email notifications")
            | UseState(false).ToBoolInput().Label("SMS notifications")
    );
}

// Hook inline in return statement
public override object? Build()
{
    return UseState(true).ToBoolInput();
}

// Hook inline as constructor argument
public override object? Build()
{
    return new Card(UseState(0).Value);
}
```

**How to fix it:**

Extract each hook call to a local variable at the top of your `Build()` method:

```csharp
// Hooks assigned to variables first
public override object? Build()
{
    var emailNotifications = UseState(true);
    var smsNotifications = UseState(false);

    return new Card(
        Layout.Vertical().Gap(3)
            | emailNotifications.ToBoolInput().Label("Email notifications")
            | smsNotifications.ToBoolInput().Label("SMS notifications")
    );
}
```

This pattern makes your state management explicit and easier to track, especially when building complex UIs with multiple hooks.

## Breaking Changes

### Visible Property Removed from Widgets

The `Visible` property and related methods have been removed from `WidgetBase`. This simplifies the widget API and encourages the use of conditional rendering, which is more idiomatic in Ivy.

**Removed APIs:**

- `widget.Visible` property
- `.Visible(bool)` extension method
- `.Show()` extension method
- `.Hide()` extension method

**Migration:**

Instead of using the `Visible` property to conditionally show/hide widgets, use C#'s conditional operators (`?:` or `??`) to conditionally render widgets:

```csharp
// Before (no longer works)
new Button("Click me")
    .Visible(isActive)

new Card()
    .Visible(showCard)

// After (use conditional rendering)
isActive ? new Button("Click me") : null

showCard ? new Card() : null
```

For more complex conditional layouts, you can use C#'s pattern matching or conditional expressions:

```csharp
// Conditional layout children
Layout.Vertical()
    | Text.H1("Title")
    | (hasContent ? contentWidget : null)
    | (showFooter ? footerWidget : null)

// Ternary for different widgets
condition
    ? new Button("Option A")
    : new Button("Option B")
```

This pattern is more explicit about what gets rendered and aligns with modern UI framework patterns like React's conditional rendering.

### Input Widget Constructors Made Internal

All Input widget constructors have been made `internal`, requiring you to use the `.To[Type]Input()` extension methods instead of direct instantiation. This enforces a more consistent API pattern across the framework.

**Affected widgets:**

- `TextInput`, `CodeInput`, `ColorInput`
- `NumberInput`, `BoolInput`, `SelectInput`
- `DateTimeInput`, `DateRangeInput`
- `FileInput`, `FeedbackInput`, `IconInput`
- `ReadOnlyInput`

**Migration:**

Direct instantiation with `new` will now result in compilation errors. You must use the extension methods:

```csharp
// Before (no longer works)
var textInput = new TextInput(state);
var selectInput = new SelectInput<string>(state, options);
var numberInput = new NumberInput();

// After (use extension methods)
var textInput = state.ToTextInput();
var selectInput = state.ToSelectInput(options);
// For stateless inputs, create a state first:
var numberState = UseState(0);
var numberInput = numberState.ToNumberInput();
```

**With values and callbacks:**

```csharp
// Before (no longer works)
var input = new TextInput("initial", onChange, placeholder: "Enter text");
var select = new SelectInput<string>("value", onSelectChange, options);

// After (use extension methods)
var input = "initial".ToTextInput(onChange, placeholder: "Enter text");
var select = "value".ToSelectInput(onSelectChange, options);
```

**Extension methods work on:**

- States: `state.ToTextInput()`
- Values: `"text".ToTextInput(onChange)`
- Primitives with state creation: Create a state first, then call the extension method

This change ensures all Input widgets are created consistently through extension methods, making the API more predictable and easier to learn.

### Widget Event Handlers Renamed from Handle*to On*

Event handler extension methods in external widget packages have been renamed from `Handle*` to `On*` to align with the naming convention established throughout the framework. This affects the Leaflet, ScreenshotFeedback, Tiptap, and Xterm widget packages.

**Affected widgets and methods:**

**Iframe widget:**

- `HandleMessageReceived` → `OnMessageReceived`

**Leaflet Map widget:**

- `HandleMapClick` → `OnMapClick`
- `HandleMarkerClick` → `OnMarkerClick`
- `HandleMarkerDrag` → `OnMarkerDrag`
- `HandleZoomChange` → `OnZoomChange`
- `HandleCenterChange` → `OnCenterChange`
- `HandleBoundsChange` → `OnBoundsChange`

**ScreenshotFeedback widget:**

- `HandleSave` → `OnSave`
- `HandleCancel` → `OnCancel`

**TiptapInput widget:**

- `HandleFocus` → `OnFocus`
- `HandleBlur` → `OnBlur`

**Xterm Terminal widget:**

- `HandleInput` → `OnInput`
- `HandleResize` → `OnResize`
- `HandleLinkClick` → `OnLinkClick`

**Migration examples:**

```csharp
// Iframe - Before
new Iframe()
    .HandleMessageReceived(e => ProcessMessage(e.Value))

// Iframe - After
new Iframe()
    .OnMessageReceived(e => ProcessMessage(e.Value))

// Leaflet Map - Before
new Map()
    .HandleMapClick(latLng => Console.WriteLine($"Clicked: {latLng}"))
    .HandleMarkerClick(markerId => Console.WriteLine($"Marker: {markerId}"))
    .HandleZoomChange(zoom => Console.WriteLine($"Zoom: {zoom}"))

// Leaflet Map - After
new Map()
    .OnMapClick(latLng => Console.WriteLine($"Clicked: {latLng}"))
    .OnMarkerClick(markerId => Console.WriteLine($"Marker: {markerId}"))
    .OnZoomChange(zoom => Console.WriteLine($"Zoom: {zoom}"))

// ScreenshotFeedback - Before
new ScreenshotFeedback()
    .HandleSave(() => SaveScreenshot())
    .HandleCancel(() => CloseDialog())

// ScreenshotFeedback - After
new ScreenshotFeedback()
    .OnSave(() => SaveScreenshot())
    .OnCancel(() => CloseDialog())

// Xterm Terminal - Before
new Terminal()
    .HandleInput(pty.HandleInput)
    .HandleResize(pty.HandleResize)
    .HandleLinkClick(url => OpenLink(url))

// Xterm Terminal - After
new Terminal()
    .OnInput(pty.HandleInput)
    .OnResize(pty.HandleResize)
    .OnLinkClick(url => OpenLink(url))

// TiptapInput - Before
new TiptapInput()
    .HandleFocus(() => Console.WriteLine("Focused"))
    .HandleBlur(() => Console.WriteLine("Blurred"))

// TiptapInput - After
new TiptapInput()
    .OnFocus(() => Console.WriteLine("Focused"))
    .OnBlur(() => Console.WriteLine("Blurred"))
```

This change makes event handling consistent across all Ivy widgets, making the API more predictable and easier to learn. Simply find and replace `Handle` with `On` in your widget event handler calls.

### CLI: Prefer `ivy ask` over `ivy question`

The CLI documentation now promotes `ivy ask` as the primary command for querying the Ivy knowledge base. While both commands work (they're aliases), `ivy ask` is now the recommended and documented form.

```terminal
# Preferred command
ivy ask "How do I implement a new Application Shell in Ivy?"

# Also works (alias)
ivy question "How do I implement a new Application Shell in Ivy?"
```

All documentation examples now use `ivy ask` for consistency and brevity. This command queries the local context dynamically using integrated Local RAG features specifically tailored to your semantic `ivyVersion`.

## Security Enhancements

### Enhanced File Upload Security with Magic Byte Validation

File uploads are now protected against MIME type spoofing attacks! Ivy now validates that uploaded file content actually matches the declared Content-Type by checking magic bytes (file signatures).

**What this means for you:**

Previously, malicious users could potentially upload dangerous files (like executables or scripts) disguised as images by simply changing the Content-Type header. Now Ivy verifies the actual file content to ensure it matches the claimed type.

**Automatic protection:**

If you're using `FileInput` widgets with file type restrictions, this security enhancement is automatically applied. No code changes needed:

```csharp
new FileInput()
    .Accept("image/*")  // Now validates actual file content, not just headers
    .OnChange(files => HandleUpload(files))
```

**Manual validation API:**

For custom file handling scenarios, you can use the new validation methods directly:

```csharp
// Validate a single file with magic byte checking
var result = FileInputValidation.ValidateFileTypeWithMagicBytes(
    file,
    "image/*",
    fileStream
);

if (!result.IsValid)
{
    // File content doesn't match declared type - potential attack
    Console.WriteLine(result.ErrorMessage);
}
```

**Validate multiple files:**

```csharp
var result = FileInputValidation.ValidateFileTypesWithMagicBytes(
    files,
    ".pdf,.docx",
    file => file.OpenReadStream()
);
```

**Supported file types:**

Magic byte validation is implemented for all common file types including:

- **Images**: JPEG, PNG, GIF, BMP, WebP, TIFF, ICO
- **Documents**: PDF, Word (.doc/.docx), Excel (.xls/.xlsx), PowerPoint (.pptx)
- **Archives**: ZIP, RAR, 7z, gzip, tar
- **Audio**: MP3, WAV, OGG
- **Video**: MP4, WebM, AVI, QuickTime
- **Text formats**: Plain text, CSV, JSON, XML, SVG (allowed without magic byte checks)

This enhancement provides defense-in-depth security for your file upload features without any breaking changes.

### Stricter App ID Validation

App ID validation has been strengthened to prevent potential security issues. The framework now blocks additional characters that could be exploited in URL manipulation or path traversal attacks.

**Blocked characters:**

App IDs can no longer contain:

- `:` (colon) - URL protocol separator
- `?` (question mark) - URL query parameter separator
- `#` (hash) - URL fragment identifier
- `&` (ampersand) - URL query parameter separator
- `%` (percent) - URL encoding character **[NEW]**
- `\` (backslash) - Path separator on Windows **[NEW]**

**What this means:**

If your app creates or validates app IDs, ensure they don't contain these characters. App IDs should use alphanumeric characters, hyphens, and underscores for best compatibility:

```csharp
// Valid app IDs
"my-app"
"user_dashboard"
"app123"

// Invalid app IDs (will fail validation)
"my%20app"      // Contains %
"app\\name"     // Contains \
"app:version"   // Contains :
```

The validation is performed automatically by `ValidationHelper.IsValidAppId()` to protect against injection attacks and ensure app IDs work correctly across all platforms and URL contexts.

## Bug Fixes

- **DataTable**: Height in flex containers no longer grows infinitely; uses `flexGrow: 1` and `minHeight: 0` so tables size correctly in vertical layouts.
- **Breadcrumbs**: Serialization and frontend fixed; click handlers work; icon corrected to `Icons.House`.
- **App ID validation**: `%` and `\` are now disallowed in App IDs.
- **Calendar**: Empty slot content is no longer rendered when no children are provided.
- **Charts**: Area/Bar/Line respect `.Name()` in legends and tooltips; funnel tooltips use formatted values and percentages; bar chart background rectangles disabled by default; legend bottom spacing increased to 50px; chord legend hidden when not configured; radar chart center/radius adjust when legend is enabled.
- **Docker**: Server binds to `*` when `DOTNET_RUNNING_IN_CONTAINER` is set, fixing health probes in containers.
- **Card**: Density scaling for header icons and spacing (small/medium/large); content body visibility fixed (flex-auto instead of flex-1).
- **Text (Markdown)**: Density setting is now applied to the markdown variant.
- **External widgets**: React.lazy wrappers cached by type name to avoid unnecessary re-renders.
- **Ivy.Analyser**: IVYHOOK007 no longer flags hook calls that use the null-forgiving operator (`!`).
- **Static files**: `.md` files are served as static files instead of being routed by PathToAppIdMiddleware.
- **FileInput (Default variant)**: Only the upload button opens the file dialog; remove buttons and file items no longer trigger selection; container uses cursor-default.
- **FileInput**: Button scales correctly for `Density.Large`; default button text is "Select file" vs "Select files" based on single vs multiple selection.
- **Sidebar**: Hook names (e.g. UseRefreshToken) no longer split with spaces; "Use" prefix names are shown unchanged.
