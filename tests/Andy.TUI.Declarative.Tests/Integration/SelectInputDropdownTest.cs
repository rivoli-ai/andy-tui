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
/// Test that reproduces the exact dropdown rendering issue from example 11.
/// The dropdown should only be visible when focused and opened, not always.
/// </summary>
public class SelectInputDropdownTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputDropdownTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void Dropdown_ShouldOnlyShowWhenFocused()
    {
        using (BeginScenario("Dropdown Should Only Show When Focused"))
        {
            LogStep("Setting up SelectInput");
            
            var terminal = new MockTerminal(80, 20);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Test SelectInput").Bold(),
                        new SelectInput<string>(
                            items,
                            new Binding<Optional<string>>(
                                () => selected,
                                v => selected = v,
                                "Selected"
                            ),
                            visibleItems: 3
                        ).Placeholder("Choose a fruit..."),
                        new Button("Done", () => { }) // Add another focusable component
                    }
                }.WithPadding(new Andy.TUI.Layout.Spacing(1, 1, 1, 1)); // Add padding to avoid clipping
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
            
            LogStep("Initial render - dropdown should be CLOSED");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial (unfocused)", initialBuffer);
            
            // CRITICAL: When not focused, the dropdown should NOT show the item list
            LogAssertion("Dropdown should be closed when not focused");
            Assert.DoesNotContain("Apple", initialBuffer);
            Assert.DoesNotContain("Banana", initialBuffer);
            Assert.DoesNotContain("Cherry", initialBuffer);
            Assert.DoesNotContain("│", initialBuffer); // No box borders
            Assert.DoesNotContain("▶", initialBuffer); // No selection indicator
            
            // Should only show placeholder or selected value
            Assert.Contains("Choose a fruit...", initialBuffer);
            
            LogStep("Focus the SelectInput");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var focusedBuffer = GetBufferContent(terminal);
            LogBufferContent("After Focus", focusedBuffer);
            
            // When focused, dropdown should expand
            LogAssertion("Dropdown should open when focused");
            Assert.Contains("Apple", focusedBuffer);
            Assert.Contains("Banana", focusedBuffer);
            Assert.Contains("Cherry", focusedBuffer);
            Assert.Contains("│", focusedBuffer); // Box borders should appear
            Assert.Contains("▶", focusedBuffer); // Selection indicator should appear
            
            LogStep("Unfocus by tabbing away");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var unfocusedBuffer = GetBufferContent(terminal);
            LogBufferContent("After Unfocus", unfocusedBuffer);
            
            // Dropdown should close again
            LogAssertion("Dropdown should close when focus is lost");
            Assert.DoesNotContain("Apple", unfocusedBuffer);
            Assert.DoesNotContain("Banana", unfocusedBuffer);
            Assert.DoesNotContain("│", unfocusedBuffer);
            
            input.Stop();
        }
    }

    [Fact]
    public void Dropdown_NavigationShouldNotCorruptRendering()
    {
        using (BeginScenario("Dropdown Navigation Should Not Corrupt Rendering"))
        {
            LogStep("Setting up SelectInput with navigation");
            
            var terminal = new MockTerminal(80, 25);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6", "Item 7", "Item 8" };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Navigation Test").Bold(),
                        new SelectInput<string>(
                            items,
                            new Binding<Optional<string>>(
                                () => selected,
                                v => selected = v,
                                "Selected"
                            ),
                            visibleItems: 4
                        ).Placeholder("Select item..."),
                        new Button("Submit", () => { }) // Add another focusable component
                    }
                }.WithPadding(new Andy.TUI.Layout.Spacing(1, 1, 1, 1)); // Add padding to avoid clipping
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
            
            LogStep("Focus and navigate");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var focusedBuffer = GetBufferContent(terminal);
            LogBufferContent("Focused", focusedBuffer);
            
            // Navigate down multiple times
            for (int i = 0; i < 5; i++)
            {
                LogStep($"Navigate down {i+1}");
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(50);
                
                var navBuffer = GetBufferContent(terminal);
                LogBufferContent($"After Down {i+1}", navBuffer);
                
                // Check for rendering corruption
                LogAssertion($"No corruption after navigation {i+1}");
                
                // Items should still be properly displayed
                Assert.Contains("Item", navBuffer);
                
                // Box structure should be intact
                Assert.Contains("┌", navBuffer);
                Assert.Contains("┘", navBuffer);
                Assert.Contains("│", navBuffer);
                
                // No garbage characters (excluding null padding from mock terminal)
                Assert.DoesNotContain("�", navBuffer);
                
                // Check that old content is properly cleared
                var lines = navBuffer.Split('\n');
                foreach (var line in lines)
                {
                    // Lines should not have overlapping text
                    if (line.Contains("Item") && line.Contains("┌"))
                    {
                        // This would indicate text overlapping with border
                        Logger.Error($"Overlapping content detected: {line}");
                        Assert.Fail($"Rendering overlap in line: {line}");
                    }
                }
            }
            
            input.Stop();
        }
    }

    private string GetBufferContent(MockTerminal terminal)
    {
        var content = "";
        for (int y = 0; y < Math.Min(20, terminal.Height); y++)
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
        for (int i = 0; i < Math.Min(20, lines.Length); i++)
        {
            if (lines[i].Length > 0)
            {
                _output.WriteLine($"  {i:D2}: [{lines[i]}]");
            }
        }
        _output.WriteLine("=================\n");
    }
}