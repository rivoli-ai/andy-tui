using System;
using System.Collections.Generic;

namespace Andy.TUI.Terminal.Rendering;

public abstract record DisplayItem(int Layer);

public sealed record PushClipItem(int X, int Y, int Width, int Height) : DisplayItem(Layer: int.MinValue)
{
    public Rectangle ToRect() => new Rectangle(X, Y, Width, Height);
}

public sealed record PopClipItem() : DisplayItem(Layer: int.MaxValue);

public sealed record DrawRectItem(int X, int Y, int Width, int Height, Style Style, int DrawLayer) : DisplayItem(DrawLayer)
{
    public Rectangle ToRect() => new Rectangle(X, Y, Width, Height);
}

public sealed record DrawTextItem(int X, int Y, int Width, int Height, string Text, Style Style, int DrawLayer) : DisplayItem(DrawLayer)
{
    public Rectangle ToRect() => new Rectangle(X, Y, Width, Height);
}

public sealed record DrawBoxItem(int X, int Y, int Width, int Height, Style Style, BoxStyle BoxStyle, int DrawLayer) : DisplayItem(DrawLayer)
{
    public Rectangle ToRect() => new Rectangle(X, Y, Width, Height);
}

public static class DisplayListInvariants
{
    public static void Validate(IReadOnlyList<DisplayItem> items)
    {
        if (items == null) throw new DisplayListInvariantViolationException("Display list is null");

        var clipStack = new Stack<Rectangle>();
        int lastLayer = int.MinValue;

        foreach (var item in items)
        {
            switch (item)
            {
                case PushClipItem push:
                    var rect = push.ToRect();
                    if (clipStack.Count > 0)
                    {
                        var inter = clipStack.Peek().Intersect(rect);
                        Guard(!inter.IsEmpty, $"Pushed clip has no intersection with current clip: push={rect}, current={clipStack.Peek()}");
                        clipStack.Push(inter);
                    }
                    else
                    {
                        clipStack.Push(rect);
                    }
                    break;
                case PopClipItem:
                    Guard(clipStack.Count > 0, "PopClip without matching PushClip");
                    clipStack.Pop();
                    break;
                case DrawRectItem dr:
                    // Layer monotonic non-decreasing
                    Guard(dr.Layer >= lastLayer, $"Layer decreased: {dr.Layer} < {lastLayer}");
                    lastLayer = dr.Layer;
                    // Must be within clip if any
                    if (clipStack.Count > 0)
                    {
                        var r = dr.ToRect();
                        var top = clipStack.Peek();
                        Guard(top.Contains(r), $"DrawRect outside clip. Draw={r} Clip={top}");
                    }
                    break;
                case DrawTextItem dt:
                    Guard(dt.Layer >= lastLayer, $"Layer decreased: {dt.Layer} < {lastLayer}");
                    lastLayer = dt.Layer;
                    if (clipStack.Count > 0)
                    {
                        var r = dt.ToRect();
                        var top = clipStack.Peek();
                        Guard(top.Contains(r), $"DrawText outside clip. Draw={r} Clip={top}");
                    }
                    break;
                case DrawBoxItem db:
                    Guard(db.Layer >= lastLayer, $"Layer decreased: {db.Layer} < {lastLayer}");
                    lastLayer = db.Layer;
                    if (clipStack.Count > 0)
                    {
                        var r = db.ToRect();
                        var top = clipStack.Peek();
                        Guard(top.Contains(r), $"DrawBox outside clip. Draw={r} Clip={top}");
                    }
                    break;
            }
        }

        Guard(clipStack.Count == 0, $"Unbalanced clip stack. Remaining: {clipStack.Count}");
    }

    private static void Guard(bool condition, string message)
    {
        if (!condition) throw new DisplayListInvariantViolationException(message);
    }
}

public sealed class DisplayListInvariantViolationException : Exception
{
    public DisplayListInvariantViolationException(string message) : base(message) { }
}
