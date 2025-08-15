using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
/// Tests using real terminal rendering, not mock terminal.
/// This captures what actually gets rendered.
/// </summary>
public class RealRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public RealRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void TestRealSelectInputRendering()
    {
        using (BeginScenario("Real SelectInput Rendering"))
        {
            // Use a StringBufferTerminal that captures actual ANSI output
            var buffer = new StringBufferTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(buffer);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "Red", "Green", "Blue", "Yellow", "Purple" };
            var selectedItem = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    new Text("SelectInput Test").Color(Color.Cyan),
                    new SelectInput<string>(
                        items,
                        new Binding<Optional<string>>(
                            () => selectedItem,
                            v => selectedItem = v,
                            "SelectedItem"
                        ),
                        item => item,
                        visibleItems: 5,
                        placeholder: "Choose a color..."
                    ),
                    selectedItem.TryGetValue(out var item)
                        ? new Text($"Selected: {item}").Color(Color.Green)
                        : new Text("No color selected").Color(Color.DarkGray)
                };
            }

            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try
                {
                    renderer.Run(BuildUI);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread error");
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            
            // Capture initial render
            var initial = buffer.GetBuffer();
            _output.WriteLine("=== Initial Render ===");
            _output.WriteLine(RemoveAnsiCodes(initial));
            
            // Check what's visible
            Assert.Contains("SelectInput Test", initial);
            
            // Tab to focus
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var afterTab = buffer.GetBuffer();
            _output.WriteLine("\n=== After Tab (Focused) ===");
            _output.WriteLine(RemoveAnsiCodes(afterTab));
            
            // Should show dropdown
            Assert.Contains("┌", afterTab);
            Assert.Contains("Red", afterTab);
            
            // Navigate down
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterDown = buffer.GetBuffer();
            _output.WriteLine("\n=== After DownArrow ===");
            _output.WriteLine(RemoveAnsiCodes(afterDown));
            
            // Check for issues
            var issues = AnalyzeBuffer(afterDown);
            foreach (var issue in issues)
            {
                _output.WriteLine($"Issue: {issue}");
            }
            
            Assert.Empty(issues);
            
            input.Stop();
        }
    }

    [Fact]
    public void TestComplexExample11Rendering()
    {
        using (BeginScenario("Complex Example 11 Rendering"))
        {
            var buffer = new StringBufferTerminal(120, 40);
            using var renderingSystem = new RenderingSystem(buffer);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            // Recreate Example 11
            var countries = new List<Country>
            {
                new Country { Code = "US", Name = "United States" },
                new Country { Code = "CN", Name = "China" },
                new Country { Code = "IN", Name = "India" }
            };

            var colors = new[] { "Red", "Green", "Blue" };
            var fruits = new[] { "Apple", "Banana", "Orange" };

            var selectedCountry = Optional<Country>.None;
            var selectedColor = Optional<string>.None;
            var selectedFruit = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    new Text("SelectInput Component Demo").Bold().Color(Color.Cyan),
                    new Newline(),
                    
                    new Text("1. Country Selection:").Bold().Color(Color.Yellow),
                    new SelectInput<Country>(
                        countries,
                        new Binding<Optional<Country>>(
                            () => selectedCountry,
                            v => selectedCountry = v,
                            "SelectedCountry"
                        ),
                        country => $"{country.Code} - {country.Name}",
                        visibleItems: 3,
                        placeholder: "Choose a country..."
                    ),
                    
                    new Newline(),
                    
                    new Text("2. Color Selection:").Bold().Color(Color.Yellow),
                    new SelectInput<string>(
                        colors,
                        new Binding<Optional<string>>(
                            () => selectedColor,
                            v => selectedColor = v,
                            "SelectedColor"
                        ),
                        visibleItems: 3
                    ).Placeholder("Pick a color..."),
                    
                    new Newline(),
                    
                    new Text("3. Fruit Selection:").Bold().Color(Color.Yellow),
                    new SelectInput<string>(
                        fruits,
                        new Binding<Optional<string>>(
                            () => selectedFruit,
                            v => selectedFruit = v,
                            "SelectedFruit"
                        ),
                        visibleItems: 3
                    ).HideIndicator()
                };
            }

            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try
                {
                    renderer.Run(BuildUI);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread error");
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            
            // Initial state
            var initial = buffer.GetBuffer();
            _output.WriteLine("=== Initial State ===");
            var cleanInitial = RemoveAnsiCodes(initial);
            _output.WriteLine(cleanInitial);
            
            // Check visibility
            if (!cleanInitial.Contains("SelectInput Component Demo"))
            {
                _output.WriteLine("ERROR: Title not visible!");
            }
            
            // Down arrow without focus
            _output.WriteLine("\n=== Pressing DownArrow without focus ===");
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterDown = buffer.GetBuffer();
            _output.WriteLine(RemoveAnsiCodes(afterDown));
            
            // Tab to first
            _output.WriteLine("\n=== Tab to first SelectInput ===");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var afterTab = buffer.GetBuffer();
            _output.WriteLine(RemoveAnsiCodes(afterTab));
            
            // Navigate
            for (int i = 1; i <= 3; i++)
            {
                _output.WriteLine($"\n=== DownArrow {i} ===");
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                
                var afterNav = buffer.GetBuffer();
                var clean = RemoveAnsiCodes(afterNav);
                _output.WriteLine(clean);
                
                // Check for gray issue
                if (IsEverythingGray(afterNav))
                {
                    _output.WriteLine("ERROR: Everything turned gray!");
                    Assert.Fail("Everything turned gray after navigation");
                }
            }
            
            // Tab to next
            _output.WriteLine("\n=== Tab to second SelectInput ===");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var afterTab2 = buffer.GetBuffer();
            _output.WriteLine(RemoveAnsiCodes(afterTab2));
            
            if (IsGarbage(afterTab2))
            {
                _output.WriteLine("ERROR: Garbage rendering detected!");
                Assert.Fail("Garbage rendering after tab to second SelectInput");
            }
            
            input.Stop();
        }
    }

    private List<string> AnalyzeBuffer(string buffer)
    {
        var issues = new List<string>();
        
        // Check for multiple highlights (multiple inverse video sections)
        var inverseCount = CountOccurrences(buffer, "\x1b[7m");
        if (inverseCount > 1)
        {
            issues.Add($"Multiple highlights detected: {inverseCount} inverse video sections");
        }
        
        // Check for partial backgrounds
        var lines = buffer.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("\x1b[47m") && !line.EndsWith("\x1b[0m"))
            {
                issues.Add("Partial background detected - background doesn't span full line");
            }
        }
        
        return issues;
    }

    private bool IsEverythingGray(string buffer)
    {
        // Check if most text is rendered in gray/dark gray
        var grayCount = CountOccurrences(buffer, "\x1b[90m") + CountOccurrences(buffer, "\x1b[37m");
        var normalCount = CountOccurrences(buffer, "\x1b[97m") + CountOccurrences(buffer, "\x1b[39m");
        
        return grayCount > normalCount * 3;
    }

    private bool IsGarbage(string buffer)
    {
        // Check for rendering artifacts
        var clean = RemoveAnsiCodes(buffer);
        
        // Check for broken box drawing
        if (clean.Contains("���") || clean.Contains("???"))
            return true;
            
        // Check for overlapping text
        var lines = clean.Split('\n');
        foreach (var line in lines)
        {
            if (line.Length > 120) // Line overflow
                return true;
        }
        
        return false;
    }

    private string RemoveAnsiCodes(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[^@-~]*[@-~]", "");
    }

    private int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }
}

/// <summary>
/// A terminal that captures output in a string buffer for testing.
/// This gives us the actual ANSI sequences that would be sent to a real terminal.
/// </summary>
public class StringBufferTerminal : ITerminal
{
    private readonly StringWriter _buffer = new StringWriter();
    private int _cursorX, _cursorY;
    private readonly int _width, _height;
    private ConsoleColor _currentFg = ConsoleColor.Gray;
    private ConsoleColor _currentBg = ConsoleColor.Black;

    public StringBufferTerminal(int width, int height)
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
        if (color != _currentFg)
        {
            _currentFg = color;
            _buffer.Write(GetAnsiColorCode(color, true));
        }
    }

    public void SetBackgroundColor(ConsoleColor color)
    {
        if (color != _currentBg)
        {
            _currentBg = color;
            _buffer.Write(GetAnsiColorCode(color, false));
        }
    }

    public void ResetColors()
    {
        _buffer.Write("\x1b[0m");
        _currentFg = ConsoleColor.Gray;
        _currentBg = ConsoleColor.Black;
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
        // No-op for string buffer
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