using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates advanced text styling including RGB colors, text attributes, and styled output.
/// </summary>
public class StyledTextExample
{
    public static void Run()
    {
        Console.WriteLine("=== Styled Text Example ===\n");

        // Create ANSI terminal for advanced styling
        var terminal = new AnsiTerminal();

        terminal.EnterAlternateScreen();
        terminal.Clear();

        try
        {
            // Basic console colors
            terminal.MoveCursor(0, 0);
            terminal.WriteLine("Standard Console Colors:");

            var consoleColors = new[]
            {
                (Color.Black, "Black"),
                (Color.DarkRed, "DarkRed"),
                (Color.DarkGreen, "DarkGreen"),
                (Color.DarkYellow, "DarkYellow"),
                (Color.DarkBlue, "DarkBlue"),
                (Color.DarkMagenta, "DarkMagenta"),
                (Color.DarkCyan, "DarkCyan"),
                (Color.Gray, "Gray"),
                (Color.DarkGray, "DarkGray"),
                (Color.Red, "Red"),
                (Color.Green, "Green"),
                (Color.Yellow, "Yellow"),
                (Color.Blue, "Blue"),
                (Color.Magenta, "Magenta"),
                (Color.Cyan, "Cyan"),
                (Color.White, "White")
            };

            int row = 2;
            int col = 0;
            foreach (var (color, name) in consoleColors)
            {
                terminal.MoveCursor(col, row);
                terminal.ApplyStyle(Style.WithForeground(color));
                terminal.Write($"██ {name}");
                terminal.ResetColors();

                col += 15;
                if (col > 60)
                {
                    col = 0;
                    row++;
                }
            }

            // RGB colors (if supported)
            row += 2;
            terminal.MoveCursor(0, row);
            terminal.WriteLine("RGB Colors (24-bit):");
            row++;

            // Rainbow gradient
            for (int i = 0; i < 40; i++)
            {
                var hue = i * 9; // 0-360 degrees
                var rgb = HsvToRgb(hue, 1.0, 1.0);
                terminal.MoveCursor(i * 2, row);
                terminal.ApplyStyle(Style.WithForeground(Color.FromRgb(rgb.r, rgb.g, rgb.b)));
                terminal.Write("██");
            }
            terminal.ResetColors();

            // Text attributes
            row += 3;
            terminal.MoveCursor(0, row);
            terminal.WriteLine("Text Attributes:");
            row += 2;

            var attributes = new[]
            {
                (Style.Default.WithBold(), "Bold text"),
                (Style.Default.WithItalic(), "Italic text"),
                (Style.Default.WithUnderline(), "Underlined text"),
                (Style.Default.WithStrikethrough(), "Strikethrough text"),
                (Style.Default.WithDim(), "Dim text"),
                (Style.Default.WithInverse(), "Inverse text"),
                (Style.Default.WithBlink(), "Blinking text")
            };

            foreach (var (style, description) in attributes)
            {
                terminal.MoveCursor(0, row++);
                terminal.ApplyStyle(style);
                terminal.Write(description);
                terminal.ResetColors();
            }

            // Combined styles
            row += 2;
            terminal.MoveCursor(0, row);
            terminal.WriteLine("Combined Styles:");
            row += 2;

            // Bold red on yellow
            terminal.MoveCursor(0, row++);
            var style1 = Style.Default
                .WithForegroundColor(Color.Red)
                .WithBackgroundColor(Color.Yellow)
                .WithBold();
            terminal.ApplyStyle(style1);
            terminal.Write(" Bold red on yellow ");
            terminal.ResetColors();

            // Underlined blue italic
            terminal.MoveCursor(0, row++);
            var style2 = Style.Default
                .WithForegroundColor(Color.Blue)
                .WithItalic()
                .WithUnderline();
            terminal.ApplyStyle(style2);
            terminal.Write("Underlined blue italic text");
            terminal.ResetColors();

            // RGB gradient with bold
            terminal.MoveCursor(0, row++);
            for (int i = 0; i < 20; i++)
            {
                var intensity = (byte)(i * 12);
                var style = Style.Default
                    .WithForegroundColor(Color.FromRgb(255, intensity, intensity))
                    .WithBold();
                terminal.ApplyStyle(style);
                terminal.Write("█");
            }
            terminal.ResetColors();

            terminal.MoveCursor(0, terminal.Height - 2);
            terminal.Write("Press any key to exit...");
            terminal.Flush();

            Console.ReadKey(true);
        }
        finally
        {
            terminal.ExitAlternateScreen();
            terminal.ResetColors();
            terminal.CursorVisible = true;
            terminal.Flush();
        }
    }

    private static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
    {
        double r, g, b;

        int i = (int)(h / 60) % 6;
        double f = h / 60 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        switch (i)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return ((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
}