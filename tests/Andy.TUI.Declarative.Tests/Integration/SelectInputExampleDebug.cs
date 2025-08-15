using System;
using System.Collections.Generic;
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
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Debug test for SelectInput Example #11 to capture actual behavior
/// </summary>
public class SelectInputExampleDebug : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputExampleDebug(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void Debug_SelectInputExample11_CaptureActualBehavior()
    {
        using (BeginScenario("SelectInput Example #11 Debug"))
        {
            var recorder = new ScreenRecorder("SelectInput_Example11_Debug", 120, 40);
            var terminal = new MockTerminal(120, 40);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            // Recreate Example 11 setup
            var countries = new List<Country>
            {
                new Country { Code = "US", Name = "United States", Population = 331_000_000 },
                new Country { Code = "CN", Name = "China", Population = 1_412_000_000 },
                new Country { Code = "IN", Name = "India", Population = 1_380_000_000 },
                new Country { Code = "ID", Name = "Indonesia", Population = 273_000_000 },
                new Country { Code = "PK", Name = "Pakistan", Population = 225_000_000 },
                new Country { Code = "BR", Name = "Brazil", Population = 213_000_000 },
                new Country { Code = "NG", Name = "Nigeria", Population = 211_000_000 },
                new Country { Code = "BD", Name = "Bangladesh", Population = 169_000_000 }
            };

            var colors = new[] { "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "White", "Black" };
            var fruits = new[] { "Apple", "Banana", "Orange", "Grape", "Mango", "Pear" };

            var selectedCountry = Optional<Country>.None;
            var selectedColor = Optional<string>.None;
            var selectedFruit = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1) {
                    new Text("SelectInput Component Demo").Bold().Color(Color.Cyan),
                    new Newline(),
                    
                    // Country Selection
                    new Text("1. Country Selection (custom renderer):").Bold().Color(Color.Yellow),
                    new SelectInput<Country>(
                        countries,
                        new Binding<Optional<Country>>(
                            () => selectedCountry,
                            v => selectedCountry = v,
                            "SelectedCountry"
                        ),
                        country => $"{country.Code} - {country.Name}",
                        visibleItems: 5,
                        placeholder: "Choose a country..."
                    ),
                    selectedCountry.TryGetValue(out var country)
                        ? new Text($"   Population: {country.Population:N0}").Color(Color.Green)
                        : new Text("   No country selected").Color(Color.DarkGray),

                    new Newline(),
                    
                    // Color Selection
                    new Text("2. Color Selection (simple list):").Bold().Color(Color.Yellow),
                    new SelectInput<string>(
                        colors,
                        new Binding<Optional<string>>(
                            () => selectedColor,
                            v => selectedColor = v,
                            "SelectedColor"
                        ),
                        visibleItems: 4
                    ).Placeholder("Pick a color..."),
                    selectedColor.TryGetValue(out var color)
                        ? new Text($"   Selected: {color}").Color(ParseColor(color))
                        : new Text("   No color selected").Color(Color.DarkGray),

                    new Newline(),
                    
                    // Fruit Selection
                    new Text("3. Fruit Selection (without indicator):").Bold().Color(Color.Yellow),
                    new SelectInput<string>(
                        fruits,
                        new Binding<Optional<string>>(
                            () => selectedFruit,
                            v => selectedFruit = v,
                            "SelectedFruit"
                        ),
                        visibleItems: 3
                    ).HideIndicator(),
                    selectedFruit.TryGetValue(out var fruit)
                        ? new Text($"   You selected: {fruit}").Color(Color.Green)
                        : new Text("   No fruit selected").Color(Color.DarkGray),

                    new Newline(),
                    
                    // Navigation help
                    new Box {
                        new VStack(spacing: 0) {
                            new Text("Navigation:").Bold(),
                            new Text("• Tab: Switch between inputs"),
                            new Text("• ↑/↓: Navigate items"),
                            new Text("• Enter/Space: Select item"),
                            new Text("• Ctrl+C: Exit")
                        }
                    }.WithPadding(1)
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
            
            // Record initial state
            recorder.RecordFrame(terminal, "Initial");
            _output.WriteLine($"Initial state - Lines with content: {CountLinesWithContent(terminal)}");
            LogVisibleContent(terminal, "Initial");
            
            // Press down arrow without focusing
            _output.WriteLine("\n=== Pressing DownArrow without focus ===");
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After DownArrow (no focus)");
            _output.WriteLine($"After DownArrow - Lines with content: {CountLinesWithContent(terminal)}");
            LogVisibleContent(terminal, "After DownArrow (no focus)");
            
            // Tab to focus first SelectInput
            _output.WriteLine("\n=== Tabbing to first SelectInput ===");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab - Focus First SelectInput");
            _output.WriteLine($"After Tab - Lines with content: {CountLinesWithContent(terminal)}");
            LogVisibleContent(terminal, "After Tab");
            
            // Navigate down a few times
            for (int i = 1; i <= 3; i++)
            {
                _output.WriteLine($"\n=== DownArrow {i} ===");
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                recorder.RecordFrame(terminal, $"After DownArrow {i}");
                _output.WriteLine($"After DownArrow {i} - Lines with content: {CountLinesWithContent(terminal)}");
                LogVisibleContent(terminal, $"DownArrow {i}");
                
                // Check for gray/invisible issue
                CheckForGrayIssue(terminal, i);
            }
            
            // Tab to next SelectInput
            _output.WriteLine("\n=== Tabbing to second SelectInput ===");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab - Focus Second SelectInput");
            _output.WriteLine($"After Tab to second - Lines with content: {CountLinesWithContent(terminal)}");
            LogVisibleContent(terminal, "Tab to second");
            
            // Save recording
            var recordingPath = recorder.SaveToFile();
            _output.WriteLine($"\n📹 Recording saved to: {recordingPath}");
            
            // Analyze recording
            var report = recorder.Analyze();
            _output.WriteLine($"\n📊 Analysis Report:");
            _output.WriteLine($"  Total frames: {report.TotalFrames}");
            _output.WriteLine($"  Issues found: {report.Issues.Count}");
            foreach (var issue in report.Issues)
            {
                _output.WriteLine($"  - {issue.Type}: {issue.Description}");
            }
            
            input.Stop();
        }
    }

    private int CountLinesWithContent(MockTerminal terminal)
    {
        int count = 0;
        for (int y = 0; y < terminal.Height; y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrWhiteSpace(line))
                count++;
        }
        return count;
    }

    private void LogVisibleContent(MockTerminal terminal, string context)
    {
        _output.WriteLine($"=== Visible content at {context} ===");
        for (int y = 0; y < Math.Min(30, terminal.Height); y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrWhiteSpace(line))
            {
                _output.WriteLine($"  [{y:D2}]: {line}");
            }
        }
    }

    private void CheckForGrayIssue(MockTerminal terminal, int frame)
    {
        // Check if everything has become gray (DarkGray foreground)
        int grayCount = 0;
        int totalNonEmpty = 0;
        
        for (int y = 0; y < terminal.Height; y++)
        {
            for (int x = 0; x < terminal.Width; x++)
            {
                var (ch, fg, bg) = terminal.GetCharAt(x, y);
                if (ch != ' ' && ch != '\0')
                {
                    totalNonEmpty++;
                    if (fg == ConsoleColor.DarkGray || fg == ConsoleColor.Gray)
                    {
                        grayCount++;
                    }
                }
            }
        }
        
        if (totalNonEmpty > 0)
        {
            var grayPercentage = (grayCount * 100) / totalNonEmpty;
            if (grayPercentage > 80)
            {
                _output.WriteLine($"⚠️ Frame {frame}: {grayPercentage}% of content is gray! ({grayCount}/{totalNonEmpty})");
            }
        }
    }

    private Color ParseColor(string colorName)
    {
        return colorName switch
        {
            "Red" => Color.Red,
            "Green" => Color.Green,
            "Blue" => Color.Blue,
            "Yellow" => Color.Yellow,
            "Magenta" => Color.Magenta,
            "Cyan" => Color.Cyan,
            "White" => Color.White,
            "Black" => Color.Black,
            _ => Color.Gray
        };
    }

    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Population { get; set; }
    }
}