using System;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

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
    
    public ButtonInstance(string id) : base(id)
    {
    }
    
    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;
    
    public void OnGotFocus()
    {
        _isFocused = true;
        InvalidateView();
    }
    
    public void OnLostFocus()
    {
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
    
    public override VirtualNode Render()
    {
        // Visual styling based on focus
        var bgColor = _isFocused ? Color.Cyan : _backgroundColor;
        var style = Style.Default
            .WithForegroundColor(_textColor)
            .WithBackgroundColor(bgColor);
        
        // Visual indication of focus
        var prefix = _isFocused ? "> " : "  ";
            
        return Element("text")
            .WithProp("style", style)
            .WithProp("x", 0)
            .WithProp("y", 0)
            .WithChild(new TextNode($"{prefix}[ {_title} ]"))
            .Build();
    }
}