using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Text view.
/// </summary>
public class TextInstance : ViewInstance
{
    private string _content = "";
    private Style _style = Style.Default;
    private TextWrap _wrap = TextWrap.NoWrap;
    private int? _maxLines = null;
    private TruncationMode _truncationMode = TruncationMode.Tail;
    private int? _maxWidth = null;
    private List<string> _wrappedLines = new();
    private readonly ILogger _logger;
    
    public TextInstance(string id) : base(id)
    {
        _logger = DebugContext.Logger.ForCategory("TextInstance");
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Text text)
            throw new ArgumentException("Expected Text declaration");
        
        _content = text.GetContent();
        _style = text.GetStyle();
        _wrap = text.GetWrap();
        _maxLines = text.GetMaxLines();
        _truncationMode = text.GetTruncationMode();
        _maxWidth = text.GetMaxWidth();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Handle infinite constraints - don't wrap if we have infinite space
        var effectiveMaxWidth = float.IsPositiveInfinity(constraints.MaxWidth) 
            ? int.MaxValue 
            : (int)constraints.MaxWidth;
            
        var maxWidth = _maxWidth.HasValue 
            ? Math.Min(_maxWidth.Value, effectiveMaxWidth) 
            : effectiveMaxWidth;
        
        // Apply text wrapping
        _wrappedLines = WrapText(_content, maxWidth, _wrap);
        
        // Apply truncation if enabled and maxWidth is specified
        if (_truncationMode != TruncationMode.None && _maxWidth.HasValue)
        {
            for (int i = 0; i < _wrappedLines.Count; i++)
            {
                _wrappedLines[i] = ApplyTruncation(_wrappedLines[i], maxWidth);
            }
        }
        
        // Apply max lines constraint
        if (_maxLines.HasValue && _wrappedLines.Count > _maxLines.Value)
        {
            _wrappedLines = _wrappedLines.Take(_maxLines.Value).ToList();
            
            // Apply truncation to the last line if needed
            if (_wrappedLines.Count > 0 && _truncationMode != TruncationMode.None)
            {
                var lastIndex = _wrappedLines.Count - 1;
                _wrappedLines[lastIndex] = ApplyTruncation(_wrappedLines[lastIndex], maxWidth);
            }
        }
        
        // Calculate dimensions
        var contentWidth = _wrappedLines.Count > 0 
            ? _wrappedLines.Max(line => line.Length) 
            : 0;
        var height = _wrappedLines.Count;
        
        // For wrapped text, if any line would have exceeded maxWidth, use maxWidth as layout width
        // This ensures proper alignment and space usage when text wraps
        var wouldExceedWidth = _wrap != TextWrap.NoWrap && _content.Length > maxWidth;
        var width = wouldExceedWidth ? maxWidth : contentWidth;
        
        var layout = new LayoutBox
        {
            Width = width,
            Height = height
        };
        
        // Constrain to available space
        layout.Width = constraints.ConstrainWidth(layout.Width);
        // For height, use the calculated height directly when constraints allow infinity
        // This allows text to report its natural height for auto-sizing containers
        if (float.IsPositiveInfinity(constraints.MaxHeight))
        {
            layout.Height = height;
        }
        else
        {
            layout.Height = constraints.ConstrainHeight(layout.Height);
        }
        
        _logger.Debug("TextInstance layout: calculated height={0}, constrained height={1}, constraints={2}", 
            height, layout.Height, constraints);
        
        return layout;
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        _logger.Debug("RenderWithLayout called - content: '{0}', wrappedLines: {1}, layout: x={2}, y={3}, w={4}, h={5}", 
            _content, _wrappedLines.Count, layout.AbsoluteX, layout.AbsoluteY, layout.Width, layout.Height);
            
        if (_wrappedLines.Count == 0)
        {
            _logger.Debug("No wrapped lines, returning empty fragment");
            return Fragment();
        }
        
        var elements = new List<VirtualNode>();
        
        for (int i = 0; i < _wrappedLines.Count && i < layout.Height; i++)
        {
            var line = _wrappedLines[i];
            if (line.Length > layout.Width)
            {
                line = ApplyTruncation(line, (int)layout.Width);
            }
            
            _logger.Debug("Rendering line {0}: '{1}' at ({2}, {3})", i, line, layout.AbsoluteX, layout.AbsoluteY + i);
            
            elements.Add(
                Element("text")
                    .WithProp("style", _style)
                    .WithProp("x", layout.AbsoluteX)
                    .WithProp("y", layout.AbsoluteY + i)
                    .WithProp("z-index", AbsoluteZIndex)
                    .WithChild(new TextNode(line))
                    .Build()
            );
        }
        
        _logger.Debug("Rendered {0} text elements", elements.Count);
        return Fragment(elements.ToArray());
    }
    
    private List<string> WrapText(string text, int maxWidth, TextWrap wrap)
    {
        // Expand tabs to spaces (4 spaces per tab)
        text = text.Replace("\t", "    ");
        
        // First split by newlines
        var inputLines = text.Split('\n');
        var allLines = new List<string>();
        
        foreach (var inputLine in inputLines)
        {
            if (wrap == TextWrap.NoWrap || maxWidth <= 0)
            {
                allLines.Add(inputLine);
            }
            else if (wrap == TextWrap.Word)
            {
                var words = inputLine.Split(' ');
                var currentLine = "";
                
                foreach (var word in words)
                {
                    if (string.IsNullOrEmpty(currentLine))
                    {
                        currentLine = word;
                    }
                    else if (currentLine.Length + 1 + word.Length <= maxWidth)
                    {
                        currentLine += " " + word;
                    }
                    else
                    {
                        allLines.Add(currentLine);
                        currentLine = word;
                    }
                }
                
                if (!string.IsNullOrEmpty(currentLine))
                {
                    allLines.Add(currentLine);
                }
                else if (string.IsNullOrEmpty(inputLine))
                {
                    // Preserve empty lines
                    allLines.Add("");
                }
            }
            else // Character wrap
            {
                if (inputLine.Length == 0)
                {
                    allLines.Add("");
                }
                else
                {
                    for (int i = 0; i < inputLine.Length; i += maxWidth)
                    {
                        var remainingLength = Math.Min(maxWidth, inputLine.Length - i);
                        allLines.Add(inputLine.Substring(i, remainingLength));
                    }
                }
            }
        }
        
        return allLines.Count > 0 ? allLines : new List<string> { "" };
    }
    
    private string ApplyTruncation(string text, int maxWidth)
    {
        if (text.Length <= maxWidth || maxWidth <= 3)
            return text;
        
        const string ellipsis = "...";
        
        return _truncationMode switch
        {
            TruncationMode.None => text.Substring(0, maxWidth),
            TruncationMode.Head => ellipsis + text.Substring(text.Length - (maxWidth - 3)),
            TruncationMode.Middle => 
                text.Substring(0, (maxWidth - 3) / 2) + 
                ellipsis + 
                text.Substring(text.Length - ((maxWidth - 3) - (maxWidth - 3) / 2)),
            TruncationMode.Tail or _ => text.Substring(0, maxWidth - 3) + ellipsis
        };
    }
}