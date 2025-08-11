using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for Badge component.
/// </summary>
public class BadgeInstance : ViewInstance
{
    private string _content = "";
    private BadgeStyle _style = BadgeStyle.Default;
    private BadgeVariant _variant = BadgeVariant.Default;
    private Color? _customColor;
    private Color? _customBackgroundColor;
    private bool _bold = true;
    private string _prefix = "";
    private string _suffix = "";
    
    public BadgeInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Badge badge)
            throw new InvalidOperationException($"Expected Badge, got {viewDeclaration.GetType()}");
        
        _content = badge.GetContent();
        _style = badge.GetStyle();
        _variant = badge.GetVariant();
        _customColor = badge.GetCustomColor();
        _customBackgroundColor = badge.GetCustomBackgroundColor();
        _bold = badge.GetBold();
        _prefix = badge.GetPrefix();
        _suffix = badge.GetSuffix();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var (leftBorder, rightBorder) = Badge.GetStyleBorders(_style);
        var totalLength = _prefix.Length + leftBorder.Length + _content.Length + rightBorder.Length + _suffix.Length;
        
        // Special handling for dot style (just a single character)
        if (_style == BadgeStyle.Dot)
        {
            totalLength = 1;
        }
        
        return new LayoutBox 
        { 
            Width = Math.Min(totalLength, constraints.MaxWidth),
            Height = 1
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        // Get colors
        var (defaultFg, defaultBg) = Badge.GetVariantColors(_variant);
        var foregroundColor = _customColor ?? defaultFg;
        var backgroundColor = _customBackgroundColor ?? defaultBg;
        
        // Get borders
        var (leftBorder, rightBorder) = Badge.GetStyleBorders(_style);
        
        // Build the complete badge text
        string badgeText;
        if (_style == BadgeStyle.Dot)
        {
            badgeText = leftBorder; // Just the dot character
        }
        else
        {
            badgeText = _prefix + leftBorder + _content + rightBorder + _suffix;
        }
        
        // Create style
        var style = Style.Default
            .WithForegroundColor(foregroundColor)
            .WithBackgroundColor(backgroundColor)
            .WithBold(_bold);
        
        return Element("text")
            .WithProp("style", style)
            .WithProp("x", (int)layout.AbsoluteX)
            .WithProp("y", (int)layout.AbsoluteY)
            .WithChild(new TextNode(badgeText))
            .Build();
    }
}