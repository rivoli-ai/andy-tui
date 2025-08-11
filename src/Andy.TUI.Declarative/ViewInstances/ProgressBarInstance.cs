using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for ProgressBar component.
/// </summary>
public class ProgressBarInstance : ViewInstance
{
    private float _value = 0f;
    private float _minValue = 0f;
    private float _maxValue = 100f;
    private int _width = 20;
    private ProgressBarStyle _style = ProgressBarStyle.Solid;
    private char _filledChar = '█';
    private char _emptyChar = '░';
    private Color _filledColor = Color.Green;
    private Color _emptyColor = Color.DarkGray;
    private bool _showPercentage = true;
    private string _label = "";
    
    public ProgressBarInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not ProgressBar progressBar)
            throw new InvalidOperationException($"Expected ProgressBar, got {viewDeclaration.GetType()}");
        
        _value = progressBar.GetValue();
        _minValue = progressBar.GetMinValue();
        _maxValue = progressBar.GetMaxValue();
        _width = progressBar.GetWidth();
        _style = progressBar.GetStyle();
        _filledChar = progressBar.GetFilledChar();
        _emptyChar = progressBar.GetEmptyChar();
        _filledColor = progressBar.GetFilledColor();
        _emptyColor = progressBar.GetEmptyColor();
        _showPercentage = progressBar.GetShowPercentage();
        _label = progressBar.GetLabel();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var totalWidth = _width;
        if (_showPercentage)
        {
            totalWidth += 5; // " 100%"
        }
        if (!string.IsNullOrEmpty(_label))
        {
            totalWidth = Math.Max(totalWidth, _label.Length);
        }
        
        var height = string.IsNullOrEmpty(_label) ? 1 : 2;
        
        return new LayoutBox 
        { 
            Width = Math.Min(totalWidth, constraints.MaxWidth),
            Height = Math.Min(height, constraints.MaxHeight)
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var children = new System.Collections.Generic.List<VirtualNode>();
        var currentY = 0;
        
        // Render label if present
        if (!string.IsNullOrEmpty(_label))
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default)
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode(_label))
                .Build());
            currentY++;
        }
        
        // Calculate progress
        var range = _maxValue - _minValue;
        var normalizedValue = range > 0 ? (_value - _minValue) / range : 0f;
        normalizedValue = Math.Max(0f, Math.Min(1f, normalizedValue));
        
        var filledWidth = (int)Math.Round(_width * normalizedValue);
        var emptyWidth = _width - filledWidth;
        
        // Get style characters
        var (styleFilledChar, styleEmptyChar) = ProgressBar.GetStyleChars(_style);
        if (_style != ProgressBarStyle.Custom)
        {
            _filledChar = styleFilledChar;
            _emptyChar = styleEmptyChar;
        }
        
        // Render filled part
        if (filledWidth > 0)
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(_filledColor))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(new string(_filledChar, filledWidth)))
                .Build());
        }
        
        // Render empty part
        if (emptyWidth > 0)
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(_emptyColor))
                .WithProp("x", (int)(layout.AbsoluteX + filledWidth))
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(new string(_emptyChar, emptyWidth)))
                .Build());
        }
        
        // Render percentage if enabled
        if (_showPercentage)
        {
            var percentage = (int)Math.Round(normalizedValue * 100);
            var percentText = $" {percentage}%";
            
            children.Add(Element("text")
                .WithProp("style", Style.Default)
                .WithProp("x", (int)(layout.AbsoluteX + _width))
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(percentText))
                .Build());
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
}