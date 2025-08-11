using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates the complete rendering system with double buffering,
/// dirty region tracking, and efficient ANSI rendering.
/// </summary>
public class RenderingSystemExample
{
    public static void Run()
    {
        Console.WriteLine("=== Rendering System Example ===");
        Console.WriteLine("This example demonstrates:");
        Console.WriteLine("- Double buffering for smooth updates");
        Console.WriteLine("- Dirty region tracking for efficiency");
        Console.WriteLine("- ANSI color rendering");
        Console.WriteLine("- Frame rate limiting");
        Console.WriteLine("\nPress any key to start...");
        Console.ReadKey(true);

        // Create terminal and rendering system
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        try
        {
            // Initialize the rendering system
            renderingSystem.Initialize();

            // Monitor performance
            renderingSystem.Scheduler.AfterRender += (s, e) =>
            {
                // Update performance metrics display
                renderingSystem.WriteText(1, terminal.Height - 2,
                    $"FPS: {renderingSystem.Scheduler.ActualFps:F1} | " +
                    $"Render: {e.RenderTimeMs:F1}ms | " +
                    $"Cells: {e.DirtyCellCount}",
                    new Style { Foreground = Color.Cyan, Dim = true });
            };

            // Run the demo
            RunDemo(renderingSystem);
        }
        finally
        {
            // Clean shutdown
            renderingSystem.Shutdown();
        }

        Console.WriteLine("\nExample completed!");
    }

    private static void RunDemo(RenderingSystem renderingSystem)
    {
        var terminal = renderingSystem.Terminal;
        var width = terminal.Width;
        var height = terminal.Height;

        // Clear the screen
        renderingSystem.Clear();

        // Draw a title
        var title = "Andy.TUI Rendering System Demo";
        var titleX = (width - title.Length) / 2;
        renderingSystem.WriteText(titleX, 1, title, new Style
        {
            Foreground = Color.Cyan,
            Bold = true
        });

        // Draw different styled boxes
        DrawStyledBoxes(renderingSystem);

        // Animate some content
        AnimateContent(renderingSystem);

        Console.WriteLine("\nPress Ctrl+C to exit...");

        // Keep running until interrupted
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            Task.Delay(-1, cts.Token).Wait();
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelled
        }
    }

    private static void DrawStyledBoxes(RenderingSystem renderingSystem)
    {
        var styles = new[]
        {
            (BoxStyle.Single, "Single", Color.Green),
            (BoxStyle.Double, "Double", Color.Blue),
            (BoxStyle.Rounded, "Rounded", Color.Magenta),
            (BoxStyle.Heavy, "Heavy", Color.Yellow),
            (BoxStyle.Ascii, "ASCII", Color.Cyan)
        };

        int y = 4;
        foreach (var (boxStyle, name, color) in styles)
        {
            // Draw box
            renderingSystem.DrawBox(5, y, 20, 5, new Style { Foreground = color }, boxStyle);

            // Label inside
            renderingSystem.WriteText(7, y + 2, $"{name} Box", new Style { Foreground = color });

            y += 6;
        }
    }

    private static void AnimateContent(RenderingSystem renderingSystem)
    {
        var terminal = renderingSystem.Terminal;
        var animationX = 30;
        var animationY = 4;
        var animationWidth = terminal.Width - animationX - 5;
        var animationHeight = 20;

        // Draw animation area
        renderingSystem.DrawBox(animationX, animationY, animationWidth, animationHeight,
            new Style { Foreground = Color.White }, BoxStyle.Double);

        renderingSystem.WriteText(animationX + 2, animationY + 1,
            "Animated Content (Dirty Region Tracking)",
            new Style { Foreground = Color.Yellow });

        // Animate some moving bars
        Task.Run(async () =>
        {
            var bars = new[]
            {
                new { Y = 0, Color = Color.Red, Speed = 1.0, Char = '█' },
                new { Y = 2, Color = Color.Green, Speed = 1.5, Char = '▓' },
                new { Y = 4, Color = Color.Blue, Speed = 0.8, Char = '▒' },
                new { Y = 6, Color = Color.Magenta, Speed = 2.0, Char = '░' },
                new { Y = 8, Color = Color.Yellow, Speed = 1.2, Char = '█' }
            };

            var startTime = DateTime.UtcNow;
            var maxWidth = animationWidth - 6;

            while (true)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;

                foreach (var bar in bars)
                {
                    // Calculate position
                    var t = elapsed * bar.Speed;
                    var x = (int)((Math.Sin(t) + 1) * 0.5 * maxWidth);

                    // Clear previous position (simple approach)
                    renderingSystem.Scheduler.QueueRender(() =>
                    {
                        // Clear the line
                        for (int i = 0; i < maxWidth; i++)
                        {
                            renderingSystem.Buffer.SetCell(animationX + 3 + i,
                                animationY + 3 + bar.Y, ' ');
                        }

                        // Draw bar at new position
                        var barLength = 10;
                        for (int i = 0; i < barLength && x + i < maxWidth; i++)
                        {
                            renderingSystem.Buffer.SetCell(animationX + 3 + x + i,
                                animationY + 3 + bar.Y,
                                bar.Char,
                                new Style { Foreground = bar.Color });
                        }
                    });
                }

                await Task.Delay(16); // ~60 FPS
            }
        });

        // Animate a spinner
        Task.Run(async () =>
        {
            var spinnerChars = new[] { '⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏' };
            var spinnerIndex = 0;

            while (true)
            {
                renderingSystem.WriteText(animationX + animationWidth - 10, animationY + animationHeight - 2,
                    $"Loading {spinnerChars[spinnerIndex]}",
                    new Style { Foreground = Color.Cyan });

                spinnerIndex = (spinnerIndex + 1) % spinnerChars.Length;
                await Task.Delay(100);
            }
        });

        // Animate RGB color gradient
        Task.Run(async () =>
        {
            while (true)
            {
                var time = DateTime.UtcNow.Ticks / 10000000.0; // Seconds

                renderingSystem.Scheduler.QueueRender(() =>
                {
                    for (int x = 0; x < 20; x++)
                    {
                        var hue = (x / 20.0 + time * 0.1) % 1.0;
                        var (r, g, b) = HsvToRgb(hue, 1.0, 1.0);

                        renderingSystem.Buffer.SetCell(animationX + 3 + x,
                            animationY + animationHeight - 4,
                            '▄',
                            new Style { Foreground = Color.FromRgb((byte)r, (byte)g, (byte)b) });
                    }
                });

                await Task.Delay(50);
            }
        });
    }

    private static (int r, int g, int b) HsvToRgb(double h, double s, double v)
    {
        var c = v * s;
        var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        var m = v - c;

        double r, g, b;
        var hi = (int)(h * 6) % 6;

        switch (hi)
        {
            case 0: (r, g, b) = (c, x, 0); break;
            case 1: (r, g, b) = (x, c, 0); break;
            case 2: (r, g, b) = (0, c, x); break;
            case 3: (r, g, b) = (0, x, c); break;
            case 4: (r, g, b) = (x, 0, c); break;
            default: (r, g, b) = (c, 0, x); break;
        }

        return ((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
    }
}