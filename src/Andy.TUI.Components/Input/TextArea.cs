using System.Text;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Input;

/// <summary>
/// A multi-line text input component with cursor, selection, and scrolling support.
/// </summary>
public class TextArea : InputComponent
{
    private readonly List<StringBuilder> _lines = new() { new StringBuilder() };
    private int _cursorLine;
    private int _cursorColumn;
    private int _selectionStartLine = -1;
    private int _selectionStartColumn = -1;
    private int _selectionEndLine = -1;
    private int _selectionEndColumn = -1;
    private int _scrollTop;
    private int _scrollLeft;
    
    /// <summary>
    /// Gets or sets the text value.
    /// </summary>
    public string Text
    {
        get => string.Join("\n", _lines.Select(l => l.ToString()));
        set
        {
            _lines.Clear();
            
            if (string.IsNullOrEmpty(value))
            {
                _lines.Add(new StringBuilder());
            }
            else
            {
                var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    _lines.Add(new StringBuilder(line));
                }
            }
            
            // Clamp cursor position
            _cursorLine = Math.Min(_cursorLine, _lines.Count - 1);
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
            
            ClearSelection();
            OnTextChanged();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the placeholder text shown when the input is empty.
    /// </summary>
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// Gets or sets the number of visible lines.
    /// </summary>
    public int VisibleLines { get; set; } = 5;
    
    /// <summary>
    /// Gets or sets whether the input is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }
    
    /// <summary>
    /// Gets or sets whether word wrap is enabled.
    /// </summary>
    public bool WordWrap { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of lines allowed.
    /// </summary>
    public int? MaxLines { get; set; }
    
    /// <summary>
    /// Gets the current line index (0-based).
    /// </summary>
    public int CurrentLine => _cursorLine;
    
    /// <summary>
    /// Gets the current column index (0-based).
    /// </summary>
    public int CurrentColumn => _cursorColumn;
    
    /// <summary>
    /// Gets the total number of lines.
    /// </summary>
    public int LineCount => _lines.Count;
    
    /// <summary>
    /// Gets whether text is currently selected.
    /// </summary>
    public bool HasSelection => _selectionStartLine >= 0 && _selectionEndLine >= 0 &&
                                (_selectionStartLine != _selectionEndLine || _selectionStartColumn != _selectionEndColumn);
    
    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string SelectedText
    {
        get
        {
            if (!HasSelection) return string.Empty;
            
            var startLine = Math.Min(_selectionStartLine, _selectionEndLine);
            var endLine = Math.Max(_selectionStartLine, _selectionEndLine);
            var startCol = _selectionStartLine < _selectionEndLine ? _selectionStartColumn : _selectionEndColumn;
            var endCol = _selectionStartLine < _selectionEndLine ? _selectionEndColumn : _selectionStartColumn;
            
            if (startLine == endLine)
            {
                var start = Math.Min(startCol, endCol);
                var end = Math.Max(startCol, endCol);
                return _lines[startLine].ToString(start, end - start);
            }
            
            var result = new StringBuilder();
            
            // First line
            result.Append(_lines[startLine].ToString(startCol, _lines[startLine].Length - startCol));
            result.AppendLine();
            
            // Middle lines
            for (int i = startLine + 1; i < endLine; i++)
            {
                result.AppendLine(_lines[i].ToString());
            }
            
            // Last line
            result.Append(_lines[endLine].ToString(0, endCol));
            
            return result.ToString();
        }
    }
    
    /// <summary>
    /// Occurs when the text value changes.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;
    
    protected override Size MeasureCore(Size availableSize)
    {
        var width = Math.Min(availableSize.Width - Padding.Horizontal, 60); // Default width
        var height = Math.Min(VisibleLines + Padding.Vertical, availableSize.Height);
        
        return new Size(width + Padding.Horizontal, height);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        // Base handles bounds
    }
    
    protected override VirtualNode OnRender()
    {
        var children = new List<VirtualNode>();
        var contentBounds = ContentBounds;
        
        // Background
        if (!IsEnabled)
        {
            children.Add(CreateBackground(Color.DarkGray));
        }
        else if (IsFocused)
        {
            children.Add(CreateBackground(Color.DarkBlue));
        }
        
        // Text content or placeholder
        if (_lines.Count == 1 && _lines[0].Length == 0 && !string.IsNullOrEmpty(Placeholder) && !IsFocused)
        {
            children.Add(CreateText(contentBounds.X, contentBounds.Y, Placeholder, Color.Gray));
        }
        else
        {
            RenderVisibleLines(children, contentBounds);
        }
        
        // Cursor
        if (IsFocused && !IsReadOnly)
        {
            children.Add(CreateCursor(contentBounds));
        }
        
        // Border
        if (IsFocused)
        {
            children.Add(CreateBorder(BorderStyle.Single, Color.Cyan));
        }
        else
        {
            children.Add(CreateBorder(BorderStyle.Single, Color.Gray));
        }
        
        // Scrollbars if needed
        if (_lines.Count > contentBounds.Height)
        {
            children.Add(CreateVerticalScrollbar(contentBounds));
        }
        
        return CreateLayoutNode("textarea", children.ToArray());
    }
    
    protected override bool OnKeyPress(KeyEventArgs args)
    {
        if (IsReadOnly)
            return false;
            
        switch (args.Key)
        {
            case ConsoleKey.LeftArrow:
                MoveCursorLeft(args.Shift);
                return true;
                
            case ConsoleKey.RightArrow:
                MoveCursorRight(args.Shift);
                return true;
                
            case ConsoleKey.UpArrow:
                MoveCursorUp(args.Shift);
                return true;
                
            case ConsoleKey.DownArrow:
                MoveCursorDown(args.Shift);
                return true;
                
            case ConsoleKey.Home:
                if (args.Control)
                    MoveCursorToStart(args.Shift);
                else
                    MoveCursorToLineStart(args.Shift);
                return true;
                
            case ConsoleKey.End:
                if (args.Control)
                    MoveCursorToEnd(args.Shift);
                else
                    MoveCursorToLineEnd(args.Shift);
                return true;
                
            case ConsoleKey.PageUp:
                MoveCursorPageUp(args.Shift);
                return true;
                
            case ConsoleKey.PageDown:
                MoveCursorPageDown(args.Shift);
                return true;
                
            case ConsoleKey.Backspace:
                DeleteBackward();
                return true;
                
            case ConsoleKey.Delete:
                DeleteForward();
                return true;
                
            case ConsoleKey.Enter:
                InsertNewLine();
                return true;
                
            case ConsoleKey.Tab:
                InsertText("    "); // 4 spaces
                return true;
                
            case ConsoleKey.A when args.Control:
                SelectAll();
                return true;
                
            case ConsoleKey.C when args.Control:
                CopyToClipboard();
                return true;
                
            case ConsoleKey.V when args.Control:
                PasteFromClipboard();
                return true;
                
            case ConsoleKey.X when args.Control:
                CutToClipboard();
                return true;
                
            default:
                if (args.KeyChar != '\0' && !char.IsControl(args.KeyChar))
                {
                    InsertText(args.KeyChar.ToString());
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    /// <summary>
    /// Inserts text at the current cursor position.
    /// </summary>
    public void InsertText(string text)
    {
        if (IsReadOnly || string.IsNullOrEmpty(text))
            return;
            
        if (HasSelection)
        {
            DeleteSelection();
        }
        
        var currentLine = _lines[_cursorLine];
        currentLine.Insert(_cursorColumn, text);
        _cursorColumn += text.Length;
        
        EnsureCursorVisible();
        OnTextChanged();
        RequestRender();
    }
    
    /// <summary>
    /// Inserts a new line at the current cursor position.
    /// </summary>
    public void InsertNewLine()
    {
        if (IsReadOnly)
            return;
            
        if (MaxLines.HasValue && _lines.Count >= MaxLines.Value)
            return;
            
        if (HasSelection)
        {
            DeleteSelection();
        }
        
        var currentLine = _lines[_cursorLine];
        var remainingText = currentLine.ToString(_cursorColumn, currentLine.Length - _cursorColumn);
        currentLine.Remove(_cursorColumn, currentLine.Length - _cursorColumn);
        
        _lines.Insert(_cursorLine + 1, new StringBuilder(remainingText));
        _cursorLine++;
        _cursorColumn = 0;
        
        EnsureCursorVisible();
        OnTextChanged();
        RequestRender();
    }
    
    /// <summary>
    /// Deletes the character before the cursor.
    /// </summary>
    public void DeleteBackward()
    {
        if (IsReadOnly)
            return;
            
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (_cursorColumn > 0)
        {
            _lines[_cursorLine].Remove(_cursorColumn - 1, 1);
            _cursorColumn--;
            EnsureCursorVisible();
            OnTextChanged();
            RequestRender();
        }
        else if (_cursorLine > 0)
        {
            // Merge with previous line
            var previousLine = _lines[_cursorLine - 1];
            _cursorColumn = previousLine.Length;
            previousLine.Append(_lines[_cursorLine]);
            _lines.RemoveAt(_cursorLine);
            _cursorLine--;
            EnsureCursorVisible();
            OnTextChanged();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Deletes the character after the cursor.
    /// </summary>
    public void DeleteForward()
    {
        if (IsReadOnly)
            return;
            
        if (HasSelection)
        {
            DeleteSelection();
        }
        else if (_cursorColumn < _lines[_cursorLine].Length)
        {
            _lines[_cursorLine].Remove(_cursorColumn, 1);
            OnTextChanged();
            RequestRender();
        }
        else if (_cursorLine < _lines.Count - 1)
        {
            // Merge with next line
            _lines[_cursorLine].Append(_lines[_cursorLine + 1]);
            _lines.RemoveAt(_cursorLine + 1);
            OnTextChanged();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Selects all text.
    /// </summary>
    public void SelectAll()
    {
        if (_lines.Count > 0)
        {
            _selectionStartLine = 0;
            _selectionStartColumn = 0;
            _selectionEndLine = _lines.Count - 1;
            _selectionEndColumn = _lines[_lines.Count - 1].Length;
            _cursorLine = _selectionEndLine;
            _cursorColumn = _selectionEndColumn;
            EnsureCursorVisible();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Clears the current selection.
    /// </summary>
    public void ClearSelection()
    {
        if (HasSelection)
        {
            _selectionStartLine = -1;
            _selectionStartColumn = -1;
            _selectionEndLine = -1;
            _selectionEndColumn = -1;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Sets the cursor position.
    /// </summary>
    public void SetCursorPosition(int line, int column)
    {
        _cursorLine = Math.Max(0, Math.Min(line, _lines.Count - 1));
        _cursorColumn = Math.Max(0, Math.Min(column, _lines[_cursorLine].Length));
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorLeft(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        if (_cursorColumn > 0)
        {
            _cursorColumn--;
        }
        else if (_cursorLine > 0)
        {
            _cursorLine--;
            _cursorColumn = _lines[_cursorLine].Length;
        }
        
        if (extendSelection && HasSelection)
        {
            _selectionEndLine = _cursorLine;
            _selectionEndColumn = _cursorColumn;
        }
        
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorRight(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        if (_cursorColumn < _lines[_cursorLine].Length)
        {
            _cursorColumn++;
        }
        else if (_cursorLine < _lines.Count - 1)
        {
            _cursorLine++;
            _cursorColumn = 0;
        }
        
        if (extendSelection && HasSelection)
        {
            _selectionEndLine = _cursorLine;
            _selectionEndColumn = _cursorColumn;
        }
        
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorUp(bool extendSelection)
    {
        if (_cursorLine > 0)
        {
            if (extendSelection)
                ExtendSelection();
            else
                ClearSelection();
                
            _cursorLine--;
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
            
            if (extendSelection && HasSelection)
            {
                _selectionEndLine = _cursorLine;
                _selectionEndColumn = _cursorColumn;
            }
            
            EnsureCursorVisible();
            RequestRender();
        }
    }
    
    private void MoveCursorDown(bool extendSelection)
    {
        if (_cursorLine < _lines.Count - 1)
        {
            if (extendSelection)
                ExtendSelection();
            else
                ClearSelection();
                
            _cursorLine++;
            _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
            
            if (extendSelection && HasSelection)
            {
                _selectionEndLine = _cursorLine;
                _selectionEndColumn = _cursorColumn;
            }
            
            EnsureCursorVisible();
            RequestRender();
        }
    }
    
    private void MoveCursorToLineStart(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorColumn = 0;
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorToLineEnd(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorColumn = _lines[_cursorLine].Length;
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorToStart(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorLine = 0;
        _cursorColumn = 0;
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorToEnd(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorLine = _lines.Count - 1;
        _cursorColumn = _lines[_cursorLine].Length;
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorPageUp(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorLine = Math.Max(0, _cursorLine - VisibleLines);
        _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void MoveCursorPageDown(bool extendSelection)
    {
        if (extendSelection)
            ExtendSelection();
        else
            ClearSelection();
            
        _cursorLine = Math.Min(_lines.Count - 1, _cursorLine + VisibleLines);
        _cursorColumn = Math.Min(_cursorColumn, _lines[_cursorLine].Length);
        EnsureCursorVisible();
        RequestRender();
    }
    
    private void ExtendSelection()
    {
        if (!HasSelection)
        {
            _selectionStartLine = _cursorLine;
            _selectionStartColumn = _cursorColumn;
            _selectionEndLine = _cursorLine;
            _selectionEndColumn = _cursorColumn;
        }
    }
    
    private void DeleteSelection()
    {
        if (!HasSelection)
            return;
            
        var startLine = Math.Min(_selectionStartLine, _selectionEndLine);
        var endLine = Math.Max(_selectionStartLine, _selectionEndLine);
        var startCol = _selectionStartLine < _selectionEndLine ? _selectionStartColumn : 
                       _selectionStartLine > _selectionEndLine ? _selectionEndColumn :
                       Math.Min(_selectionStartColumn, _selectionEndColumn);
        var endCol = _selectionStartLine < _selectionEndLine ? _selectionEndColumn :
                     _selectionStartLine > _selectionEndLine ? _selectionStartColumn :
                     Math.Max(_selectionStartColumn, _selectionEndColumn);
        
        if (startLine == endLine)
        {
            // Single line selection
            _lines[startLine].Remove(startCol, endCol - startCol);
        }
        else
        {
            // Multi-line selection
            _lines[startLine].Remove(startCol, _lines[startLine].Length - startCol);
            _lines[startLine].Append(_lines[endLine].ToString(endCol, _lines[endLine].Length - endCol));
            
            // Remove lines in between
            for (int i = endLine; i > startLine; i--)
            {
                _lines.RemoveAt(i);
            }
        }
        
        _cursorLine = startLine;
        _cursorColumn = startCol;
        ClearSelection();
        EnsureCursorVisible();
        OnTextChanged();
        RequestRender();
    }
    
    private void EnsureCursorVisible()
    {
        var visibleHeight = ContentBounds.Height;
        
        // Vertical scrolling
        if (_cursorLine < _scrollTop)
        {
            _scrollTop = _cursorLine;
        }
        else if (_cursorLine >= _scrollTop + visibleHeight)
        {
            _scrollTop = _cursorLine - visibleHeight + 1;
        }
        
        // Horizontal scrolling (if not word wrapped)
        if (!WordWrap)
        {
            var visibleWidth = ContentBounds.Width;
            
            if (_cursorColumn < _scrollLeft)
            {
                _scrollLeft = _cursorColumn;
            }
            else if (_cursorColumn >= _scrollLeft + visibleWidth)
            {
                _scrollLeft = _cursorColumn - visibleWidth + 1;
            }
        }
    }
    
    private void RenderVisibleLines(List<VirtualNode> children, Rectangle contentBounds)
    {
        var visibleHeight = contentBounds.Height;
        var endLine = Math.Min(_scrollTop + visibleHeight, _lines.Count);
        
        for (int i = _scrollTop; i < endLine; i++)
        {
            var y = contentBounds.Y + (i - _scrollTop);
            var line = _lines[i].ToString();
            
            if (!WordWrap && line.Length > _scrollLeft)
            {
                var visibleText = line.Substring(_scrollLeft, Math.Min(line.Length - _scrollLeft, contentBounds.Width));
                children.Add(CreateText(contentBounds.X, y, visibleText, GetTextColor()));
            }
            else if (WordWrap)
            {
                // TODO: Implement word wrapping
                children.Add(CreateText(contentBounds.X, y, line, GetTextColor()));
            }
            
            // Selection highlight
            if (HasSelection && IsLineInSelection(i))
            {
                children.Add(CreateSelectionHighlight(i, contentBounds, y));
            }
        }
    }
    
    private bool IsLineInSelection(int line)
    {
        if (!HasSelection)
            return false;
            
        var startLine = Math.Min(_selectionStartLine, _selectionEndLine);
        var endLine = Math.Max(_selectionStartLine, _selectionEndLine);
        
        return line >= startLine && line <= endLine;
    }
    
    private VirtualNode CreateSelectionHighlight(int line, Rectangle contentBounds, int y)
    {
        var startLine = Math.Min(_selectionStartLine, _selectionEndLine);
        var endLine = Math.Max(_selectionStartLine, _selectionEndLine);
        var startCol = _selectionStartLine < _selectionEndLine ? _selectionStartColumn : 
                       _selectionStartLine > _selectionEndLine ? _selectionEndColumn :
                       Math.Min(_selectionStartColumn, _selectionEndColumn);
        var endCol = _selectionStartLine < _selectionEndLine ? _selectionEndColumn :
                     _selectionStartLine > _selectionEndLine ? _selectionStartColumn :
                     Math.Max(_selectionStartColumn, _selectionEndColumn);
        
        int highlightStart, highlightLength;
        
        if (line == startLine && line == endLine)
        {
            // Single line selection
            highlightStart = startCol;
            highlightLength = endCol - startCol;
        }
        else if (line == startLine)
        {
            // First line of multi-line selection
            highlightStart = startCol;
            highlightLength = _lines[line].Length - startCol;
        }
        else if (line == endLine)
        {
            // Last line of multi-line selection
            highlightStart = 0;
            highlightLength = endCol;
        }
        else
        {
            // Middle line of multi-line selection
            highlightStart = 0;
            highlightLength = _lines[line].Length;
        }
        
        // Adjust for horizontal scroll
        if (!WordWrap && highlightStart < _scrollLeft)
        {
            highlightLength -= (_scrollLeft - highlightStart);
            highlightStart = 0;
        }
        else if (!WordWrap)
        {
            highlightStart -= _scrollLeft;
        }
        
        if (highlightLength > 0)
        {
            return new ElementNode("highlight", new Dictionary<string, object?>
            {
                ["x"] = contentBounds.X + highlightStart,
                ["y"] = y,
                ["width"] = Math.Min(highlightLength, contentBounds.Width - highlightStart),
                ["height"] = 1,
                ["background"] = Color.DarkCyan
            });
        }
        
        return new FragmentNode();
    }
    
    private Color GetTextColor()
    {
        if (!IsEnabled)
            return Color.DarkGray;
            
        return IsFocused ? Color.White : Color.Gray;
    }
    
    private void CopyToClipboard()
    {
        // In a real implementation, this would use platform clipboard
        var textToCopy = HasSelection ? SelectedText : Text;
        // TODO: Implement clipboard support
    }
    
    private void PasteFromClipboard()
    {
        // In a real implementation, this would use platform clipboard
        // TODO: Implement clipboard support
    }
    
    private void CutToClipboard()
    {
        if (HasSelection)
        {
            CopyToClipboard();
            DeleteSelection();
        }
    }
    
    private void OnTextChanged()
    {
        TextChanged?.Invoke(this, new TextChangedEventArgs(Text));
    }
    
    private VirtualNode CreateBackground(Color color)
    {
        return new ElementNode("rect", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height,
            ["fill"] = color
        });
    }
    
    private VirtualNode CreateText(int x, int y, string text, Color color)
    {
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = x,
            ["y"] = y,
            ["color"] = color
        }, new TextNode(text));
    }
    
    private VirtualNode CreateCursor(Rectangle contentBounds)
    {
        if (_cursorLine >= _scrollTop && _cursorLine < _scrollTop + contentBounds.Height)
        {
            var cursorX = contentBounds.X + (_cursorColumn - _scrollLeft);
            var cursorY = contentBounds.Y + (_cursorLine - _scrollTop);
            
            if (cursorX >= contentBounds.X && cursorX < contentBounds.Right)
            {
                return new ElementNode("cursor", new Dictionary<string, object?>
                {
                    ["x"] = cursorX,
                    ["y"] = cursorY,
                    ["style"] = new Style { Foreground = Color.White, Background = Color.White }
                }, new TextNode("│"));
            }
        }
        
        return new FragmentNode();
    }
    
    private VirtualNode CreateBorder(BorderStyle style, Color color)
    {
        var chars = GetBorderChars(style);
        var nodes = new List<VirtualNode>();
        
        // Top border
        nodes.Add(CreateText(Bounds.X, Bounds.Y, chars.TopLeft.ToString(), color));
        for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, Bounds.Y, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(Bounds.Right - 1, Bounds.Y, chars.TopRight.ToString(), color));
        
        // Side borders
        for (int y = Bounds.Y + 1; y < Bounds.Bottom - 1; y++)
        {
            nodes.Add(CreateText(Bounds.X, y, chars.Vertical.ToString(), color));
            nodes.Add(CreateText(Bounds.Right - 1, y, chars.Vertical.ToString(), color));
        }
        
        // Bottom border
        nodes.Add(CreateText(Bounds.X, Bounds.Bottom - 1, chars.BottomLeft.ToString(), color));
        for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, Bounds.Bottom - 1, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(Bounds.Right - 1, Bounds.Bottom - 1, chars.BottomRight.ToString(), color));
        
        return new FragmentNode(nodes);
    }
    
    private VirtualNode CreateVerticalScrollbar(Rectangle contentBounds)
    {
        var scrollbarX = Bounds.Right - 1;
        var scrollbarHeight = contentBounds.Height;
        var thumbHeight = Math.Max(1, (scrollbarHeight * scrollbarHeight) / _lines.Count);
        var thumbY = contentBounds.Y + (_scrollTop * scrollbarHeight) / _lines.Count;
        
        var nodes = new List<VirtualNode>();
        
        // Scrollbar track
        for (int y = contentBounds.Y; y < contentBounds.Bottom; y++)
        {
            nodes.Add(CreateText(scrollbarX, y, "│", Color.DarkGray));
        }
        
        // Scrollbar thumb
        for (int i = 0; i < thumbHeight && thumbY + i < contentBounds.Bottom; i++)
        {
            nodes.Add(CreateText(scrollbarX, thumbY + i, "█", Color.Gray));
        }
        
        return new FragmentNode(nodes);
    }
    
    private (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => ('╭', '╮', '╰', '╯', '─', '│'),
            _ => ('+', '+', '+', '+', '-', '|')
        };
    }
}