using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class DebugProgram
{
    static void Main2(string[] args)
    {
        // Simple test to see if dropdown renders
        var app = new DebugApp();
        app.Run();
    }
}

class DebugApp
{
    private string selectedItem = "";
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        
        // Create a simple dropdown
        var countries = new[] { "USA", "Canada", "UK" };
        var dropdown = new Dropdown<string>("Select a country...", countries, this.Bind(() => selectedItem));
        
        // Get instance
        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "dropdown1");
        
        // Render
        var dom = instance.Render();
        var renderer = new VirtualDomRenderer(renderingSystem);
        renderer.Render(dom);
        
        // Check what was rendered
        Console.SetCursorPosition(0, 5);
        Console.WriteLine($"Rendered type: {dom.GetType()}");
        Console.WriteLine($"Instance type: {instance.GetType()}");
        
        Console.ReadKey();
    }
}