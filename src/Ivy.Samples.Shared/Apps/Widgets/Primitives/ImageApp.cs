
namespace Ivy.Samples.Shared.Apps.Widgets.Primitives;

[App(icon: Icons.Image, path: ["Widgets", "Primitives"], searchHints: ["picture", "photo", "img", "graphics", "media", "visual"])]
public class ImageApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new object[]
        {
            new Image("https://placehold.co/600x400") { Alt = "A placeholder image", Caption = "A placeholder image" },
            new Image("https://placehold.co/600x400") { Caption = "Click to visit example.com" }.Link("https://example.com"),
        };
    }
}
