---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
- responsive
- breakpoint
- mobile
- tablet
- desktop
- wide
- adaptive
---

# Responsive Design

<Ingress>
Build adaptive layouts that respond to viewport size using the built-in breakpoint system. Vary width, height, visibility, columns, orientation, gap, padding, and density at different screen sizes.
</Ingress>

Ivy's responsive design system lets you vary widget properties by viewport size using `Responsive<T>` values with mobile-first cascading.

## Breakpoints

| Breakpoint | Max Width | Typical Device |
|---|---|---|
| `Mobile` | 640px | Phones |
| `Tablet` | 768px | Tablets |
| `Desktop` | 1024px | Laptops |
| `Wide` | 1280px | Large screens |

Values **cascade upward**: when you set a value at a smaller breakpoint, it applies to all larger breakpoints unless overridden. This is called **mobile-first** design — you start with the smallest screen and layer on changes for bigger viewports.

```csharp
// Setting columns at Mobile means it applies to Mobile, Tablet, Desktop, and Wide
// unless a larger breakpoint overrides it
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)        // 1 col on Mobile AND Tablet (cascades up)
        .And(Breakpoint.Desktop, 3))         // 3 cols on Desktop AND Wide
```

In this example, `Tablet` inherits the `Mobile` value of 1 column, and `Wide` inherits the `Desktop` value of 3 columns.

## The `Responsive<T>` Type

At the core of the responsive system is `Responsive<T>` — a generic record that holds per-breakpoint values:

```csharp
public record Responsive<T>
{
    public T? Default { get; init; }
    public T? Mobile { get; init; }
    public T? Tablet { get; init; }
    public T? Desktop { get; init; }
    public T? Wide { get; init; }
}
```

**Implicit conversion** ensures full backward compatibility. Any plain value is automatically wrapped as a `Responsive<T>` with `Default` set:

```csharp
// Non-responsive (backward compatible) — implicit conversion from T to Responsive<T>
Layout.Grid().Columns(3)

// Responsive — explicit breakpoint values using .At() and .And()
Layout.Grid().Columns(1.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 3))
```

### The `.At()` and `.And()` Builder Pattern

Use `.At(Breakpoint)` to start a responsive chain, and `.And(Breakpoint, value)` to add more breakpoints:

```csharp
// .At() starts the chain — sets the value for a specific breakpoint
Size.Full().At(Breakpoint.Mobile)

// .And() adds additional breakpoint values to the chain
Size.Full().At(Breakpoint.Mobile)
    .And(Breakpoint.Tablet, Size.Units(80))
    .And(Breakpoint.Desktop, Size.Half())
```

The `.At()` extension method is available for the following types:

| Type | Example | Used For |
|---|---|---|
| `Size` (class) | `Size.Full().At(Breakpoint.Mobile)` | Width, Height |
| `int` | `1.At(Breakpoint.Mobile)` | Gap, Columns |
| `Orientation` | `Orientation.Vertical.At(Breakpoint.Mobile)` | Layout orientation |
| `Density` | `Density.Large.At(Breakpoint.Mobile)` | Touch target size |
| `bool` | `true.At(Breakpoint.Mobile)` | Visibility |

## Responsive Width

Use `.At()` and `.And()` to create responsive width values. This works on any widget via the `WidgetBase` extension methods.

```csharp
new Box(Text.P("Responsive box"))
    .Width(Size.Full().At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Size.Half()))
```

```csharp demo
Layout.Vertical()
    | new Box(Text.P("Full on mobile, half on desktop"))
        .Width(Size.Full().At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Size.Half()))
        .Background(Colors.Primary)
```

## Responsive Height

Height also supports responsive values on all widgets:

```csharp
new Box(Text.P("Tall on mobile, shorter on desktop"))
    .Height(Size.Units(80).At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Size.Units(40)))
```

```csharp demo
Layout.Vertical()
    | new Box(Text.P("Tall on mobile, shorter on desktop"))
        .Height(Size.Units(80).At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Size.Units(40)))
        .Background(Colors.Primary)
```

## Conditional Visibility

Hide or show widgets at specific breakpoints using `.HideOn()` and `.ShowOn()`:

```csharp
// Hidden on mobile and tablet
new Badge("Desktop only").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)

// Only visible on mobile
new Badge("Mobile only").ShowOn(Breakpoint.Mobile)
```

Under the hood, these methods build a `Responsive<bool?>` value. `.HideOn()` starts with `Default = true` (visible) and sets specified breakpoints to `false`. `.ShowOn()` starts with `Default = false` (hidden) and sets specified breakpoints to `true`.

```csharp demo
Layout.Vertical()
    | new Badge("Desktop only").HideOn(Breakpoint.Mobile, Breakpoint.Tablet)
    | new Badge("Mobile only").ShowOn(Breakpoint.Mobile)
    | new Badge("Always visible")
```

## Responsive Grid Columns

Adjust grid column count by viewport:

```csharp
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3))
    .Gap(4)
```

```csharp demo
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3))
    .Gap(4)
    | new Card("Card 1")
    | new Card("Card 2")
    | new Card("Card 3")
    | new Card("Card 4")
    | new Card("Card 5")
    | new Card("Card 6")
```

## Responsive Orientation

Switch between horizontal and vertical layouts based on screen size:

```csharp
Layout.Horizontal()
    .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Orientation.Horizontal))
```

```csharp demo
Layout.Horizontal()
    .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Orientation.Horizontal))
    | new Button("Action 1")
    | new Button("Action 2")
    | new Button("Action 3")
```

## Responsive Gap

Vary spacing between items by viewport size:

```csharp
Layout.Vertical()
    .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
```

```csharp demo
Layout.Vertical()
    .Gap(2.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
    | Text.P("Item 1")
    | Text.P("Item 2")
    | Text.P("Item 3")
```

## Responsive Padding

Adjust layout padding by breakpoint. Since there is no `.At()` extension for `Thickness`, construct the `Responsive<Thickness?>` directly using object initialization:

```csharp
Layout.Vertical()
    .Padding(new Responsive<Thickness?>
    {
        Mobile = new Thickness(8),
        Desktop = new Thickness(24)
    })
```

```csharp demo
Layout.Vertical()
    .Padding(new Responsive<Thickness?>
    {
        Mobile = new Thickness(8),
        Desktop = new Thickness(24)
    })
    | new Box(Text.P("Compact padding on mobile, spacious on desktop"))
        .Background(Colors.Primary)
    | new Box(Text.P("Notice the padding change"))
        .Background(Colors.Secondary)
```

## Responsive Density

Adjust touch target size by device — useful for making buttons and inputs larger on touch devices:

```csharp
new Button("Adaptive Button")
    .Density(Density.Large.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Density.Medium))
```

```csharp demo
new Button("Adaptive Button")
    .Density(Density.Large.At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Density.Medium))
```

## Common Patterns

### Collapsing Sidebar

Hide the sidebar on mobile and show it on larger screens. The main content grows to fill available space.

```csharp demo
Layout.Horizontal()
    | new Box(Text.P("Sidebar"))
        .Width(Size.Units(60))
        .HideOn(Breakpoint.Mobile)
        .Background(Colors.Muted)
    | new Box(Text.P("Main Content"))
        .Width(Size.Grow())
        .Background(Colors.Primary)
```

### Mobile-First Card Grid

A progressive grid that goes from 1 column on mobile up to 4 columns on wide screens:

```csharp demo
Layout.Grid()
    .Columns(1.At(Breakpoint.Mobile)
        .And(Breakpoint.Tablet, 2)
        .And(Breakpoint.Desktop, 3)
        .And(Breakpoint.Wide, 4))
    .Gap(4)
    | new Card("Card 1")
    | new Card("Card 2")
    | new Card("Card 3")
    | new Card("Card 4")
```

### Responsive Dashboard

Combine responsive grid, visibility, and orientation to build a dashboard that adapts across device sizes:

```csharp demo
Layout.Vertical().Gap(4)
    | new Badge("Dashboard").HideOn(Breakpoint.Mobile)
    | (Layout.Grid()
        .Columns(1.At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, 3))
        .Gap(4)
        | new Card("Revenue")
        | new Card("Users")
        | new Card("Orders"))
    | (Layout.Horizontal()
        .Orientation(Orientation.Vertical.At(Breakpoint.Mobile)
            .And(Breakpoint.Desktop, Orientation.Horizontal))
        .Gap(4)
        | new Box(Text.P("Chart Area")).Width(Size.Grow()).Background(Colors.Muted)
        | new Box(Text.P("Activity Feed")).Width(Size.Units(60).At(Breakpoint.Desktop)).HideOn(Breakpoint.Mobile).Background(Colors.Muted))
```

### Mobile-First Form

Constrain form width on larger screens and adjust spacing:

```csharp demo
Layout.Vertical()
    .Width(Size.Full().At(Breakpoint.Mobile)
        .And(Breakpoint.Desktop, Size.Fraction(0.5f)))
    .Gap(4.At(Breakpoint.Mobile).And(Breakpoint.Desktop, 6))
    | new Box(Text.P("Form field 1")).Background(Colors.Muted)
    | new Box(Text.P("Form field 2")).Background(Colors.Muted)
    | new Button("Submit")
```

## API Reference

Summary of all responsive-capable properties:

| Property | Widget/Layout | Type | Fluent Method |
|---|---|---|---|
| Width | All widgets | `Responsive<Size>` | `.Width()` |
| Height | All widgets | `Responsive<Size>` | `.Height()` |
| Visible | All widgets | `Responsive<bool?>` | `.HideOn()` / `.ShowOn()` |
| Density | All widgets | `Responsive<Density?>` | `.Density()` |
| Columns | GridLayout | `Responsive<int?>` | `.Columns()` |
| Orientation | StackLayout | `Responsive<Orientation?>` | `.Orientation()` |
| Gap | StackLayout, GridLayout | `Responsive<int?>` | `.Gap()` |
| Padding | StackLayout | `Responsive<Thickness?>` | `.Padding()` |
