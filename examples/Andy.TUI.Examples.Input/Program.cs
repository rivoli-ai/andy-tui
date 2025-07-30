using System;
using System.Threading;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.Input;

class Program
{
    static void Main(string[] args)
    {
        var app = new InputDemoApp();
        app.Run();
    }
}

class InputDemoApp
{
    private string name = "";
    private string password = "";
    private string selectedCountry = "";
    
    private readonly string[] countries = { "United States", "Canada", "United Kingdom", "Germany", "France" };

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        renderer.Run(() => CreateDeclarativeUI());
    }
    
    private ISimpleComponent CreateDeclarativeUI()
    {
        // 🎉 TRUE SWIFTUI-STYLE DECLARATIVE SYNTAX! 🎉
        // This is EXACTLY like SwiftUI with collection initializers and method chaining
        return new VStack(spacing: 1) {
            // Title with method chaining - just like SwiftUI!
            new Text("🚀 Andy.TUI Input Components Demo")
                .Title()
                .Color(Color.Cyan),
            
            " ", // Empty line
            
            // Form rows using nested layouts - pure declarative composition!
            new HStack(spacing: 2) {
                new Text("  Name:").Bold().Color(Color.White),
                new TextField("Enter your name...", this.Bind(() => name))
            },
            
            new HStack(spacing: 2) {
                new Text("  Pass:").Bold().Color(Color.White),
                new TextField("Enter password...", this.Bind(() => password)).Secure()
            },
            
            new HStack(spacing: 2) {
                new Text("Country:").Bold().Color(Color.White),
                new Dropdown<string>("Select a country...", countries, this.Bind(() => selectedCountry))
                    .Color(Color.White)
                    .PlaceholderColor(Color.Gray)
            },
            
            " ", // Empty line
            
            // Button row with method chaining for styling
            new HStack(spacing: 3) {
                new Button("Submit", HandleSubmit).Primary(),
                new Button("Cancel", HandleCancel).Secondary()
            },
            
            " ", // Empty line
            
            // Status and information section
            new Text("✅ SUCCESS: SwiftUI-like Collection Initializer Syntax!")
                .Bold()
                .Color(Color.Green),
                
            new Text("This is EXACTLY like SwiftUI syntax:")
                .Color(Color.Yellow),
                
            new Text("  • VStack(spacing: 1) { ... } collection initializers")
                .Color(Color.White),
                
            new Text("  • Method chaining: .Bold().Color(Color.Red)")
                .Color(Color.White),
                
            new Text("  • Nested layouts: HStack inside VStack")
                .Color(Color.White),
                
            new Text("  • Two-way binding: this.Bind(() => property)")
                .Color(Color.White),
                
            new Text("  • Zero AddChild() calls - pure declarative!")
                .Color(Color.White),
            
            " ", // Empty line
            
            // Live state display
            new Text("Current state values:")
                .Color(Color.Cyan),
            new Text($"  Name: '{name}'")
                .Color(Color.Gray),
            new Text($"  Password: '{new string('•', password.Length)}'")
                .Color(Color.Gray),
            new Text($"  Country: '{(string.IsNullOrEmpty(selectedCountry) ? "Not selected" : selectedCountry)}'")
                .Color(Color.Gray),
                
            " ", // Empty line
            new Text("Use [Tab] to navigate between fields, [Enter] to submit")
                .Color(Color.Yellow),
            new Text("Press Ctrl+C to exit")
                .Color(Color.DarkGray)
                
        };
    }

    private void HandleSubmit()
    {
        Console.Clear();
        Console.WriteLine($"Name: {name}");
        Console.WriteLine($"Password: {new string('•', password.Length)}");
        Console.WriteLine($"Country: {(string.IsNullOrEmpty(selectedCountry) ? "Not selected" : selectedCountry)}");
        Environment.Exit(0);
    }

    private void HandleCancel()
    {
        Environment.Exit(0);
    }

}

