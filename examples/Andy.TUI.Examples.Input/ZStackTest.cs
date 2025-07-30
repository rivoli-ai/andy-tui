using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class ZStackTest
{
    static void Main(string[] args)
    {
        var app = new ZStackTestApp();
        app.Run();
    }
}

class ZStackTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create UI demonstrating ZStack layering
        var ui = new VStack(spacing: 2) {
            new Text("ZStack Demo - Layered Views").Bold().Color(Color.Cyan),
            
            // Example 1: Simple overlapping boxes
            new Text("Overlapping boxes:").Bold(),
            new ZStack {
                // Background box (renders first, appears behind)
                new Box {
                    new Text(" ").Color(Color.Blue)
                }
                .WithWidth(40)
                .WithHeight(10)
                .WithPadding(1),
                
                // Middle box
                new Box {
                    new Text("Middle Layer").Color(Color.Yellow)
                }
                .WithWidth(30)
                .WithHeight(7)
                .WithPadding(1),
                
                // Foreground box (renders last, appears on top)
                new Box {
                    new Text("Top Layer").Bold().Color(Color.Red)
                }
                .WithWidth(20)
                .WithHeight(4)
                .WithPadding(1)
            },
            
            new Spacer(minLength: 2),
            
            // Example 2: Different alignments
            new Text("Different alignments:").Bold(),
            new HStack(spacing: 2) {
                new ZStack(AlignItems.FlexStart) {
                    new Box { new Text("Bottom") }.WithWidth(15).WithHeight(5).WithPadding(1),
                    new Text("Top-Left").Color(Color.Green)
                },
                
                new ZStack(AlignItems.Center) {
                    new Box { new Text("Bottom") }.WithWidth(15).WithHeight(5).WithPadding(1),
                    new Text("Center").Color(Color.Yellow)
                },
                
                new ZStack(AlignItems.FlexEnd) {
                    new Box { new Text("Bottom") }.WithWidth(15).WithHeight(5).WithPadding(1),
                    new Text("Bot-Right").Color(Color.Red)
                }
            },
            
            new Spacer(minLength: 2),
            
            // Example 3: Card with overlay
            new Text("Card with overlay:").Bold(),
            new ZStack {
                // Card background
                new Box {
                    new VStack {
                        new Text("Card Title").Bold(),
                        new Text("This is a card with some content"),
                        new Text("It has multiple lines of text")
                    }
                }
                .WithWidth(35)
                .WithHeight(8)
                .WithPadding(2),
                
                // Badge overlay in top-right corner
                new HStack {
                    new Spacer(),
                    new Text("NEW").Color(Color.White).BackgroundColor(Color.Red)
                }
                .WithWidth(35)
            }
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}