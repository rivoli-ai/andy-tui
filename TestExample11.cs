using System;
using System.Threading;
using System.IO;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Diagnostics;

public class TestExample11
{
    public static void Main()
    {
        // Enable comprehensive logging
        ComprehensiveLoggingInitializer.Initialize(isTestMode: false);
        
        // Create a file to capture output
        var outputFile = "example11-output.txt";
        using var fileWriter = new StreamWriter(outputFile);
        
        Console.WriteLine("Starting Example 11 test...");
        
        // Create terminal and rendering system
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new CrossPlatformInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
        
        // Example 11 data
        var countries = new[]
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
                        v => { 
                            selectedCountry = v;
                            fileWriter.WriteLine($"Country selected: {(v.TryGetValue(out var c) ? $"{c.Code} - {c.Name}" : "None")}");
                            fileWriter.Flush();
                        },
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
                        v => {
                            selectedColor = v;
                            fileWriter.WriteLine($"Color selected: {(v.TryGetValue(out var col) ? col : "None")}");
                            fileWriter.Flush();
                        },
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
                        v => {
                            selectedFruit = v;
                            fileWriter.WriteLine($"Fruit selected: {(v.TryGetValue(out var f) ? f : "None")}");
                            fileWriter.Flush();
                        },
                        "SelectedFruit"
                    ),
                    visibleItems: 3
                ).HideIndicator(),
                
                new Newline(),
                new Text("Press 'q' to quit").Color(Color.DarkGray)
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
                fileWriter.WriteLine($"Renderer error: {ex}");
                Console.Error.WriteLine($"Renderer error: {ex}");
            }
        })
        { IsBackground = true };
        
        rendererThread.Start();
        
        // Give it time to render
        Thread.Sleep(500);
        fileWriter.WriteLine("Initial render complete");
        fileWriter.Flush();
        
        // Simulate user input
        Console.WriteLine("\nSimulating user input:");
        Console.WriteLine("1. Tab to first SelectInput");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(500);
        fileWriter.WriteLine("Tabbed to first SelectInput");
        fileWriter.Flush();
        
        Console.WriteLine("2. Down arrow to navigate");
        input.EmitKey('\0', ConsoleKey.DownArrow);
        Thread.Sleep(500);
        fileWriter.WriteLine("Pressed down arrow");
        fileWriter.Flush();
        
        Console.WriteLine("3. Down arrow again");
        input.EmitKey('\0', ConsoleKey.DownArrow);
        Thread.Sleep(500);
        fileWriter.WriteLine("Pressed down arrow again");
        fileWriter.Flush();
        
        Console.WriteLine("4. Enter to select");
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(500);
        fileWriter.WriteLine("Pressed enter");
        fileWriter.Flush();
        
        Console.WriteLine("5. Tab to next SelectInput");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(500);
        fileWriter.WriteLine("Tabbed to second SelectInput");
        fileWriter.Flush();
        
        Console.WriteLine("6. Down arrow");
        input.EmitKey('\0', ConsoleKey.DownArrow);
        Thread.Sleep(500);
        fileWriter.WriteLine("Pressed down arrow on second SelectInput");
        fileWriter.Flush();
        
        // Stop
        Console.WriteLine("\nStopping...");
        input.Stop();
        Thread.Sleep(100);
        
        fileWriter.WriteLine("Test complete");
        Console.WriteLine($"\nOutput captured to {outputFile}");
    }
}

public class Country
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}