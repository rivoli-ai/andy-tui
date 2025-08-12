using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Button view with preserved state.
/// </summary>
public class ButtonInstance : ViewInstance, IFocusable
{
    private string _title = "";
    private Action? _action;
    private Color _backgroundColor = Color.Gray;
    private Color _textColor = Color.White;
    private bool _isFocused;
    private readonly ILogger _logger;

    public ButtonInstance(string id) : base(id)
    {
        _logger = DebugContext.Logger.ForCategory("ButtonInstance");
    }

    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;

    public void OnGotFocus()
    {
        _logger.Debug("Button {0} got focus", Id);
        _isFocused = true;
        InvalidateView();
        _logger.Debug("Button {0} InvalidateView called", Id);
    }

    public void OnLostFocus()
    {
        _logger.Debug("Button {0} lost focus", Id);
        _isFocused = false;
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                _action?.Invoke();
                return true;
        }

        return false;
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Button button)
            throw new ArgumentException("Expected Button declaration");

        _title = button.GetTitle();
        _action = button.GetAction();
        _backgroundColor = button.GetBackgroundColor();
        _textColor = button.GetTextColor();
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();

        // Button content with focus indicator and brackets
        var prefix = _isFocused ? "> " : "  ";
        var content = $"{prefix}[ {_title} ]";

        // Button size is based on content
        layout.Width = constraints.ConstrainWidth(content.Length);
        layout.Height = constraints.ConstrainHeight(1);

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        // Visual styling based on focus
        // High-contrast theme for buttons
        var bgColor = _isFocused ? Color.Blue : (_backgroundColor.ConsoleColor.HasValue ? _backgroundColor : Color.DarkGray);
        var style = Style.Default
            .WithForegroundColor(Color.White)
            .WithBackgroundColor(bgColor);

        // Visual indication of focus
        var prefix = _isFocused ? "> " : "  ";

        _logger.Debug("Button {0} rendering: focused={1}, bgColor={2}, text='{3}'",
            Id, _isFocused, bgColor, $"{prefix}[ {_title} ]");

        return Element("text")
            .WithProp("style", style)
            .WithProp("x", layout.AbsoluteX)
            .WithProp("y", layout.AbsoluteY)
            .WithChild(new TextNode($"{prefix}[ {_title} ]"))
            .Build();
    }
}