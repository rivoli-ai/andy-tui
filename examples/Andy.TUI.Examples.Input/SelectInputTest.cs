using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using static Andy.TUI.Declarative.State.Optional<Andy.TUI.Examples.Input.SelectInputTestApp.Country>;
using static Andy.TUI.Declarative.State.Optional<string>;

namespace Andy.TUI.Examples.Input;

class SelectInputTestApp
{
    // Sample data
    private readonly List<Country> countries = new()
    {
        new Country { Code = "US", Name = "United States", Population = 331_000_000 },
        new Country { Code = "CN", Name = "China", Population = 1_412_000_000 },
        new Country { Code = "IN", Name = "India", Population = 1_380_000_000 },
        new Country { Code = "ID", Name = "Indonesia", Population = 273_000_000 },
        new Country { Code = "PK", Name = "Pakistan", Population = 225_000_000 },
        new Country { Code = "BR", Name = "Brazil", Population = 213_000_000 },
        new Country { Code = "NG", Name = "Nigeria", Population = 211_000_000 },
        new Country { Code = "BD", Name = "Bangladesh", Population = 169_000_000 },
        new Country { Code = "RU", Name = "Russia", Population = 146_000_000 },
        new Country { Code = "MX", Name = "Mexico", Population = 128_000_000 },
        new Country { Code = "JP", Name = "Japan", Population = 125_000_000 },
        new Country { Code = "ET", Name = "Ethiopia", Population = 120_000_000 },
        new Country { Code = "PH", Name = "Philippines", Population = 111_000_000 },
        new Country { Code = "EG", Name = "Egypt", Population = 104_000_000 },
        new Country { Code = "VN", Name = "Vietnam", Population = 98_000_000 }
    };
    
    private readonly string[] colors = { "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "White", "Black" };
    
    private Optional<Country> selectedCountry = Optional<Country>.None;
    private Optional<string> selectedColor = Optional<string>.None;
    private Optional<string> selectedFruit = Optional<string>.None;
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        renderer.Run(() => CreateUI());
    }
    
    private ISimpleComponent CreateUI()
    {
        return new VStack(spacing: 2) {
            new Text("SelectInput Component Demo").Bold().Color(Color.Cyan),
            
            new HStack(spacing: 3) {
                // Left column
                new VStack(spacing: 1) {
                    new Text("1. Country Selection (custom renderer):").Bold(),
                    new SelectInput<Country>(
                        countries,
                        this.Bind(() => selectedCountry),
                        country => $"{country.Code} - {country.Name}",
                        visibleItems: 8,
                        placeholder: "Choose a country..."
                    ),
                    
                    selectedCountry.TryGetValue(out var country)
                        ? new Text($"Population: {country.Population:N0}").Color(Color.Gray)
                        : new Text("No country selected").Color(Color.DarkGray)
                },
                
                // Right column
                new VStack(spacing: 1) {
                    new Text("2. Color Selection (simple list):").Bold(),
                    new SelectInput<string>(
                        colors,
                        this.Bind(() => selectedColor),
                        visibleItems: 5
                    ).Placeholder("Pick a color..."),
                    
                    selectedColor.TryGetValue(out var color)
                        ? new Box {
                            new Text($"  {color}  ")
                                .Color(ParseColor(color))
                          }
                          .WithPadding(1)
                        : new Text("No color selected").Color(Color.DarkGray),
                    
                    new Text("3. Fruit Selection (without indicator):").Bold(),
                    new SelectInput<string>(
                        new[] { "Apple", "Banana", "Orange", "Grape", "Mango", "Pear" },
                        this.Bind(() => selectedFruit),
                        visibleItems: 4
                    ).HideIndicator(),
                    
                    selectedFruit.TryGetValue(out var fruit)
                        ? new Text($"You selected: {fruit}").Color(Color.Green)
                        : new Text("No fruit selected").Color(Color.DarkGray)
                }
            },
            
            new Text("Navigation:").Bold().Color(Color.Yellow),
            new Text("• Tab to switch between inputs").Color(Color.Gray),
            new Text("• ↑/↓ arrows to navigate items").Color(Color.Gray),
            new Text("• Home/End to jump to first/last").Color(Color.Gray),
            new Text("• PageUp/PageDown to scroll quickly").Color(Color.Gray),
            new Text("• Enter/Space to select").Color(Color.Gray),
            new Text("• Ctrl+C to exit").Color(Color.Gray),
            
            new HStack(spacing: 2) {
                new Button("Clear All", () => {
                    selectedCountry = Optional<Country>.None;
                    selectedColor = Optional<string>.None;
                    selectedFruit = Optional<string>.None;
                }).Secondary(),
                new Button("Submit", HandleSubmit).Primary()
            }
        };
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
    
    private void HandleSubmit()
    {
        Console.Clear();
        Console.WriteLine("Selected values:");
        Console.WriteLine("================");
        Console.WriteLine($"Country: {(selectedCountry.TryGetValue(out var c) ? c.Name : "None")}");
        Console.WriteLine($"Color: {(selectedColor.TryGetValue(out var col) ? col : "None")}");
        Console.WriteLine($"Fruit: {(selectedFruit.TryGetValue(out var f) ? f : "None")}");
        Environment.Exit(0);
    }
    
    // Sample data class
    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Population { get; set; }
    }
}