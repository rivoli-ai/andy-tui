using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative button component with SwiftUI-like syntax.
/// </summary>
public class Button : ISimpleComponent
{
    private readonly string _title;
    private readonly Action _action;
    private Color _backgroundColor = Color.Gray;
    private Color _textColor = Color.White;

    public Button(string title, Action action)
    {
        _title = title ?? string.Empty;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Button Primary()
    {
        return new Button(_title, _action)
        {
            _backgroundColor = Color.Blue,
            _textColor = Color.White
        };
    }

    public Button Secondary()
    {
        return new Button(_title, _action)
        {
            _backgroundColor = Color.Gray,
            _textColor = Color.White
        };
    }

    // Internal accessors for view instance
    internal string GetTitle() => _title;
    internal Action GetAction() => _action;
    internal Color GetBackgroundColor() => _backgroundColor;
    internal Color GetTextColor() => _textColor;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Button declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}