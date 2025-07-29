using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates drawing Pac-Man and ghost characters with animation.
/// </summary>
public class PacManExample
{
    public static void Run()
    {
        Console.WriteLine("=== Pac-Man Example ===");
        Console.WriteLine("Watch Pac-Man chase the ghosts!");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        // First show a static display of all characters
        ShowCharacterGallery();
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
        // Create input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
        };
        inputHandler.Start();
        
        // Animation parameters
        var frameCount = 0;
        var startTime = DateTime.Now;
        const int animationY = 10;
        const int trackLength = 60;
        
        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 60;
        
        // Game state
        var score = 0;
        var ghostsEaten = new Dictionary<string, int>
        {
            { "Blinky", 0 },
            { "Pinky", 0 },
            { "Inky", 0 },
            { "Clyde", 0 }
        };
        
        // Star burst effects
        var starBursts = new List<StarBurst>();
        var random = new Random();
        
        // Ghost colors
        var blinkyStyle = Style.Default.WithForegroundColor(Color.Red);
        var pinkyStyle = Style.Default.WithForegroundColor(Color.Magenta);
        var inkyStyle = Style.Default.WithForegroundColor(Color.Cyan);
        var clydeStyle = Style.Default.WithForegroundColor(Color.FromRgb(255, 165, 0)); // Orange
        
        // Pac-Man style
        var pacmanStyle = Style.Default.WithForegroundColor(Color.Yellow);
        
        // Power pellet style
        var pelletStyle = Style.Default.WithForegroundColor(Color.White);
        var powerPelletStyle = Style.Default.WithForegroundColor(Color.White).WithBold();
        
        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit || frameCount >= 600) // Run for ~10 seconds at 60 FPS
                return;
                
            renderingSystem.Clear();
            
            // Draw title
            var title = " 🟡 PAC-MAN CHASE 👻 ";
            renderingSystem.WriteText((renderingSystem.Terminal.Width - title.Length) / 2, 2, title, 
                Style.Default.WithForegroundColor(Color.Yellow).WithBold());
            
            // Draw track with pellets
            for (int x = 5; x < trackLength + 5; x++)
            {
                if (x % 3 == 0)
                {
                    var isPowerPellet = x % 15 == 0;
                    renderingSystem.Buffer.SetCell(x, animationY, isPowerPellet ? '●' : '·', 
                        isPowerPellet ? powerPelletStyle : pelletStyle);
                }
            }
            
            // Calculate positions
            var time = (DateTime.Now - startTime).TotalSeconds;
            var pacmanX = 5 + (int)(time * 10) % trackLength;
            var blinkyX = 5 + (int)(time * 8 + 10) % trackLength;
            var pinkyX = 5 + (int)(time * 7 + 20) % trackLength;
            var inkyX = 5 + (int)(time * 9 + 30) % trackLength;
            var clydeX = 5 + (int)(time * 6 + 40) % trackLength;
            
            // Animate Pac-Man mouth
            var mouthOpen = (frameCount / 5) % 2 == 0;
            var pacmanChar = mouthOpen ? 'C' : 'O';
            
            // Check for ghost collisions and create star bursts
            score += CheckGhostCollision(pacmanX, animationY, blinkyX, animationY, "Blinky", blinkyStyle, starBursts, random, ghostsEaten);
            score += CheckGhostCollision(pacmanX, animationY, pinkyX, animationY, "Pinky", pinkyStyle, starBursts, random, ghostsEaten);
            score += CheckGhostCollision(pacmanX, animationY, inkyX, animationY, "Inky", inkyStyle, starBursts, random, ghostsEaten);
            score += CheckGhostCollision(pacmanX, animationY, clydeX, animationY, "Clyde", clydeStyle, starBursts, random, ghostsEaten);
            
            // Draw ghosts (behind Pac-Man if at same position)
            DrawGhost(renderingSystem, blinkyX, animationY, 'ᗣ', blinkyStyle);
            DrawGhost(renderingSystem, pinkyX, animationY, 'ᗣ', pinkyStyle);
            DrawGhost(renderingSystem, inkyX, animationY, 'ᗣ', inkyStyle);
            DrawGhost(renderingSystem, clydeX, animationY, 'ᗣ', clydeStyle);
            
            // Draw Pac-Man (on top)
            renderingSystem.Buffer.SetCell(pacmanX, animationY, pacmanChar, pacmanStyle);
            
            // Update and draw star bursts
            for (int i = starBursts.Count - 1; i >= 0; i--)
            {
                var burst = starBursts[i];
                burst.Update();
                
                if (burst.IsAlive)
                {
                    burst.Draw(renderingSystem);
                }
                else
                {
                    starBursts.RemoveAt(i);
                }
            }
            
            // Draw score and info
            var baseScore = frameCount * 10;
            var totalScore = baseScore + score;
            renderingSystem.WriteText(5, animationY + 3, $"Score: {totalScore:D6} (Ghosts eaten: {ghostsEaten.Values.Sum()})", 
                Style.Default.WithForegroundColor(Color.White));
            
            renderingSystem.WriteText(5, animationY + 4, "Ghosts: ", 
                Style.Default.WithForegroundColor(Color.White));
            renderingSystem.WriteText(13, animationY + 4, "Blinky ", blinkyStyle);
            renderingSystem.WriteText(20, animationY + 4, "Pinky ", pinkyStyle);
            renderingSystem.WriteText(26, animationY + 4, "Inky ", inkyStyle);
            renderingSystem.WriteText(31, animationY + 4, "Clyde", clydeStyle);
            
            // Draw legend
            renderingSystem.WriteText(5, animationY + 6, "· = Pellet (10 pts)   ● = Power Pellet (50 pts)", 
                Style.Default.WithForegroundColor(Color.DarkGray));
            
            // Calculate and draw performance stats
            var elapsed = (DateTime.Now - startTime).TotalSeconds;
            var fps = frameCount / elapsed;
            
            // Get memory stats
            var currentProcess = Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64 / (1024 * 1024); // MB
            var gcMemory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);
            
            // Draw stats at the bottom
            var statsY = renderingSystem.Terminal.Height - 5;
            renderingSystem.DrawBox(0, statsY - 1, renderingSystem.Terminal.Width, 6, 
                Style.Default.WithForegroundColor(Color.DarkGray), BoxStyle.Single);
            
            renderingSystem.WriteText(2, statsY, $"Performance: FPS: {fps:F1} / 60 | Frame: {frameCount} | Time: {elapsed:F1}s",
                Style.Default.WithForegroundColor(Color.Yellow));
            renderingSystem.WriteText(2, statsY + 1, $"Memory: {gcMemory:F1} MB (Working Set: {workingSet:F1} MB)",
                Style.Default.WithForegroundColor(Color.Cyan));
            renderingSystem.WriteText(2, statsY + 2, $"GC Gen 0: {gen0} | Gen 1: {gen1} | Gen 2: {gen2}",
                Style.Default.WithForegroundColor(Color.Green));
            renderingSystem.WriteText(2, statsY + 3, "Press ESC or Q to exit",
                Style.Default.WithForegroundColor(Color.White).WithDim());
            
            frameCount++;
            
            // Queue next frame
            renderingSystem.Scheduler.QueueRender(renderFrame);
        };
        
        // Start animation
        renderingSystem.Scheduler.QueueRender(renderFrame);
        
        // Wait for exit or animation completion
        while (!exit && frameCount < 600)
        {
            Thread.Sleep(50);
        }
        
        inputHandler.Stop();
        inputHandler.Dispose();
        
        Console.Clear();
        Console.WriteLine("\nGame Over!");
        Console.WriteLine($"Final Score: {(frameCount * 10 + score):D6}");
        Console.WriteLine($"Ghosts Eaten: Blinky: {ghostsEaten["Blinky"]}, Pinky: {ghostsEaten["Pinky"]}, Inky: {ghostsEaten["Inky"]}, Clyde: {ghostsEaten["Clyde"]}");
    }
    
    private static void DrawGhost(RenderingSystem renderingSystem, int x, int y, char ghostChar, Style style)
    {
        // Draw ghost with wavy bottom using Unicode
        renderingSystem.Buffer.SetCell(x, y, ghostChar, style);
    }
    
    private static void ShowCharacterGallery()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();
        
        renderingSystem.Clear();
        
        // Title
        renderingSystem.WriteText(5, 2, "PAC-MAN CHARACTER GALLERY", 
            Style.Default.WithForegroundColor(Color.Yellow).WithBold());
        
        // Draw Pac-Man variations
        var y = 5;
        renderingSystem.WriteText(5, y, "Pac-Man: ", Style.Default.WithForegroundColor(Color.White));
        renderingSystem.Buffer.SetCell(15, y, 'C', Style.Default.WithForegroundColor(Color.Yellow));
        renderingSystem.WriteText(17, y, "(mouth open)  ", Style.Default.WithForegroundColor(Color.DarkGray));
        renderingSystem.Buffer.SetCell(32, y, 'O', Style.Default.WithForegroundColor(Color.Yellow));
        renderingSystem.WriteText(34, y, "(mouth closed)", Style.Default.WithForegroundColor(Color.DarkGray));
        
        // Draw all ghosts on one line
        y += 2;
        renderingSystem.WriteText(5, y, "Ghosts:  ", Style.Default.WithForegroundColor(Color.White));
        
        // Blinky (Red)
        renderingSystem.Buffer.SetCell(15, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Red));
        renderingSystem.WriteText(17, y, "Blinky  ", Style.Default.WithForegroundColor(Color.Red));
        
        // Pinky (Pink/Magenta)
        renderingSystem.Buffer.SetCell(26, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Magenta));
        renderingSystem.WriteText(28, y, "Pinky  ", Style.Default.WithForegroundColor(Color.Magenta));
        
        // Inky (Cyan)
        renderingSystem.Buffer.SetCell(36, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Cyan));
        renderingSystem.WriteText(38, y, "Inky  ", Style.Default.WithForegroundColor(Color.Cyan));
        
        // Clyde (Orange)
        renderingSystem.Buffer.SetCell(45, y, 'ᗣ', Style.Default.WithForegroundColor(Color.FromRgb(255, 165, 0)));
        renderingSystem.WriteText(47, y, "Clyde", Style.Default.WithForegroundColor(Color.FromRgb(255, 165, 0)));
        
        // Draw pellets
        y += 2;
        renderingSystem.WriteText(5, y, "Items:   ", Style.Default.WithForegroundColor(Color.White));
        renderingSystem.Buffer.SetCell(15, y, '·', Style.Default.WithForegroundColor(Color.White));
        renderingSystem.WriteText(17, y, "Pellet     ", Style.Default.WithForegroundColor(Color.DarkGray));
        renderingSystem.Buffer.SetCell(29, y, '●', Style.Default.WithForegroundColor(Color.White).WithBold());
        renderingSystem.WriteText(31, y, "Power Pellet", Style.Default.WithForegroundColor(Color.DarkGray));
        
        // Draw a sample game line
        y += 3;
        renderingSystem.WriteText(5, y, "Sample Game Line:", Style.Default.WithForegroundColor(Color.White));
        y += 1;
        
        // Draw pellets
        for (int x = 5; x < 55; x += 2)
        {
            renderingSystem.Buffer.SetCell(x, y, '·', Style.Default.WithForegroundColor(Color.White).WithDim());
        }
        
        // Draw power pellet
        renderingSystem.Buffer.SetCell(10, y, '●', Style.Default.WithForegroundColor(Color.White).WithBold());
        
        // Draw Pac-Man
        renderingSystem.Buffer.SetCell(20, y, 'C', Style.Default.WithForegroundColor(Color.Yellow));
        
        // Draw ghosts
        renderingSystem.Buffer.SetCell(30, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Red));
        renderingSystem.Buffer.SetCell(35, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Magenta));
        renderingSystem.Buffer.SetCell(40, y, 'ᗣ', Style.Default.WithForegroundColor(Color.Cyan));
        renderingSystem.Buffer.SetCell(45, y, 'ᗣ', Style.Default.WithForegroundColor(Color.FromRgb(255, 165, 0)));
        
        renderingSystem.WriteText(5, y + 3, "Press any key to start the animation...", 
            Style.Default.WithForegroundColor(Color.DarkGray));
        
        renderingSystem.Render();
        
        Console.ReadKey(true);
    }
    
    private static int CheckGhostCollision(int pacX, int pacY, int ghostX, int ghostY, 
        string ghostName, Style ghostStyle, List<StarBurst> starBursts, Random random, 
        Dictionary<string, int> ghostsEaten)
    {
        // Check if Pac-Man is at the same position as the ghost
        if (Math.Abs(pacX - ghostX) <= 1 && pacY == ghostY)
        {
            // Create a star burst at the collision point
            var burst = new StarBurst(ghostX, ghostY, ghostStyle.Foreground, random);
            
            // Check if we don't already have a recent burst at this location
            var hasRecentBurst = starBursts.Any(b => 
                Math.Abs(b.X - ghostX) <= 2 && b.Y == ghostY && b.Age < 5);
            
            if (!hasRecentBurst)
            {
                starBursts.Add(burst);
                ghostsEaten[ghostName]++;
                return 200; // Points for eating a ghost
            }
        }
        return 0;
    }
    
    private class StarBurst
    {
        private readonly int _x;
        private readonly int _y;
        private readonly Color _color;
        private readonly Star[] _stars;
        private int _age;
        
        public int X => _x;
        public int Y => _y;
        public int Age => _age;
        public bool IsAlive => _age < 15;
        
        public StarBurst(int x, int y, Color color, Random random)
        {
            _x = x;
            _y = y;
            _color = color;
            _age = 0;
            
            // Create 8 stars in different directions
            _stars = new Star[8];
            var angles = new[] { 0, 45, 90, 135, 180, 225, 270, 315 };
            
            for (int i = 0; i < 8; i++)
            {
                var angle = angles[i] * Math.PI / 180;
                var speed = 0.3 + random.NextDouble() * 0.2;
                _stars[i] = new Star
                {
                    DX = Math.Cos(angle) * speed,
                    DY = Math.Sin(angle) * speed * 0.5, // Compress Y for terminal aspect ratio
                    Character = GetStarChar(i)
                };
            }
        }
        
        private char GetStarChar(int index)
        {
            // Use different star characters for variety
            var stars = new[] { '✦', '✧', '★', '✩', '✪', '⋆', '＊', '✸' };
            return stars[index % stars.Length];
        }
        
        public void Update()
        {
            _age++;
        }
        
        public void Draw(RenderingSystem renderingSystem)
        {
            var fadeAlpha = 1.0 - (_age / 15.0);
            var style = Style.Default.WithForegroundColor(_color);
            
            // Apply dimming effect as stars fade
            if (fadeAlpha < 0.5)
            {
                style = style.WithDim();
            }
            
            for (int i = 0; i < _stars.Length; i++)
            {
                var star = _stars[i];
                var distance = _age * 0.8;
                var x = (int)Math.Round(_x + star.DX * distance);
                var y = (int)Math.Round(_y + star.DY * distance);
                
                // Only draw if within bounds
                if (x >= 0 && x < renderingSystem.Terminal.Width && y >= 0 && y < renderingSystem.Terminal.Height - 6)
                {
                    renderingSystem.Buffer.SetCell(x, y, star.Character, style);
                }
            }
        }
        
        private class Star
        {
            public double DX { get; set; }
            public double DY { get; set; }
            public char Character { get; set; }
        }
    }
}