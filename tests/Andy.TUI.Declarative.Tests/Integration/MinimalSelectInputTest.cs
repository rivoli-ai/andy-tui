using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Minimal test to isolate and debug SelectInput rendering issues.
/// </summary>
public class MinimalSelectInputTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public MinimalSelectInputTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void MinimalSelectInput_ShouldRenderCorrectly()
    {
        using (BeginScenario("Minimal SelectInput Test"))
        {
            // Use real terminal to capture actual output
            var terminal = new DebugTerminal(80, 20);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "Item1", "Item2", "Item3" };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new SelectInput<string>(
                    items,
                    new Binding<Optional<string>>(
                        () => selected,
                        v => selected = v,
                        "Selected"
                    ),
                    item => item,
                    visibleItems: 3,
                    placeholder: "Choose..."
                );
            }

            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try { renderer.Run(BuildUI); }
                catch (Exception ex) { Logger.Error(ex, "Renderer error"); }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(100);
            
            // Check initial unfocused state
            var unfocused = terminal.GetScreen();
            _output.WriteLine("=== Unfocused State ===");
            _output.WriteLine(unfocused);
            Assert.Contains("[ Choose... ]", unfocused);
            
            // Tab to focus
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var focused = terminal.GetScreen();
            _output.WriteLine("\n=== Focused State ===");
            _output.WriteLine(focused);
            
            // Should show dropdown box with items
            Assert.Contains("┌", focused);
            Assert.Contains("│", focused);
            Assert.Contains("└", focused);
            Assert.Contains("Item1", focused);
            Assert.Contains("Item2", focused);
            Assert.Contains("Item3", focused);
            
            // Navigate down
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterNav = terminal.GetScreen();
            _output.WriteLine("\n=== After Navigation ===");
            _output.WriteLine(afterNav);
            
            // Should still have all items visible
            Assert.Contains("Item1", afterNav);
            Assert.Contains("Item2", afterNav);
            Assert.Contains("Item3", afterNav);
            
            // Check that we don't have overlapping renders
            var lines = afterNav.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Item"))
                {
                    _output.WriteLine($"Item line: '{line}'");
                    // Each item should appear cleanly on its own line
                    var itemCount = 0;
                    if (line.Contains("Item1")) itemCount++;
                    if (line.Contains("Item2")) itemCount++;
                    if (line.Contains("Item3")) itemCount++;
                    Assert.True(itemCount <= 1, $"Multiple items on same line: {line}");
                }
            }
            
            input.Stop();
        }
    }
}

/// <summary>
/// Debug terminal that captures all rendering for analysis.
/// </summary>
public class DebugTerminal : ITerminal
{
    private readonly char[,] _buffer;
    private readonly ConsoleColor[,] _foreground;
    private readonly ConsoleColor[,] _background;
    private int _cursorX, _cursorY;
    private readonly int _width, _height;

    public DebugTerminal(int width, int height)
    {
        _width = width;
        _height = height;
        _buffer = new char[height, width];
        _foreground = new ConsoleColor[height, width];
        _background = new ConsoleColor[height, width];
        Clear();
    }

    public int Width => _width;
    public int Height => _height;
    public bool CursorVisible { get; set; }
    public bool SupportsColor => true;
    public bool SupportsAnsi => false;

    public (int Column, int Row) CursorPosition
    {
        get => (_cursorX, _cursorY);
        set
        {
            _cursorX = Math.Max(0, Math.Min(_width - 1, value.Column));
            _cursorY = Math.Max(0, Math.Min(_height - 1, value.Row));
        }
    }

    public void Clear()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _buffer[y, x] = ' ';
                _foreground[y, x] = ConsoleColor.Gray;
                _background[y, x] = ConsoleColor.Black;
            }
        }
        _cursorX = _cursorY = 0;
    }

    public void ClearLine()
    {
        for (int x = _cursorX; x < _width; x++)
        {
            _buffer[_cursorY, x] = ' ';
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
            if (_cursorX < _width && _cursorY < _height)
            {
                _buffer[_cursorY, _cursorX] = c;
                _cursorX++;
                if (_cursorX >= _width)
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
        _cursorX = 0;
        _cursorY++;
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        // Track color changes
    }

    public void SetBackgroundColor(ConsoleColor color)
    {
        // Track color changes
    }

    public void ResetColors()
    {
        // Reset to defaults
    }

    public void SaveCursorPosition() { }
    public void RestoreCursorPosition() { }
    public void EnterAlternateScreen() { }
    public void ExitAlternateScreen() { }
    public void Flush() { }

    public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged
    {
        add { }
        remove { }
    }

    public string GetScreen()
    {
        var result = "";
        for (int y = 0; y < _height; y++)
        {
            var line = "";
            for (int x = 0; x < _width; x++)
            {
                line += _buffer[y, x];
            }
            result += line.TrimEnd() + "\n";
        }
        return result.TrimEnd();
    }

    public void Dispose() { }
}