using System;
using System.Collections.Generic;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

public class MockTerminal : ITerminal
{
    private readonly char[,] _buffer;
    private readonly ConsoleColor[,] _foregroundColors;
    private readonly ConsoleColor[,] _backgroundColors;
    private int _cursorX;
    private int _cursorY;
    private int _savedCursorX;
    private int _savedCursorY;
    private bool _cursorVisible = true;
    private ConsoleColor _currentForeground = ConsoleColor.White;
    private ConsoleColor _currentBackground = ConsoleColor.Black;
    
    public MockTerminal(int width, int height)
    {
        Width = width;
        Height = height;
        _buffer = new char[height, width];
        _foregroundColors = new ConsoleColor[height, width];
        _backgroundColors = new ConsoleColor[height, width];
        Clear();
    }
    
    public int Width { get; }
    public int Height { get; }
    public bool CursorVisible 
    { 
        get => _cursorVisible; 
        set => _cursorVisible = value; 
    }
    
    public (int Column, int Row) CursorPosition
    {
        get => (_cursorX, _cursorY);
        set
        {
            _cursorX = Math.Max(0, Math.Min(value.Column, Width - 1));
            _cursorY = Math.Max(0, Math.Min(value.Row, Height - 1));
        }
    }
    
    public bool SupportsColor => true;
    public bool SupportsAnsi => true;
    
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
#pragma warning restore CS0067
    
    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _buffer[y, x] = ' ';
                _foregroundColors[y, x] = ConsoleColor.White;
                _backgroundColors[y, x] = ConsoleColor.Black;
            }
        }
        _cursorX = 0;
        _cursorY = 0;
    }
    
    public void ClearLine()
    {
        if (_cursorY < Height)
        {
            for (int x = _cursorX; x < Width; x++)
            {
                _buffer[_cursorY, x] = ' ';
                _foregroundColors[_cursorY, x] = _currentForeground;
                _backgroundColors[_cursorY, x] = _currentBackground;
            }
        }
    }
    
    public void MoveCursor(int column, int row)
    {
        CursorPosition = (column, row);
    }
    
    public void Write(string text)
    {
        foreach (char c in text)
        {
            if (c == '\n')
            {
                _cursorY++;
                _cursorX = 0;
            }
            else if (_cursorX < Width && _cursorY < Height)
            {
                _buffer[_cursorY, _cursorX] = c;
                _foregroundColors[_cursorY, _cursorX] = _currentForeground;
                _backgroundColors[_cursorY, _cursorX] = _currentBackground;
                _cursorX++;
                
                if (_cursorX >= Width)
                {
                    _cursorX = 0;
                    _cursorY++;
                }
            }
        }
    }
    
    public void WriteLine(string text)
    {
        Write(text);
        Write("\n");
    }
    
    public void SetForegroundColor(ConsoleColor color)
    {
        _currentForeground = color;
    }
    
    public void SetBackgroundColor(ConsoleColor color)
    {
        _currentBackground = color;
    }
    
    public void ResetColors()
    {
        _currentForeground = ConsoleColor.White;
        _currentBackground = ConsoleColor.Black;
    }
    
    public void SaveCursorPosition()
    {
        _savedCursorX = _cursorX;
        _savedCursorY = _cursorY;
    }
    
    public void RestoreCursorPosition()
    {
        _cursorX = _savedCursorX;
        _cursorY = _savedCursorY;
    }
    
    public void EnterAlternateScreen()
    {
        // No-op for mock terminal
    }
    
    public void ExitAlternateScreen()
    {
        // No-op for mock terminal
    }
    
    public void Flush()
    {
        // No-op for mock terminal
    }
    
    public string GetTextAt(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return _buffer[y, x].ToString();
        }
        return "";
    }
    
    public string GetLine(int y)
    {
        if (y < 0 || y >= Height) return "";
        
        var line = "";
        for (int x = 0; x < Width; x++)
        {
            line += _buffer[y, x];
        }
        return line.TrimEnd();
    }
    
    public string[] GetAllLines()
    {
        var lines = new string[Height];
        for (int y = 0; y < Height; y++)
        {
            lines[y] = GetLine(y);
        }
        return lines;
    }
}