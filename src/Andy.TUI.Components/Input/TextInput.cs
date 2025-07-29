using System.Text;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Input;

/// <summary>
/// A single-line text input component with cursor and selection support.
/// </summary>
public class TextInput : InputComponent
{
    private readonly StringBuilder _text = new();
    private int _cursorPosition;
    private int _selectionStart = -1;
    private int _selectionEnd = -1;
    private int _scrollOffset;
    
    /// <summary>
    /// Gets or sets the text value.
    /// </summary>
    public string Text
    {
        get => _text.ToString();
        set
        {
            if (_text.ToString() != value)
            {
                _text.Clear();
                _text.Append(value ?? string.Empty);
                _cursorPosition = Math.Min(_cursorPosition, _text.Length);
                ClearSelection();
                OnTextChanged();
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the placeholder text shown when the input is empty.
    /// </summary>
    public string? Placeholder { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum length of text allowed.
    /// </summary>
    public int? MaxLength { get; set; }
    
    /// <summary>
    /// Gets or sets whether the input is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }
    
    /// <summary>
    /// Gets or sets the character used for password input.
    /// </summary>
    public char? PasswordChar { get; set; }
    
    /// <summary>
    /// Gets or sets the validation function.
    /// </summary>
    public Func<string, bool>? Validator { get; set; }
    
    /// <summary>
    /// Gets whether the current text is valid.
    /// </summary>
    public bool IsValid => Validator?.Invoke(Text) ?? true;
    
    /// <summary>
    /// Gets or sets the cursor position.
    /// </summary>
    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            var newPos = Math.Max(0, Math.Min(value, _text.Length));
            if (_cursorPosition != newPos)
            {
                _cursorPosition = newPos;
                EnsureCursorVisible();
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets whether text is currently selected.
    /// </summary>
    public bool HasSelection => _selectionStart >= 0 && _selectionEnd >= 0 && _selectionStart != _selectionEnd;
    
    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string SelectedText
    {
        get
        {
            if (!HasSelection) return string.Empty;
            var start = Math.Min(_selectionStart, _selectionEnd);
            var end = Math.Max(_selectionStart, _selectionEnd);
            return _text.ToString(start, end - start);
        }
    }
    
    /// <summary>
    /// Occurs when the text value changes.
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;
    
    /// <summary>
    /// Occurs when Enter is pressed.
    /// </summary>
    public event EventHandler? Submitted;
    
    protected override Size MeasureCore(Size availableSize)
    {
        // Single line input with standard height
        var width = Math.Min(availableSize.Width - Padding.Horizontal, 50); // Default width
        var height = 1 + Padding.Vertical;
        
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
        var displayText = GetDisplayText();
        var textColor = GetTextColor();
        
        if (!string.IsNullOrEmpty(displayText))
        {
            var visibleText = GetVisibleText(displayText, contentBounds.Width);
            children.Add(CreateText(contentBounds.X, contentBounds.Y, visibleText, textColor));
        }
        
        // Selection highlight
        if (HasSelection && IsFocused)
        {
            children.Add(CreateSelectionHighlight(contentBounds));
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
        
        return CreateLayoutNode("textinput", children.ToArray());
    }
    
    protected override bool OnKeyPress(KeyEventArgs args)
    {
        if (IsReadOnly)
            return false;
            
        switch (args.Key)
        {
            case ConsoleKey.LeftArrow:
                MoveCursor(-1, args.Shift);
                return true;
                
            case ConsoleKey.RightArrow:
                MoveCursor(1, args.Shift);
                return true;
                
            case ConsoleKey.Home:
                MoveCursorToStart(args.Shift);
                return true;
                
            case ConsoleKey.End:
                MoveCursorToEnd(args.Shift);
                return true;
                
            case ConsoleKey.Backspace:
                DeleteBackward();
                return true;
                
            case ConsoleKey.Delete:
                DeleteForward();
                return true;
                
            case ConsoleKey.Enter:
                OnSubmitted();
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
            
        // Check max length
        if (MaxLength.HasValue)
        {
            var remainingLength = MaxLength.Value - _text.Length;
            if (HasSelection)
            {
                remainingLength += SelectedText.Length;
            }
            
            if (text.Length > remainingLength)
            {
                text = text.Substring(0, remainingLength);
            }
            
            if (text.Length == 0)
                return;
        }
        
        if (HasSelection)
        {
            DeleteSelection();
        }
        
        _text.Insert(_cursorPosition, text);
        _cursorPosition += text.Length;
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
        else if (_cursorPosition > 0)
        {
            _text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
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
        else if (_cursorPosition < _text.Length)
        {
            _text.Remove(_cursorPosition, 1);
            OnTextChanged();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Selects all text.
    /// </summary>
    public void SelectAll()
    {
        if (_text.Length > 0)
        {
            _selectionStart = 0;
            _selectionEnd = _text.Length;
            _cursorPosition = _text.Length;
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
            _selectionStart = -1;
            _selectionEnd = -1;
            RequestRender();
        }
    }
    
    private void MoveCursor(int delta, bool extendSelection)
    {
        var newPos = Math.Max(0, Math.Min(_cursorPosition + delta, _text.Length));
        
        if (extendSelection)
        {
            if (!HasSelection)
            {
                _selectionStart = _cursorPosition;
            }
            _selectionEnd = newPos;
        }
        else
        {
            ClearSelection();
        }
        
        CursorPosition = newPos;
    }
    
    private void MoveCursorToStart(bool extendSelection)
    {
        if (extendSelection)
        {
            if (!HasSelection)
            {
                _selectionStart = _cursorPosition;
            }
            _selectionEnd = 0;
        }
        else
        {
            ClearSelection();
        }
        
        CursorPosition = 0;
    }
    
    private void MoveCursorToEnd(bool extendSelection)
    {
        if (extendSelection)
        {
            if (!HasSelection)
            {
                _selectionStart = _cursorPosition;
            }
            _selectionEnd = _text.Length;
        }
        else
        {
            ClearSelection();
        }
        
        CursorPosition = _text.Length;
    }
    
    private void DeleteSelection()
    {
        if (!HasSelection)
            return;
            
        var start = Math.Min(_selectionStart, _selectionEnd);
        var end = Math.Max(_selectionStart, _selectionEnd);
        
        _text.Remove(start, end - start);
        _cursorPosition = start;
        ClearSelection();
        EnsureCursorVisible();
        OnTextChanged();
        RequestRender();
    }
    
    private void EnsureCursorVisible()
    {
        var visibleWidth = ContentBounds.Width - 1; // Leave space for cursor
        
        if (_cursorPosition < _scrollOffset)
        {
            _scrollOffset = _cursorPosition;
        }
        else if (_cursorPosition >= _scrollOffset + visibleWidth)
        {
            _scrollOffset = _cursorPosition - visibleWidth + 1;
        }
    }
    
    private string GetDisplayText()
    {
        if (_text.Length > 0)
        {
            return PasswordChar.HasValue 
                ? new string(PasswordChar.Value, _text.Length)
                : _text.ToString();
        }
        
        return !IsFocused && !string.IsNullOrEmpty(Placeholder) 
            ? Placeholder 
            : string.Empty;
    }
    
    private string GetVisibleText(string text, int width)
    {
        if (text.Length <= width)
            return text;
            
        var visibleLength = Math.Min(text.Length - _scrollOffset, width);
        return text.Substring(_scrollOffset, visibleLength);
    }
    
    private Color GetTextColor()
    {
        if (!IsEnabled)
            return Color.DarkGray;
            
        if (_text.Length == 0 && !string.IsNullOrEmpty(Placeholder) && !IsFocused)
            return Color.Gray;
            
        if (!IsValid)
            return Color.Red;
            
        return IsFocused ? Color.White : Color.Gray;
    }
    
    private void CopyToClipboard()
    {
        // In a real implementation, this would use platform clipboard
        // For now, we'll just track it internally
        var textToCopy = HasSelection ? SelectedText : Text;
        // TODO: Implement clipboard support
    }
    
    private void PasteFromClipboard()
    {
        // In a real implementation, this would use platform clipboard
        // For now, we'll just simulate with a simple string
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
    
    private void OnSubmitted()
    {
        Submitted?.Invoke(this, EventArgs.Empty);
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
        var cursorX = contentBounds.X + (_cursorPosition - _scrollOffset);
        
        if (cursorX >= contentBounds.X && cursorX < contentBounds.Right)
        {
            return new ElementNode("cursor", new Dictionary<string, object?>
            {
                ["x"] = cursorX,
                ["y"] = contentBounds.Y,
                ["style"] = new Style { Foreground = Color.White, Background = Color.White }
            }, new TextNode("│"));
        }
        
        return new FragmentNode();
    }
    
    private VirtualNode CreateSelectionHighlight(Rectangle contentBounds)
    {
        if (!HasSelection)
            return new FragmentNode();
            
        var start = Math.Min(_selectionStart, _selectionEnd);
        var end = Math.Max(_selectionStart, _selectionEnd);
        
        // Adjust for scroll offset
        var visibleStart = Math.Max(0, start - _scrollOffset);
        var visibleEnd = Math.Min(contentBounds.Width, end - _scrollOffset);
        
        if (visibleEnd > visibleStart)
        {
            return new ElementNode("highlight", new Dictionary<string, object?>
            {
                ["x"] = contentBounds.X + visibleStart,
                ["y"] = contentBounds.Y,
                ["width"] = visibleEnd - visibleStart,
                ["height"] = 1,
                ["background"] = Color.DarkCyan
            });
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

/// <summary>
/// Event arguments for text change events.
/// </summary>
public class TextChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new text value.
    /// </summary>
    public string Text { get; }
    
    public TextChangedEventArgs(string text)
    {
        Text = text;
    }
}

