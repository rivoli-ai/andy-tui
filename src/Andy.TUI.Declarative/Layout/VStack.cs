using Andy.TUI.VirtualDom;
using System.Collections;
using System.ComponentModel;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Layout;

/// <summary>
/// A vertical stack container with SwiftUI-like collection initializer syntax.
/// Usage: VStack(spacing: 1) { Text("Hello"), Button("Click") { DoSomething(); } }
/// </summary>
public class VStack : ISimpleComponent, IEnumerable<ISimpleComponent>
{
    private readonly List<ISimpleComponent> _children = new();
    private readonly int _spacing;

    public VStack(int spacing = 0)
    {
        _spacing = spacing;
    }

    /// <summary>
    /// DO NOT CALL DIRECTLY. Use collection initializer syntax: new VStack { component1, component2 }
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Add(ISimpleComponent component)
    {
        if (component != null)
        {
            _children.Add(component);
        }
    }

    /// <summary>
    /// DO NOT CALL DIRECTLY. Use collection initializer syntax: new VStack { "text1", "text2" }
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Add(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _children.Add(new Components.Text(text));
        }
    }

    public VirtualNode Render()
    {
        throw new InvalidOperationException("VStack declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Required for IEnumerable
    public IEnumerator<ISimpleComponent> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Internal method to access children for component registration
    internal IReadOnlyList<ISimpleComponent> GetChildren() => _children;
    internal int GetSpacing() => _spacing;
}