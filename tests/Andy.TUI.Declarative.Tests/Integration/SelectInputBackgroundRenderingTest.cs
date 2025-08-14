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
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test to reproduce and fix the issue where SelectInput dropdown shows
/// partial backgrounds and mixed selections when navigating with arrow keys.
/// </summary>
public class SelectInputBackgroundRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputBackgroundRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void SelectInput_HighlightedItemShouldHaveFullBackgroundColor()
    {
        using (BeginScenario("SelectInput Highlighted Item Full Background"))
        {
            LogStep("Setting up SelectInput with multiple items");
            
            var terminal = new MockTerminal(80, 25);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { 
                "First Item", 
                "Second Item", 
                "Third Item", 
                "Fourth Item", 
                "Fifth Item" 
            };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("SelectInput Background Test").Bold(),
                        new SelectInput<string>(
                            items,
                            new Binding<Optional<string>>(
                                () => selected,
                                v => selected = v,
                                "Selected"
                            ),
                            visibleItems: 5
                        ).Placeholder("Select an item...")
                    }
                }.WithPadding(new Andy.TUI.Layout.Spacing(2, 2, 2, 2));
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
            
            LogStep("Focus the SelectInput");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial Focused", initialBuffer);
            
            // First item should be highlighted with full background
            LogAssertion("First item should have complete highlight background");
            var lines = initialBuffer.Split('\n');
            
            // Find the line with the first item (should have arrow indicator)
            string? firstItemLine = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("▶") && lines[i].Contains("First Item"))
                {
                    firstItemLine = lines[i];
                    _output.WriteLine($"Found first item at line {i}: [{firstItemLine}]");
                    break;
                }
            }
            
            Assert.NotNull(firstItemLine);
            
            LogStep("Navigate down to second item");
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterDownBuffer = GetBufferContent(terminal);
            LogBufferContent("After Down Arrow", afterDownBuffer);
            
            // Check that only the second item is highlighted
            lines = afterDownBuffer.Split('\n');
            
            // First item should NOT be highlighted anymore
            string? firstItemAfterDown = null;
            string? secondItemAfterDown = null;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("First Item"))
                {
                    firstItemAfterDown = lines[i];
                    _output.WriteLine($"First item after down at line {i}: [{firstItemAfterDown}]");
                    
                    // Should not have selection indicator
                    Assert.DoesNotContain("▶", firstItemAfterDown);
                }
                if (lines[i].Contains("Second Item"))
                {
                    secondItemAfterDown = lines[i];
                    _output.WriteLine($"Second item after down at line {i}: [{secondItemAfterDown}]");
                    
                    // Should have selection indicator
                    Assert.Contains("▶", secondItemAfterDown);
                }
            }
            
            LogStep("Navigate down multiple times rapidly");
            for (int i = 0; i < 3; i++)
            {
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(50);
            }
            
            var afterMultipleDownBuffer = GetBufferContent(terminal);
            LogBufferContent("After Multiple Down", afterMultipleDownBuffer);
            
            // Check that only ONE item has the selection indicator
            lines = afterMultipleDownBuffer.Split('\n');
            int highlightCount = 0;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("▶"))
                {
                    highlightCount++;
                    _output.WriteLine($"Found highlight at line {i}: [{lines[i]}]");
                }
            }
            
            LogAssertion("Only one item should be highlighted");
            Assert.Equal(1, highlightCount);
            
            // Verify no partial rendering artifacts
            LogStep("Check for rendering artifacts");
            foreach (var line in lines)
            {
                if (line.Contains("Item") && line.Length > 0)
                {
                    // Check that the line doesn't have mixed content
                    // (e.g., parts of two different items on the same line)
                    var itemCount = 0;
                    if (line.Contains("First Item")) itemCount++;
                    if (line.Contains("Second Item")) itemCount++;
                    if (line.Contains("Third Item")) itemCount++;
                    if (line.Contains("Fourth Item")) itemCount++;
                    if (line.Contains("Fifth Item")) itemCount++;
                    
                    if (itemCount > 1)
                    {
                        Logger.Error($"Line has multiple items mixed: {line}");
                        Assert.Fail($"Rendering artifact - multiple items on same line: {line}");
                    }
                }
            }
            
            input.Stop();
        }
    }

    [Fact]
    public void SelectInput_BackgroundShouldClearProperly()
    {
        using (BeginScenario("SelectInput Background Clearing"))
        {
            LogStep("Setting up SelectInput to test background clearing");
            
            var terminal = new MockTerminal(50, 20);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { 
                "Short", 
                "Medium Length", 
                "This is a very long item name", 
                "Short again", 
                "Another medium one" 
            };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new SelectInput<string>(
                        items,
                        new Binding<Optional<string>>(
                            () => selected,
                            v => selected = v,
                            "Selected"
                        ),
                        visibleItems: 4
                    ).Placeholder("Choose...")
                }.WithPadding(new Andy.TUI.Layout.Spacing(1, 1, 1, 1));
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
            
            LogStep("Focus and navigate through items");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            // Navigate down through all items
            for (int nav = 0; nav < 4; nav++)
            {
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                
                var buffer = GetBufferContent(terminal);
                LogBufferContent($"After navigation {nav + 1}", buffer);
                
                // Check that the background is properly cleared
                var lines = buffer.Split('\n');
                
                // Count how many lines have the highlight indicator
                int highlightedLines = 0;
                foreach (var line in lines)
                {
                    if (line.Contains("▶"))
                    {
                        highlightedLines++;
                        
                        // The highlighted line should have consistent formatting
                        // Check that the line after the indicator is properly formatted
                        var afterIndicator = line.Substring(line.IndexOf("▶") + 1);
                        
                        // Should have a space after the indicator
                        if (afterIndicator.Length > 0)
                        {
                            Assert.True(afterIndicator[0] == ' ', 
                                $"Expected space after indicator, got '{afterIndicator[0]}'");
                        }
                    }
                }
                
                Assert.Equal(1, highlightedLines);
            }
            
            input.Stop();
        }
    }

    private string GetBufferContent(MockTerminal terminal)
    {
        var content = "";
        for (int y = 0; y < Math.Min(25, terminal.Height); y++)
        {
            var line = terminal.GetLine(y);
            content += line.TrimEnd() + "\n";
        }
        return content;
    }

    private void LogBufferContent(string label, string content)
    {
        _output.WriteLine($"\n=== {label} Buffer ===");
        var lines = content.Split('\n');
        for (int i = 0; i < Math.Min(25, lines.Length); i++)
        {
            if (lines[i].Length > 0)
            {
                _output.WriteLine($"  {i:D2}: [{lines[i]}]");
            }
        }
        _output.WriteLine("=================\n");
    }
}