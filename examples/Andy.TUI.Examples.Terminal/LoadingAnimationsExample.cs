using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates various loading animation patterns and progress indicators.
/// </summary>
public class LoadingAnimationsExample
{
    private enum AnimationType
    {
        Spinner,
        ProgressBar,
        Dots,
        Pulse,
        Snake,
        Bounce,
        Wave,
        Matrix
    }

    private class LoadingAnimation
    {
        public string Name { get; set; } = "";
        public AnimationType Type { get; set; }
        public int Progress { get; set; }
        public int Frame { get; set; }
        public bool IsComplete { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public static void Run()
    {
        Console.WriteLine("=== Loading Animations Showcase ===");
        Console.WriteLine("Demonstrating various loading patterns and progress indicators");
        Console.WriteLine("Starting animations...");
        Thread.Sleep(1000);

        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);

        // Hide cursor
        terminal.CursorVisible = false;

        // Create input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
        };
        inputHandler.Start();

        // Initialize animations
        var animations = new List<LoadingAnimation>
        {
            new LoadingAnimation { Name = "Spinner", Type = AnimationType.Spinner, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(8) },
            new LoadingAnimation { Name = "Progress Bar", Type = AnimationType.ProgressBar, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(10) },
            new LoadingAnimation { Name = "Loading Dots", Type = AnimationType.Dots, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(6) },
            new LoadingAnimation { Name = "Pulse Effect", Type = AnimationType.Pulse, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(12) },
            new LoadingAnimation { Name = "Snake Loader", Type = AnimationType.Snake, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(9) },
            new LoadingAnimation { Name = "Bounce Ball", Type = AnimationType.Bounce, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(7) },
            new LoadingAnimation { Name = "Wave Pattern", Type = AnimationType.Wave, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(11) },
            new LoadingAnimation { Name = "Matrix Rain", Type = AnimationType.Matrix, StartTime = DateTime.Now, Duration = TimeSpan.FromSeconds(15) }
        };

        var frameCount = 0;
        var startTime = DateTime.Now;

        while (!exit && animations.Any(a => !a.IsComplete))
        {
            renderer.BeginFrame();
            renderer.Clear();

            // Update animations
            var currentTime = DateTime.Now;
            foreach (var animation in animations)
            {
                var elapsed = currentTime - animation.StartTime;
                animation.Progress = Math.Min(100, (int)((elapsed.TotalSeconds / animation.Duration.TotalSeconds) * 100));
                animation.IsComplete = elapsed >= animation.Duration;
                animation.Frame++;
            }

            // Draw title
            var titleStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
            renderer.DrawText(2, 1, "🔄 Loading Animations Showcase", titleStyle);

            // Draw animations
            int yOffset = 4;
            foreach (var animation in animations)
            {
                DrawAnimation(renderer, animation, 2, yOffset, 50);
                yOffset += 3;
            }

            // Draw instructions
            var instructionStyle = Style.Default.WithForegroundColor(Color.DarkGray);
            renderer.DrawText(2, renderer.Height - 2, "Press ESC or Q to exit", instructionStyle);

            // Draw performance stats
            var totalElapsed = (DateTime.Now - startTime).TotalSeconds;
            var fps = frameCount / totalElapsed;
            var statsStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(renderer.Width - 15, 1, $"FPS: {fps:F1}", statsStyle);

            renderer.EndFrame();
            frameCount++;
            Thread.Sleep(50); // ~20 FPS for smooth animation
        }

        inputHandler.Stop();
        inputHandler.Dispose();

        // Restore cursor
        terminal.CursorVisible = true;

        Console.Clear();
        Console.WriteLine("\nAll animations completed!");
    }

    private static void DrawAnimation(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        var nameStyle = Style.Default.WithForegroundColor(Color.White);
        var progressStyle = Style.Default.WithForegroundColor(Color.Cyan);
        var completeStyle = Style.Default.WithForegroundColor(Color.Green);

        // Draw animation name
        renderer.DrawText(x, y, animation.Name, animation.IsComplete ? completeStyle : nameStyle);

        // Draw progress percentage
        var progressText = animation.IsComplete ? "✓ Complete" : $"{animation.Progress}%";
        renderer.DrawText(x + 20, y, progressText, animation.IsComplete ? completeStyle : progressStyle);

        // Draw the specific animation
        switch (animation.Type)
        {
            case AnimationType.Spinner:
                DrawSpinner(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.ProgressBar:
                DrawProgressBar(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Dots:
                DrawLoadingDots(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Pulse:
                DrawPulse(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Snake:
                DrawSnakeLoader(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Bounce:
                DrawBounceBall(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Wave:
                DrawWave(renderer, animation, x, y + 1, width);
                break;
            case AnimationType.Matrix:
                DrawMatrixRain(renderer, animation, x, y + 1, width);
                break;
        }
    }

    private static void DrawSpinner(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete) return;

        var spinnerChars = new[] { '|', '/', '-', '\\' };
        var spinnerIndex = (animation.Frame / 3) % spinnerChars.Length;
        var style = Style.Default.WithForegroundColor(Color.Yellow);

        renderer.DrawChar(x, y, spinnerChars[spinnerIndex], style);
        renderer.DrawText(x + 2, y, "Loading...", style);
    }

    private static void DrawProgressBar(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        var barWidth = Math.Min(width - 10, 40);
        var filledWidth = (animation.Progress * barWidth) / 100;

        var frameStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        var fillStyle = Style.Default.WithForegroundColor(animation.IsComplete ? Color.Green : Color.Blue);
        var emptyStyle = Style.Default.WithForegroundColor(Color.DarkGray);

        // Draw frame
        renderer.DrawChar(x, y, '[', frameStyle);
        renderer.DrawChar(x + barWidth + 1, y, ']', frameStyle);

        // Draw progress
        for (int i = 0; i < barWidth; i++)
        {
            if (i < filledWidth)
            {
                renderer.DrawChar(x + 1 + i, y, '█', fillStyle);
            }
            else
            {
                renderer.DrawChar(x + 1 + i, y, '░', emptyStyle);
            }
        }
    }

    private static void DrawLoadingDots(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(x, y, "● ● ● Done!", completeStyle);
            return;
        }

        var dotCount = 3;
        var activeDot = (animation.Frame / 10) % (dotCount + 1);
        var style = Style.Default.WithForegroundColor(Color.Magenta);
        var dimStyle = Style.Default.WithForegroundColor(Color.DarkGray);

        for (int i = 0; i < dotCount; i++)
        {
            var dotStyle = (i <= activeDot) ? style : dimStyle;
            renderer.DrawChar(x + i * 2, y, '●', dotStyle);
        }

        renderer.DrawText(x + 8, y, "Loading", style);
    }

    private static void DrawPulse(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green).WithBold();
            renderer.DrawText(x, y, "◉ Ready!", completeStyle);
            return;
        }

        var pulsePhase = Math.Sin((animation.Frame * 0.2)) * 0.5 + 0.5;
        var intensity = (int)(pulsePhase * 255);
        var color = Color.FromRgb((byte)intensity, 0, (byte)(255 - intensity));
        var style = Style.Default.WithForegroundColor(color);

        var pulseChar = pulsePhase > 0.7 ? '◉' : pulsePhase > 0.4 ? '◎' : '○';
        renderer.DrawChar(x, y, pulseChar, style);
        renderer.DrawText(x + 2, y, "Pulsing...", style);
    }

    private static void DrawSnakeLoader(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(x, y, "▓▓▓▓▓▓▓▓ Complete!", completeStyle);
            return;
        }

        var snakeLength = 8;
        var position = (animation.Frame / 2) % (width - 20);
        var style = Style.Default.WithForegroundColor(Color.Cyan);
        var tailStyle = Style.Default.WithForegroundColor(Color.Blue);

        // Clear the line
        for (int i = 0; i < width - 20; i++)
        {
            renderer.DrawChar(x + i, y, ' ', Style.Default);
        }

        // Draw snake
        for (int i = 0; i < snakeLength; i++)
        {
            var segmentX = position - i;
            if (segmentX >= 0 && segmentX < width - 20)
            {
                var segmentStyle = i == 0 ? style : tailStyle;
                var segmentChar = i == 0 ? '▓' : '▒';
                renderer.DrawChar(x + segmentX, y, segmentChar, segmentStyle);
            }
        }
    }

    private static void DrawBounceBall(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(x, y, "● Finished bouncing!", completeStyle);
            return;
        }

        var bounceWidth = Math.Min(width - 20, 30);
        var t = (animation.Frame * 0.15) % (Math.PI * 2);
        var ballX = (int)(Math.Sin(t) * (bounceWidth / 2) + (bounceWidth / 2));
        var ballY = Math.Abs(Math.Sin(t * 2)) > 0.5 ? 0 : 1;

        var style = Style.Default.WithForegroundColor(Color.Red);

        // Clear the bounce area
        for (int i = 0; i < bounceWidth; i++)
        {
            renderer.DrawChar(x + i, y, ' ', Style.Default);
            renderer.DrawChar(x + i, y + 1, ' ', Style.Default);
        }

        // Draw ball
        renderer.DrawChar(x + ballX, y + ballY, '●', style);

        // Draw floor
        var floorStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        for (int i = 0; i < bounceWidth; i++)
        {
            renderer.DrawChar(x + i, y + 2, '─', floorStyle);
        }
    }

    private static void DrawWave(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(x, y, "∿∿∿∿∿∿∿∿ Wave complete!", completeStyle);
            return;
        }

        var waveWidth = Math.Min(width - 20, 35);
        var time = animation.Frame * 0.3;
        var style = Style.Default.WithForegroundColor(Color.Blue);

        for (int i = 0; i < waveWidth; i++)
        {
            var waveHeight = Math.Sin((i * 0.5) + time) * 2;
            var waveY = y + (int)waveHeight + 1;
            
            // Clear column
            for (int clearY = y - 1; clearY <= y + 3; clearY++)
            {
                renderer.DrawChar(x + i, clearY, ' ', Style.Default);
            }
            
            // Draw wave point
            if (waveY >= y - 1 && waveY <= y + 3)
            {
                renderer.DrawChar(x + i, waveY, '∿', style);
            }
        }
    }

    private static void DrawMatrixRain(TerminalRenderer renderer, LoadingAnimation animation, int x, int y, int width)
    {
        if (animation.IsComplete)
        {
            var completeStyle = Style.Default.WithForegroundColor(Color.Green);
            renderer.DrawText(x, y, "█ █ █ Matrix loaded!", completeStyle);
            return;
        }

        var matrixWidth = Math.Min(width - 20, 20);
        var random = new Random(animation.Frame);
        
        for (int i = 0; i < matrixWidth; i += 2)
        {
            if (random.NextDouble() > 0.7)
            {
                var intensity = random.NextDouble();
                var green = (byte)(intensity * 255);
                var style = Style.Default.WithForegroundColor(Color.FromRgb(0, green, 0));
                
                var matrixChar = random.NextDouble() > 0.5 ? '1' : '0';
                renderer.DrawChar(x + i, y, matrixChar, style);
            }
            else
            {
                renderer.DrawChar(x + i, y, ' ', Style.Default);
            }
        }
    }
}