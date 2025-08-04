using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class FlexShrinkTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create UI demonstrating flex-shrink
        var ui = new VStack(spacing: 2) {
            new Text("Flex Shrink Demo").Bold().Color(Color.Cyan),
            
            // Container with fixed width (40) - items will need to shrink
            new Box {
                new Box { new Text("Item 1 (shrink: 1)").Color(Color.Green) }
                    .WithMinWidth(20)
                    .Shrink(1),
                new Box { new Text("Item 2 (shrink: 2)").Color(Color.Yellow) }
                    .WithMinWidth(20)
                    .Shrink(2),
                new Box { new Text("Item 3 (shrink: 0)").Color(Color.Red) }
                    .WithMinWidth(20)
                    .Shrink(0)
            }
            .Direction(FlexDirection.Row)
            .WithWidth(40)
            .WithPadding(1),
            
            new Text("Container width: 40, Items min-width: 20 each").Color(Color.Gray),
            new Text("Item 2 shrinks twice as much as Item 1").Color(Color.Gray),
            new Text("Item 3 doesn't shrink (shrink: 0)").Color(Color.Gray),
            
            new Spacer(),
            
            // Another example with different shrink values
            new Text("Equal shrink values:").Bold(),
            new Box {
                new Box { new Text("A").Color(Color.Blue) }.WithMinWidth(15).Shrink(1),
                new Box { new Text("B").Color(Color.Blue) }.WithMinWidth(15).Shrink(1),
                new Box { new Text("C").Color(Color.Blue) }.WithMinWidth(15).Shrink(1)
            }
            .Direction(FlexDirection.Row)
            .WithWidth(30)
            .WithPadding(1)
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}