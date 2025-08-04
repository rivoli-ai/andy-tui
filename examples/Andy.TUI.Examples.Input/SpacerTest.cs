using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class SpacerTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create UI with spacers
        var ui = new VStack(spacing: 1) {
            // Header pushed to top
            new Text("Header (pushed to top)").Bold().Color(Color.Cyan),
            
            // Spacer pushes content apart
            new Spacer(),
            
            // Middle content
            new HStack {
                new Text("Left").Color(Color.Green),
                new Spacer(),
                new Text("Center").Color(Color.Yellow),
                new Spacer(),
                new Text("Right").Color(Color.Red)
            },
            
            // Another spacer
            new Spacer(),
            
            // Footer pushed to bottom
            new Text("Footer (pushed to bottom)").Bold().Color(Color.Cyan)
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}