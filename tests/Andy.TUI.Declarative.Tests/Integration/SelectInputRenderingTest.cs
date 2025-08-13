using System;
using System.Collections.Generic;
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
/// Test to reproduce and debug SelectInput dropdown rendering issues.
/// Tests real rendering without mocking to catch actual problems.
/// </summary>
public class SelectInputRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        // Enable comprehensive logging for debugging
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void SelectInput_DropdownRenderingDuringNavigation()
    {
        using (BeginScenario("SelectInput Dropdown Rendering During Navigation"))
        {
            LogStep("Setting up SelectInput with multiple items");
            
            var terminal = new MockTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var countries = new[]
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
            
            var selectedCountry = Optional<Country>.None;

            ISimpleComponent BuildUI()
            {
                Logger.Debug("BuildUI called - creating SelectInput UI");
                return new VStack(spacing: 1)
                {
                    new Text("SelectInput Test").Bold().Color(Color.Cyan),
                    new Text("Country Selection:").Color(Color.Yellow),
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
                        ? new Text($"Population: {country.Population:N0}").Color(Color.Green)
                        : new Text("No country selected").Color(Color.DarkGray)
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
            
            LogStep("Initial render - dropdown should be closed");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial", initialBuffer);
            
            Assert.Contains("Choose a country...", initialBuffer);
            Assert.Contains("No country selected", initialBuffer);
            
            LogStep("Focus the SelectInput");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var focusedBuffer = GetBufferContent(terminal);
            LogBufferContent("After Focus", focusedBuffer);
            
            LogStep("Open dropdown with Space");
            input.EmitKey(' ', ConsoleKey.Spacebar);
            Thread.Sleep(100);
            
            var openDropdownBuffer = GetBufferContent(terminal);
            LogBufferContent("Dropdown Open", openDropdownBuffer);
            
            // Dropdown items should be visible
            LogAssertion("Dropdown items should be visible when open");
            Assert.Contains("US - United States", openDropdownBuffer);
            Assert.Contains("CN - China", openDropdownBuffer);
            Assert.Contains("IN - India", openDropdownBuffer);
            
            LogStep("Navigate down with arrow key");
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterDownBuffer = GetBufferContent(terminal);
            LogBufferContent("After Down Arrow", afterDownBuffer);
            
            // Check that items are still visible and selection moved
            LogAssertion("Dropdown should remain open and items visible after navigation");
            Assert.Contains("US - United States", afterDownBuffer);
            Assert.Contains("CN - China", afterDownBuffer);
            
            LogStep("Navigate down again");
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(100);
            
            var afterSecondDownBuffer = GetBufferContent(terminal);
            LogBufferContent("After Second Down", afterSecondDownBuffer);
            
            // Items should still be visible
            LogAssertion("Items should still be visible after multiple navigations");
            Assert.Contains("US - United States", afterSecondDownBuffer);
            Assert.Contains("IN - India", afterSecondDownBuffer);
            
            LogStep("Select item with Enter");
            input.EmitKey('\r', ConsoleKey.Enter);
            Thread.Sleep(100);
            
            var afterSelectBuffer = GetBufferContent(terminal);
            LogBufferContent("After Selection", afterSelectBuffer);
            
            // Dropdown should close and selected item should be shown
            LogAssertion("Dropdown should close after selection");
            Assert.DoesNotContain("US - United States", afterSelectBuffer);
            Assert.Contains("Population:", afterSelectBuffer); // Should show population of selected country
            
            input.Stop();
            
            LogStep("Analyzing rendering consistency");
            // Ensure no rendering artifacts or corruption
            var lines = afterSelectBuffer.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("�") || line.Contains("\0"))
                {
                    Logger.Error($"Rendering corruption detected: {line}");
                    Assert.Fail($"Rendering corruption in line: {line}");
                }
            }
        }
    }

    [Fact]
    public void SelectInput_MultipleDropdownsInteraction()
    {
        using (BeginScenario("Multiple SelectInputs Interaction"))
        {
            LogStep("Setting up multiple SelectInputs");
            
            var terminal = new MockTerminal(80, 40);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var colors = new[] { "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan" };
            var fruits = new[] { "Apple", "Banana", "Orange", "Grape", "Mango", "Pear" };
            
            var selectedColor = Optional<string>.None;
            var selectedFruit = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    new Text("Multiple Dropdowns Test").Bold().Color(Color.Cyan),
                    
                    new Text("Color:").Color(Color.Yellow),
                    new SelectInput<string>(
                        colors,
                        new Binding<Optional<string>>(
                            () => selectedColor,
                            v => selectedColor = v,
                            "SelectedColor"
                        ),
                        visibleItems: 4
                    ).Placeholder("Pick a color..."),
                    
                    new Text("Fruit:").Color(Color.Yellow),
                    new SelectInput<string>(
                        fruits,
                        new Binding<Optional<string>>(
                            () => selectedFruit,
                            v => selectedFruit = v,
                            "SelectedFruit"
                        ),
                        visibleItems: 3
                    ).Placeholder("Pick a fruit...")
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
            
            LogStep("Focus first dropdown");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            LogStep("Open first dropdown");
            input.EmitKey(' ', ConsoleKey.Spacebar);
            Thread.Sleep(100);
            
            var firstOpenBuffer = GetBufferContent(terminal);
            LogBufferContent("First Dropdown Open", firstOpenBuffer);
            
            Assert.Contains("Red", firstOpenBuffer);
            Assert.Contains("Green", firstOpenBuffer);
            Assert.Contains("Blue", firstOpenBuffer);
            
            LogStep("Tab to second dropdown without selecting");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            
            var switchedBuffer = GetBufferContent(terminal);
            LogBufferContent("Switched to Second", switchedBuffer);
            
            // First dropdown should be closed
            LogAssertion("First dropdown should close when focus moves");
            Assert.DoesNotContain("Red", switchedBuffer);
            Assert.DoesNotContain("Green", switchedBuffer);
            
            LogStep("Open second dropdown");
            input.EmitKey(' ', ConsoleKey.Spacebar);
            Thread.Sleep(100);
            
            var secondOpenBuffer = GetBufferContent(terminal);
            LogBufferContent("Second Dropdown Open", secondOpenBuffer);
            
            Assert.Contains("Apple", secondOpenBuffer);
            Assert.Contains("Banana", secondOpenBuffer);
            
            // Ensure first dropdown items aren't bleeding through
            LogAssertion("First dropdown items should not appear when second is open");
            Assert.DoesNotContain("Red", secondOpenBuffer);
            Assert.DoesNotContain("Green", secondOpenBuffer);
            
            input.Stop();
        }
    }

    private string GetBufferContent(MockTerminal terminal)
    {
        var content = "";
        for (int y = 0; y < terminal.Height; y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrWhiteSpace(line))
            {
                content += line.TrimEnd() + "\n";
            }
        }
        return content;
    }

    private void LogBufferContent(string label, string content)
    {
        _output.WriteLine($"\n=== {label} Buffer ===");
        var lines = content.Split('\n');
        for (int i = 0; i < Math.Min(30, lines.Length); i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                _output.WriteLine($"  {i:D2}: {lines[i]}");
            }
        }
        _output.WriteLine("=================\n");
    }

    // Test data class
    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Population { get; set; }
    }
}