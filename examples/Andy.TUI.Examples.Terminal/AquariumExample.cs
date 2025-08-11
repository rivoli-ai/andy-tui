using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates an animated aquarium with various fish, bubbles, and seaweed.
/// </summary>
public class AquariumExample
{
    private abstract class SeaCreature
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public bool FacingRight { get; set; }
        public Color Color { get; set; }
        
        public abstract void Update(double width, double height);
        public abstract void Draw(RenderingSystem renderingSystem);
    }
    
    private class Fish : SeaCreature
    {
        public FishType Type { get; set; }
        public int AnimationFrame { get; set; }
        private int _animationCounter;
        
        public override void Update(double width, double height)
        {
            // Move fish
            X += VelocityX;
            Y += VelocityY;
            
            // Bounce off walls
            if (X <= 0 || X >= width - 10)
            {
                VelocityX = -VelocityX;
                FacingRight = VelocityX > 0;
            }
            
            // Gentle vertical movement
            Y += Math.Sin(X * 0.05) * 0.05; // Slower wave motion
            
            // Keep within bounds
            if (Y < 5) Y = 5;
            if (Y > height - 10) Y = height - 10;
            
            // Animate
            _animationCounter++;
            if (_animationCounter > 10)
            {
                _animationCounter = 0;
                AnimationFrame = (AnimationFrame + 1) % 2;
            }
        }
        
        public override void Draw(RenderingSystem renderingSystem)
        {
            // Calculate water background color at fish position
            var depth = (double)Y / renderingSystem.Terminal.Height;
            var blue = (int)(50 + depth * 150);
            var green = (int)(50 + depth * 50);
            var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
            
            var style = Style.Default
                .WithForegroundColor(Color)
                .WithBackgroundColor(waterColor);
            
            switch (Type)
            {
                case FishType.Small:
                    DrawSmallFish(renderingSystem, (int)X, (int)Y, style);
                    break;
                case FishType.Medium:
                    DrawMediumFish(renderingSystem, (int)X, (int)Y, style);
                    break;
                case FishType.Large:
                    DrawLargeFish(renderingSystem, (int)X, (int)Y, style);
                    break;
                case FishType.Tropical:
                    DrawTropicalFish(renderingSystem, (int)X, (int)Y, style);
                    break;
            }
        }
        
        private void DrawSmallFish(RenderingSystem renderingSystem, int x, int y, Style style)
        {
            var text = FacingRight 
                ? (AnimationFrame == 0 ? "><>" : ">><")
                : (AnimationFrame == 0 ? "<><" : "><<");
                
            // Draw each character without background to preserve water
            for (int i = 0; i < text.Length; i++)
            {
                renderingSystem.Buffer.SetCell(x + i, y, text[i], style);
            }
        }
        
        private void DrawMediumFish(RenderingSystem renderingSystem, int x, int y, Style style)
        {
            var text = FacingRight 
                ? (AnimationFrame == 0 ? "><(((°>" : "><((°>")
                : (AnimationFrame == 0 ? "<°)))><" : "<°))><");
                
            // Draw each character without background to preserve water
            for (int i = 0; i < text.Length; i++)
            {
                renderingSystem.Buffer.SetCell(x + i, y, text[i], style);
            }
        }
        
        private void DrawLargeFish(RenderingSystem renderingSystem, int x, int y, Style baseStyle)
        {
            // Helper to draw text with appropriate water background
            void DrawTextWithWaterBg(int px, int py, string text)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] != ' ') // Skip spaces to preserve background
                    {
                        // Calculate water background color at this position
                        var depth = (double)py / renderingSystem.Terminal.Height;
                        var blue = (int)(50 + depth * 150);
                        var green = (int)(50 + depth * 50);
                        var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
                        
                        var style = baseStyle.WithBackgroundColor(waterColor);
                        renderingSystem.Buffer.SetCell(px + i, py, text[i], style);
                    }
                }
            }
            
            if (FacingRight)
            {
                DrawTextWithWaterBg(x, y - 1, "    ,");
                DrawTextWithWaterBg(x, y, AnimationFrame == 0 ? ">=(°>" : ">={°>");
                DrawTextWithWaterBg(x, y + 1, "    '");
            }
            else
            {
                DrawTextWithWaterBg(x, y - 1, ",");
                DrawTextWithWaterBg(x, y, AnimationFrame == 0 ? "<°)=<" : "<°}=<");
                DrawTextWithWaterBg(x, y + 1, "'");
            }
        }
        
        private void DrawTropicalFish(RenderingSystem renderingSystem, int x, int y, Style baseStyle)
        {
            var stripeStyle = Style.Default.WithForegroundColor(Color.FromRgb(255, 255, 0));
            
            // Helper to draw text with water background
            void DrawTextWithWaterBg(int px, int py, string text, Style s)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] != ' ')
                    {
                        var depth = (double)py / renderingSystem.Terminal.Height;
                        var blue = (int)(50 + depth * 150);
                        var green = (int)(50 + depth * 50);
                        var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
                        
                        var style = s.WithBackgroundColor(waterColor);
                        renderingSystem.Buffer.SetCell(px + i, py, text[i], style);
                    }
                }
            }
            
            // Helper to draw single char with water background
            void DrawCharWithWaterBg(int px, int py, char ch, Style s)
            {
                var depth = (double)py / renderingSystem.Terminal.Height;
                var blue = (int)(50 + depth * 150);
                var green = (int)(50 + depth * 50);
                var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
                
                var style = s.WithBackgroundColor(waterColor);
                renderingSystem.Buffer.SetCell(px, py, ch, style);
            }
            
            if (FacingRight)
            {
                DrawTextWithWaterBg(x, y - 1, "  ___", baseStyle);
                DrawCharWithWaterBg(x + 1, y, '>', baseStyle);
                DrawCharWithWaterBg(x + 2, y, '(', stripeStyle);
                DrawCharWithWaterBg(x + 3, y, AnimationFrame == 0 ? '°' : 'o', baseStyle);
                DrawCharWithWaterBg(x + 4, y, ')', stripeStyle);
                DrawCharWithWaterBg(x + 5, y, '>', baseStyle);
                DrawTextWithWaterBg(x, y + 1, "  ¯¯¯", baseStyle);
            }
            else
            {
                DrawTextWithWaterBg(x, y - 1, "___", baseStyle);
                DrawCharWithWaterBg(x, y, '<', baseStyle);
                DrawCharWithWaterBg(x + 1, y, '(', stripeStyle);
                DrawCharWithWaterBg(x + 2, y, AnimationFrame == 0 ? '°' : 'o', baseStyle);
                DrawCharWithWaterBg(x + 3, y, ')', stripeStyle);
                DrawCharWithWaterBg(x + 4, y, '<', baseStyle);
                DrawTextWithWaterBg(x, y + 1, "¯¯¯", baseStyle);
            }
        }
    }
    
    private enum FishType
    {
        Small,
        Medium,
        Large,
        Tropical
    }
    
    private class Bubble
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public double WobblePhase { get; set; }
        
        public void Update()
        {
            // Rise up
            Y -= 0.3 + (0.1 * Size);
            
            // Wobble side to side
            WobblePhase += 0.1;
            X += Math.Sin(WobblePhase) * 0.2;
        }
        
        public void Draw(RenderingSystem renderingSystem)
        {
            var x = (int)X;
            var y = (int)Y;
            
            // Calculate water background color at bubble position
            var depth = (double)y / renderingSystem.Terminal.Height;
            var blue = (int)(50 + depth * 150);
            var green = (int)(50 + depth * 50);
            var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
            
            var style = Style.Default
                .WithForegroundColor(Color.FromRgb(200, 200, 255))
                .WithBackgroundColor(waterColor);
            
            if (Size < 0.5)
                renderingSystem.Buffer.SetCell(x, y, '·', style);
            else if (Size < 1.0)
                renderingSystem.Buffer.SetCell(x, y, 'o', style);
            else
                renderingSystem.Buffer.SetCell(x, y, 'O', style);
        }
    }
    
    private class Seaweed
    {
        public int X { get; set; }
        public int Height { get; set; }
        public double SwayPhase { get; set; }
        public Color Color { get; set; }
        
        public void Update()
        {
            SwayPhase += 0.05;
        }
        
        public void Draw(RenderingSystem renderingSystem, int groundY)
        {
            for (int i = 0; i < Height; i++)
            {
                var sway = Math.Sin(SwayPhase + i * 0.3) * 2;
                var x = X + (int)sway;
                var y = groundY - i;
                
                // Calculate water background color at seaweed position
                var depth = (double)y / renderingSystem.Terminal.Height;
                var blue = (int)(50 + depth * 150);
                var green = (int)(50 + depth * 50);
                var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
                
                var style = Style.Default
                    .WithForegroundColor(Color)
                    .WithBackgroundColor(waterColor);
                
                if (i == Height - 1)
                    renderingSystem.Buffer.SetCell(x, y, '♣', style);
                else if (i % 3 == 0)
                    renderingSystem.Buffer.SetCell(x, y, ')', style);
                else if (i % 3 == 1)
                    renderingSystem.Buffer.SetCell(x, y, '|', style);
                else
                    renderingSystem.Buffer.SetCell(x, y, '(', style);
            }
        }
    }
    
    public static void Run()
    {
        Console.WriteLine("=== Terminal Aquarium ===");
        Console.WriteLine("Watch the fish swim by!");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
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
        
        // Initialize aquarium creatures
        var random = new Random();
        var fish = new List<Fish>();
        var bubbles = new List<Bubble>();
        var seaweeds = new List<Seaweed>();
        
        // Create fish
        for (int i = 0; i < 8; i++)
        {
            var fishType = (FishType)random.Next(4);
            var fish1 = new Fish
            {
                Type = fishType,
                X = random.Next(10, renderingSystem.Terminal.Width - 10),
                Y = random.Next(5, renderingSystem.Terminal.Height - 15),
                VelocityX = (random.NextDouble() - 0.5) * 0.8, // Slower movement
                FacingRight = random.NextDouble() > 0.5,
                Color = GetRandomFishColor(random),
                AnimationFrame = random.Next(2)
            };
            fish1.FacingRight = fish1.VelocityX > 0;
            fish.Add(fish1);
        }
        
        // Create seaweed
        for (int i = 0; i < renderingSystem.Terminal.Width / 15; i++)
        {
            seaweeds.Add(new Seaweed
            {
                X = random.Next(5, renderingSystem.Terminal.Width - 5),
                Height = random.Next(5, 12),
                SwayPhase = random.NextDouble() * Math.PI * 2,
                Color = Color.FromRgb(0, (byte)random.Next(100, 200), 0)
            });
        }
        
        // Animation parameters
        var frameCount = 0;
        var startTime = DateTime.Now;
        var lastFpsUpdate = DateTime.Now;
        var framesSinceLastUpdate = 0;
        var currentFps = 0.0;
        
        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 20;
        
        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit)
                return;
                
            // Clear buffer
            renderingSystem.Clear();
            
            // Draw water background
            DrawWaterBackground(renderingSystem);
            
            // Draw sand at bottom
            DrawSandBottom(renderingSystem);
            
            // Update and draw seaweed
            foreach (var weed in seaweeds)
            {
                weed.Update();
                weed.Draw(renderingSystem, renderingSystem.Terminal.Height - 5);
            }
            
            // Update and draw fish
            foreach (var f in fish)
            {
                f.Update(renderingSystem.Terminal.Width, renderingSystem.Terminal.Height);
                f.Draw(renderingSystem);
            }
            
            // Create new bubbles occasionally
            if (frameCount % 30 == 0)
            {
                bubbles.Add(new Bubble
                {
                    X = random.Next(5, renderingSystem.Terminal.Width - 5),
                    Y = renderingSystem.Terminal.Height - 6,
                    Size = random.NextDouble() * 1.5,
                    WobblePhase = random.NextDouble() * Math.PI * 2
                });
            }
            
            // Update and draw bubbles
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                var bubble = bubbles[i];
                bubble.Update();
                
                if (bubble.Y < 2)
                {
                    bubbles.RemoveAt(i);
                }
                else
                {
                    bubble.Draw(renderingSystem);
                }
            }
            
            // Draw title
            var titleStyle = Style.Default
                .WithForegroundColor(Color.White)
                .WithBackgroundColor(Color.FromRgb(0, 50, 100));
            renderingSystem.WriteText(2, 1, " 🐠 Terminal Aquarium 🐟 ", titleStyle);
            
            // Draw info
            renderingSystem.WriteText(2, renderingSystem.Terminal.Height - 2, "ESC or Q to exit", 
                Style.Default.WithForegroundColor(Color.FromRgb(100, 100, 100)));
            
            // Update FPS calculation
            frameCount++;
            framesSinceLastUpdate++;
            
            var currentTime = DateTime.Now;
            if ((currentTime - lastFpsUpdate).TotalSeconds >= 0.5)
            {
                currentFps = framesSinceLastUpdate / (currentTime - lastFpsUpdate).TotalSeconds;
                framesSinceLastUpdate = 0;
                lastFpsUpdate = currentTime;
            }
            
            // Draw FPS with water background
            var fpsDepth = 1.0 / renderingSystem.Terminal.Height;
            var fpsBlue = (int)(50 + fpsDepth * 150);
            var fpsGreen = (int)(50 + fpsDepth * 50);
            var fpsWaterColor = Color.FromRgb(0, (byte)fpsGreen, (byte)fpsBlue);
            var fpsStyle = Style.Default
                .WithForegroundColor(Color.BrightGreen)
                .WithBackgroundColor(fpsWaterColor);
            renderingSystem.WriteText(renderingSystem.Terminal.Width - 15, 1, $"FPS: {currentFps:F1}", fpsStyle);
            
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
        Console.WriteLine("\nThanks for visiting the aquarium!");
    }
    
    private static void DrawWaterBackground(RenderingSystem renderingSystem)
    {
        // Create gradient water effect
        for (int y = 0; y < renderingSystem.Terminal.Height; y++)
        {
            var depth = (double)y / renderingSystem.Terminal.Height;
            var blue = (int)(50 + depth * 150);
            var green = (int)(50 + depth * 50);
            var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
            var style = Style.Default.WithBackgroundColor(waterColor);
            
            for (int x = 0; x < renderingSystem.Terminal.Width; x++)
            {
                renderingSystem.Buffer.SetCell(x, y, ' ', style);
            }
        }
    }
    
    private static void DrawSandBottom(RenderingSystem renderingSystem)
    {
        var sandColor = Color.FromRgb(194, 178, 128);
        var darkSandColor = Color.FromRgb(160, 140, 90);
        
        for (int y = renderingSystem.Terminal.Height - 5; y < renderingSystem.Terminal.Height; y++)
        {
            for (int x = 0; x < renderingSystem.Terminal.Width; x++)
            {
                var isDark = (x + y) % 3 == 0;
                var style = Style.Default.WithBackgroundColor(isDark ? darkSandColor : sandColor);
                renderingSystem.Buffer.SetCell(x, y, ' ', style);
                
                // Add some texture
                if (y == renderingSystem.Terminal.Height - 5 && x % 7 == 0)
                {
                    renderingSystem.Buffer.SetCell(x, y, '~', Style.Default.WithForegroundColor(darkSandColor));
                }
            }
        }
    }
    
    private static Color GetRandomFishColor(Random random)
    {
        var colors = new[]
        {
            Color.FromRgb(255, 100, 0),   // Orange
            Color.FromRgb(255, 255, 0),   // Yellow
            Color.FromRgb(255, 0, 100),   // Pink
            Color.FromRgb(100, 255, 100), // Light green
            Color.FromRgb(255, 200, 100), // Gold
            Color.FromRgb(200, 100, 255), // Purple
            Color.FromRgb(100, 200, 255), // Light blue
            Color.FromRgb(255, 100, 200)  // Magenta
        };
        
        return colors[random.Next(colors.Length)];
    }
}