using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates double buffering with the TerminalRenderer for smooth animations.
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
        using var renderer = new TerminalRenderer(terminal);
        
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
                X = random.Next(10, renderer.Width - 10),
                Y = random.Next(5, renderer.Height - 10), // Account for stats area
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
        
        // Animation loop
        var frameCount = 0;
        var startTime = DateTime.Now;
        var targetFps = 60.0; // Target 60 FPS
        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / targetFps);
        var nextFrameTime = DateTime.Now;
        
        while (!exit && frameCount < 600) // Run for ~10 seconds at 60 FPS
        {
            renderer.BeginFrame();
            
            // Clear the screen
            renderer.Clear();
            
            // Draw border (leave room for stats at bottom)
            renderer.DrawBox(0, 0, renderer.Width, renderer.Height - 4, BorderStyle.Double, 
                Style.WithForeground(Color.DarkGray));
            
            // Draw title
            var title = " Double Buffer Animation ";
            renderer.DrawText((renderer.Width - title.Length) / 2, 0, title, 
                Style.WithForeground(Color.White).WithBold());
            
            // Update and draw balls
            foreach (var ball in balls)
            {
                // Update position
                ball.X += ball.VX;
                ball.Y += ball.VY;
                
                // Bounce off walls
                if (ball.X <= 1 || ball.X >= renderer.Width - 2)
                {
                    ball.VX = -ball.VX;
                    ball.X = Math.Clamp(ball.X, 1, renderer.Width - 2);
                }
                if (ball.Y <= 1 || ball.Y >= renderer.Height - 6) // Account for stats area
                {
                    ball.VY = -ball.VY;
                    ball.Y = Math.Clamp(ball.Y, 1, renderer.Height - 6);
                }
                
                // Draw ball with trail effect
                if (ball.PrevX != 0 && ball.PrevY != 0)
                {
                    // Draw fading trail
                    var trailStyle = ball.Style.WithDim();
                    renderer.DrawChar(ball.PrevX, ball.PrevY, '·', trailStyle);
                }
                
                // Draw ball
                renderer.DrawChar(ball.X, ball.Y, ball.Symbol, ball.Style);
                
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
            var statsY = renderer.Height - 4;
            renderer.DrawText(2, statsY, $"FPS: {fps:F1} | Frame: {frameCount} | Target: {targetFps} FPS",
                Style.WithForeground(Color.Yellow));
            renderer.DrawText(2, statsY + 1, $"Memory: {gcMemory:F1} MB (Working Set: {workingSet:F1} MB)",
                Style.WithForeground(Color.Cyan));
            renderer.DrawText(2, statsY + 2, $"GC Gen 0: {gen0} | Gen 1: {gen1} | Gen 2: {gen2}",
                Style.WithForeground(Color.Green));
            renderer.DrawText(2, statsY + 3, "Press ESC or Q to exit",
                Style.WithForeground(Color.White).WithDim());
            
            renderer.EndFrame();
            
            frameCount++;
            
            // Precise frame rate control
            var now = DateTime.Now;
            var sleepTime = nextFrameTime - now;
            if (sleepTime > TimeSpan.Zero)
            {
                Thread.Sleep(sleepTime);
            }
            nextFrameTime += targetFrameTime;
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
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