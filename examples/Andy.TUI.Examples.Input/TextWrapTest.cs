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

class TextWrapTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        renderingSystem.Initialize();
        terminal.Clear();

        var longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        var veryLongWord = "Supercalifragilisticexpialidocious-antidisestablishmentarianism-pneumonoultramicroscopicsilicovolcanoconiosis";

        // Create UI demonstrating text wrapping and truncation
        var ui = new VStack(spacing: 2) {
            new Text("Text Wrapping and Truncation Demo").Bold().Color(Color.Cyan),
            
            // No wrap (default)
            new Text("1. No Wrap (default):").Bold(),
            new Box {
                new Text(longText).Color(Color.Green)
            }
            .WithWidth(40)
            .WithPadding(1)
,
            
            // Word wrap
            new Text("2. Word Wrap:").Bold(),
            new Box {
                new Text(longText)
                    .Wrap(TextWrap.Word)
                    .Color(Color.Yellow)
            }
            .WithWidth(40)
            .WithPadding(1)
,
            
            // Character wrap
            new Text("3. Character Wrap:").Bold(),
            new Box {
                new Text(veryLongWord)
                    .Wrap(TextWrap.Character)
                    .Color(Color.Magenta)
            }
            .WithWidth(40)
            .WithPadding(1)
,
            
            // Max lines with ellipsis
            new Text("4. Max Lines (3) with Ellipsis:").Bold(),
            new Box {
                new Text(longText)
                    .Wrap(TextWrap.Word)
                    .MaxLines(3)
                    .Truncate(TruncationMode.Ellipsis)
                    .Color(Color.Cyan)
            }
            .WithWidth(40)
            .WithPadding(1)
,
            
            // Head truncation
            new Text("5. Head Truncation:").Bold(),
            new Box {
                new Text("This is a very long line that will be truncated at the beginning")
                    .Truncate(TruncationMode.Head)
                    .MaxWidth(30)
                    .Color(Color.Red)
            }
            .WithPadding(1),
            
            // Middle truncation
            new Text("6. Middle Truncation:").Bold(),
            new Box {
                new Text("/very/long/path/to/some/deeply/nested/file.txt")
                    .Truncate(TruncationMode.Middle)
                    .MaxWidth(30)
                    .Color(Color.Blue)
            }
            .WithPadding(1),
            
            // Tail truncation (default)
            new Text("7. Tail Truncation (default):").Bold(),
            new Box {
                new Text("This text will be truncated at the end with ellipsis")
                    .Truncate(TruncationMode.Tail)
                    .MaxWidth(30)
                    .Color(Color.Green)
            }
            .WithPadding(1)
        };

        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);

        // Force a render flush to ensure everything is displayed
        renderingSystem.Render();

        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}