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

class TransformTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        renderingSystem.Initialize();
        terminal.Clear();

        var originalText = "hello WORLD, this IS a Test!";

        var ui = new VStack(spacing: 1) {
            new Text("Transform Component Demo").Bold().Color(Color.Cyan),
            new Newline(),

            new Text($"Original: {originalText}").Color(Color.Gray),
            new Newline(),

            new HStack(spacing: 2) {
                new Text("Uppercase:").Bold(),
                new Transform(originalText).Uppercase().Color(Color.Red)
            },

            new HStack(spacing: 2) {
                new Text("Lowercase:").Bold(),
                new Transform(originalText).Lowercase().Color(Color.Green)
            },

            new HStack(spacing: 2) {
                new Text("Capitalize:").Bold(),
                new Transform(originalText).Capitalize().Color(Color.Blue)
            },

            new HStack(spacing: 2) {
                new Text("Capitalize First:").Bold(),
                new Transform(originalText).CapitalizeFirst().Color(Color.Magenta)
            },

            new Newline(2),

            new Text("Styled Transforms:").Bold(),
            new HStack(spacing: 2) {
                new Transform("bold uppercase").Uppercase().Bold(),
                new Transform("italic lowercase").Lowercase().Italic(),
                new Transform("underlined capitalize").Capitalize().Underline()
            },

            new Newline(2),

            new Text("Real-world Examples:").Bold().Color(Color.Yellow),
            new Box {
                new VStack {
                    new HStack(spacing: 1) {
                        new Transform("name:").Capitalize().Bold(),
                        new Text(" John Doe")
                    },
                    new HStack(spacing: 1) {
                        new Transform("email:").Capitalize().Bold(),
                        new Text(" john.doe@example.com")
                    },
                    new HStack(spacing: 1) {
                        new Transform("status:").Capitalize().Bold(),
                        new Transform(" active").Uppercase().Color(Color.Green)
                    }
                }
            }
            .WithPadding(1),

            new Newline(),
            new Text("Press any key to exit...").Color(Color.DarkGray)
        };

        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);

        Console.SetCursorPosition(0, terminal.Height - 1);
        Console.ReadKey();
    }
}