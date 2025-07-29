using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates an interactive ASCII art gallery with various art pieces and display effects.
/// </summary>
public class AsciiArtGalleryExample
{
    private class ArtPiece
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string[] Lines { get; set; } = Array.Empty<string>();
        public Color PrimaryColor { get; set; } = Color.White;
        public Color SecondaryColor { get; set; } = Color.Gray;
        public string Description { get; set; } = "";
    }

    private enum DisplayEffect
    {
        None,
        TypeWriter,
        FadeIn,
        SlideIn,
        Rainbow,
        Glitch
    }

    public static void Run()
    {
        Console.WriteLine("=== ASCII Art Gallery ===");
        Console.WriteLine("Navigate through various ASCII art pieces");
        Console.WriteLine("Use arrow keys to browse, SPACE for effects");
        Console.WriteLine("Starting gallery...");
        Thread.Sleep(1500);

        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);

        // Hide cursor
        terminal.CursorVisible = false;

        // Create input handler
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        var pressedKeys = new HashSet<ConsoleKey>();

        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
            else
                pressedKeys.Add(e.Key);
        };
        inputHandler.Start();

        // Initialize art gallery
        var artPieces = CreateArtCollection();
        int currentPiece = 0;
        var currentEffect = DisplayEffect.None;
        var effectFrame = 0;
        var lastEffectTime = DateTime.Now;

        var frameCount = 0;
        var startTime = DateTime.Now;

        while (!exit)
        {
            renderer.BeginFrame();
            renderer.Clear();

            // Handle input
            if (pressedKeys.Contains(ConsoleKey.LeftArrow))
            {
                currentPiece = (currentPiece - 1 + artPieces.Count) % artPieces.Count;
                effectFrame = 0;
                pressedKeys.Remove(ConsoleKey.LeftArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.RightArrow))
            {
                currentPiece = (currentPiece + 1) % artPieces.Count;
                effectFrame = 0;
                pressedKeys.Remove(ConsoleKey.RightArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.Spacebar))
            {
                currentEffect = (DisplayEffect)(((int)currentEffect + 1) % Enum.GetValues<DisplayEffect>().Length);
                effectFrame = 0;
                lastEffectTime = DateTime.Now;
                pressedKeys.Remove(ConsoleKey.Spacebar);
            }

            // Draw gallery
            DrawGallery(renderer, artPieces[currentPiece], currentPiece, artPieces.Count, 
                       currentEffect, effectFrame, frameCount, startTime);

            renderer.EndFrame();

            effectFrame++;
            frameCount++;
            Thread.Sleep(50); // ~20 FPS
        }

        inputHandler.Stop();
        inputHandler.Dispose();

        // Restore cursor
        terminal.CursorVisible = true;

        Console.Clear();
        Console.WriteLine("\nThanks for visiting the ASCII Art Gallery!");
    }

    private static List<ArtPiece> CreateArtCollection()
    {
        return new List<ArtPiece>
        {
            new ArtPiece
            {
                Title = "Classic Computer",
                Artist = "Digital Archives",
                PrimaryColor = Color.Green,
                SecondaryColor = Color.DarkGreen,
                Description = "A retro computer terminal from the early computing era",
                Lines = new[]
                {
                    "┌─────────────────────────────┐",
                    "│  ████████████████████████   │",
                    "│  █                      █   │",
                    "│  █   > Hello, World!    █   │",
                    "│  █   > _                █   │",
                    "│  █                      █   │",
                    "│  █                      █   │",
                    "│  ████████████████████████   │",
                    "└─────────────────────────────┘",
                    "        ┌─────────────┐        ",
                    "        │ ┌─┐ ┌─┐ ┌─┐ │        ",
                    "        │ └─┘ └─┘ └─┘ │        ",
                    "        └─────────────┘        "
                }
            },
            new ArtPiece
            {
                Title = "Butterfly",
                Artist = "Nature Collection",
                PrimaryColor = Color.Magenta,
                SecondaryColor = Color.Yellow,
                Description = "A delicate butterfly spreading its wings",
                Lines = new[]
                {
                    "                   .-.",
                    "                  (   )",
                    "               .-'  |  '-.",
                    "              (     |     )",
                    "               '-._/ \\_.-'",
                    "                 /| |\\",
                    "              .-' | | '-.",
                    "             (    | |    )",
                    "              '-./ \\ \\.-'",
                    "                 |  |",
                    "                 |  |",
                    "                 '--'"
                }
            },
            new ArtPiece
            {
                Title = "Castle",
                Artist = "Medieval Times",
                PrimaryColor = Color.Blue,
                SecondaryColor = Color.Cyan,
                Description = "A majestic castle fortress on a hill",
                Lines = new[]
                {
                    "                    /\\",
                    "                   /  \\",
                    "                  /____\\",
                    "                  |    |",
                    "               /\\ |    | /\\",
                    "              /  \\|    |/  \\",
                    "             /____\\    /____\\",
                    "             |    |    |    |",
                    "             |    |____|    |",
                    "             |              |",
                    "             |  []      []  |",
                    "             |              |",
                    "             |    ______    |",
                    "             |   |      |   |",
                    "          ___|___|______|___|___",
                    "         /                     \\",
                    "        /_______________________\\"
                }
            },
            new ArtPiece
            {
                Title = "Rocket Ship",
                Artist = "Space Explorer",
                PrimaryColor = Color.Red,
                SecondaryColor = Color.FromRgb(255, 165, 0),
                Description = "A rocket ship ready for launch to the stars",
                Lines = new[]
                {
                    "           /\\",
                    "          /  \\",
                    "         /____\\",
                    "        |      |",
                    "        | NASA |",
                    "        |      |",
                    "        |  /\\  |",
                    "        | /  \\ |",
                    "        |/____\\|",
                    "        |      |",
                    "        |      |",
                    "       /|      |\\",
                    "      / |______| \\",
                    "     /____________\\",
                    "     \\    ^^^^    /",
                    "      \\   ^^^^   /",
                    "       \\  ^^^^  /",
                    "        \\ ^^^^ /",
                    "         \\____/"
                }
            },
            new ArtPiece
            {
                Title = "Dragon",
                Artist = "Fantasy Realms",
                PrimaryColor = Color.FromRgb(255, 100, 0),
                SecondaryColor = Color.Yellow,
                Description = "A fearsome dragon breathing fire",
                Lines = new[]
                {
                    "                 /|  /|  ",
                    "                ( :v:  ) ",
                    "               |~~     ~~|",
                    "              (|    _    |)",
                    "               |   (_)   |",
                    "               |         |",
                    "              /|         |\\",
                    "             ( |         | )",
                    "              \\|         |/",
                    "               |    _    |",
                    "               |   |_|   |",
                    "              /           \\",
                    "             /   ^^   ^^   \\",
                    "            /               \\",
                    "           /_________________\\",
                    "          /~~~~~~~~~~~~~~~~~~~\\"
                }
            },
            new ArtPiece
            {
                Title = "Tree of Life",
                Artist = "Nature's Art",
                PrimaryColor = Color.Green,
                SecondaryColor = Color.FromRgb(139, 69, 19),
                Description = "A magnificent tree representing growth and life",
                Lines = new[]
                {
                    "           &&& &&  & &&",
                    "       && &\\/&\\|& ()|/ @, &&",
                    "       &\\/(/&/&||/& /_/)_&/_&",
                    "    &() &\\/&|()|/&\\/ '% & ()",
                    "   &_\\_&&_\\ |& |&&/&__%_/_& &&",
                    " &&   && & &| &| /& & % ()& /&&",
                    "  ()&_---()&\\&\\|&&-&&--%---()~",
                    "      &&     \\|||",
                    "              |||",
                    "              |||",
                    "              |||",
                    "        , -=-~  .-^- _",
                    "              `"
                }
            }
        };
    }

    private static void DrawGallery(TerminalRenderer renderer, ArtPiece piece, int currentIndex, int totalPieces,
                                   DisplayEffect effect, int effectFrame, int frameCount, DateTime startTime)
    {
        // Draw header
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        renderer.DrawText(2, 1, "🎨 ASCII Art Gallery", headerStyle);

        // Draw navigation info
        var navStyle = Style.Default.WithForegroundColor(Color.Cyan);
        renderer.DrawText(2, 2, $"Piece {currentIndex + 1} of {totalPieces}", navStyle);

        // Draw art piece info
        var titleStyle = Style.Default.WithForegroundColor(piece.PrimaryColor).WithBold();
        var artistStyle = Style.Default.WithForegroundColor(piece.SecondaryColor);
        var descStyle = Style.Default.WithForegroundColor(Color.White);

        renderer.DrawText(2, 4, $"Title: {piece.Title}", titleStyle);
        renderer.DrawText(2, 5, $"Artist: {piece.Artist}", artistStyle);
        renderer.DrawText(2, 6, piece.Description, descStyle);

        // Draw the ASCII art with effects
        int artStartY = 8;
        int artStartX = Math.Max(2, (renderer.Width - GetMaxLineLength(piece.Lines)) / 2);

        DrawArtWithEffect(renderer, piece, artStartX, artStartY, effect, effectFrame);

        // Draw effect info
        var effectStyle = Style.Default.WithForegroundColor(Color.Magenta);
        renderer.DrawText(2, renderer.Height - 4, $"Current Effect: {effect}", effectStyle);

        // Draw controls
        var controlsStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        renderer.DrawText(2, renderer.Height - 3, "← → Navigate | SPACE Change Effect | ESC/Q Exit", controlsStyle);

        // Draw performance stats
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        var fps = frameCount / elapsed;
        var statsStyle = Style.Default.WithForegroundColor(Color.Green);
        renderer.DrawText(renderer.Width - 15, 1, $"FPS: {fps:F1}", statsStyle);
    }

    private static void DrawArtWithEffect(TerminalRenderer renderer, ArtPiece piece, int x, int y,
                                         DisplayEffect effect, int effectFrame)
    {
        switch (effect)
        {
            case DisplayEffect.None:
                DrawArtNormal(renderer, piece, x, y);
                break;
            case DisplayEffect.TypeWriter:
                DrawArtTypeWriter(renderer, piece, x, y, effectFrame);
                break;
            case DisplayEffect.FadeIn:
                DrawArtFadeIn(renderer, piece, x, y, effectFrame);
                break;
            case DisplayEffect.SlideIn:
                DrawArtSlideIn(renderer, piece, x, y, effectFrame);
                break;
            case DisplayEffect.Rainbow:
                DrawArtRainbow(renderer, piece, x, y, effectFrame);
                break;
            case DisplayEffect.Glitch:
                DrawArtGlitch(renderer, piece, x, y, effectFrame);
                break;
        }
    }

    private static void DrawArtNormal(TerminalRenderer renderer, ArtPiece piece, int x, int y)
    {
        var style = Style.Default.WithForegroundColor(piece.PrimaryColor);
        
        for (int i = 0; i < piece.Lines.Length; i++)
        {
            renderer.DrawText(x, y + i, piece.Lines[i], style);
        }
    }

    private static void DrawArtTypeWriter(TerminalRenderer renderer, ArtPiece piece, int x, int y, int frame)
    {
        var style = Style.Default.WithForegroundColor(piece.PrimaryColor);
        int charsToShow = Math.Max(0, frame - 10); // Delay start
        int currentChar = 0;

        for (int lineIndex = 0; lineIndex < piece.Lines.Length; lineIndex++)
        {
            string line = piece.Lines[lineIndex];
            string visiblePart = "";

            for (int charIndex = 0; charIndex < line.Length; charIndex++)
            {
                if (currentChar < charsToShow)
                {
                    visiblePart += line[charIndex];
                }
                else if (currentChar == charsToShow)
                {
                    // Add cursor
                    visiblePart += "█";
                    break;
                }
                else
                {
                    break;
                }
                currentChar++;
            }

            renderer.DrawText(x, y + lineIndex, visiblePart, style);
            currentChar++; // Count newline
        }
    }

    private static void DrawArtFadeIn(TerminalRenderer renderer, ArtPiece piece, int x, int y, int frame)
    {
        double intensity = Math.Min(1.0, frame / 100.0);
        var rgb = piece.PrimaryColor.Rgb ?? (255, 255, 255);
        var r = (byte)(rgb.R * intensity);
        var g = (byte)(rgb.G * intensity);
        var b = (byte)(rgb.B * intensity);
        
        var style = Style.Default.WithForegroundColor(Color.FromRgb(r, g, b));
        
        for (int i = 0; i < piece.Lines.Length; i++)
        {
            renderer.DrawText(x, y + i, piece.Lines[i], style);
        }
    }

    private static void DrawArtSlideIn(TerminalRenderer renderer, ArtPiece piece, int x, int y, int frame)
    {
        var style = Style.Default.WithForegroundColor(piece.PrimaryColor);
        int slideOffset = Math.Max(0, 50 - frame);
        
        for (int i = 0; i < piece.Lines.Length; i++)
        {
            renderer.DrawText(x + slideOffset, y + i, piece.Lines[i], style);
        }
    }

    private static void DrawArtRainbow(TerminalRenderer renderer, ArtPiece piece, int x, int y, int frame)
    {
        for (int lineIndex = 0; lineIndex < piece.Lines.Length; lineIndex++)
        {
            string line = piece.Lines[lineIndex];
            for (int charIndex = 0; charIndex < line.Length; charIndex++)
            {
                if (line[charIndex] != ' ')
                {
                    double hue = ((charIndex + lineIndex + frame * 0.1) % 360) / 360.0;
                    var color = HsvToRgb(hue, 1.0, 1.0);
                    var style = Style.Default.WithForegroundColor(color);
                    renderer.DrawChar(x + charIndex, y + lineIndex, line[charIndex], style);
                }
            }
        }
    }

    private static void DrawArtGlitch(TerminalRenderer renderer, ArtPiece piece, int x, int y, int frame)
    {
        var random = new Random(frame / 5); // Change every few frames
        var baseStyle = Style.Default.WithForegroundColor(piece.PrimaryColor);
        
        for (int lineIndex = 0; lineIndex < piece.Lines.Length; lineIndex++)
        {
            string line = piece.Lines[lineIndex];
            int glitchOffset = 0;
            
            // Randomly glitch some lines
            if (random.NextDouble() < 0.3)
            {
                glitchOffset = random.Next(-3, 4);
                var glitchColors = new[] { Color.Red, Color.Green, Color.Blue, Color.Magenta, Color.Cyan };
                var glitchColor = glitchColors[random.Next(glitchColors.Length)];
                var glitchStyle = Style.Default.WithForegroundColor(glitchColor);
                
                // Draw glitched version
                renderer.DrawText(x + glitchOffset, y + lineIndex, line, glitchStyle);
                
                // Sometimes overlay with different colors
                if (random.NextDouble() < 0.5)
                {
                    var overlay = glitchColors[random.Next(glitchColors.Length)];
                    var overlayStyle = Style.Default.WithForegroundColor(overlay);
                    renderer.DrawText(x + glitchOffset + 1, y + lineIndex, line, overlayStyle);
                }
            }
            else
            {
                renderer.DrawText(x, y + lineIndex, line, baseStyle);
            }
        }
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        int i = (int)(h * 6);
        double f = h * 6 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        double r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
            default: r = g = b = 0; break;
        }

        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private static int GetMaxLineLength(string[] lines)
    {
        return lines.Length > 0 ? lines.Max(line => line.Length) : 0;
    }
}