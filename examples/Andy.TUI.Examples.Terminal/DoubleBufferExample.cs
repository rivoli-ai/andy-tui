using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates double buffering with the RenderingSystem for smooth animations.
/// </summary>
public class DoubleBufferExample
{
    public static void Run()
    {
        Console.WriteLine("=== Double Buffer Example ===");
        Console.WriteLine("This example shows smooth animation using double buffering.");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);

        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        // Animation parameters
        const int ballCount = 5;
        var balls = new Ball[ballCount];
        var random = new Random();
        var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta };

        // Initialize balls with random positions and velocities
        for (int i = 0; i < ballCount; i++)
        {
            balls[i] = new Ball
            {
                X = random.Next(10, renderingSystem.Terminal.Width - 10),
                Y = random.Next(5, renderingSystem.Terminal.Height - 10), // Account for stats area
                VX = random.Next(-2, 3),
                VY = random.Next(-1, 2),
                Symbol = '●',
                Style = Style.WithForeground(colors[i % colors.Length])
            };
        }

        // Create a simple input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
        };
        inputHandler.Start();

        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 60;

        // Animation state
        var frameCount = 0;
        var startTime = DateTime.Now;

        // Animation function that will be called by the scheduler
        Action? animateFrame = null;
        animateFrame = () =>
        {
            if (exit || frameCount >= 600) // Run for ~10 seconds at 60 FPS
                return;

            // Clear the screen
            renderingSystem.Clear();

            // Draw border (leave room for stats at bottom)
            renderingSystem.DrawBox(0, 0, renderingSystem.Terminal.Width, renderingSystem.Terminal.Height - 4,
                Style.WithForeground(Color.DarkGray), BoxStyle.Double);

            // Draw title
            var title = " Double Buffer Animation ";
            renderingSystem.WriteText((renderingSystem.Terminal.Width - title.Length) / 2, 0, title,
                Style.WithForeground(Color.White).WithBold());

            // Update and draw balls
            foreach (var ball in balls)
            {
                // Update position
                ball.X += ball.VX;
                ball.Y += ball.VY;

                // Bounce off walls
                if (ball.X <= 1 || ball.X >= renderingSystem.Terminal.Width - 2)
                {
                    ball.VX = -ball.VX;
                    ball.X = Math.Clamp(ball.X, 1, renderingSystem.Terminal.Width - 2);
                }
                if (ball.Y <= 1 || ball.Y >= renderingSystem.Terminal.Height - 6) // Account for stats area
                {
                    ball.VY = -ball.VY;
                    ball.Y = Math.Clamp(ball.Y, 1, renderingSystem.Terminal.Height - 6);
                }

                // Draw ball with trail effect
                if (ball.PrevX != 0 && ball.PrevY != 0)
                {
                    // Draw fading trail
                    var trailStyle = ball.Style.WithDim();
                    renderingSystem.Buffer.SetCell(ball.PrevX, ball.PrevY, '·', trailStyle);
                }

                // Draw ball
                renderingSystem.Buffer.SetCell(ball.X, ball.Y, ball.Symbol, ball.Style);

                // Store previous position
                ball.PrevX = ball.X;
                ball.PrevY = ball.Y;
            }

            // Draw stats
            var elapsed = (DateTime.Now - startTime).TotalSeconds;
            var fps = frameCount / elapsed;

            // Get memory stats
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64 / (1024 * 1024); // MB
            var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            // Draw multiple stat lines
            var statsY = renderingSystem.Terminal.Height - 4;
            renderingSystem.WriteText(2, statsY, $"FPS: {fps:F1} | Frame: {frameCount} | Target: {renderingSystem.Scheduler.TargetFps} FPS",
                Style.WithForeground(Color.Yellow));
            renderingSystem.WriteText(2, statsY + 1, $"Memory: {gcMemory:F1} MB (Working Set: {workingSet:F1} MB)",
                Style.WithForeground(Color.Cyan));
            renderingSystem.WriteText(2, statsY + 2, $"GC Gen 0: {gen0} | Gen 1: {gen1} | Gen 2: {gen2}",
                Style.WithForeground(Color.Green));
            renderingSystem.WriteText(2, statsY + 3, "Press ESC or Q to exit",
                Style.WithForeground(Color.White).WithDim());

            frameCount++;

            // Queue the next frame
            renderingSystem.Scheduler.QueueRender(animateFrame);
        };

        // Start the animation
        renderingSystem.Scheduler.QueueRender(animateFrame);

        // Main thread waits for exit
        while (!exit && frameCount < 600)
        {
            inputHandler.Poll();
            Thread.Sleep(10);
        }

        inputHandler.Stop();
        inputHandler.Dispose();
        renderingSystem.Shutdown();

        Console.Clear();
        Console.WriteLine("\nAnimation complete!");
        Console.WriteLine($"Total frames: {frameCount}");
        Console.WriteLine($"Average FPS: {frameCount / (DateTime.Now - startTime).TotalSeconds:F1}");
    }

    private class Ball
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int PrevX { get; set; }
        public int PrevY { get; set; }
        public int VX { get; set; }
        public int VY { get; set; }
        public char Symbol { get; set; }
        public Style Style { get; set; }
    }
}