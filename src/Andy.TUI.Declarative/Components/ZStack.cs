using System.Collections;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Layout;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A view that overlays its children, aligning them in both axes.
/// Similar to SwiftUI's ZStack or CSS position:absolute with z-index.
/// </summary>
public class ZStack : ISimpleComponent, IEnumerable<ISimpleComponent>
{
    private readonly List<ISimpleComponent> _children = new();
    private readonly AlignItems _alignment;

    /// <summary>
    /// Creates a new ZStack with center alignment.
    /// </summary>
    public ZStack() : this(AlignItems.Center)
    {
    }

    /// <summary>
    /// Creates a new ZStack with the specified alignment.
    /// </summary>
    /// <param name="alignment">How to align children within the stack.</param>
    public ZStack(AlignItems alignment)
    {
        _alignment = alignment;
    }

    // Collection initializer support
    public void Add(ISimpleComponent component)
    {
        if (component != null)
        {
            _children.Add(component);
        }
    }

    public void Add(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _children.Add(new Text(text));
        }
    }

    public VirtualNode Render()
    {
        throw new InvalidOperationException("ZStack declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // IEnumerable implementation
    public IEnumerator<ISimpleComponent> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Internal methods for view instance access
    internal IReadOnlyList<ISimpleComponent> GetChildren() => _children;
    internal AlignItems GetAlignment() => _alignment;
}