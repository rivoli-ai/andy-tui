using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class FlexBasisTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create UI demonstrating flex-basis
        var ui = new VStack(spacing: 2) {
            new Text("Flex Basis Demo").Bold().Color(Color.Cyan),
            
            // Example 1: flex-basis with flex-grow
            new Text("With flex-grow:").Bold(),
            new Box {
                new Box { new Text("basis: 20").Color(Color.Green) }
                    .Basis(20)
                    .Grow(1),
                new Box { new Text("basis: 40").Color(Color.Yellow) }
                    .Basis(40)
                    .Grow(1),
                new Box { new Text("basis: 10").Color(Color.Red) }
                    .Basis(10)
                    .Grow(1)
            }
            .Direction(FlexDirection.Row)
            .WithWidth(100)
            .WithPadding(1),
            
            new Text("All items have flex-grow: 1, different flex-basis").Color(Color.Gray),
            
            // Example 2: flex-basis without flex-grow
            new Text("Without flex-grow:").Bold(),
            new Box {
                new Box { new Text("basis: 20").Color(Color.Blue) }
                    .Basis(20),
                new Box { new Text("basis: 40").Color(Color.Magenta) }
                    .Basis(40),
                new Box { new Text("basis: 10").Color(Color.Cyan) }
                    .Basis(10)
            }
            .Direction(FlexDirection.Row)
            .WithWidth(100)
            .WithPadding(1),
            
            new Text("Items use their flex-basis as fixed width").Color(Color.Gray),
            
            // Example 3: flex-basis with percentage
            new Text("With percentage basis:").Bold(),
            new Box {
                new Box { new Text("25%").Color(Color.Green) }
                    .Basis(Length.Percentage(25)),
                new Box { new Text("50%").Color(Color.Yellow) }
                    .Basis(Length.Percentage(50)),
                new Box { new Text("25%").Color(Color.Red) }
                    .Basis(Length.Percentage(25))
            }
            .Direction(FlexDirection.Row)
            .WithWidth(80)
            .WithPadding(1),
            
            new Text("Items use percentage of container width").Color(Color.Gray)
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}