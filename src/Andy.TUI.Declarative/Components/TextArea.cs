using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative multi-line text area component with SwiftUI-like syntax.
/// </summary>
public class TextArea : ISimpleComponent
{
    private readonly string _placeholder;
    private readonly Binding<string> _text;
    private readonly int _rows;
    private readonly int _cols;
    private readonly bool _wordWrap;

    public TextArea(string placeholder, Binding<string> text, int rows = 5, int cols = 40, bool wordWrap = true)
    {
        _placeholder = placeholder ?? string.Empty;
        _text = text ?? throw new ArgumentNullException(nameof(text));
        _rows = Math.Max(1, rows);
        _cols = Math.Max(10, cols);
        _wordWrap = wordWrap;
    }

    public TextArea Rows(int rows)
    {
        return new TextArea(_placeholder, _text, rows, _cols, _wordWrap);
    }

    public TextArea Cols(int cols)
    {
        return new TextArea(_placeholder, _text, _rows, cols, _wordWrap);
    }

    public TextArea WordWrap(bool wrap)
    {
        return new TextArea(_placeholder, _text, _rows, _cols, wrap);
    }

    // Internal accessors for view instance
    internal string GetPlaceholder() => _placeholder;
    internal Binding<string> GetBinding() => _text;
    internal int GetRows() => _rows;
    internal int GetCols() => _cols;
    internal bool GetWordWrap() => _wordWrap;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("TextArea declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}