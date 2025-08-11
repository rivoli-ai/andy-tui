using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Layout;

namespace Andy.TUI.Examples.Input;

class LayoutTest
{
    static void TestLayout(string[] args)
    {
        var app = new LayoutTestApp();
        app.Run();
    }
}

class LayoutTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create a simple layout test
        var box1 = new Box {
            new Text("Start"),
            new Text("Center"),
            new Text("End")
        };
        box1.Direction(FlexDirection.Row)
            .Justify(JustifyContent.SpaceBetween)
            .WithWidth(40)
            .WithPadding(2);
            
        var box2 = new Box {
            new Text("Centered Text"),
            new Text("In a Box")
        };
        box2.Direction(FlexDirection.Column)
            .Align(AlignItems.Center)
            .WithWidth(40)
            .WithHeight(5)
            .WithPadding(1);
        
        var ui = new VStack(spacing: 2) {
            new Text("Layout Test").Bold().Color(Color.Cyan),
            
            new HStack(spacing: 1) {
                new Text("Left").Color(Color.Green),
                new Text("Middle").Color(Color.Yellow),
                new Text("Right").Color(Color.Red)
            },
            
            // HStack with Spacer to push content apart
            new HStack {
                new Text("Start").Color(Color.Green),
                new Spacer(),
                new Text("End").Color(Color.Red)
            },
            
            // HStack with multiple Spacers for even distribution
            new HStack {
                new Spacer(),
                new Text("Centered").Color(Color.Yellow),
                new Spacer()
            },
            
            box1,
            box2,
            
            new Spacer(), // Push remaining content to bottom
            
            new Text("Bottom text pushed down by Spacer").Color(Color.Gray)
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, 20);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}