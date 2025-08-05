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
        new Country { Code = "BD", Name = "Bangladesh", Population = 169_000_000 }
    };
    
    private readonly string[] colors = { "Red", "Green", "Blue", "Yellow", "Magenta", "Cyan", "White", "Black" };
    private readonly string[] fruits = { "Apple", "Banana", "Orange", "Grape", "Mango", "Pear" };
    
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
        return new VStack(spacing: 1) {
            new Text("SelectInput Component Demo").Bold().Color(Color.Cyan),
            new Newline(),
            
            // Country Selection
            new Text("1. Country Selection (custom renderer):").Bold().Color(Color.Yellow),
            new SelectInput<Country>(
                countries,
                this.Bind(() => selectedCountry),
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
                this.Bind(() => selectedColor),
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
                this.Bind(() => selectedFruit),
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
            }.WithPadding(1),
            
            new Newline(),
            
            // Buttons
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