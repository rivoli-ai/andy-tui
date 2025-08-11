using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a simple cowsay implementation with speech bubbles and ASCII art.
/// </summary>
public class CowsayExample
{
    public static void Run()
    {
        Console.WriteLine("=== Cowsay Example ===");
        Console.WriteLine("A simple cowsay implementation");
        Console.WriteLine();

        // Get message from user
        Console.Write("Enter a message (or press Enter for default): ");
        var message = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Hello from Andy.TUI!";
        }

        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        renderingSystem.Clear();

        // Draw the cow with message
        DrawCowsay(renderingSystem, message, 5, 2);

        // Draw additional examples
        var y = 20;
        renderingSystem.WriteText(2, y, "Other animals:", Style.Default.WithForegroundColor(Color.Yellow).WithBold());
        y += 2;

        // Draw a thinking cow
        DrawCowsay(renderingSystem, "I'm thinking...", 5, y, "think", "cow");

        // Draw a small dragon
        DrawCowsay(renderingSystem, "RAWR!", 45, y, "say", "dragon");

        // Draw a cat
        DrawCowsay(renderingSystem, "Meow!", 75, y, "say", "cat");

        // Instructions
        renderingSystem.WriteText(2, renderingSystem.Terminal.Height - 2, "Press any key to exit...",
            Style.Default.WithForegroundColor(Color.DarkGray));

        renderingSystem.Render();

        Console.ReadKey(true);

        renderingSystem.Shutdown();
    }

    private static void DrawCowsay(RenderingSystem renderingSystem, string message, int x, int y,
        string mode = "say", string animal = "cow")
    {
        // Wrap message if too long
        var maxWidth = 40;
        var lines = WrapText(message, maxWidth);

        // Draw speech bubble
        DrawSpeechBubble(renderingSystem, lines, x, y, mode);

        // Draw the animal
        var bubbleHeight = lines.Count + 2;
        DrawAnimal(renderingSystem, x, y + bubbleHeight, animal, mode);
    }

    private static List<string> WrapText(string text, int maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > maxWidth)
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine.Trim());
                    currentLine = "";
                }
            }

            currentLine += word + " ";
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine.Trim());
        }

        if (lines.Count == 0)
        {
            lines.Add("");
        }

        return lines;
    }

    private static void DrawSpeechBubble(RenderingSystem renderingSystem, List<string> lines, int x, int y, string mode)
    {
        var maxLength = lines.Max(l => l.Length);
        var bubbleStyle = Style.Default.WithForegroundColor(Color.White);

        // Top border
        renderingSystem.Buffer.SetCell(x, y, ' ', bubbleStyle);
        renderingSystem.Buffer.SetCell(x + 1, y, '_', bubbleStyle);
        for (int i = 0; i < maxLength + 2; i++)
        {
            renderingSystem.Buffer.SetCell(x + 2 + i, y, '_', bubbleStyle);
        }

        // Lines with borders
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var paddedLine = line.PadRight(maxLength);

            if (lines.Count == 1)
            {
                // Single line
                renderingSystem.Buffer.SetCell(x, y + 1, '<', bubbleStyle);
                renderingSystem.WriteText(x + 2, y + 1, paddedLine, bubbleStyle);
                renderingSystem.Buffer.SetCell(x + 2 + maxLength + 1, y + 1, '>', bubbleStyle);
            }
            else if (i == 0)
            {
                // First line
                renderingSystem.Buffer.SetCell(x, y + 1 + i, '/', bubbleStyle);
                renderingSystem.WriteText(x + 2, y + 1 + i, paddedLine, bubbleStyle);
                renderingSystem.Buffer.SetCell(x + 2 + maxLength + 1, y + 1 + i, '\\', bubbleStyle);
            }
            else if (i == lines.Count - 1)
            {
                // Last line
                renderingSystem.Buffer.SetCell(x, y + 1 + i, '\\', bubbleStyle);
                renderingSystem.WriteText(x + 2, y + 1 + i, paddedLine, bubbleStyle);
                renderingSystem.Buffer.SetCell(x + 2 + maxLength + 1, y + 1 + i, '/', bubbleStyle);
            }
            else
            {
                // Middle lines
                renderingSystem.Buffer.SetCell(x, y + 1 + i, '|', bubbleStyle);
                renderingSystem.WriteText(x + 2, y + 1 + i, paddedLine, bubbleStyle);
                renderingSystem.Buffer.SetCell(x + 2 + maxLength + 1, y + 1 + i, '|', bubbleStyle);
            }
        }

        // Bottom border
        renderingSystem.Buffer.SetCell(x, y + lines.Count + 1, ' ', bubbleStyle);
        renderingSystem.Buffer.SetCell(x + 1, y + lines.Count + 1, '-', bubbleStyle);
        for (int i = 0; i < maxLength + 2; i++)
        {
            renderingSystem.Buffer.SetCell(x + 2 + i, y + lines.Count + 1, '-', bubbleStyle);
        }
    }

    private static void DrawAnimal(RenderingSystem renderingSystem, int x, int y, string animal, string mode)
    {
        var animalStyle = Style.Default.WithForegroundColor(Color.White);
        var eyeStyle = Style.Default.WithForegroundColor(Color.Cyan);

        // Draw connector based on mode
        if (mode == "think")
        {
            renderingSystem.WriteText(x + 4, y, "o", animalStyle);
            renderingSystem.WriteText(x + 3, y + 1, "o", animalStyle);
        }
        else
        {
            renderingSystem.WriteText(x + 2, y, "\\", animalStyle);
            renderingSystem.WriteText(x + 3, y + 1, "\\", animalStyle);
        }

        switch (animal.ToLower())
        {
            case "cow":
                DrawCow(renderingSystem, x + 4, y + 2, animalStyle, eyeStyle);
                break;
            case "dragon":
                DrawDragon(renderingSystem, x + 4, y + 2, animalStyle, eyeStyle);
                break;
            case "cat":
                DrawCat(renderingSystem, x + 4, y + 2, animalStyle, eyeStyle);
                break;
            default:
                DrawCow(renderingSystem, x + 4, y + 2, animalStyle, eyeStyle);
                break;
        }
    }

    private static void DrawCow(RenderingSystem renderingSystem, int x, int y, Style bodyStyle, Style eyeStyle)
    {
        var cow = new[]
        {
            "   ^__^",
            "   (oo)\\_______",
            "   (__)\\       )\\/\\",
            "       ||----w |",
            "       ||     ||"
        };

        for (int i = 0; i < cow.Length; i++)
        {
            var line = cow[i];
            for (int j = 0; j < line.Length; j++)
            {
                var ch = line[j];
                if (ch == 'o')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, eyeStyle);
                }
                else if (ch != ' ')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, bodyStyle);
                }
            }
        }
    }

    private static void DrawDragon(RenderingSystem renderingSystem, int x, int y, Style bodyStyle, Style eyeStyle)
    {
        var dragon = new[]
        {
            "     \\||/",
            "     |  @___oo",
            "   /|  \\ \\",
            "  / |   | |",
            "    |  |\\|"
        };

        for (int i = 0; i < dragon.Length; i++)
        {
            var line = dragon[i];
            for (int j = 0; j < line.Length; j++)
            {
                var ch = line[j];
                if (ch == '@')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, eyeStyle);
                }
                else if (ch != ' ')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, bodyStyle);
                }
            }
        }
    }

    private static void DrawCat(RenderingSystem renderingSystem, int x, int y, Style bodyStyle, Style eyeStyle)
    {
        var cat = new[]
        {
            " /\\_/\\",
            "( o.o )",
            " > ^ <"
        };

        for (int i = 0; i < cat.Length; i++)
        {
            var line = cat[i];
            for (int j = 0; j < line.Length; j++)
            {
                var ch = line[j];
                if (ch == 'o' || ch == '.')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, eyeStyle);
                }
                else if (ch != ' ')
                {
                    renderingSystem.Buffer.SetCell(x + j, y + i, ch, bodyStyle);
                }
            }
        }
    }
}