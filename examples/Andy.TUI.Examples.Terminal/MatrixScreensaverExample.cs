using Andy.TUI.Terminal;
using System.Diagnostics;
using System.Text;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a Matrix-style digital rain screensaver effect.
/// </summary>
public class MatrixScreensaverExample
{
    private class MatrixColumn
    {
        public int Position { get; set; }
        public int Speed { get; set; }
        public int Length { get; set; }
        public int TrailLength { get; set; }
        public string[] Characters { get; set; }
        public int CharIndex { get; set; }
        
        public MatrixColumn(int height, Random random)
        {
            Reset(height, random);
            Position = random.Next(-height, 0); // Start some columns above screen
            Characters = new string[height + TrailLength];
            GenerateCharacters(random);
        }
        
        public void Reset(int height, Random random)
        {
            Position = -random.Next(5, 20); // Random start position above screen
            Speed = random.Next(1, 3); // Slower fall speeds (1-2 instead of 1-3)
            Length = random.Next(5, height / 2); // Variable column lengths
            TrailLength = random.Next(10, 20); // Trail fade length
            CharIndex = 0;
        }
        
        public void GenerateCharacters(Random random)
        {
            // Matrix characters - mix of Katakana, numbers, and symbols
            var matrixChars = new[]
            {
                // Katakana characters
                "ｱ", "ｲ", "ｳ", "ｴ", "ｵ", "ｶ", "ｷ", "ｸ", "ｹ", "ｺ",
                "ｻ", "ｼ", "ｽ", "ｾ", "ｿ", "ﾀ", "ﾁ", "ﾂ", "ﾃ", "ﾄ",
                "ﾅ", "ﾆ", "ﾇ", "ﾈ", "ﾉ", "ﾊ", "ﾋ", "ﾌ", "ﾍ", "ﾎ",
                "ﾏ", "ﾐ", "ﾑ", "ﾒ", "ﾓ", "ﾔ", "ﾕ", "ﾖ", "ﾗ", "ﾘ",
                "ﾙ", "ﾚ", "ﾛ", "ﾜ", "ﾝ",
                // Numbers and symbols
                "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                ":", ".", "=", "*", "+", "-", "<", ">", "¦", "|"
            };
            
            for (int i = 0; i < Characters.Length; i++)
            {
                Characters[i] = matrixChars[random.Next(matrixChars.Length)];
            }
        }
        
        public void Update(Random random)
        {
            // Update position every other frame for smoother, slower movement at 60 FPS
            if (random.NextDouble() < 0.5 + (Speed * 0.25))
            {
                Position += Speed;
            }
            
            // Occasionally change a character
            if (random.NextDouble() < 0.02)
            {
                CharIndex = random.Next(Characters.Length);
                GenerateCharacters(random);
            }
        }
    }
    
    public static void Run()
    {
        Console.WriteLine("=== Matrix Screensaver Example ===");
        Console.WriteLine("Digital rain effect inspired by The Matrix");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
        // Hide cursor for better effect
        terminal.CursorVisible = false;
        
        // Create input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        inputHandler.KeyPressed += (_, e) =>
        {
            exit = true; // Any key exits
        };
        inputHandler.Start();
        
        // Initialize columns (accounting for stats area)
        var random = new Random();
        var statsHeight = 7; // Height of statistics box
        var effectiveHeight = renderingSystem.Terminal.Height - statsHeight - 1;
        var columns = new MatrixColumn[renderingSystem.Terminal.Width];
        for (int i = 0; i < columns.Length; i++)
        {
            columns[i] = new MatrixColumn(effectiveHeight, random);
        }
        
        // Animation parameters
        var frameCount = 0;
        var startTime = DateTime.Now;
        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 60;
        
        // Performance tracking
        var lastFpsUpdate = DateTime.Now;
        var framesSinceLastUpdate = 0;
        var currentFps = 0.0;
        
        // Color styles
        var brightGreen = Style.Default.WithForegroundColor(Color.FromRgb(0, 255, 0));
        var mediumGreen = Style.Default.WithForegroundColor(Color.FromRgb(0, 200, 0));
        var darkGreen = Style.Default.WithForegroundColor(Color.FromRgb(0, 150, 0));
        var veryDarkGreen = Style.Default.WithForegroundColor(Color.FromRgb(0, 100, 0));
        var fadedGreen = Style.Default.WithForegroundColor(Color.FromRgb(0, 50, 0));
        
        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit)
                return;
                
            // Clear with black background
            renderingSystem.Clear();
            
            // Update and draw each column
            for (int x = 0; x < columns.Length; x++)
            {
                var column = columns[x];
                column.Update(random);
                
                // Draw the column (stop before statistics area)
                var maxY = renderingSystem.Terminal.Height - statsHeight - 1;
                
                for (int i = 0; i < column.Length + column.TrailLength; i++)
                {
                    int y = column.Position - i;
                    
                    if (y >= 0 && y < maxY && i < column.Characters.Length)
                    {
                        Style style;
                        
                        if (i == 0)
                        {
                            // Leading character is white/bright
                            style = Style.Default.WithForegroundColor(Color.White);
                        }
                        else if (i < 3)
                        {
                            // Next few are bright green
                            style = brightGreen;
                        }
                        else if (i < column.Length)
                        {
                            // Main body is medium green
                            style = mediumGreen;
                        }
                        else if (i < column.Length + 5)
                        {
                            // Start of trail
                            style = darkGreen;
                        }
                        else if (i < column.Length + 10)
                        {
                            // Mid trail
                            style = veryDarkGreen;
                        }
                        else
                        {
                            // End of trail
                            style = fadedGreen;
                        }
                        
                        // Occasionally make a character flicker
                        if (random.NextDouble() < 0.001)
                        {
                            style = Style.Default.WithForegroundColor(Color.White);
                        }
                        
                        renderingSystem.WriteText(x, y, column.Characters[i], style);
                    }
                }
                
                // Reset column when it goes off screen (considering stats area)
                if (column.Position - column.Length - column.TrailLength > maxY)
                {
                    column.Reset(maxY, random);
                    column.GenerateCharacters(random);
                }
            }
            
            // Draw statistics at bottom
            DrawStatistics(renderingSystem, frameCount, startTime, currentFps, columns);
            
            frameCount++;
            framesSinceLastUpdate++;
            
            // Update FPS calculation
            var currentTime = DateTime.Now;
            if ((currentTime - lastFpsUpdate).TotalSeconds >= 0.5)
            {
                currentFps = framesSinceLastUpdate / (currentTime - lastFpsUpdate).TotalSeconds;
                framesSinceLastUpdate = 0;
                lastFpsUpdate = currentTime;
            }
            
            // Queue next frame
            renderingSystem.Scheduler.QueueRender(renderFrame);
        };
        
        // Start animation
        renderingSystem.Scheduler.QueueRender(renderFrame);
        
        // Wait for exit
        while (!exit)
        {
            Thread.Sleep(50);
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
        // Restore cursor
        terminal.CursorVisible = true;
        renderingSystem.Shutdown();
        
        Console.Clear();
        Console.WriteLine("\nExiting the Matrix...");
    }
    
    private static void DrawStatistics(RenderingSystem renderingSystem, int frameCount, DateTime startTime, double fps, MatrixColumn[] columns)
    {
        // Calculate statistics
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        var avgFps = frameCount / elapsed;
        
        // Get memory stats
        var currentProcess = Process.GetCurrentProcess();
        var workingSet = currentProcess.WorkingSet64 / (1024 * 1024); // MB
        var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);
        
        // Create a semi-transparent background for stats
        var statsY = renderingSystem.Terminal.Height - 6;
        var statsStyle = Style.Default.WithForegroundColor(Color.FromRgb(0, 150, 0));
        var labelStyle = Style.Default.WithForegroundColor(Color.FromRgb(0, 100, 0));
        
        // Draw stats border
        renderingSystem.DrawBox(0, statsY - 1, renderingSystem.Terminal.Width, 7, labelStyle, BoxStyle.Single);
        
        // Title
        var title = " MATRIX DIGITAL RAIN ";
        renderingSystem.WriteText((renderingSystem.Terminal.Width - title.Length) / 2, statsY - 1, title, statsStyle);
        
        // Performance stats
        renderingSystem.WriteText(2, statsY, "FPS: ", labelStyle);
        renderingSystem.WriteText(7, statsY, $"{fps:F1} (avg: {avgFps:F1})", statsStyle);
        renderingSystem.WriteText(30, statsY, "Frame: ", labelStyle);
        renderingSystem.WriteText(37, statsY, $"{frameCount}", statsStyle);
        renderingSystem.WriteText(50, statsY, "Time: ", labelStyle);
        renderingSystem.WriteText(56, statsY, $"{elapsed:F1}s", statsStyle);
        
        // Memory stats
        renderingSystem.WriteText(2, statsY + 1, "Memory: ", labelStyle);
        renderingSystem.WriteText(10, statsY + 1, $"{gcMemory:F1} MB", statsStyle);
        renderingSystem.WriteText(25, statsY + 1, "Working Set: ", labelStyle);
        renderingSystem.WriteText(38, statsY + 1, $"{workingSet:F1} MB", statsStyle);
        
        // GC stats
        renderingSystem.WriteText(2, statsY + 2, "GC: ", labelStyle);
        renderingSystem.WriteText(6, statsY + 2, $"Gen0: {gen0}  Gen1: {gen1}  Gen2: {gen2}", statsStyle);
        
        // Matrix stats
        var activeColumns = columns.Count(c => c.Position >= -c.TrailLength && c.Position <= renderingSystem.Terminal.Height);
        var totalCharacters = columns.Sum(c => c.Characters.Length);
        renderingSystem.WriteText(2, statsY + 3, "Rain Columns: ", labelStyle);
        renderingSystem.WriteText(16, statsY + 3, $"{activeColumns}/{columns.Length}", statsStyle);
        renderingSystem.WriteText(30, statsY + 3, "Characters: ", labelStyle);
        renderingSystem.WriteText(42, statsY + 3, $"{totalCharacters:N0}", statsStyle);
        renderingSystem.WriteText(55, statsY + 3, "Speed: ", labelStyle);
        renderingSystem.WriteText(62, statsY + 3, "Variable", statsStyle);
        
        // Exit instruction
        renderingSystem.WriteText(2, statsY + 4, "Press any key to exit", 
            Style.Default.WithForegroundColor(Color.FromRgb(0, 200, 0)).WithDim());
    }
}