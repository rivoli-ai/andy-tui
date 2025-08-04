using System;
using System.Collections.Generic;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for Gradient component.
/// </summary>
public class GradientInstance : ViewInstance
{
    private string _text = "";
    private Color _startColor = Color.White;
    private Color _endColor = Color.Black;
    private GradientDirection _direction = GradientDirection.Horizontal;
    private bool _bold = false;
    private bool _italic = false;
    private bool _underline = false;
    
    public GradientInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Gradient gradient)
            throw new InvalidOperationException($"Expected Gradient, got {viewDeclaration.GetType()}");
        
        _text = gradient.GetText();
        _startColor = gradient.GetStartColor();
        _endColor = gradient.GetEndColor();
        _direction = gradient.GetDirection();
        _bold = gradient.GetBold();
        _italic = gradient.GetItalic();
        _underline = gradient.GetUnderline();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var lines = _text.Split('\n');
        var maxWidth = 0;
        foreach (var line in lines)
        {
            maxWidth = Math.Max(maxWidth, line.Length);
        }
        
        return new LayoutBox 
        { 
            Width = Math.Min(maxWidth, constraints.MaxWidth),
            Height = Math.Min(lines.Length, constraints.MaxHeight)
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var children = new List<VirtualNode>();
        var lines = _text.Split('\n');
        
        for (int lineIndex = 0; lineIndex < lines.Length && lineIndex < layout.Height; lineIndex++)
        {
            var line = lines[lineIndex];
            
            if (_direction == GradientDirection.Horizontal)
            {
                // Render each character with interpolated color
                for (int charIndex = 0; charIndex < line.Length && charIndex < layout.Width; charIndex++)
                {
                    var t = line.Length > 1 ? (float)charIndex / (line.Length - 1) : 0f;
                    var color = Gradient.InterpolateColor(_startColor, _endColor, t);
                    
                    var style = Style.Default
                        .WithForegroundColor(color)
                        .WithBold(_bold)
                        .WithItalic(_italic)
                        .WithUnderline(_underline);
                    
                    children.Add(Element("text")
                        .WithProp("style", style)
                        .WithProp("x", (int)(layout.AbsoluteX + charIndex))
                        .WithProp("y", (int)(layout.AbsoluteY + lineIndex))
                        .WithChild(new TextNode(line[charIndex].ToString()))
                        .Build());
                }
            }
            else if (_direction == GradientDirection.Vertical)
            {
                // Render entire line with interpolated color
                var t = lines.Length > 1 ? (float)lineIndex / (lines.Length - 1) : 0f;
                var color = Gradient.InterpolateColor(_startColor, _endColor, t);
                
                var style = Style.Default
                    .WithForegroundColor(color)
                    .WithBold(_bold)
                    .WithItalic(_italic)
                    .WithUnderline(_underline);
                
                children.Add(Element("text")
                    .WithProp("style", style)
                    .WithProp("x", (int)layout.AbsoluteX)
                    .WithProp("y", (int)(layout.AbsoluteY + lineIndex))
                    .WithChild(new TextNode(line))
                    .Build());
            }
            else if (_direction == GradientDirection.Diagonal)
            {
                // Diagonal gradient
                for (int charIndex = 0; charIndex < line.Length && charIndex < layout.Width; charIndex++)
                {
                    var maxDist = (float)Math.Sqrt(layout.Width * layout.Width + layout.Height * layout.Height);
                    var dist = (float)Math.Sqrt(charIndex * charIndex + lineIndex * lineIndex);
                    var t = maxDist > 0 ? dist / maxDist : 0f;
                    var color = Gradient.InterpolateColor(_startColor, _endColor, t);
                    
                    var style = Style.Default
                        .WithForegroundColor(color)
                        .WithBold(_bold)
                        .WithItalic(_italic)
                        .WithUnderline(_underline);
                    
                    children.Add(Element("text")
                        .WithProp("style", style)
                        .WithProp("x", (int)(layout.AbsoluteX + charIndex))
                        .WithProp("y", (int)(layout.AbsoluteY + lineIndex))
                        .WithChild(new TextNode(line[charIndex].ToString()))
                        .Build());
                }
            }
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
}