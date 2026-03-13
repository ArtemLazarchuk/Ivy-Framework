using System.Runtime.CompilerServices;
using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Represents a single breadcrumb item with label and optional click action.
/// </summary>
public record BreadcrumbItem(string Label, Action? OnClick = null, Icons? Icon = null, string? Tooltip = null, bool Disabled = false);

/// <summary>
/// A secondary navigation component showing hierarchical location with clickable trail.
/// </summary>
public record Breadcrumbs : WidgetBase<Breadcrumbs>
{
    [OverloadResolutionPriority(1)]
    public Breadcrumbs(params IEnumerable<BreadcrumbItem> items)
    {
        Items = items.ToArray();
    }

    public Breadcrumbs(params BreadcrumbItem[] items)
    {
        Items = items;
    }

    internal Breadcrumbs()
    {
    }

    [Prop] public BreadcrumbItem[] Items { get; set; } = [];

    [Prop] public string Separator { get; set; } = "/";

    [Prop] public bool Disabled { get; set; }
}

public static class BreadcrumbsExtensions
{
    public static Breadcrumbs Separator(this Breadcrumbs breadcrumbs, string separator) => breadcrumbs with { Separator = separator };

    public static Breadcrumbs Disabled(this Breadcrumbs breadcrumbs, bool disabled = true) => breadcrumbs with { Disabled = disabled };
}
