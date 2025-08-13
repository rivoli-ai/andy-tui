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
    private Color _backgroundColor = Color.None;
    private Color _textColor = Color.None;
    private bool _isPrimary;
    private bool _isSecondary;

    public Button(string title, Action action)
    {
        _title = title ?? string.Empty;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Button Primary()
    {
        return new Button(_title, _action)
        {
            _isPrimary = true,
            _isSecondary = false
        };
    }

    public Button Secondary()
    {
        return new Button(_title, _action)
        {
            _isPrimary = false,
            _isSecondary = true
        };
    }

    public Button WithColors(Color textColor, Color backgroundColor)
    {
        return new Button(_title, _action)
        {
            _textColor = textColor,
            _backgroundColor = backgroundColor,
            _isPrimary = _isPrimary,
            _isSecondary = _isSecondary
        };
    }

    // Internal accessors for view instance
    internal string GetTitle() => _title;
    internal Action GetAction() => _action;
    internal Color GetBackgroundColor() => _backgroundColor;
    internal Color GetTextColor() => _textColor;
    internal bool GetIsPrimary() => _isPrimary;
    internal bool GetIsSecondary() => _isSecondary;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Button declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}