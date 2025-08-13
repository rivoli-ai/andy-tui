using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using Andy.TUI.Theming;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Button view with preserved state.
/// </summary>
public class ButtonInstance : ViewInstance, IFocusable
{
    private string _title = "";
    private Action? _action;
    private Color? _backgroundColor;
    private Color? _textColor;
    private bool _isPrimary;
    private bool _isSecondary;
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
        
        // Check if button has specific style modifiers
        _isPrimary = button.GetIsPrimary();
        _isSecondary = button.GetIsSecondary();
        
        // Only use custom colors if explicitly set
        var bgColor = button.GetBackgroundColor();
        var txtColor = button.GetTextColor();
        _backgroundColor = bgColor.ConsoleColor.HasValue || bgColor.Rgb.HasValue ? bgColor : (Color?)null;
        _textColor = txtColor.ConsoleColor.HasValue || txtColor.Rgb.HasValue ? txtColor : (Color?)null;
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
        var theme = ThemeManager.Instance.CurrentTheme;
        
        // Determine which color scheme to use
        ColorScheme colorScheme;
        if (_isFocused)
        {
            // Use primary colors when focused for all button types
            colorScheme = theme.Primary;
        }
        else if (_isPrimary)
        {
            colorScheme = theme.Primary;
        }
        else if (_isSecondary)
        {
            colorScheme = theme.Secondary;
        }
        else
        {
            // Default button uses secondary scheme
            colorScheme = theme.Secondary;
        }
        
        // Apply custom colors if explicitly set, otherwise use theme colors
        var fgColor = _textColor ?? new Color(colorScheme.Foreground.R, colorScheme.Foreground.G, colorScheme.Foreground.B);
        var bgColor = _backgroundColor ?? new Color(colorScheme.Background.R, colorScheme.Background.G, colorScheme.Background.B);
        
        var style = Style.Default
            .WithForegroundColor(fgColor)
            .WithBackgroundColor(bgColor);

        // Visual indication of focus
        var prefix = _isFocused ? "> " : "  ";

        _logger.Debug("Button {0} rendering: focused={1}, primary={2}, secondary={3}, theme={4}",
            Id, _isFocused, _isPrimary, _isSecondary, theme.Name);

        return Element("text")
            .WithProp("style", style)
            .WithProp("x", layout.AbsoluteX)
            .WithProp("y", layout.AbsoluteY)
            .WithChild(new TextNode($"{prefix}[ {_title} ]"))
            .Build();
    }
}