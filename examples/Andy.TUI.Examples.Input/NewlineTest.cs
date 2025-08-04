using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class NewlineTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        var ui = new VStack {
            new Text("Newline Component Demo").Bold().Color(Color.Cyan),
            new Newline(), // Single line break
            
            new Text("This text has a single newline above it."),
            new Newline(2), // Two line breaks
            
            new Text("This text has two newlines above it."),
            new Newline(3), // Three line breaks
            
            new Text("This text has three newlines above it."),
            
            // Can also use implicit conversion (but not in collection initializer)
            new Newline(1), // Single newline
            
            new Text("Using implicit conversion (1 newline above)."),
            
            // Comparison with empty string approach
            new Box {
                new VStack {
                    new Text("Inside a box:").Bold(),
                    new Newline(),
                    new Text("Line 1"),
                    new Newline(),
                    new Text("Line 2"),
                    new Newline(2),
                    new Text("Line 3 (with extra space)")
                }
            }
            .WithPadding(1)
            .WithMargin(1),
            
            new Newline(),
            new Text("Benefits over empty strings:").Bold().Color(Color.Yellow),
            new Text("• More semantic and clear intent"),
            new Text("• Can specify multiple lines easily"),
            new Text("• Works consistently in all layouts"),
            
            new Newline(2),
            new Text("Press any key to exit...").Color(Color.DarkGray)
        };
        
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 1);
        Console.ReadKey();
    }
}