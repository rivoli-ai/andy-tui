using Andy.TUI.Examples.ZIndex;
using System;

// Z-Index Examples Launcher
Console.WriteLine("Z-Index Examples:");
Console.WriteLine("1. Minimal Z-Index Example");
Console.WriteLine("2. Simple TabView Example");
Console.WriteLine("3. TabView Working Example");
Console.WriteLine();
Console.Write("Select example (1-3): ");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        RunExample(MinimalZIndexExample.Create(), "Minimal Z-Index Example");
        break;
    case "2":
        RunExample(SimpleTabViewExample.Create(), "Simple TabView Example");
        break;
    case "3":
        RunExample(TabViewWorkingExample.Create(), "TabView Working Example");
        break;
    default:
        Console.WriteLine("Invalid choice");
        break;
}

static void RunExample(Andy.TUI.Declarative.ISimpleComponent component, string title)
{
    var terminal = new Andy.TUI.Terminal.AnsiTerminal();
    using var renderingSystem = new Andy.TUI.Terminal.RenderingSystem(terminal);
    var renderer = new Andy.TUI.Declarative.Rendering.DeclarativeRenderer(renderingSystem);
    
    renderingSystem.Initialize();
    
    renderer.Run(() => component);
}