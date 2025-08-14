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
/// Test to ensure navigation in SelectInput doesn't cause mixed/partial rendering.
/// Specifically tests for issues where multiple items appear selected or
/// backgrounds are partially rendered.
/// </summary>
public class SelectInputNavigationRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputNavigationRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void SelectInput_NavigationShouldNotCauseMixedBackgrounds()
    {
        using (BeginScenario("SelectInput Navigation No Mixed Backgrounds"))
        {
            LogStep("Setting up SelectInput for navigation test");
            
            var terminal = new MockTerminal(60, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { 
                "First Option", 
                "Second Choice", 
                "Third Alternative", 
                "Fourth Selection", 
                "Fifth Pick",
                "Sixth Item",
                "Seventh Entry"
            };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Navigation Test").Bold().Color(Color.Cyan),
                        new SelectInput<string>(
                            items,
                            new Binding<Optional<string>>(
                                () => selected,
                                v => selected = v,
                                "Selected"
                            ),
                            visibleItems: 5,
                            placeholder: "Pick an option..."
                        )
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
            
            // Verify initial state
            var initialBuffer = GetDetailedBuffer(terminal);
            LogDetailedBuffer("Initial", initialBuffer);
            
            // First item should be highlighted
            var firstItemLine = FindLineWithItem(initialBuffer, "First Option");
            Assert.NotNull(firstItemLine);
            Assert.Contains("▶", firstItemLine.Value.content);
            LogAssertion($"First item highlighted at line {firstItemLine.Value.lineNumber}");
            
            // Navigate down and check each step
            for (int navStep = 0; navStep < 5; navStep++)
            {
                LogStep($"Navigate down step {navStep + 1}");
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                
                var buffer = GetDetailedBuffer(terminal);
                LogDetailedBuffer($"After Down {navStep + 1}", buffer);
                
                // Verify exactly one item has the highlight indicator
                int highlightCount = 0;
                int expectedHighlightIndex = navStep + 1;
                string expectedItem = items[expectedHighlightIndex];
                
                for (int i = 0; i < buffer.lines.Length; i++)
                {
                    var line = buffer.lines[i];
                    if (line.Contains("▶"))
                    {
                        highlightCount++;
                        LogAssertion($"Found highlight at line {i}: [{line}]");
                        
                        // Verify it's the correct item
                        Assert.Contains(expectedItem, line);
                    }
                    
                    // Check no partial backgrounds (looking for mixed styles)
                    if (line.Contains("Option") || line.Contains("Choice") || 
                        line.Contains("Alternative") || line.Contains("Selection") ||
                        line.Contains("Pick") || line.Contains("Item") || line.Contains("Entry"))
                    {
                        // Count how many items appear on this line
                        int itemsOnLine = 0;
                        foreach (var item in items)
                        {
                            if (line.Contains(item)) itemsOnLine++;
                        }
                        
                        if (itemsOnLine > 1)
                        {
                            Logger.Error($"Multiple items on same line at {i}: {line}");
                            Assert.Fail($"Multiple items rendered on same line: {line}");
                        }
                    }
                }
                
                LogAssertion($"Highlight count should be 1, was {highlightCount}");
                Assert.Equal(1, highlightCount);
            }
            
            // Navigate back up
            LogStep("Navigate back up");
            for (int navStep = 0; navStep < 3; navStep++)
            {
                input.EmitKey('\0', ConsoleKey.UpArrow);
                Thread.Sleep(100);
                
                var buffer = GetDetailedBuffer(terminal);
                LogDetailedBuffer($"After Up {navStep + 1}", buffer);
                
                // Verify exactly one highlight
                int highlightCount = 0;
                for (int i = 0; i < buffer.lines.Length; i++)
                {
                    if (buffer.lines[i].Contains("▶"))
                    {
                        highlightCount++;
                    }
                }
                
                Assert.Equal(1, highlightCount);
            }
            
            input.Stop();
        }
    }

    [Fact]
    public void SelectInput_RapidNavigationShouldNotCorrupt()
    {
        using (BeginScenario("SelectInput Rapid Navigation No Corruption"))
        {
            LogStep("Setting up SelectInput for rapid navigation");
            
            var terminal = new MockTerminal(60, 25);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
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
                        visibleItems: 6,
                        placeholder: "..."
                    )
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
            
            LogStep("Focus and rapidly navigate");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            // Rapid navigation without waiting
            for (int i = 0; i < 7; i++)
            {
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(20); // Very short delay
            }
            
            // Wait for rendering to catch up
            Thread.Sleep(200);
            
            var finalBuffer = GetDetailedBuffer(terminal);
            LogDetailedBuffer("After Rapid Navigation", finalBuffer);
            
            // Should have exactly one highlighted item
            int highlightCount = 0;
            string? highlightedItem = null;
            
            for (int i = 0; i < finalBuffer.lines.Length; i++)
            {
                var line = finalBuffer.lines[i];
                if (line.Contains("▶"))
                {
                    highlightCount++;
                    highlightedItem = line;
                    _output.WriteLine($"Highlighted line {i}: [{line}]");
                }
            }
            
            LogAssertion($"Should have exactly 1 highlight after rapid nav, found {highlightCount}");
            Assert.Equal(1, highlightCount);
            Assert.NotNull(highlightedItem);
            Assert.Contains("H", highlightedItem); // Should be on H (7 downs from A)
            
            input.Stop();
        }
    }

    private (string[] lines, int lineCount) GetDetailedBuffer(MockTerminal terminal)
    {
        var lines = new string[terminal.Height];
        for (int y = 0; y < terminal.Height; y++)
        {
            lines[y] = terminal.GetLine(y).TrimEnd();
        }
        return (lines, terminal.Height);
    }

    private (int lineNumber, string content)? FindLineWithItem(
        (string[] lines, int lineCount) buffer, 
        string itemText)
    {
        for (int i = 0; i < buffer.lineCount; i++)
        {
            if (buffer.lines[i].Contains(itemText))
            {
                return (i, buffer.lines[i]);
            }
        }
        return null;
    }

    private void LogDetailedBuffer(string label, (string[] lines, int lineCount) buffer)
    {
        _output.WriteLine($"\n=== {label} Buffer ===");
        for (int i = 0; i < Math.Min(30, buffer.lineCount); i++)
        {
            if (!string.IsNullOrWhiteSpace(buffer.lines[i]))
            {
                _output.WriteLine($"  {i:D2}: [{buffer.lines[i]}]");
            }
        }
        _output.WriteLine("=================\n");
    }
}