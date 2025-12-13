using System.Diagnostics;

namespace Ivy.Core;

public readonly record struct PathSegment(string Type, string? Key, int Index, bool IsWidget)
{
    public override string ToString()
    {
        return $"{Type}:{Key ?? Index.ToString()}";
    }
}

[DebuggerDisplay("{ToString()}")]
public class TreePath : Stack<PathSegment>
{
    public void Push(IView view, int index)
    {
        Push(new PathSegment(view.GetType().Name!, view.Key, index, false));
    }

    public void Push(IWidget widget, int index)
    {
        Push(new PathSegment(widget.GetType().Name!, widget.Key, index, true));
    }

    public TreePath Clone()
    {
        TreePath clone = new();
        var segments = this.ToList();
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            var segment = segments[i];
            clone.Push(new PathSegment(segment.Type, segment.Key, segment.Index, segment.IsWidget));
        }
        return clone;
    }
    
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        bool first = true;
        foreach (var e in this)
        {
            if (!first) sb.Append('>');
            first = false;
            sb.Append(e.Type);
            sb.Append(':');
            if (e.Key is not null) sb.Append(e.Key);
            else sb.Append(e.Index);
        }
        return sb.ToString();
    }
}