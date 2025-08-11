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

class OverflowTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        renderingSystem.Initialize();
        terminal.Clear();

        // Create UI demonstrating overflow handling
        var ui = new VStack(spacing: 2) {
            new Text("Overflow Demo").Bold().Color(Color.Cyan),
            
            // Overflow: Visible (default)
            new Text("Overflow: Visible (default)").Bold(),
            new Box {
                new Text("This is a very long text that should overflow the container bounds").Color(Color.Green),
                new Text("Second line of overflowing content").Color(Color.Yellow),
                new Text("Third line that goes beyond the box").Color(Color.Red)
            }
            .Direction(FlexDirection.Column)
            .WithWidth(30)
            .WithHeight(2)
            .WithPadding(1)
            .WithOverflow(Overflow.Visible),

            new Spacer(minLength: 2),
            
            // Overflow: Hidden
            new Text("Overflow: Hidden").Bold(),
            new Box {
                new Text("This is a very long text that should be clipped").Color(Color.Blue),
                new Text("Second line that will be hidden").Color(Color.Magenta),
                new Text("Third line you won't see").Color(Color.Cyan)
            }
            .Direction(FlexDirection.Column)
            .WithWidth(30)
            .WithHeight(2)
            .WithPadding(1)
            .WithOverflow(Overflow.Hidden),

            new Spacer(minLength: 2),
            
            // Nested boxes with overflow
            new Text("Nested boxes with overflow").Bold(),
            new Box {
                new Box {
                    new Text("Nested content that overflows").Color(Color.Green),
                    new Text("More nested content").Color(Color.Yellow)
                }
                .Direction(FlexDirection.Column)
                .WithWidth(20)
                .WithHeight(3)
                .WithOverflow(Overflow.Hidden),

                new Box {
                    new Text("Another nested box").Color(Color.Red)
                }
                .WithMargin(new Spacing(0, 2))
            }
            .Direction(FlexDirection.Row)
            .WithWidth(50)
            .WithHeight(5)
            .WithPadding(1)
            .WithOverflow(Overflow.Hidden)
        };

        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);

        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}