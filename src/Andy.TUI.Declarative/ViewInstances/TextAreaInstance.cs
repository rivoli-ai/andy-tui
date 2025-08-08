using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a TextArea view with multi-line editing support.
/// </summary>
public class TextAreaInstance : ViewInstance, IFocusable
{
    private string _placeholder = "";
    private Binding<string>? _textBinding;
    private int _rows = 5;
    private int _cols = 40;
    private bool _wordWrap = true;
    private bool _isFocused;
    private int _cursorRow;
    private int _cursorCol;
    private int _scrollOffset;
    private List<string> _lines = new();
    private IDisposable? _bindingSubscription;
    private readonly ILogger _logger;
    
    public TextAreaInstance(string id) : base(id)
    {
        _logger = DebugContext.Logger.ForCategory("TextAreaInstance");
    }
    
    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;
    
    public void OnGotFocus()
    {
        _isFocused = true;
        UpdateLinesFromText();
        InvalidateView();
    }
    
    public void OnLostFocus()
    {
        _isFocused = false;
        InvalidateView();
    }
    
    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (_textBinding == null) return false;
        
        switch (keyInfo.Key)
        {
            case ConsoleKey.Backspace:
                HandleBackspace();
                return true;
                
            case ConsoleKey.Delete:
                HandleDelete();
                return true;
                
            case ConsoleKey.Enter:
                HandleEnter();
                return true;
                
            case ConsoleKey.LeftArrow:
                MoveCursorLeft();
                return true;
                
            case ConsoleKey.RightArrow:
                MoveCursorRight();
                return true;
                
            case ConsoleKey.UpArrow:
                MoveCursorUp();
                return true;
                
            case ConsoleKey.DownArrow:
                MoveCursorDown();
                return true;
                
            case ConsoleKey.Home:
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    // Ctrl+Home - go to beginning of text
                    _cursorRow = 0;
                    _cursorCol = 0;
                }
                else
                {
                    // Home - go to beginning of line
                    _cursorCol = 0;
                }
                UpdateScroll();
                InvalidateView();
                return true;
                
            case ConsoleKey.End:
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    // Ctrl+End - go to end of text
                    _cursorRow = Math.Max(0, _lines.Count - 1);
                    _cursorCol = _lines.Count > 0 ? _lines[_cursorRow].Length : 0;
                }
                else
                {
                    // End - go to end of line
                    _cursorCol = _cursorRow < _lines.Count ? _lines[_cursorRow].Length : 0;
                }
                UpdateScroll();
                InvalidateView();
                return true;
                
            default:
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    InsertCharacter(keyInfo.KeyChar);
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    private void HandleBackspace()
    {
        if (_cursorCol > 0)
        {
            // Delete character before cursor
            _lines[_cursorRow] = _lines[_cursorRow].Remove(_cursorCol - 1, 1);
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            // Join with previous line
            var currentLine = _lines[_cursorRow];
            _lines.RemoveAt(_cursorRow);
            _cursorRow--;
            _cursorCol = _lines[_cursorRow].Length;
            _lines[_cursorRow] += currentLine;
        }
        
        UpdateTextFromLines();
        UpdateScroll();
        InvalidateView();
    }
    
    private void HandleDelete()
    {
        if (_cursorRow < _lines.Count)
        {
            if (_cursorCol < _lines[_cursorRow].Length)
            {
                // Delete character at cursor
                _lines[_cursorRow] = _lines[_cursorRow].Remove(_cursorCol, 1);
            }
            else if (_cursorRow < _lines.Count - 1)
            {
                // Join with next line
                _lines[_cursorRow] += _lines[_cursorRow + 1];
                _lines.RemoveAt(_cursorRow + 1);
            }
        }
        
        UpdateTextFromLines();
        InvalidateView();
    }
    
    private void HandleEnter()
    {
        if (_cursorRow < _lines.Count)
        {
            var currentLine = _lines[_cursorRow];
            var beforeCursor = currentLine.Substring(0, _cursorCol);
            var afterCursor = currentLine.Substring(_cursorCol);
            
            _lines[_cursorRow] = beforeCursor;
            _lines.Insert(_cursorRow + 1, afterCursor);
        }
        else
        {
            _lines.Add("");
        }
        
        _cursorRow++;
        _cursorCol = 0;
        
        UpdateTextFromLines();
        UpdateScroll();
        InvalidateView();
    }
    
    private void InsertCharacter(char ch)
    {
        _logger.Debug("InsertCharacter: '{0}' at ({1},{2})", ch, _cursorRow, _cursorCol);
        
        if (_cursorRow >= _lines.Count)
        {
            _lines.Add("");
        }
        
        var oldLine = _lines[_cursorRow];
        _lines[_cursorRow] = _lines[_cursorRow].Insert(_cursorCol, ch.ToString());
        _logger.Debug("Line {0} changed from '{1}' to '{2}'", _cursorRow, oldLine, _lines[_cursorRow]);
        _cursorCol++;
        
        // Handle word wrap if enabled
        if (_wordWrap && _lines[_cursorRow].Length > _cols - 2)
        {
            WrapCurrentLine();
        }
        
        UpdateTextFromLines();
        InvalidateView();
    }
    
    private void WrapCurrentLine()
    {
        var line = _lines[_cursorRow];
        if (line.Length <= _cols - 2) return;
        
        // Find wrap point (preferably at a space)
        var wrapPoint = _cols - 2;
        for (int i = wrapPoint; i > 0; i--)
        {
            if (char.IsWhiteSpace(line[i]))
            {
                wrapPoint = i;
                break;
            }
        }
        
        var beforeWrap = line.Substring(0, wrapPoint).TrimEnd();
        var afterWrap = line.Substring(wrapPoint).TrimStart();
        
        _lines[_cursorRow] = beforeWrap;
        
        if (_cursorRow < _lines.Count - 1)
        {
            // Prepend to next line
            _lines[_cursorRow + 1] = afterWrap + " " + _lines[_cursorRow + 1];
        }
        else
        {
            // Add new line
            _lines.Add(afterWrap);
        }
        
        // Adjust cursor if it was after wrap point
        if (_cursorCol > wrapPoint)
        {
            _cursorRow++;
            _cursorCol = _cursorCol - wrapPoint;
        }
    }
    
    private void MoveCursorLeft()
    {
        if (_cursorCol > 0)
        {
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = _lines[_cursorRow].Length;
        }
        UpdateScroll();
        InvalidateView();
    }
    
    private void MoveCursorRight()
    {
        if (_cursorRow < _lines.Count && _cursorCol < _lines[_cursorRow].Length)
        {
            _cursorCol++;
        }
        else if (_cursorRow < _lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = 0;
        }
        UpdateScroll();
        InvalidateView();
    }
    
    private void MoveCursorUp()
    {
        if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = Math.Min(_cursorCol, _lines[_cursorRow].Length);
        }
        UpdateScroll();
        InvalidateView();
    }
    
    private void MoveCursorDown()
    {
        if (_cursorRow < _lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = Math.Min(_cursorCol, _lines[_cursorRow].Length);
        }
        UpdateScroll();
        InvalidateView();
    }
    
    private void UpdateScroll()
    {
        // Ensure cursor is visible
        if (_cursorRow < _scrollOffset)
        {
            _scrollOffset = _cursorRow;
        }
        else if (_cursorRow >= _scrollOffset + _rows)
        {
            _scrollOffset = _cursorRow - _rows + 1;
        }
    }
    
    private void UpdateLinesFromText()
    {
        var text = _textBinding?.Value ?? string.Empty;
        _lines = text.Split('\n').ToList();
        
        if (_lines.Count == 0)
        {
            _lines.Add("");
        }
        
        // Ensure cursor is within bounds
        _cursorRow = Math.Min(_cursorRow, _lines.Count - 1);
        _cursorCol = Math.Min(_cursorCol, _lines[_cursorRow].Length);
    }
    
    private void UpdateTextFromLines()
    {
        if (_textBinding != null)
        {
            var newText = string.Join("\n", _lines);
            _textBinding.Value = newText;
            _logger.Debug("TextArea content updated: '{0}'", newText.Replace("\n", "\\n"));
        }
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not TextArea textArea)
            throw new ArgumentException("Expected TextArea declaration");
        
        // Update properties from declaration
        _placeholder = textArea.GetPlaceholder();
        _rows = textArea.GetRows();
        _cols = textArea.GetCols();
        _wordWrap = textArea.GetWordWrap();
        
        // Handle binding changes
        var newBinding = textArea.GetBinding();
        if (newBinding != _textBinding)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();
            
            // Subscribe to new binding
            _textBinding = newBinding;
            if (_textBinding != null)
            {
                _bindingSubscription = new BindingSubscription(_textBinding, () => 
                {
                    UpdateLinesFromText();
                    InvalidateView();
                });
                UpdateLinesFromText();
            }
        }
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();
        
        // TextArea has fixed dimensions based on rows/cols
        layout.Width = constraints.ConstrainWidth(_cols + 2); // +2 for borders
        layout.Height = constraints.ConstrainHeight(_rows + 2); // +2 for borders
        
        return layout;
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        _logger.Debug("TextArea rendering with {0} lines, cursor at ({1},{2})", _lines.Count, _cursorRow, _cursorCol);
        var elements = new List<VirtualNode>();
        
        // Draw border
        var borderStyle = _isFocused
            ? Style.Default.WithForegroundColor(Color.White)
            : Style.Default.WithForegroundColor(Color.DarkGray);
        
        // Top border
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                .WithProp("style", borderStyle)
                .WithChild(new TextNode("┌" + new string('─', _cols) + "┐"))
                .Build()
        );
        
        // Content area with sides
        for (int i = 0; i < _rows; i++)
        {
            var lineIndex = _scrollOffset + i;
            var lineContent = "";
            
            if (_lines.Count == 0 || (_lines.Count == 1 && string.IsNullOrEmpty(_lines[0])))
            {
                // Show placeholder on first line if empty
                if (i == 0)
                {
                    lineContent = _placeholder;
                }
            }
            else if (lineIndex < _lines.Count)
            {
                lineContent = _lines[lineIndex];
                
                // Show cursor if focused and on this line
                if (_isFocused && lineIndex == _cursorRow)
                {
                    var col = Math.Min(_cursorCol, lineContent.Length);
                    if (col < lineContent.Length)
                    {
                        lineContent = lineContent.Insert(col, "│");
                    }
                    else
                    {
                        lineContent += "│";
                    }
                }
            }
            
            // Truncate or pad to fit
            if (lineContent.Length > _cols)
            {
                lineContent = lineContent.Substring(0, _cols);
            }
            else
            {
                lineContent = lineContent.PadRight(_cols);
            }
            
            // Line style
            var lineStyle = (_lines.Count == 0 || (_lines.Count == 1 && string.IsNullOrEmpty(_lines[0]))) && i == 0
                ? Style.Default.WithForegroundColor(Color.DarkGray) // Placeholder style
                : Style.Default;
            
            // Left border + content + right border
            elements.Add(
                Fragment(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", borderStyle)
                        .WithChild(new TextNode("│"))
                        .Build(),
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + 1)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", lineStyle)
                        .WithChild(new TextNode(lineContent))
                        .Build(),
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + _cols + 1)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", borderStyle)
                        .WithChild(new TextNode("│"))
                        .Build()
                )
            );
        }
        
        // Bottom border
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY + _rows + 1)
                .WithProp("style", borderStyle)
                .WithChild(new TextNode("└" + new string('─', _cols) + "┘"))
                .Build()
        );
        
        // Show scroll indicator if needed
        if (_lines.Count > _rows)
        {
            var scrollBarHeight = Math.Max(1, (_rows * _rows) / _lines.Count);
            var scrollBarPos = (_scrollOffset * (_rows - scrollBarHeight)) / (_lines.Count - _rows);
            
            for (int i = 0; i < _rows; i++)
            {
                var isScrollBar = i >= scrollBarPos && i < scrollBarPos + scrollBarHeight;
                var scrollChar = isScrollBar ? "█" : "░";
                
                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + _cols + 2)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                        .WithChild(new TextNode(scrollChar))
                        .Build()
                );
            }
        }
        
        return Fragment(elements.ToArray());
    }
    
    public override void Dispose()
    {
        _bindingSubscription?.Dispose();
        base.Dispose();
    }
    
    // Helper class for binding subscriptions
    private class BindingSubscription : IDisposable
    {
        private readonly Binding<string> _binding;
        private readonly Action _callback;
        
        public BindingSubscription(Binding<string> binding, Action callback)
        {
            _binding = binding;
            _callback = callback;
            _binding.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object? sender, EventArgs e)
        {
            _callback();
        }
        
        public void Dispose()
        {
            _binding.PropertyChanged -= OnPropertyChanged;
        }
    }
}