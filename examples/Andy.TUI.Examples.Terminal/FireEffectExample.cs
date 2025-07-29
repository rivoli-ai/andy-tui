using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a particle-based fire effect with heat map colors and sparks.
/// </summary>
public class FireEffectExample
{
    private class Particle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Temperature { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
        public ParticleType Type { get; set; }
        
        public void Update()
        {
            // Apply physics
            X += VelocityX;
            Y += VelocityY;
            
            // Gravity and air resistance
            VelocityY -= 0.1; // Upward movement
            VelocityX *= 0.98; // Air resistance
            VelocityY *= 0.95;
            
            // Cool down over time
            Temperature *= 0.98;
            Life -= 1;
            
            // Add some turbulence
            var turbulence = (Math.Sin(X * 0.1) + Math.Cos(Y * 0.15)) * 0.1;
            VelocityX += turbulence;
        }
        
        public bool IsAlive => Life > 0 && Temperature > 0.1;
        
        public Color GetColor()
        {
            var intensity = Temperature * (Life / MaxLife);
            
            return Type switch
            {
                ParticleType.Fire => GetFireColor(intensity),
                ParticleType.Smoke => GetSmokeColor(intensity),
                ParticleType.Spark => GetSparkColor(intensity),
                _ => Color.White
            };
        }
        
        public char GetCharacter()
        {
            var intensity = Temperature * (Life / MaxLife);
            
            return Type switch
            {
                ParticleType.Fire => GetFireChar(intensity),
                ParticleType.Smoke => GetSmokeChar(intensity),
                ParticleType.Spark => GetSparkChar(intensity),
                _ => ' '
            };
        }
        
        private static Color GetFireColor(double intensity)
        {
            if (intensity > 0.8)
                return Color.White; // Hottest - white
            else if (intensity > 0.6)
                return Color.FromRgb(255, 255, 100); // Very hot - yellow
            else if (intensity > 0.4)
                return Color.FromRgb(255, 150, 0); // Hot - orange
            else if (intensity > 0.2)
                return Color.FromRgb(255, 50, 0); // Medium - red
            else
                return Color.FromRgb(150, 0, 0); // Cool - dark red
        }
        
        private static Color GetSmokeColor(double intensity)
        {
            var gray = (byte)(50 + intensity * 150);
            return Color.FromRgb(gray, gray, gray);
        }
        
        private static Color GetSparkColor(double intensity)
        {
            if (intensity > 0.7)
                return Color.FromRgb(255, 255, 255); // Bright white
            else if (intensity > 0.4)
                return Color.FromRgb(255, 255, 0); // Yellow
            else
                return Color.FromRgb(255, 100, 0); // Orange
        }
        
        private static char GetFireChar(double intensity)
        {
            if (intensity > 0.8)
                return '█';
            else if (intensity > 0.6)
                return '▓';
            else if (intensity > 0.4)
                return '▒';
            else if (intensity > 0.2)
                return '░';
            else
                return '·';
        }
        
        private static char GetSmokeChar(double intensity)
        {
            if (intensity > 0.6)
                return '▓';
            else if (intensity > 0.3)
                return '▒';
            else
                return '░';
        }
        
        private static char GetSparkChar(double intensity)
        {
            if (intensity > 0.8)
                return '*';
            else if (intensity > 0.5)
                return '+';
            else if (intensity > 0.2)
                return '·';
            else
                return '.';
        }
    }
    
    private enum ParticleType
    {
        Fire,
        Smoke,
        Spark
    }
    
    private class FireSource
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Intensity { get; set; }
        public double Width { get; set; }
        
        public IEnumerable<Particle> EmitParticles(Random random, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var offsetX = (random.NextDouble() - 0.5) * Width;
                var particle = new Particle
                {
                    X = X + offsetX,
                    Y = Y,
                    VelocityX = (random.NextDouble() - 0.5) * 2,
                    VelocityY = random.NextDouble() * 3 + 1,
                    Temperature = 0.8 + random.NextDouble() * 0.4,
                    Life = random.Next(20, 60),
                    MaxLife = 60,
                    Type = GetRandomParticleType(random)
                };
                particle.MaxLife = particle.Life;
                yield return particle;
            }
        }
        
        private static ParticleType GetRandomParticleType(Random random)
        {
            var r = random.NextDouble();
            if (r < 0.7)
                return ParticleType.Fire;
            else if (r < 0.9)
                return ParticleType.Smoke;
            else
                return ParticleType.Spark;
        }
    }
    
    public static void Run()
    {
        Console.WriteLine("=== ASCII Fire Effect ===");
        Console.WriteLine("Interactive fire simulation with heat map colors");
        Console.WriteLine("Use arrow keys to move the fire source");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
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
        
        // Initialize fire simulation
        var random = new Random();
        var particles = new List<Particle>();
        var fireSource = new FireSource
        {
            X = renderingSystem.Terminal.Width / 2,
            Y = renderingSystem.Terminal.Height - 5,
            Intensity = 1.0,
            Width = 10
        };
        
        // Animation parameters
        var frameCount = 0;
        var startTime = DateTime.Now;
        
        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 30;
        
        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit)
                return;
                
            renderingSystem.Clear();
            
            // Handle input for moving fire source
            if (pressedKeys.Contains(ConsoleKey.LeftArrow))
            {
                fireSource.X = Math.Max(5, fireSource.X - 2);
                pressedKeys.Remove(ConsoleKey.LeftArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.RightArrow))
            {
                fireSource.X = Math.Min(renderingSystem.Terminal.Width - 5, fireSource.X + 2);
                pressedKeys.Remove(ConsoleKey.RightArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.UpArrow))
            {
                fireSource.Intensity = Math.Min(2.0, fireSource.Intensity + 0.1);
                pressedKeys.Remove(ConsoleKey.UpArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.DownArrow))
            {
                fireSource.Intensity = Math.Max(0.2, fireSource.Intensity - 0.1);
                pressedKeys.Remove(ConsoleKey.DownArrow);
            }
            
            // Emit new particles
            var emissionRate = (int)(fireSource.Intensity * 8);
            var newParticles = fireSource.EmitParticles(random, emissionRate);
            particles.AddRange(newParticles);
            
            // Update all particles
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var particle = particles[i];
                particle.Update();
                
                if (!particle.IsAlive || particle.Y < 0 || 
                    particle.X < 0 || particle.X >= renderingSystem.Terminal.Width)
                {
                    particles.RemoveAt(i);
                }
            }
            
            // Draw ground/base
            DrawGround(renderingSystem);
            
            // Draw fire source
            DrawFireSource(renderingSystem, fireSource);
            
            // Sort particles by Y coordinate (back to front)
            var sortedParticles = particles.OrderBy(p => p.Y).ToList();
            
            // Draw particles
            foreach (var particle in sortedParticles)
            {
                var x = (int)Math.Round(particle.X);
                var y = (int)Math.Round(particle.Y);
                
                if (x >= 0 && x < renderingSystem.Terminal.Width && y >= 0 && y < renderingSystem.Terminal.Height)
                {
                    var color = particle.GetColor();
                    var character = particle.GetCharacter();
                    var style = Style.Default.WithForegroundColor(color);
                    
                    renderingSystem.Buffer.SetCell(x, y, character, style);
                }
            }
            
            // Draw UI
            DrawUI(renderingSystem, fireSource, particles.Count, frameCount, startTime);
            
            frameCount++;
            
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
        Console.WriteLine("\nFire extinguished!");
    }
    
    private static void DrawGround(RenderingSystem renderingSystem)
    {
        var groundColor = Color.FromRgb(100, 50, 0);
        var style = Style.Default.WithBackgroundColor(groundColor);
        
        for (int x = 0; x < renderingSystem.Terminal.Width; x++)
        {
            renderingSystem.Buffer.SetCell(x, renderingSystem.Terminal.Height - 1, ' ', style);
            renderingSystem.Buffer.SetCell(x, renderingSystem.Terminal.Height - 2, ' ', style);
        }
        
        // Add some texture
        var textureStyle = Style.Default.WithForegroundColor(Color.FromRgb(80, 40, 0));
        for (int x = 0; x < renderingSystem.Terminal.Width; x += 3)
        {
            renderingSystem.Buffer.SetCell(x, renderingSystem.Terminal.Height - 2, '▄', textureStyle);
        }
    }
    
    private static void DrawFireSource(RenderingSystem renderingSystem, FireSource source)
    {
        var sourceColor = Color.FromRgb(200, 0, 0);
        var style = Style.Default.WithForegroundColor(sourceColor);
        
        // Draw fire source as glowing coals
        for (int i = 0; i < source.Width; i++)
        {
            var x = (int)(source.X - source.Width / 2 + i);
            if (x >= 0 && x < renderingSystem.Terminal.Width)
            {
                var intensity = 1.0 - Math.Abs(i - source.Width / 2.0) / (source.Width / 2.0);
                var glowIntensity = intensity * source.Intensity;
                
                if (glowIntensity > 0.8)
                    renderingSystem.Buffer.SetCell(x, (int)source.Y, '█', style);
                else if (glowIntensity > 0.5)
                    renderingSystem.Buffer.SetCell(x, (int)source.Y, '▓', style);
                else if (glowIntensity > 0.2)
                    renderingSystem.Buffer.SetCell(x, (int)source.Y, '▒', style);
            }
        }
    }
    
    private static void DrawUI(RenderingSystem renderingSystem, FireSource source, int particleCount, 
        int frameCount, DateTime startTime)
    {
        var uiStyle = Style.Default.WithForegroundColor(Color.White);
        var highlightStyle = Style.Default.WithForegroundColor(Color.Yellow);
        
        // Title
        renderingSystem.WriteText(2, 1, "🔥 ASCII Fire Effect", highlightStyle);
        
        // Controls
        renderingSystem.WriteText(2, 3, "Controls:", uiStyle);
        renderingSystem.WriteText(2, 4, "← → Move fire source", Style.Default.WithForegroundColor(Color.Green));
        renderingSystem.WriteText(2, 5, "↑ ↓ Adjust intensity", Style.Default.WithForegroundColor(Color.Green));
        renderingSystem.WriteText(2, 6, "ESC/Q Exit", Style.Default.WithForegroundColor(Color.Red));
        
        // Stats
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        var fps = frameCount / elapsed;
        
        renderingSystem.WriteText(2, 8, $"Fire Intensity: {source.Intensity:F1}", uiStyle);
        renderingSystem.WriteText(2, 9, $"Particles: {particleCount}", uiStyle);
        renderingSystem.WriteText(2, 10, $"FPS: {fps:F1}", uiStyle);
        
        // Heat scale legend
        renderingSystem.WriteText(renderingSystem.Terminal.Width - 20, 3, "Heat Scale:", uiStyle);
        
        var heatColors = new[]
        {
            (Color.White, "█ White (Hottest)"),
            (Color.FromRgb(255, 255, 100), "█ Yellow"),
            (Color.FromRgb(255, 150, 0), "█ Orange"),
            (Color.FromRgb(255, 50, 0), "█ Red"),
            (Color.FromRgb(150, 0, 0), "█ Dark Red (Cool)")
        };
        
        for (int i = 0; i < heatColors.Length; i++)
        {
            var (color, text) = heatColors[i];
            var style = Style.Default.WithForegroundColor(color);
            renderingSystem.WriteText(renderingSystem.Terminal.Width - 20, 4 + i, text, style);
        }
    }
}