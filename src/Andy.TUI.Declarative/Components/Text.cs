using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative text component with SwiftUI-like syntax.
/// </summary>
public class Text : ISimpleComponent
{
    private readonly string _content;
    private Style _style = Style.Default;
    private TextWrap _wrap = TextWrap.NoWrap;
    private int? _maxLines = null;
    private TruncationMode _truncationMode = TruncationMode.Tail;
    private int? _maxWidth = null;

    public Text(string content)
    {
        _content = content ?? string.Empty;
    }

    public Text Color(Color color)
    {
        return new Text(_content) 
        { 
            _style = _style.WithForegroundColor(color),
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }

    public Text Bold()
    {
        return new Text(_content) 
        { 
            _style = _style.WithBold(true),
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }

    public Text Title()
    {
        return new Text(_content) 
        { 
            _style = _style.WithBold(true).WithForegroundColor(Terminal.Color.White),
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }

    public Text Dim()
    {
        return new Text(_content) 
        { 
            _style = _style.WithDim(true),
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }

    public Text Underline()
    {
        return new Text(_content) 
        { 
            _style = _style.WithUnderline(true),
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }

    public Text Center()
    {
        // For now, just return self - in full implementation this would add center alignment
        return this;
    }
    
    public Text Wrap(TextWrap wrap)
    {
        return new Text(_content) 
        { 
            _style = _style,
            _wrap = wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }
    
    public Text MaxLines(int lines)
    {
        return new Text(_content) 
        { 
            _style = _style,
            _wrap = _wrap,
            _maxLines = lines,
            _truncationMode = _truncationMode,
            _maxWidth = _maxWidth
        };
    }
    
    public Text Truncate(TruncationMode mode)
    {
        return new Text(_content) 
        { 
            _style = _style,
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = mode,
            _maxWidth = _maxWidth
        };
    }
    
    public Text MaxWidth(int width)
    {
        return new Text(_content) 
        { 
            _style = _style,
            _wrap = _wrap,
            _maxLines = _maxLines,
            _truncationMode = _truncationMode,
            _maxWidth = width
        };
    }
    
    // Internal accessors for view instance
    internal string GetContent() => _content;
    internal Style GetStyle() => _style;
    internal TextWrap GetWrap() => _wrap;
    internal int? GetMaxLines() => _maxLines;
    internal TruncationMode GetTruncationMode() => _truncationMode;
    internal int? GetMaxWidth() => _maxWidth;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Text declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    public static implicit operator Text(string content) => new(content);
}