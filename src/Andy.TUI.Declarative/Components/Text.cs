using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative text component with SwiftUI-like syntax.
/// </summary>
public class Text : ISimpleComponent
{
    private readonly string _content;
    private Style _style = Style.Default;

    public Text(string content)
    {
        _content = content ?? string.Empty;
    }

    public Text Color(Color color)
    {
        return new Text(_content) { _style = _style.WithForegroundColor(color) };
    }

    public Text Bold()
    {
        return new Text(_content) { _style = _style.WithBold(true) };
    }

    public Text Title()
    {
        return new Text(_content) { _style = _style.WithBold(true).WithForegroundColor(Terminal.Color.White) };
    }

    public Text Center()
    {
        // For now, just return self - in full implementation this would add center alignment
        return this;
    }
    
    // Internal accessors for view instance
    internal string GetContent() => _content;
    internal Style GetStyle() => _style;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Text declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    public static implicit operator Text(string content) => new(content);
}