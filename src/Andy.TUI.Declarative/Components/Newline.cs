using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A simple component that renders one or more newlines/empty lines.
/// </summary>
public class Newline : ISimpleComponent
{
    private readonly int _count;

    public Newline(int count = 1)
    {
        _count = Math.Max(1, count);
    }

    // Internal accessor for view instance
    internal int GetCount() => _count;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Newline declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Implicit conversion from int for convenience
    public static implicit operator Newline(int count) => new(count);
}