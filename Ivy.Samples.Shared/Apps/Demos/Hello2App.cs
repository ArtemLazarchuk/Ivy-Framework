using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.PartyPopper, title: "Hello", searchHints: ["welcome", "getting-started", "introduction", "first", "tutorial", "example"])]
public class Hello2App : ViewBase
{
    public override object? Build()
    {
        var nameState = this.UseState<string>();
        return Layout.Vertical()
               | nameState.ToInput()
               | nameState;
    }
}