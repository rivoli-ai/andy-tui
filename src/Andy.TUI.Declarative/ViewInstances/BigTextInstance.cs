using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for BigText component.
/// </summary>
public class BigTextInstance : ViewInstance
{
    private string _text = "";
    private BigTextFont _font = BigTextFont.Block;
    private Dictionary<char, string[]>? _customFont;
    private Color _color = Color.White;
    private char _fillChar = '█';
    private int _spacing = 1;
    
    public BigTextInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not BigText bigText)
            throw new InvalidOperationException($"Expected BigText, got {viewDeclaration.GetType()}");
        
        _text = bigText.GetText();
        _font = bigText.GetFont();
        _customFont = bigText.GetCustomFont();
        _color = bigText.GetColor();
        _fillChar = bigText.GetFillChar();
        _spacing = bigText.GetSpacing();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (string.IsNullOrEmpty(_text))
        {
            return new LayoutBox { Width = 0, Height = 0 };
        }
        
        // Get the height of the font
        var sampleChar = BigText.GetCharacterPattern('A', _font) ?? _customFont?.Values.FirstOrDefault();
        var charHeight = sampleChar?.Length ?? 5;
        
        // Calculate total width needed
        var totalWidth = 0;
        for (int i = 0; i < _text.Length; i++)
        {
            var pattern = GetCharPattern(_text[i]);
            if (pattern != null && pattern.Length > 0)
            {
                totalWidth += pattern[0].Length;
                if (i < _text.Length - 1)
                {
                    totalWidth += _spacing;
                }
            }
        }
        
        return new LayoutBox 
        { 
            Width = Math.Min(totalWidth, constraints.MaxWidth),
            Height = Math.Min(charHeight, constraints.MaxHeight)
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (string.IsNullOrEmpty(_text))
        {
            return Element("container").Build();
        }
        
        var children = new List<VirtualNode>();
        var currentX = 0;
        
        for (int charIndex = 0; charIndex < _text.Length; charIndex++)
        {
            var pattern = GetCharPattern(_text[charIndex]);
            if (pattern == null || pattern.Length == 0) continue;
            
            // Render each row of the character
            for (int row = 0; row < pattern.Length && row < layout.Height; row++)
            {
                var line = pattern[row];
                
                // Render each column
                for (int col = 0; col < line.Length && currentX + col < layout.Width; col++)
                {
                    if (line[col] != ' ')
                    {
                        var ch = _fillChar == '\0' ? line[col] : _fillChar;
                        
                        children.Add(Element("text")
                            .WithProp("style", Style.Default.WithForegroundColor(_color))
                            .WithProp("x", (int)(layout.AbsoluteX + currentX + col))
                            .WithProp("y", (int)(layout.AbsoluteY + row))
                            .WithChild(new TextNode(ch.ToString()))
                            .Build());
                    }
                }
            }
            
            currentX += pattern[0].Length;
            if (charIndex < _text.Length - 1)
            {
                currentX += _spacing;
            }
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
    
    private string[]? GetCharPattern(char c)
    {
        if (_font == BigTextFont.Custom && _customFont != null)
        {
            return _customFont.TryGetValue(c, out var pattern) ? pattern : null;
        }
        
        return BigText.GetCharacterPattern(c, _font);
    }
}