# Ivy Framework Weekly Notes - Week of 2026-03-23

## Breaking Changes

### Server Metadata Property Reorganization

Server metadata properties have been reorganized into a dedicated `ServerMetadata` record for better structure. The flat properties on `ServerArgs` have been moved into a nested `Metadata` property.

**Migration:**

```csharp
// Old (no longer works)
var server = new Server(new ServerArgs
{
    MetaTitle = "My App",
    MetaDescription = "A cool app",
    MetaGitHubUrl = "https://github.com/user/repo"
});

// New
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My App",
        Description = "A cool app",
        GitHubUrl = "https://github.com/user/repo"
    }
});

// Or use the fluent API (unchanged)
server
    .SetMetaTitle("My App")
    .SetMetaDescription("A cool app")
    .SetMetaGitHubUrl("https://github.com/user/repo");
```

**Property mappings:**

- `ServerArgs.MetaTitle` → `ServerArgs.Metadata.Title`
- `ServerArgs.MetaDescription` → `ServerArgs.Metadata.Description`
- `ServerArgs.MetaGitHubUrl` → `ServerArgs.Metadata.GitHubUrl`

The fluent methods (`SetMetaTitle()`, `SetMetaDescription()`, `SetMetaGitHubUrl()`) continue to work unchanged, so if you're using those, no migration is needed.

### Chrome Renamed to AppShell

The `Chrome` API has been renamed to `AppShell` throughout the framework for better clarity and to avoid confusion with the Chrome browser. This affects all Chrome-related classes, interfaces, and methods.

**Migration Guide:**

```csharp
// Old (no longer works)
app.UseChrome<DefaultSidebarChrome>();
IChrome chrome = ...;

// New
app.UseAppShell<DefaultSidebarAppShell>();
IAppShell appShell = ...;
```

**What to update in your code:**

- `UseChrome()` → `UseAppShell()`
- `IChrome` → `IAppShell`
- `DefaultSidebarChrome` → `DefaultSidebarAppShell`
- Any custom Chrome classes should be renamed to AppShell
- Documentation and comments referencing "Chrome"

The term "AppShell" better describes the purpose of this component: providing the outer shell/layout for your application (sidebars, headers, navigation, etc.).

### UseService<T> Now Throws on Missing Registrations

`UseService<T>()` now throws an `InvalidOperationException` when a service is not registered in dependency injection, instead of silently returning `null`. This change helps catch configuration errors early rather than causing `NullReferenceException` errors later in your code.

**Migration**: If you have code that relies on `UseService<T>()` returning null for optional services, use the new `TryUseService<T>()` method instead (see below).

## New Features

### DateTimeInput Min, Max, and Step Constraints

Added `Min()`, `Max()`, and `Step()` constraint methods to `DateTimeInput` widgets, following the same pattern as `NumberInput`. You can now restrict date/time selections to specific ranges and enforce step intervals for time-based inputs.

```csharp
// Restrict date selection to a specific range
var birthDate = new DateTimeInput()
    .Variant(DateTimeInputVariant.Date)
    .Min(DateTime.Parse("1900-01-01"))
    .Max(DateTime.Now)
    .Bind(dateState);

// Enforce time intervals (e.g., 15-minute increments)
var appointmentTime = new DateTimeInput()
    .Variant(DateTimeInputVariant.Time)
    .Step(TimeSpan.FromMinutes(15))
    .Bind(timeState);

// Combine constraints for datetime inputs
var scheduleInput = new DateTimeInput()
    .Variant(DateTimeInputVariant.DateTime)
    .Min(DateTime.Now)
    .Max(DateTime.Now.AddDays(30))
    .Step(TimeSpan.FromMinutes(30))
    .Bind(scheduleState);
```

The constraints are enforced both in the calendar/picker UI and in manual input, ensuring users can only select valid values. The `Step` constraint is particularly useful for time inputs to restrict selections to specific intervals (e.g., 15-minute, 30-minute, or hourly slots).

### Box Widget Opacity Methods

Added `Opacity()` and `BorderOpacity()` fluent extension methods to the `Box` widget, allowing you to set opacity independently without needing to also specify a color.

```csharp
// Set background opacity without changing the background color
var box = new Box()
    .Background(Colors.Blue)
    .Opacity(0.5f); // 50% opacity

// Set border opacity independently
var box = new Box()
    .BorderColor(Colors.Red)
    .BorderOpacity(0.75f); // 75% opacity

// Or combine them in one fluent chain
var box = new Box()
    .Background(Colors.Green)
    .Opacity(0.8f)
    .BorderColor(Colors.Black)
    .BorderOpacity(0.6f);
```

Previously, you had to use `Background(color, opacity)` or `BorderColor(color, opacity)` overloads which required specifying the color and opacity together.

### Card and Box Shadow Hover Effect

Added a new `CardHoverVariant.Shadow` hover variant for Card and Box widgets, providing a subtle shadow elevation effect on hover. This creates a Material Design-inspired lifting animation without position translation.

```csharp
// Apply shadow hover effect to a Card
var card = new Card(content)
    .HoverVariant(CardHoverVariant.Shadow);

// Also works with Box widgets
var box = new Box(content)
    .HoverVariant(BoxHoverVariant.Shadow);
```

The shadow hover variant applies `hover:shadow-lg` on hover and `active:shadow-md` on click, creating a smooth elevation transition. This is useful for clickable cards that need visual feedback but shouldn't move on hover (unlike `CardHoverVariant.PointerAndTranslate`).

**Available hover variants:**

- `None` - No hover effect
- `Pointer` - Cursor changes to pointer only
- `PointerAndTranslate` - Cursor changes to pointer with subtle position shift
- `Shadow` - Cursor changes to pointer with shadow elevation effect

### Button Keyboard Shortcuts

Added `ShortcutKey()` method to the Button widget, allowing you to associate keyboard shortcuts with button actions. The shortcut listener is registered globally on the window, so buttons don't need to be focused to trigger their actions.

```csharp
Layout.Horizontal().Gap(8)
    | new Button("Search", _ => client.Toast("Searching..."))
        .Primary()
        .ShortcutKey("Ctrl+K")
    | new Button("Save", _ => client.Toast("Saved!"))
        .Secondary()
        .ShortcutKey("Ctrl+S")
```

This is particularly useful for creating keyboard-driven interfaces and improving productivity for power users who prefer keyboard navigation. The shortcuts work anywhere in your app, regardless of focus state.

### Open Graph and Twitter Card Meta Tags

Ivy now automatically adds Open Graph and Twitter Card meta tags to your application's HTML for rich social media previews when your app is shared on platforms like Twitter, LinkedIn, Slack, Discord, and more. The new `OpenGraphFilter` is automatically included in the HTML pipeline and intelligently derives metadata from your server configuration.

**Automatic features:**

- **Auto-generated social images**: When you set a GitHub URL, Ivy automatically generates an `og:image` URL using the pattern `https://banner.ivy.app/ogImage?text={Title}&repo={owner/repo}`
- **Smart site name**: The `og:site_name` is automatically derived from your entry assembly name if not explicitly set
- **Twitter Card support**: Automatically includes `twitter:*` meta tags for proper Twitter/X card rendering
- **Sensible defaults**: `og:type` defaults to "website", `og:locale` to "en_US", and `twitter:card` to "summary_large_image"

```csharp
// Basic setup - uses automatic image generation and site name
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app"
    }
});
// Automatically generates:
// - og:image: https://banner.ivy.app/ogImage?text=My+Ivy+App&repo=user/my-ivy-app
// - og:site_name: (derived from assembly name)

// Advanced: Override auto-generated values
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app",
        OgImage = "https://example.com/custom-image.png",  // Custom image
        OgSiteName = "My Company",                         // Custom site name
        OgType = "article",                                // Change type
        TwitterCard = "summary"                            // Change card type
    }
});
```

**Available ServerMetadata properties:**

| Property | Default | Description |
|----------|---------|-------------|
| `Title` | `null` | Page title (used for `og:title` and `twitter:title`) |
| `Description` | `null` | Page description (used for `og:description` and `twitter:description`) |
| `GitHubUrl` | `null` | GitHub repository URL (used to auto-generate `og:image`) |
| `OgImage` | Auto-generated from `GitHubUrl` + `Title` | Open Graph image URL |
| `OgSiteName` | Auto-derived from assembly name | Site name for `og:site_name` |
| `OgType` | `"website"` | Open Graph type |
| `OgLocale` | `"en_US"` | Open Graph locale |
| `TwitterCard` | `"summary_large_image"` | Twitter card type |

The generated meta tags include proper image dimensions (`og:image:width: 1200`, `og:image:height: 630`) and alt text for accessibility. This ensures your Ivy applications look great when shared on social media platforms without any additional configuration.

### Server Metadata: GitHub URL Support

Added `SetMetaGitHubUrl()` method to the Server API, allowing you to add GitHub repository metadata to your application's HTML for SEO and social sharing purposes.

```csharp
var server = new Server(new ServerArgs
{
    Metadata = new ServerMetadata
    {
        Title = "My Ivy App",
        Description = "A powerful web application",
        GitHubUrl = "https://github.com/user/my-ivy-app"
    }
});

// Or set it using the fluent API
server
    .SetMetaTitle("My Ivy App")
    .SetMetaDescription("A cool app")
    .SetMetaGitHubUrl("https://github.com/user/my-ivy-app");
```

The GitHub URL is primarily used for:

- **Auto-generating Open Graph images** for social media previews (using the pattern `https://banner.ivy.app/ogImage?text={Title}&repo={owner/repo}`)
- **SEO meta tags** added to your HTML
- **Social sharing** with proper repository attribution

The "Made with Ivy" badge in the bottom-right corner has been simplified to always show the "MADE WITH" Ivy logo and links to the Ivy Framework repository.

### Badge Color Property

Added `Color` property to the `Badge` widget, allowing you to customize badge colors using the full `Colors` enum. This provides direct color control matching the pattern used by `Icon`, `TextBlock`, and `Progress` widgets.

```csharp
// Use the fluent Color() method
var badge = new Badge("New")
    .Color(Colors.Green);

// Or set via constructor
var badge = new Badge("Error", color: Colors.Red);

// Combine with variant for more control
var badge = new Badge("Warning", BadgeVariant.Outline)
    .Color(Colors.Yellow);
```

The Color property works alongside the existing `BadgeVariant` options, giving you full control over badge appearance. When set, the color overrides the default variant styling with your custom color choice.

### UseCallback Hook for Memoizing Callbacks

Added `UseCallback()` hook as cleaner syntactic sugar over `UseMemo` for memoizing callback functions. This creates stable delegate references that only change when dependencies change, which is particularly useful for preventing unnecessary re-renders and for stable `UseEffect` dependencies.

```csharp
// Using UseCallback (recommended - cleaner syntax)
var onSubmit = UseCallback((string value) => Save(value), dependency);

// Equivalent using UseMemo (more verbose)
var onSubmit = UseMemo(() => (Action<string>)(value => Save(value)), dependency);

// Action with no parameters
var handleClick = UseCallback(() => DoSomething(), dependency);

// Func with return value
var computeValue = UseCallback((int x) => x * 2, multiplier.Value);

// Multiple parameters
var handleUpdate = UseCallback((string name, int age) => Update(name, age), userData.Value);
```

The hook provides overloads for common delegate patterns (`Action`, `Action<T>`, `Func<T>`, `Func<T, TResult>`, etc.) and is especially useful when:

- Passing callbacks to child components that check reference equality
- Using callbacks as `UseEffect` dependencies
- Creating event handlers with minimal dependencies

```csharp
// Good: Separate callbacks with minimal dependencies
var handleDataAction = UseCallback(() => DoSomethingWithData(data.Value), data.Value);
var handleFilterAction = UseCallback(() => ApplyFilter(filter.Value), filter.Value);
```

### UseMemo Auto-Unwraps IState Dependencies

`UseMemo()` now automatically unwraps `IState` dependencies, allowing you to pass state objects directly instead of manually extracting `.Value`. Both approaches work identically:

```csharp
var count = UseState(0);

// Both approaches work - auto-unwrapped internally
var doubled = UseMemo(() => count.Value * 2, count.Value);  // Explicit .Value
var doubled = UseMemo(() => count.Value * 2, count);        // Auto-unwrapped

// This is particularly useful with UseCallback
var handleClick = UseCallback(() => ProcessData(count.Value), count); // count auto-unwrapped
```

The factory function always needs `.Value` to read the current value, but the dependency array now accepts `IState` objects directly. This makes code cleaner while maintaining the same memoization behavior.

**Recommendation:** For clarity, prefer passing `.Value` explicitly in the dependency array, but know that passing the `IState` object also works.

### TryUseService<T> for Optional Service Lookups

Added `TryUseService<T>()` method for scenarios where a service might not be registered. This method returns a boolean indicating success and uses proper null-safety attributes.

```csharp
// Old approach (no longer works for optional services)
var service = UseService<IMyService>(); // throws if not registered

// New approach for optional services
if (TryUseService<IMyService>(out var service))
{
    // Service is available, use it
    service.DoSomething();
}
else
{
    // Service not registered, handle gracefully
    Console.WriteLine("Optional service not available");
}
```

### Form-Level LabelPosition API

Added the ability to set a default `LabelPosition` for all fields in a form via `FormBuilder.LabelPosition()`, with support for per-field overrides. This follows the same pattern as the existing `Density` property, making it easy to configure label positioning across your entire form.

```csharp
// Set default label position for all fields in the form
var form = new FormBuilder<UserModel>(userState)
    .LabelPosition(LabelPosition.Left)  // All labels on the left
    .Field(m => m.FirstName)
    .Field(m => m.LastName)
    .Field(m => m.Email);

// Override label position for specific fields
var form = new FormBuilder<UserModel>(userState)
    .LabelPosition(LabelPosition.Left)  // Default for all fields
    .Field(m => m.FirstName)
    .Field(m => m.LastName)
    .LabelPosition(m => m.Description, LabelPosition.Top)  // Override for this field
    .Field(m => m.Description);
```

This is particularly useful for creating consistent form layouts while maintaining flexibility for fields that need different label positioning. The effective label position for each field is determined by the per-field setting first, falling back to the form-level default if not specified.

### Dictionary Support in TableBuilder

The `ToTable()` method now supports Dictionary objects as data sources with indexer expressions in the `Header()` method. Previously, using dictionary indexers like `r => r["key"]` would throw an `ArgumentException`. Now you can create tables from dynamic dictionary data:

```csharp
var data = new List<Dictionary<string, string>>
{
    new() { ["Name"] = "Alice", ["Age"] = "30", ["Email"] = "alice@example.com" },
    new() { ["Name"] = "Bob",   ["Age"] = "25", ["Email"] = "bob@example.com" }
};

var table = data.ToTable()
    .Header(r => r["Name"], "Name")
    .Header(r => r["Age"], "Age")
    .Header(r => r["Email"], "Email");
```

**Auto-scaffolding:** If you don't specify any `Header()` calls, the table builder now automatically creates columns from the dictionary keys in the first item:

```csharp
var data = new List<Dictionary<string, string>>
{
    new() { ["Name"] = "Alice", ["Age"] = "30", ["Email"] = "alice@example.com" },
    new() { ["Name"] = "Bob",   ["Age"] = "25", ["Email"] = "bob@example.com" }
};

// No Header() calls needed - columns auto-scaffold from keys
var table = data.ToTable(); // Automatically creates Name, Age, and Email columns
```

This works with `IDictionary<string, object>`, `IDictionary<string, string>`, and generic `IDictionary` types. Auto-scaffolding makes it even easier to work with dynamic data structures, API responses, or scenarios where column names aren't known at compile time. The table builder creates columns dynamically for dictionary keys and handles missing keys gracefully by returning null.

### RichTextMarkdownParser for Streaming AI Chat

Added `RichTextMarkdownParser` for parsing markdown incrementally during LLM streaming, enabling real-time rich text formatting in AI chat interfaces. The parser handles all common markdown syntax including bold, italic, code blocks, headings, lists, tables, blockquotes, and more - all while streaming token by token.

```csharp
// Example: Stream AI response with live markdown rendering
var parser = new RichTextMarkdownParser();
var richText = new RichTextBuilder();

// As tokens arrive from your LLM:
await foreach (var token in llmStream)
{
    // Parser incrementally converts markdown to TextRuns
    var runs = parser.Append(token);
    foreach (var run in runs)
    {
        richText.AddRun(run);
    }

    // Update UI in real-time
    StateUpdated();
}

// Flush any remaining content when stream completes
var finalRuns = parser.Flush();
foreach (var run in finalRuns)
{
    richText.AddRun(run);
}
```

The parser maintains state across token boundaries, so you can safely stream character-by-character or word-by-word without worrying about breaking markdown syntax. The `RichTextBlock` widget now renders all block-level elements (headings, lists, code blocks, tables, etc.) automatically.

**Supported Markdown Features:**

- Inline: `**bold**`, `*italic*`, `` `code` ``, `~~strikethrough~~`, `[links](url)`
- Block: headings (`#`-`######`), code blocks (` ``` `), bullet lists (`-`, `*`), ordered lists (`1.`), blockquotes (`>`), tables, horizontal rules (`---`)
- Nested lists with indentation support

This is particularly powerful for building chat UIs with AI assistants - your markdown renders progressively as the AI types, creating a natural conversational experience.

### VideoPlayer Subtitle Support

Added subtitle/caption track support to the `VideoPlayer` widget. You can now serve `.srt` or `.vtt` subtitle files alongside your videos using the new `Subtitles()` fluent API method. The player supports multiple subtitle tracks for different languages, allowing users to choose their preferred language from the video controls.

```csharp
// Single subtitle track
var player = new VideoPlayer("https://example.com/video.mp4")
    .Subtitles("https://example.com/subtitles_en.vtt", "English");

// Multiple subtitle tracks for different languages
var player = new VideoPlayer("https://example.com/video.mp4")
    .Subtitles("https://example.com/subtitles_en.vtt", "English")
    .Subtitles("https://example.com/subtitles_es.vtt", "Spanish")
    .Subtitles("https://example.com/subtitles_fr.vtt", "French");

// The first track is set as default
```

The frontend automatically renders HTML5 `<track>` elements with proper CORS configuration (`crossOrigin="anonymous"`), ensuring subtitles load correctly from external sources. Users can toggle subtitles and switch between languages using the browser's native video controls.

### Kanban Column Header Humanization

Kanban widgets now automatically humanize enum-based column headers for better readability. Column headers like `TaskStatus.InProgress` now display as "In Progress" instead of "InProgress". The feature uses `EnumHelper.GetDescription()`, which respects `[Description]` attributes and falls back to smart PascalCase splitting.

```csharp
public enum TaskStatus
{
    NotStarted,      // Displays as "Not Started"
    InProgress,      // Displays as "In Progress"
    [Description("Waiting for Review")]
    PendingReview,   // Displays as "Waiting for Review"
    Completed        // Displays as "Completed"
}

// Automatic humanization - no code changes needed
var kanban = tasks.ToKanban()
    .GroupBy(t => t.Status);  // Headers are automatically humanized

// Or use the new ColumnHeader() method for custom headers
var kanban = tasks.ToKanban()
    .GroupBy(t => t.Status)
    .ColumnHeader(status => status switch
    {
        TaskStatus.InProgress => "🔄 In Progress",
        TaskStatus.Completed => "✅ Done",
        _ => status.GetDescription()
    });
```

The Kanban widget internally maintains separate `Column` (the grouping key for drag-and-drop) and `ColumnName` (the display label) properties, ensuring drag-and-drop functionality works correctly while displaying user-friendly headers.

## Testing Improvements

### Chart Widgets Playwright Readiness

All chart widgets now set a `data-chart-rendered="true"` attribute on their container when the chart finishes rendering. This allows Playwright and other E2E testing tools to wait for charts to fully render before taking screenshots or making assertions, preventing empty gray areas caused by canvas render timing issues.

```csharp
// In your Playwright tests, wait for charts to render before screenshotting
await page.WaitForSelectorAsync("[data-chart-rendered='true']");
await page.ScreenshotAsync(new() { Path = "dashboard.png" });
```

This affects all 10 chart widgets: `AreaChart`, `BarChart`, `ChordChart`, `FunnelChart`, `GaugeChart`, `LineChart`, `PieChart`, `RadarChart`, `SankeyChart`, and `ScatterChart`. The attribute is set automatically when charts complete their initial render via the ECharts `onChartReady` callback, ensuring reliable test automation without manual delays or retry logic.

## Accessibility Improvements

### FieldWidget Label Associations

Enhanced accessibility in the `FieldWidget` component by adding proper `htmlFor` attribute associations between labels and input fields. The component now automatically detects input, select, and textarea elements within its children and establishes the correct label-to-input relationship. This improvement benefits users with screen readers and assistive technologies, making forms more navigable and improving overall accessibility compliance.

### Sheet Widget Screen Reader Support

Improved accessibility in the `Sheet` widget by ensuring a DialogTitle is always present for screen readers, even when no visible title is provided. Previously, the SheetHeader and SheetTitle were only rendered if explicitly set, which could cause accessibility issues. Now the widget always renders these elements, using visually hidden (`sr-only`) styling when no title is provided, with a default "Sheet" label for screen reader users. This ensures proper ARIA dialog semantics and better screen reader navigation without affecting visual presentation.

## Performance Improvements

### DataTable Query Processing

Improved DataTable query performance by caching reflection calls in the QueryProcessor. Previously, each query execution performed ~8 expensive `GetMethods()` scans on the `Queryable` type to find LINQ methods. These reflection calls are now cached using static fields and a concurrent dictionary, resulting in faster query execution especially when processing multiple DataTable queries.

## Developer Experience Improvements

### Query Failures Now Visible in Console

Query fetch failures are now logged at Warning level instead of Debug, making them visible in console output by default. This helps quickly identify issues with queries in `MetricView`, `DataTable`, and custom components using `UseQuery()`.

```csharp
// Query failures now appear automatically in your console:
// warn: Ivy.Services.QueryService[0]
//       Fetch failed for query key my-query-key
```

Previously, these errors were only visible when debug logging was enabled. Now you'll see them immediately, making debugging much faster.

**Note**: The `MetricView` constructor parameter `useMetricData` has been renamed to `UseMetricData` (PascalCase) for C# naming consistency. Update your code if using named parameters: `useMetricData:` → `UseMetricData:`.

## Bug Fixes

### SplitPascalCase Acronym Handling

Fixed `StringHelper.SplitPascalCase()` to properly handle acronyms followed by words. Previously, inputs like `"UIDesign"` would not be split correctly and would display as "UIDesign" instead of "UI Design". The regex pattern now uses a lookahead to detect acronym boundaries, correctly splitting strings like:

- `"UIDesign"` → "UI Design"
- `"APIClient"` → "API Client"
- `"QATesting"` → "QA Testing"
- `"HTMLParser"` → "HTML Parser"

This improves readability throughout the framework, particularly affecting Kanban column headers, enum descriptions, and any other UI text that relies on PascalCase splitting for humanization.

### Multiselect Component

Fixed a visual bug where the Multiselect dropdown would shift position when opened if an overflow badge was visible. The component now properly resets its scroll position when focused, ensuring the dropdown appears in the correct location.

### SmartSearch Component

Fixed scroll overflow issues in the SmartSearch dialog. The search input is now pinned at the top of the dialog and remains visible while scrolling through search results, improving usability when browsing large result sets.

### DataTable AI Features

Fixed a crash in `DataTableBuilder` when AI chat features (`IChatClient`) are not registered in dependency injection. The component now properly uses `TryUseService<IChatClient>()` to gracefully handle scenarios where AI services are optional or not configured, preventing `InvalidOperationException` errors during table initialization.

### AppShell Components with Optional Authentication

Fixed a crash in `DefaultSidebarAppShell` (used by `UseAppShell()`) when `IAuthService` is not registered. The component now uses `TryUseService<IAuthService>()` to gracefully handle scenarios where authentication is not configured, allowing the AppShell sidebar to render properly without requiring auth setup.

### Input Widgets Setting Default Values

Fixed an issue where calling `.Set()` with a default value (like `0`, `false`, empty string, or `null`) on input widgets would not properly update the client. The framework's serializer was skipping properties that matched their type's default value, causing the UI to not reflect the programmatic change. This affected all input widgets including `NumberInput`, `BoolInput`, `TextInput`, `DateTimeInput`, `ColorInput`, `FileInput`, and others. The Value property on all input widgets now uses `AlwaysSerialize = true` to ensure default values are always sent to the client.

### UseQuery Revalidate Method

Fixed an issue in the `UseQuery` hook where calling `Revalidate()` could surface `OperationCanceledException` errors to users and show stale error states. The revalidate function now properly:

- Cancels any in-flight requests before starting a new one
- Resets the fetch state to trigger a fresh data load
- Clears previous errors and shows the loading skeleton while revalidating

This ensures a smoother experience when manually refreshing query data, with proper loading indicators instead of error flashes.

### App Attribute Documentation

Fixed incorrect documentation in AGENTS.md that showed `path:` as the parameter name for the `[App]` attribute. The correct parameter name is `group:`. This error was causing developers to write code that wouldn't compile (CS0655 error). The documentation now correctly shows:

```csharp
[App(title: "Customers", icon: Icons.Rocket, group: new[] { "CRM" })]
public class CustomersApp : ViewBase
```

### Object Array Rendering

Fixed an issue where returning `object[]` from `Build()` methods was incorrectly converted to a Table instead of a Fragment. This was happening because `DefaultContentBuilder.Format()` matched `object[]` as `IEnumerable` and automatically rendered it as a table, which broke Sheet overlay rendering and other scenarios where multiple elements need to preserve their individual rendering behavior. The framework now properly converts `object[]` to a `Fragment`, ensuring each child element renders correctly.

```csharp
// This now works correctly - each element renders as expected
public override object Build()
{
    return new object[]
    {
        new Sheet(...),
        new Box(...),
        new TextBlock(...)
    };
}
```

### DataTable with Positional Records

Fixed a crash when using `ToDataTable()` with positional records that lack a parameterless constructor. Positional records like `record Foo(int Id, string Name)` only have a primary constructor, which was causing an `ArgumentNullException` in the internal field removal logic. The framework now properly detects when a parameterless constructor is unavailable and uses the primary constructor with default values instead.

```csharp
// This now works without crashing
public record Person(int Id, string Name, string Email);

var query = dbContext.People.AsQueryable();
var dataTable = query.ToDataTable(); // Previously threw ArgumentNullException
```

### DataTable Concurrent DbContext Access

Fixed crashes and errors when using `DataTable` with Entity Framework Core `IQueryable` sources. The issue occurred because EF Core's `DbContext` is not thread-safe, and gRPC queries running on separate threads could cause concurrent access violations. The framework now serializes count and data queries per `DbContext` instance using semaphore synchronization, preventing concurrent access while still allowing different `DbContext` instances to execute queries in parallel.

Additionally, empty state handling has been optimized - the empty view is now rendered on the frontend when no rows are returned, avoiding an extra synchronous `.Count()` query during the render cycle.

### UseTrigger with Nullable Types

Fixed an issue in the `UseTrigger<T>` hook where null values couldn't be used as valid trigger values for nullable types. Previously, a null guard `triggerValue.Value != null` prevented the factory from executing when `T` is nullable and `null` is a meaningful value (e.g., `int?` where `null` might represent "create new"). The hook now uses a `hasTriggered` ref to track whether the callback was invoked, allowing `null` to be a valid trigger value.

```csharp
// This now works correctly with nullable types
var (triggerView, showTrigger) = UseTrigger((IState<bool> isOpen, int? id) =>
    new FooView(isOpen, id));

// Can now trigger with null as a valid value
new Button("Create New", () => showTrigger(null));
```

### Chart Series Labels with Where+Sum Pattern

Fixed an issue where chart series all showed the same label when using the `.Where().Sum()` pattern for aggregations. The framework now automatically extracts the filter value from `.Where()` clauses and uses it as the series name, providing much more meaningful chart labels.

```csharp
// Previously, both series would be labeled "Amount" (confusing!)
// Now they're automatically labeled "Capital Call" and "Management Fee"
var chartData = new[]
{
    cashFlows.Where(f => f.FlowType == "Capital Call").Sum(f => f.Amount),
    cashFlows.Where(f => f.FlowType == "management fee").Sum(f => f.Amount),
    cashFlows.Where(f => f.FlowType == "Distribution").Sum(f => f.Amount)
};

var chart = chartData.ToBarChart();
// Series names are now: "Capital Call", "Management Fee", "Distribution"
```

The feature also automatically applies title casing to filter values, so `"management fee"` becomes "Management Fee" for better presentation. This makes charts with categorical breakdowns much more readable without requiring manual label configuration.

### Auto-Scroll with Streaming Content

Fixed auto-scroll behavior to properly trigger when streaming content is added dynamically. The `useAutoScroll` hook now observes both the scroll container and its content wrapper, ensuring that streaming AI responses (like those rendered with `RichTextMarkdownParser`) automatically scroll to the bottom as new content arrives. This is particularly useful for AI chat interfaces where markdown is rendered progressively as the assistant types.

### Button Keyboard Shortcuts

Fixed keyboard shortcut handling in buttons to prevent shortcuts from triggering while typing in input fields. Previously, keyboard shortcuts would fire even when the user had focus in an `<input>`, `<textarea>`, or contentEditable element, causing unwanted actions while typing.

Additionally, keyboard shortcuts now work properly with URL buttons (previously only action buttons supported shortcuts). The shortcut handler now correctly handles all URL types including download links and mailto links, and respects the button's target setting for opening links in new tabs.

```csharp
// Shortcuts now work on URL buttons too
Layout.Horizontal().Gap(8)
    | new Button("Search", _ => client.Toast("Searching..."))
        .Primary()
        .ShortcutKey("Ctrl+K")
    | new Button("GitHub")
        .Url("https://github.com/Ivy-Interactive/Ivy-Framework")
        .ShortcutKey("Ctrl+G")  // Now works correctly!
```

This fix ensures keyboard shortcuts enhance productivity without interfering with normal text input.

## Infrastructure Updates

### Microsoft.Extensions.AI Upgraded to Stable Release

Upgraded `Microsoft.Extensions.AI` and related AI packages from preview versions (10.1.1-preview) to stable release (10.4.1). This improves stability and production-readiness for AI-powered features in the framework, including AI chat integration, filter generation, and EF Query agents.

**Updated packages:**

- `Microsoft.Extensions.AI` → 10.4.1 (stable)
- `Microsoft.Extensions.AI.Abstractions` → 10.4.1 (stable)
- `Microsoft.Extensions.AI.OpenAI` → 10.4.1 (stable)
- Various `Microsoft.Extensions.*` packages → 10.0.5
- Entity Framework Core packages → 10.0.5

This update also includes improvements from the broader .NET ecosystem, including bug fixes and performance enhancements in the extension libraries and EF Core.

## Documentation Improvements

### Hidden Documentation Pages

Added support for hiding documentation pages from the sidebar and smart search while keeping them accessible via direct link. You can now use the `hidden: true` frontmatter property in your markdown documentation files to create unlisted pages.

```markdown
---
title: Internal Documentation
description: This page is only accessible via direct link
hidden: true
---

# Internal Documentation

This page won't appear in the sidebar or smart search results,
but users can still access it if they have the direct URL.
```

This is useful for:

- Draft documentation that's not ready for public navigation
- Internal or deprecated pages that should remain accessible via old links
- Hidden easter eggs or special content
- Documentation that should only be linked from specific places

The `hidden` property defaults to `false`, so existing documentation remains visible unless explicitly hidden. Hidden pages are still rendered normally when accessed directly - they simply don't appear in navigation elements.

### UseEffect Behavior with State Dependencies

Enhanced UseEffect documentation to clarify an important behavioral difference from React: `UseEffect` with state dependencies only fires when the dependency **changes**, not on the initial render. This is a common source of confusion, especially for developers coming from React where `useEffect` fires both on mount and on dependency changes.

**The Problem:**

```csharp
// ❌ This will NOT populate items on first render
var count = UseState(10);
var items = UseState(() => new List<Item>());
UseEffect(() => { items.Set(GenerateItems(count.Value)); }, count);
// Result: Component shows empty state on first load
```

**The Solution:**

```csharp
// ✅ Initialize state with data directly
var count = UseState(10);
var items = UseState(() => GenerateItems(count.Value));
UseEffect(() => { items.Set(GenerateItems(count.Value)); }, count);

// Or use two effects if you need mount + change behavior
UseEffect(() => { items.Set(GenerateItems(count.Value)); });      // Runs on mount
UseEffect(() => { items.Set(GenerateItems(count.Value)); }, count); // Runs on change
```

The documentation now includes a prominent warning callout explaining this difference, plus an FAQ entry for the common symptom ("Why is my component empty on first render?"). A hallucination entry has also been added to help AI agents avoid this mistake when generating code.

### Button Widget FAQ Additions

Added comprehensive FAQ documentation to the Button widget covering common questions:

**ButtonVariant Values**: Documentation now lists all available button variants (`Primary`, `Destructive`, `Outline`, `Secondary`, `Success`, `Warning`, `Info`, `Ghost`, `Link`, `Inline`, `Ai`) with usage examples for both the `.Variant()` method and shortcut methods like `.Primary()`, `.Secondary()`, etc. Also clarifies that there is no `ButtonVariant.Default` - use `ButtonVariant.Primary` instead.

**Async Button Operations**: Added FAQ explaining how to handle async operations in button click handlers. Shows how `Button` natively accepts `Func<ValueTask>` and `Func<Event<Button>, ValueTask>` handlers, with a complete example demonstrating loading state management:

```csharp
var result = UseState<string?>(null);
var loading = UseState(false);

if (loading.Value) return new Text("Loading...");

new Button("Run", async () => {
    loading.Value = true;
    result.Value = await myService.DoWorkAsync();
    loading.Value = false;
});
```

The documentation clarifies that there is no `UseAsync` hook, and directs users to `UseQuery()` for data fetching with automatic loading/error states.

### External Widget Discovery with Prefix Assembly Names

Fixed a bug in external widget discovery where widget assemblies would fail to load if their name was a prefix of the host assembly name. For example, if you had a widget assembly named `PDF.Viewer.dll` and your host application was named `PDF.Viewer.Samples.dll`, the widget assembly would be incorrectly skipped during scanning. The framework now uses exact assembly name comparison instead of substring matching, ensuring all external widget assemblies are discovered correctly regardless of naming patterns.

### UseRef Hook Pitfall Warning

Added comprehensive documentation warning about a common `UseRef` pitfall: setting `ref.Value` does not trigger a re-render. The documentation now clearly explains why using `UseRef` + `UseEffect` for data loading is incorrect, as it leaves your component stuck on its initial state (e.g., a loading skeleton).

```csharp
// ❌ Don't do this - won't trigger re-render!
var dbRef = UseRef<MyDbContext?>();
UseEffect(() => { dbRef.Value = new MyDbContext(); });

// ✅ Do this instead - triggers re-render
var dbContext = UseState<MyDbContext?>(null);
UseEffect(() => { dbContext.Set(new MyDbContext()); });

// ✅ Or for data fetching, use UseQuery
var userData = UseQuery("user-data", async () => await FetchUserData());
```

The UseRef documentation now includes clear guidance to use `UseState` when value changes should update the UI, or `UseQuery` for data fetching scenarios.

### App Attribute Assembly Registration Requirement

Enhanced AGENTS.md documentation to clarify that `server.AddAppsFromAssembly()` must be called before `UseDefaultApp()`. Without this call, `[App]`-attributed classes are never discovered and the server throws `InvalidOperationException` at runtime. This was a recurring source of bugs where apps wouldn't appear in navigation.

```csharp
// ✅ Correct order
var server = new Server(new ServerArgs { /* ... */ });
server.AddAppsFromAssembly();  // Must be called first
server.UseDefaultApp(typeof(MyApp));

// ❌ Wrong - apps won't be discovered
var server = new Server(new ServerArgs { /* ... */ });
server.UseDefaultApp(typeof(MyApp));  // [App] classes not registered yet!
```

This requirement is now prominently documented in AGENTS.md to help developers avoid runtime discovery issues.
