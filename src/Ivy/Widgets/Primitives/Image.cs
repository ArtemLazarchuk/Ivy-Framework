using Ivy.Core;

// ReSharper disable once CheckNamespace
namespace Ivy;

/// <summary>
/// Displays an image.
/// </summary>
public record Image : WidgetBase<Image>
{
    public Image(string src) : this()
    {
        Src = src;
    }

    internal Image()
    {
        Width = Size.MinContent();
        Height = Size.MinContent();
    }

    [Prop] public string Src { get; set; } = String.Empty;
    [Prop] public string? Alt { get; set; }
    [Prop] public string? Caption { get; set; }
    [Prop] public string? Link { get; set; }
}

public static class ImageExtensions
{
    public static Image Link(this Image image, string url)
    {
        var validatedUrl = ValidationHelper.ValidateLinkUrl(url);
        if (validatedUrl == null)
        {
            throw new ArgumentException($"Invalid URL: {url}. Only safe URLs (http/https, relative paths, app://, anchors) are allowed.", nameof(url));
        }
        return image with { Link = validatedUrl };
    }
}