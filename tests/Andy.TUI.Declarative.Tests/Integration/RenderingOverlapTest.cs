using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test that specifically demonstrates and verifies the rendering overlap bug.
/// This test should FAIL before the fix and PASS after.
/// </summary>
public class RenderingOverlapTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public RenderingOverlapTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void SelectInput_Should_Not_Have_Overlapping_Renders()
    {
        using (BeginScenario("Rendering Overlap Bug"))
        {
            // Use TestBufferTerminal to capture actual ANSI output
            var buffer = new TestBufferTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(buffer);
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
            
            // Initial render - should show placeholder
            var initial = buffer.GetBuffer();
            var cleanInitial = RemoveAnsiCodes(initial);
            _output.WriteLine("=== Initial (unfocused) ===");
            _output.WriteLine(cleanInitial);
            
            // Should only have the placeholder, no dropdown
            Assert.Contains("[ Choose... ]", cleanInitial);
            Assert.DoesNotContain("┌", cleanInitial);
            Assert.DoesNotContain("Item1", cleanInitial);
            
            // Clear buffer to track what gets rendered next
            buffer.ClearBuffer();
            
            // Tab to focus - should show dropdown
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var afterTab = buffer.GetBuffer();
            var cleanAfterTab = RemoveAnsiCodes(afterTab);
            _output.WriteLine("\n=== After Tab (focused) ===");
            _output.WriteLine(cleanAfterTab);
            
            // Should NOT have the old placeholder visible
            Assert.DoesNotContain("[ Choose... ]", cleanAfterTab);
            
            // Should have the dropdown
            Assert.Contains("┌", cleanAfterTab);
            Assert.Contains("Item1", cleanAfterTab);
            Assert.Contains("Item2", cleanAfterTab);
            Assert.Contains("Item3", cleanAfterTab);
            
            // Count how many times Item1 appears (should be exactly 1)
            var item1Count = CountOccurrences(cleanAfterTab, "Item1");
            Assert.Equal(1, item1Count);
            
            // Clear buffer again
            buffer.ClearBuffer();
            
            // Navigate down - should move highlight
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterDown = buffer.GetBuffer();
            var cleanAfterDown = RemoveAnsiCodes(afterDown);
            _output.WriteLine("\n=== After DownArrow ===");
            _output.WriteLine(cleanAfterDown);
            
            // Should still have exactly one of each item
            var item1CountAfter = CountOccurrences(cleanAfterDown, "Item1");
            var item2CountAfter = CountOccurrences(cleanAfterDown, "Item2");
            var item3CountAfter = CountOccurrences(cleanAfterDown, "Item3");
            
            _output.WriteLine($"Item counts: Item1={item1CountAfter}, Item2={item2CountAfter}, Item3={item3CountAfter}");
            
            // Each item should appear exactly once
            Assert.Equal(1, item1CountAfter);
            Assert.Equal(1, item2CountAfter);
            Assert.Equal(1, item3CountAfter);
            
            // Check for duplicate borders (sign of overlapping renders)
            var topBorderCount = CountOccurrences(cleanAfterDown, "┌");
            var bottomBorderCount = CountOccurrences(cleanAfterDown, "└");
            
            _output.WriteLine($"Border counts: Top={topBorderCount}, Bottom={bottomBorderCount}");
            
            Assert.Equal(1, topBorderCount);
            Assert.Equal(1, bottomBorderCount);
            
            input.Stop();
        }
    }

    [Fact]
    public void Dynamic_Content_Should_Clear_Previous_Render()
    {
        using (BeginScenario("Dynamic Content Clear"))
        {
            var buffer = new TestBufferTerminal(80, 10);
            using var renderingSystem = new RenderingSystem(buffer);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var showLong = true;

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    showLong 
                        ? new Text("This is a very long text that takes up space")
                        : new Text("Short"),
                    new Button("Toggle", () => { showLong = !showLong; renderer.RequestRender(); })
                };
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
            
            // Initial render with long text
            var initial = buffer.GetBuffer();
            var cleanInitial = RemoveAnsiCodes(initial);
            _output.WriteLine("=== Initial (long text) ===");
            _output.WriteLine(cleanInitial);
            
            Assert.Contains("This is a very long text", cleanInitial);
            
            // Clear buffer
            buffer.ClearBuffer();
            
            // Toggle to short text
            showLong = false;
            renderer.RequestRender();
            Thread.Sleep(100);
            
            var afterToggle = buffer.GetBuffer();
            var cleanAfterToggle = RemoveAnsiCodes(afterToggle);
            _output.WriteLine("\n=== After Toggle (short text) ===");
            _output.WriteLine(cleanAfterToggle);
            
            // Should have short text
            Assert.Contains("Short", cleanAfterToggle);
            
            // Should NOT have remnants of long text
            Assert.DoesNotContain("very long", cleanAfterToggle);
            Assert.DoesNotContain("takes up space", cleanAfterToggle);
            
            input.Stop();
        }
    }

    private string RemoveAnsiCodes(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[^@-~]*[@-~]", "");
    }

    private int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}

/// <summary>
/// Enhanced TestBufferTerminal that can clear its buffer for testing.
/// </summary>
public class TestBufferTerminal : ITerminal
{
    private System.IO.StringWriter _buffer = new System.IO.StringWriter();
    private int _cursorX, _cursorY;
    private readonly int _width, _height;

    public TestBufferTerminal(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public int Width => _width;
    public int Height => _height;
    public bool CursorVisible { get; set; }

    public void Clear()
    {
        _buffer.Write("\x1b[2J");
        _cursorX = _cursorY = 0;
    }

    public void SetCursorPosition(int x, int y)
    {
        _cursorX = x;
        _cursorY = y;
        _buffer.Write($"\x1b[{y + 1};{x + 1}H");
    }

    public void Write(string text)
    {
        _buffer.Write(text);
        _cursorX += text.Length;
    }

    public void WriteLine(string text)
    {
        _buffer.WriteLine(text);
        _cursorX = 0;
        _cursorY++;
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        _buffer.Write(GetAnsiColorCode(color, true));
    }

    public void SetBackgroundColor(ConsoleColor color)
    {
        _buffer.Write(GetAnsiColorCode(color, false));
    }

    public void ResetColors()
    {
        _buffer.Write("\x1b[0m");
    }

    public void HideCursor()
    {
        CursorVisible = false;
        _buffer.Write("\x1b[?25l");
    }

    public void ShowCursor()
    {
        CursorVisible = true;
        _buffer.Write("\x1b[?25h");
    }

    public void EnterAlternateScreen()
    {
        _buffer.Write("\x1b[?1049h");
    }

    public void ExitAlternateScreen()
    {
        _buffer.Write("\x1b[?1049l");
    }

    public void ClearLine()
    {
        _buffer.Write("\x1b[2K");
    }

    public void MoveCursor(int column, int row)
    {
        SetCursorPosition(column, row);
    }

    public void SaveCursorPosition()
    {
        _buffer.Write("\x1b[s");
    }

    public void RestoreCursorPosition()
    {
        _buffer.Write("\x1b[u");
    }

    public void Flush()
    {
        _buffer.Flush();
    }

    public (int Column, int Row) CursorPosition
    {
        get => (_cursorX, _cursorY);
        set
        {
            _cursorX = value.Column;
            _cursorY = value.Row;
            SetCursorPosition(_cursorX, _cursorY);
        }
    }

    public bool SupportsColor => true;
    public bool SupportsAnsi => true;

    public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged
    {
        add { }
        remove { }
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }

    public string GetBuffer()
    {
        return _buffer.ToString();
    }

    public void ClearBuffer()
    {
        _buffer = new System.IO.StringWriter();
    }

    private string GetAnsiColorCode(ConsoleColor color, bool foreground)
    {
        var baseCode = foreground ? 30 : 40;
        return color switch
        {
            ConsoleColor.Black => $"\x1b[{baseCode}m",
            ConsoleColor.DarkRed => $"\x1b[{baseCode + 1}m",
            ConsoleColor.DarkGreen => $"\x1b[{baseCode + 2}m",
            ConsoleColor.DarkYellow => $"\x1b[{baseCode + 3}m",
            ConsoleColor.DarkBlue => $"\x1b[{baseCode + 4}m",
            ConsoleColor.DarkMagenta => $"\x1b[{baseCode + 5}m",
            ConsoleColor.DarkCyan => $"\x1b[{baseCode + 6}m",
            ConsoleColor.Gray => $"\x1b[{baseCode + 7}m",
            ConsoleColor.DarkGray => $"\x1b[{baseCode + 60}m",
            ConsoleColor.Red => $"\x1b[{baseCode + 61}m",
            ConsoleColor.Green => $"\x1b[{baseCode + 62}m",
            ConsoleColor.Yellow => $"\x1b[{baseCode + 63}m",
            ConsoleColor.Blue => $"\x1b[{baseCode + 64}m",
            ConsoleColor.Magenta => $"\x1b[{baseCode + 65}m",
            ConsoleColor.Cyan => $"\x1b[{baseCode + 66}m",
            ConsoleColor.White => $"\x1b[{baseCode + 67}m",
            _ => $"\x1b[{baseCode + 7}m"
        };
    }
}