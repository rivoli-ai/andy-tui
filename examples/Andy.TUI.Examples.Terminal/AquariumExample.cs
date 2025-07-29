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
        public abstract void Draw(TerminalRenderer renderer);
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
            Y += Math.Sin(X * 0.1) * 0.1;
            
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
        
        public override void Draw(TerminalRenderer renderer)
        {
            var style = Style.Default.WithForegroundColor(Color);
            
            switch (Type)
            {
                case FishType.Small:
                    DrawSmallFish(renderer, (int)X, (int)Y, style);
                    break;
                case FishType.Medium:
                    DrawMediumFish(renderer, (int)X, (int)Y, style);
                    break;
                case FishType.Large:
                    DrawLargeFish(renderer, (int)X, (int)Y, style);
                    break;
                case FishType.Tropical:
                    DrawTropicalFish(renderer, (int)X, (int)Y, style);
                    break;
            }
        }
        
        private void DrawSmallFish(TerminalRenderer renderer, int x, int y, Style style)
        {
            if (FacingRight)
            {
                renderer.DrawText(x, y, AnimationFrame == 0 ? "><>" : ">><", style);
            }
            else
            {
                renderer.DrawText(x, y, AnimationFrame == 0 ? "<><" : "><<", style);
            }
        }
        
        private void DrawMediumFish(TerminalRenderer renderer, int x, int y, Style style)
        {
            if (FacingRight)
            {
                renderer.DrawText(x, y, AnimationFrame == 0 ? "><(((°>" : "><((°>", style);
            }
            else
            {
                renderer.DrawText(x, y, AnimationFrame == 0 ? "<°)))><" : "<°))><", style);
            }
        }
        
        private void DrawLargeFish(TerminalRenderer renderer, int x, int y, Style style)
        {
            if (FacingRight)
            {
                renderer.DrawText(x, y - 1, "    ,", style);
                renderer.DrawText(x, y, AnimationFrame == 0 ? ">=(°>" : ">={°>", style);
                renderer.DrawText(x, y + 1, "    '", style);
            }
            else
            {
                renderer.DrawText(x, y - 1, ",", style);
                renderer.DrawText(x, y, AnimationFrame == 0 ? "<°)=<" : "<°}=<", style);
                renderer.DrawText(x, y + 1, "'", style);
            }
        }
        
        private void DrawTropicalFish(TerminalRenderer renderer, int x, int y, Style style)
        {
            var stripeStyle = Style.Default.WithForegroundColor(Color.FromRgb(255, 255, 0));
            
            if (FacingRight)
            {
                renderer.DrawText(x, y - 1, "  ___", style);
                renderer.DrawChar(x + 1, y, '>', style);
                renderer.DrawChar(x + 2, y, '(', stripeStyle);
                renderer.DrawChar(x + 3, y, AnimationFrame == 0 ? '°' : 'o', style);
                renderer.DrawChar(x + 4, y, ')', stripeStyle);
                renderer.DrawChar(x + 5, y, '>', style);
                renderer.DrawText(x, y + 1, "  ¯¯¯", style);
            }
            else
            {
                renderer.DrawText(x, y - 1, "___", style);
                renderer.DrawChar(x, y, '<', style);
                renderer.DrawChar(x + 1, y, '(', stripeStyle);
                renderer.DrawChar(x + 2, y, AnimationFrame == 0 ? '°' : 'o', style);
                renderer.DrawChar(x + 3, y, ')', stripeStyle);
                renderer.DrawChar(x + 4, y, '<', style);
                renderer.DrawText(x, y + 1, "¯¯¯", style);
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
        
        public void Draw(TerminalRenderer renderer)
        {
            var style = Style.Default.WithForegroundColor(Color.FromRgb(200, 200, 255));
            
            if (Size < 0.5)
                renderer.DrawChar((int)X, (int)Y, '·', style);
            else if (Size < 1.0)
                renderer.DrawChar((int)X, (int)Y, 'o', style);
            else
                renderer.DrawChar((int)X, (int)Y, 'O', style);
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
        
        public void Draw(TerminalRenderer renderer, int groundY)
        {
            var style = Style.Default.WithForegroundColor(Color);
            
            for (int i = 0; i < Height; i++)
            {
                var sway = Math.Sin(SwayPhase + i * 0.3) * 2;
                var x = X + (int)sway;
                var y = groundY - i;
                
                if (i == Height - 1)
                    renderer.DrawChar(x, y, '♣', style);
                else if (i % 3 == 0)
                    renderer.DrawChar(x, y, ')', style);
                else if (i % 3 == 1)
                    renderer.DrawChar(x, y, '|', style);
                else
                    renderer.DrawChar(x, y, '(', style);
            }
        }
    }
    
    public static void Run()
    {
        Console.WriteLine("=== Terminal Aquarium ===");
        Console.WriteLine("Watch the fish swim by!");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
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
                X = random.Next(10, renderer.Width - 10),
                Y = random.Next(5, renderer.Height - 15),
                VelocityX = (random.NextDouble() - 0.5) * 2,
                FacingRight = random.NextDouble() > 0.5,
                Color = GetRandomFishColor(random),
                AnimationFrame = random.Next(2)
            };
            fish1.FacingRight = fish1.VelocityX > 0;
            fish.Add(fish1);
        }
        
        // Create seaweed
        for (int i = 0; i < renderer.Width / 15; i++)
        {
            seaweeds.Add(new Seaweed
            {
                X = random.Next(5, renderer.Width - 5),
                Height = random.Next(5, 12),
                SwayPhase = random.NextDouble() * Math.PI * 2,
                Color = Color.FromRgb(0, (byte)random.Next(100, 200), 0)
            });
        }
        
        // Animation loop
        var frameCount = 0;
        
        while (!exit)
        {
            renderer.BeginFrame();
            
            // Draw water background
            DrawWaterBackground(renderer);
            
            // Draw sand at bottom
            DrawSandBottom(renderer);
            
            // Update and draw seaweed
            foreach (var weed in seaweeds)
            {
                weed.Update();
                weed.Draw(renderer, renderer.Height - 5);
            }
            
            // Update and draw fish
            foreach (var f in fish)
            {
                f.Update(renderer.Width, renderer.Height);
                f.Draw(renderer);
            }
            
            // Create new bubbles occasionally
            if (frameCount % 30 == 0)
            {
                bubbles.Add(new Bubble
                {
                    X = random.Next(5, renderer.Width - 5),
                    Y = renderer.Height - 6,
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
                    bubble.Draw(renderer);
                }
            }
            
            // Draw title
            var titleStyle = Style.Default
                .WithForegroundColor(Color.White)
                .WithBackgroundColor(Color.FromRgb(0, 50, 100));
            renderer.DrawText(2, 1, " 🐠 Terminal Aquarium 🐟 ", titleStyle);
            
            // Draw info
            renderer.DrawText(2, renderer.Height - 2, "ESC or Q to exit", 
                Style.Default.WithForegroundColor(Color.FromRgb(100, 100, 100)));
            
            renderer.EndFrame();
            
            frameCount++;
            Thread.Sleep(50); // ~20 FPS for smooth animation
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
        // Restore cursor
        terminal.CursorVisible = true;
        
        Console.Clear();
        Console.WriteLine("\nThanks for visiting the aquarium!");
    }
    
    private static void DrawWaterBackground(TerminalRenderer renderer)
    {
        // Create gradient water effect
        for (int y = 0; y < renderer.Height; y++)
        {
            var depth = (double)y / renderer.Height;
            var blue = (int)(50 + depth * 150);
            var green = (int)(50 + depth * 50);
            var waterColor = Color.FromRgb(0, (byte)green, (byte)blue);
            var style = Style.Default.WithBackgroundColor(waterColor);
            
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.DrawChar(x, y, ' ', style);
            }
        }
    }
    
    private static void DrawSandBottom(TerminalRenderer renderer)
    {
        var sandColor = Color.FromRgb(194, 178, 128);
        var darkSandColor = Color.FromRgb(160, 140, 90);
        
        for (int y = renderer.Height - 5; y < renderer.Height; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                var isDark = (x + y) % 3 == 0;
                var style = Style.Default.WithBackgroundColor(isDark ? darkSandColor : sandColor);
                renderer.DrawChar(x, y, ' ', style);
                
                // Add some texture
                if (y == renderer.Height - 5 && x % 7 == 0)
                {
                    renderer.DrawChar(x, y, '~', Style.Default.WithForegroundColor(darkSandColor));
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