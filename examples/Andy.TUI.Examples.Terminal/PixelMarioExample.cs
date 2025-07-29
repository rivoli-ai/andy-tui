using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a 16x16 pixel Mario character with animation and physics.
/// </summary>
public class PixelMarioExample
{
    private class Mario
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public bool IsJumping { get; set; }
        public bool IsFacingRight { get; set; } = true;
        public int AnimationFrame { get; set; }
        public MarioState State { get; set; } = MarioState.Standing;
    }
    
    private enum MarioState
    {
        Standing,
        Walking,
        Jumping,
        Falling
    }
    
    private class Coin
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Collected { get; set; }
        public int AnimationFrame { get; set; }
    }
    
    private class Block
    {
        public int X { get; set; }
        public int Y { get; set; }
        public BlockType Type { get; set; }
        public bool IsHit { get; set; }
        public int AnimationOffset { get; set; }
    }
    
    private enum BlockType
    {
        Brick,
        Question,
        Pipe
    }
    
    public static void Run()
    {
        Console.WriteLine("=== Pixel Mario Example ===");
        Console.WriteLine("Use arrow keys to move, SPACE to jump");
        Console.WriteLine("Collect coins and hit question blocks!");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);
        
        // Hide cursor for better effect
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
        
        // Poll for key state in game loop
        var keyPollTimer = 0;
        
        // Game state
        var groundLevel = Math.Min(40, renderer.Height - 10); // Limit height to make it playable
        var mario = new Mario 
        { 
            X = 10, 
            Y = groundLevel - 16 // Start on ground (Mario is 16 pixels tall)
        };
        
        var coins = new List<Coin>();
        var blocks = new List<Block>();
        var score = 0;
        
        // Initialize level
        InitializeLevel(coins, blocks, groundLevel);
        
        // Animation parameters
        var frameCount = 0;
        var startTime = DateTime.Now;
        var targetFps = 60.0;
        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / targetFps);
        var nextFrameTime = DateTime.Now;
        
        // Physics constants
        const double gravity = 0.4;  // Reduced gravity for higher jumps
        const double jumpPower = -8;  // Initial jump velocity - much stronger
        const double maxJumpVelocity = -15; // Maximum upward velocity when holding jump
        const double jumpBoost = -1.0; // How much velocity to add each frame when holding
        const double moveSpeed = 3;  // Increased horizontal speed
        const double friction = 0.9;  // Less friction for better momentum
        
        while (!exit)
        {
            renderer.BeginFrame();
            renderer.Clear();
            
            // Poll for current key state every few frames
            keyPollTimer++;
            if (keyPollTimer >= 3)
            {
                keyPollTimer = 0;
                // Clear old keys and check what's currently pressed
                var currentKeys = new HashSet<ConsoleKey>();
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    currentKeys.Add(key.Key);
                }
                
                // Update pressed keys
                if (currentKeys.Count > 0)
                {
                    pressedKeys = currentKeys;
                }
                else if (keyPollTimer == 0)
                {
                    // Clear keys if nothing pressed
                    pressedKeys.Clear();
                }
            }
            
            // Update physics based on current keys
            if (pressedKeys.Contains(ConsoleKey.LeftArrow))
            {
                mario.VelocityX = -moveSpeed;
                mario.IsFacingRight = false;
                if (!mario.IsJumping)
                    mario.State = MarioState.Walking;
            }
            else if (pressedKeys.Contains(ConsoleKey.RightArrow))
            {
                mario.VelocityX = moveSpeed;
                mario.IsFacingRight = true;
                if (!mario.IsJumping)
                    mario.State = MarioState.Walking;
            }
            else
            {
                mario.VelocityX *= friction;
                if (Math.Abs(mario.VelocityX) < 0.1 && !mario.IsJumping)
                    mario.State = MarioState.Standing;
            }
            
            // Jump
            if ((pressedKeys.Contains(ConsoleKey.Spacebar) || pressedKeys.Contains(ConsoleKey.UpArrow)) && !mario.IsJumping)
            {
                mario.VelocityY = jumpPower;
                mario.IsJumping = true;
                mario.State = MarioState.Jumping;
            }
            
            // Variable jump height - holding jump makes you go higher
            if (mario.IsJumping && mario.VelocityY < 0 && 
                (pressedKeys.Contains(ConsoleKey.Spacebar) || pressedKeys.Contains(ConsoleKey.UpArrow)))
            {
                // Add upward velocity while holding jump, but cap at max velocity
                mario.VelocityY = Math.Max(mario.VelocityY + jumpBoost, maxJumpVelocity);
            }
            else if (mario.IsJumping && mario.VelocityY < 0 && 
                     !pressedKeys.Contains(ConsoleKey.Spacebar) && !pressedKeys.Contains(ConsoleKey.UpArrow))
            {
                // If jump is released while ascending, cut upward velocity to start falling sooner
                if (mario.VelocityY < -2)
                {
                    mario.VelocityY = -2;
                }
            }
            
            // Apply gravity
            mario.VelocityY += gravity;
            
            // Update position
            mario.X += mario.VelocityX;
            mario.Y += mario.VelocityY;
            
            // Ground collision
            if (mario.Y >= groundLevel - 16)
            {
                mario.Y = groundLevel - 16;
                mario.VelocityY = 0;
                mario.IsJumping = false;
                if (mario.State == MarioState.Jumping || mario.State == MarioState.Falling)
                {
                    mario.State = Math.Abs(mario.VelocityX) > 0.1 ? MarioState.Walking : MarioState.Standing;
                }
            }
            else if (mario.VelocityY > 0)
            {
                mario.State = MarioState.Falling;
            }
            
            // Screen boundaries
            mario.X = Math.Max(0, Math.Min(renderer.Width - 16, mario.X));
            
            // Check collisions
            CheckCoinCollisions(mario, coins, ref score);
            CheckBlockCollisions(mario, blocks, ref score);
            
            // Update animations
            if (frameCount % 8 == 0)
            {
                mario.AnimationFrame = (mario.AnimationFrame + 1) % 2;
                foreach (var coin in coins.Where(c => !c.Collected))
                {
                    coin.AnimationFrame = (coin.AnimationFrame + 1) % 4;
                }
            }
            
            // Draw background
            DrawSky(renderer, groundLevel);
            DrawClouds(renderer, frameCount);
            DrawGround(renderer, groundLevel);
            
            // Draw level elements
            foreach (var block in blocks)
            {
                DrawBlock(renderer, block);
            }
            
            foreach (var coin in coins.Where(c => !c.Collected))
            {
                DrawCoin(renderer, coin);
            }
            
            // Draw Mario
            DrawMario(renderer, mario);
            
            // Draw HUD
            DrawHUD(renderer, score, frameCount, startTime);
            
            renderer.EndFrame();
            
            frameCount++;
            
            // Frame rate control
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
        
        // Restore cursor
        terminal.CursorVisible = true;
        
        Console.Clear();
        Console.WriteLine($"\nGame Over! Final Score: {score}");
    }
    
    private static void InitializeLevel(List<Coin> coins, List<Block> blocks, int groundLevel)
    {
        
        // Add coins
        for (int i = 0; i < 5; i++)
        {
            coins.Add(new Coin 
            { 
                X = 30 + i * 15, 
                Y = groundLevel - 20,
                AnimationFrame = i % 4
            });
        }
        
        // Add blocks (spaced apart for Mario to fit between)
        blocks.Add(new Block 
        { 
            X = 40, 
            Y = groundLevel - 25, 
            Type = BlockType.Question 
        });
        
        blocks.Add(new Block 
        { 
            X = 70, 
            Y = groundLevel - 25, 
            Type = BlockType.Brick 
        });
        
        blocks.Add(new Block 
        { 
            X = 100, 
            Y = groundLevel - 25, 
            Type = BlockType.Question 
        });
        
        // Add pipe (moved further away from blocks and made shorter)
        blocks.Add(new Block 
        { 
            X = 140, 
            Y = groundLevel - 4, 
            Type = BlockType.Pipe 
        });
    }
    
    private static void DrawSky(TerminalRenderer renderer, int groundLevel)
    {
        var skyColor = Color.FromRgb(92, 148, 252); // Classic Mario sky blue
        var skyStyle = Style.Default.WithBackgroundColor(skyColor);
        
        for (int y = 0; y < groundLevel; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.DrawChar(x, y, ' ', skyStyle);
            }
        }
    }
    
    private static void DrawClouds(TerminalRenderer renderer, int frameCount)
    {
        var cloudStyle = Style.Default
            .WithForegroundColor(Color.White)
            .WithBackgroundColor(Color.FromRgb(92, 148, 252));
        
        // Static clouds (not moving)
        DrawCloud(renderer, 20, 5, cloudStyle);
        DrawCloud(renderer, 80, 8, cloudStyle);
        DrawCloud(renderer, 140, 6, cloudStyle);
    }
    
    private static void DrawCloud(TerminalRenderer renderer, int x, int y, Style style)
    {
        renderer.DrawText(x, y, "    ☁☁☁    ", style);
        renderer.DrawText(x, y + 1, "  ☁☁☁☁☁☁  ", style);
        renderer.DrawText(x, y + 2, "☁☁☁☁☁☁☁☁", style);
    }
    
    private static void DrawGround(TerminalRenderer renderer, int groundLevel)
    {
        var groundColor = Color.FromRgb(139, 69, 19); // Brown
        var groundStyle = Style.Default.WithBackgroundColor(groundColor);
        
        for (int y = groundLevel; y < renderer.Height; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.DrawChar(x, y, ' ', groundStyle);
            }
        }
        
        // Grass on top
        var grassStyle = Style.Default
            .WithForegroundColor(Color.FromRgb(0, 255, 0))
            .WithBackgroundColor(groundColor);
        
        for (int x = 0; x < renderer.Width; x++)
        {
            if (x % 2 == 0)
                renderer.DrawChar(x, groundLevel - 1, '▀', grassStyle);
        }
    }
    
    private static void DrawMario(TerminalRenderer renderer, Mario mario)
    {
        var x = (int)mario.X;
        var y = (int)mario.Y;
        
        // Draw classic pixel Mario using block characters
        var pixels = GetMarioPixels(mario.State, mario.AnimationFrame, mario.IsFacingRight);
        
        for (int py = 0; py < 16; py++)
        {
            for (int px = 0; px < 16; px++)
            {
                var color = pixels[py, px];
                if (color != null)
                {
                    renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(color.Value));
                }
            }
        }
    }
    
    private static Color?[,] GetMarioPixels(MarioState state, int frame, bool facingRight)
    {
        // Define colors
        var red = Color.FromRgb(255, 0, 0);
        var brown = Color.FromRgb(139, 69, 19);
        var skin = Color.FromRgb(255, 205, 148);
        var blue = Color.FromRgb(0, 0, 255);
        var black = Color.Black;
        
        // Create classic 16x16 Small Mario
        var pixels = new Color?[16, 16];
        
        // Define the classic Mario sprite (standing/walking)
        // Row 0-2: Hat
        pixels[0, 5] = red; pixels[0, 6] = red; pixels[0, 7] = red; pixels[0, 8] = red; pixels[0, 9] = red;
        pixels[1, 4] = red; pixels[1, 5] = red; pixels[1, 6] = red; pixels[1, 7] = red; pixels[1, 8] = red; pixels[1, 9] = red; pixels[1, 10] = red; pixels[1, 11] = red; pixels[1, 12] = red;
        
        // Row 3-4: Hair and face top
        pixels[2, 4] = brown; pixels[2, 5] = brown; pixels[2, 6] = brown; pixels[2, 7] = skin; pixels[2, 8] = skin; pixels[2, 9] = black; pixels[2, 10] = skin;
        pixels[3, 3] = brown; pixels[3, 4] = skin; pixels[3, 5] = brown; pixels[3, 6] = skin; pixels[3, 7] = skin; pixels[3, 8] = skin; pixels[3, 9] = black; pixels[3, 10] = skin; pixels[3, 11] = skin; pixels[3, 12] = skin;
        
        // Row 5: Face with eyes
        pixels[4, 3] = brown; pixels[4, 4] = skin; pixels[4, 5] = brown; pixels[4, 6] = brown; pixels[4, 7] = skin; pixels[4, 8] = skin; pixels[4, 9] = skin; pixels[4, 10] = black; pixels[4, 11] = skin; pixels[4, 12] = skin; pixels[4, 13] = skin;
        pixels[5, 3] = brown; pixels[5, 4] = brown; pixels[5, 5] = skin; pixels[5, 6] = skin; pixels[5, 7] = skin; pixels[5, 8] = skin; pixels[5, 9] = black; pixels[5, 10] = black; pixels[5, 11] = black; pixels[5, 12] = black;
        
        // Row 6-7: Mustache and mouth
        pixels[6, 4] = skin; pixels[6, 5] = skin; pixels[6, 6] = skin; pixels[6, 7] = skin; pixels[6, 8] = skin; pixels[6, 9] = skin; pixels[6, 10] = skin; pixels[6, 11] = skin;
        
        // Row 8-11: Shirt and overalls
        pixels[7, 3] = red; pixels[7, 4] = red; pixels[7, 5] = red; pixels[7, 6] = red; pixels[7, 7] = blue; pixels[7, 8] = red; pixels[7, 9] = red; pixels[7, 10] = red;
        pixels[8, 2] = red; pixels[8, 3] = red; pixels[8, 4] = red; pixels[8, 5] = red; pixels[8, 6] = red; pixels[8, 7] = blue; pixels[8, 8] = red; pixels[8, 9] = red; pixels[8, 10] = blue; pixels[8, 11] = red; pixels[8, 12] = red; pixels[8, 13] = red;
        pixels[9, 1] = red; pixels[9, 2] = red; pixels[9, 3] = red; pixels[9, 4] = red; pixels[9, 5] = red; pixels[9, 6] = red; pixels[9, 7] = blue; pixels[9, 8] = blue; pixels[9, 9] = blue; pixels[9, 10] = blue; pixels[9, 11] = red; pixels[9, 12] = red; pixels[9, 13] = red; pixels[9, 14] = red;
        pixels[10, 1] = skin; pixels[10, 2] = skin; pixels[10, 3] = red; pixels[10, 4] = red; pixels[10, 5] = blue; pixels[10, 6] = blue; pixels[10, 7] = blue; pixels[10, 8] = blue; pixels[10, 9] = blue; pixels[10, 10] = blue; pixels[10, 11] = blue; pixels[10, 12] = blue; pixels[10, 13] = red; pixels[10, 14] = red;
        
        // Row 12-13: Overalls bottom
        pixels[11, 1] = skin; pixels[11, 2] = skin; pixels[11, 3] = skin; pixels[11, 4] = blue; pixels[11, 5] = blue; pixels[11, 6] = blue; pixels[11, 7] = blue; pixels[11, 8] = blue; pixels[11, 9] = blue; pixels[11, 10] = blue; pixels[11, 11] = blue; pixels[11, 12] = blue; pixels[11, 13] = blue;
        pixels[12, 2] = skin; pixels[12, 3] = blue; pixels[12, 4] = blue; pixels[12, 5] = blue; pixels[12, 6] = blue; pixels[12, 7] = blue; pixels[12, 8] = blue; pixels[12, 9] = blue; pixels[12, 10] = blue; pixels[12, 11] = blue; pixels[12, 12] = blue; pixels[12, 13] = blue;
        
        // Row 14-15: Shoes
        pixels[13, 3] = blue; pixels[13, 4] = blue; pixels[13, 5] = blue; pixels[13, 6] = blue; pixels[13, 9] = blue; pixels[13, 10] = blue; pixels[13, 11] = blue; pixels[13, 12] = blue;
        pixels[14, 2] = brown; pixels[14, 3] = brown; pixels[14, 4] = brown; pixels[14, 5] = brown; pixels[14, 10] = brown; pixels[14, 11] = brown; pixels[14, 12] = brown; pixels[14, 13] = brown;
        pixels[15, 1] = brown; pixels[15, 2] = brown; pixels[15, 3] = brown; pixels[15, 4] = brown; pixels[15, 11] = brown; pixels[15, 12] = brown; pixels[15, 13] = brown; pixels[15, 14] = brown;
        
        // Animate walking by adjusting feet position
        if (state == MarioState.Walking && frame == 1)
        {
            // Clear original feet
            for (int x = 0; x < 16; x++)
            {
                pixels[14, x] = null;
                pixels[15, x] = null;
            }
            // Redraw feet in walking position
            pixels[14, 3] = brown; pixels[14, 4] = brown; pixels[14, 5] = brown; pixels[14, 6] = brown; pixels[14, 9] = brown; pixels[14, 10] = brown; pixels[14, 11] = brown;
            pixels[15, 2] = brown; pixels[15, 3] = brown; pixels[15, 4] = brown; pixels[15, 5] = brown; pixels[15, 6] = brown; pixels[15, 10] = brown; pixels[15, 11] = brown; pixels[15, 12] = brown;
        }
        
        // Jumping sprite
        if (state == MarioState.Jumping || state == MarioState.Falling)
        {
            // Arms out to sides
            pixels[7, 0] = skin; pixels[7, 1] = skin; pixels[7, 14] = skin; pixels[7, 15] = skin;
            pixels[8, 0] = skin; pixels[8, 1] = skin; pixels[8, 14] = skin; pixels[8, 15] = skin;
            
            // Adjust legs for jumping
            for (int x = 0; x < 16; x++)
            {
                pixels[13, x] = null;
                pixels[14, x] = null;
                pixels[15, x] = null;
            }
            // Legs together
            pixels[13, 5] = blue; pixels[13, 6] = blue; pixels[13, 7] = blue; pixels[13, 8] = blue; pixels[13, 9] = blue; pixels[13, 10] = blue;
            pixels[14, 5] = brown; pixels[14, 6] = brown; pixels[14, 7] = brown; pixels[14, 8] = brown; pixels[14, 9] = brown; pixels[14, 10] = brown;
        }
        
        // Flip horizontally if facing left
        if (!facingRight)
        {
            var flipped = new Color?[16, 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    flipped[y, x] = pixels[y, 15 - x];
                }
            }
            return flipped;
        }
        
        return pixels;
    }
    
    private static void DrawCoin(TerminalRenderer renderer, Coin coin)
    {
        var coinColor = Color.Yellow;
        var darkYellow = Color.FromRgb(200, 180, 0);
        var style = Style.Default.WithForegroundColor(coinColor);
        var darkStyle = Style.Default.WithForegroundColor(darkYellow);
        
        // Draw 3x3 coin that rotates
        if (coin.AnimationFrame == 0 || coin.AnimationFrame == 2)
        {
            // Front view of coin
            renderer.DrawChar(coin.X + 1, coin.Y, '█', style);
            renderer.DrawChar(coin.X, coin.Y + 1, '█', style);
            renderer.DrawChar(coin.X + 1, coin.Y + 1, '█', style);
            renderer.DrawChar(coin.X + 2, coin.Y + 1, '█', style);
            renderer.DrawChar(coin.X + 1, coin.Y + 2, '█', style);
        }
        else if (coin.AnimationFrame == 1)
        {
            // Side view (thin)
            renderer.DrawChar(coin.X + 1, coin.Y, '█', darkStyle);
            renderer.DrawChar(coin.X + 1, coin.Y + 1, '█', darkStyle);
            renderer.DrawChar(coin.X + 1, coin.Y + 2, '█', darkStyle);
        }
        else
        {
            // Other side view
            renderer.DrawChar(coin.X + 1, coin.Y, '█', style);
            renderer.DrawChar(coin.X + 1, coin.Y + 1, '█', darkStyle);
            renderer.DrawChar(coin.X + 1, coin.Y + 2, '█', style);
        }
    }
    
    private static void DrawBlock(TerminalRenderer renderer, Block block)
    {
        var y = block.Y + (block.IsHit ? -block.AnimationOffset : 0);
        
        switch (block.Type)
        {
            case BlockType.Question:
                DrawQuestionBlock(renderer, block.X, y, block.IsHit);
                break;
                
            case BlockType.Brick:
                DrawBrickBlock(renderer, block.X, y);
                break;
                
            case BlockType.Pipe:
                DrawPipe(renderer, block.X, block.Y);
                break;
        }
        
        // Animate hit blocks
        if (block.IsHit && block.AnimationOffset > 0)
        {
            block.AnimationOffset--;
        }
    }
    
    private static void DrawQuestionBlock(TerminalRenderer renderer, int x, int y, bool isHit)
    {
        // Draw an 8x8 question block
        var yellow = isHit ? Color.FromRgb(139, 69, 19) : Color.Yellow;
        var brown = Color.FromRgb(139, 69, 19);
        var black = Color.Black;
        
        // Draw the block
        for (int py = 0; py < 8; py++)
        {
            for (int px = 0; px < 8; px++)
            {
                // Simple question mark pattern for 8x8
                if (!isHit)
                {
                    // Question mark pixels
                    if ((py == 1 || py == 2) && (px >= 3 && px <= 4)) // Top
                        renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(black));
                    else if (py == 3 && px == 5) // Right side
                        renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(black));
                    else if (py == 4 && px == 4) // Middle
                        renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(black));
                    else if (py == 6 && (px == 3 || px == 4)) // Dot
                        renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(black));
                    else
                        renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(yellow));
                }
                else
                {
                    // Hit block - all brown
                    renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(brown));
                }
            }
        }
    }
    
    private static void DrawBrickBlock(TerminalRenderer renderer, int x, int y)
    {
        // Draw an 8x8 brick block
        var brown = Color.FromRgb(139, 69, 19);
        var darkBrown = Color.FromRgb(100, 50, 10);
        
        for (int py = 0; py < 8; py++)
        {
            for (int px = 0; px < 8; px++)
            {
                // Create brick pattern
                bool isDarkLine = (py % 4 == 0); // Horizontal mortar lines every 4 pixels
                bool isVerticalLine = ((py / 4) % 2 == 0) ? (px == 0 || px == 4) : (px == 2 || px == 6); // Offset vertical lines
                
                if (isDarkLine || isVerticalLine)
                {
                    renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(darkBrown));
                }
                else
                {
                    renderer.DrawChar(x + px, y + py, '█', Style.Default.WithForegroundColor(brown));
                }
            }
        }
    }
    
    private static void DrawPipe(TerminalRenderer renderer, int x, int y)
    {
        var pipeColor = Color.FromRgb(0, 255, 0);
        var darkGreen = Color.FromRgb(0, 200, 0);
        var black = Color.Black;
        
        // Draw filled pipe (8 units wide, 4 units tall)
        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 8; px++)
            {
                // Pipe rim (top 1 row only for shorter pipe)
                if (py < 1)
                {
                    // Black outline
                    if (px == 0 || px == 7)
                    {
                        renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(black));
                    }
                    else
                    {
                        renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(pipeColor));
                    }
                }
                // Pipe body
                else
                {
                    // Narrower body (1 pixel in from each side)
                    if (px >= 1 && px <= 6)
                    {
                        // Black outline on sides
                        if (px == 1 || px == 6)
                        {
                            renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(black));
                        }
                        // Highlight on left side
                        else if (px == 2)
                        {
                            renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(pipeColor));
                        }
                        // Shadow on right side
                        else if (px == 5)
                        {
                            renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(darkGreen));
                        }
                        // Main body
                        else
                        {
                            renderer.DrawChar(x - 2 + px, y + py, '█', Style.Default.WithForegroundColor(pipeColor));
                        }
                    }
                }
            }
        }
    }
    
    private static void CheckCoinCollisions(Mario mario, List<Coin> coins, ref int score)
    {
        var marioRect = new Rectangle((int)mario.X, (int)mario.Y, 16, 16);
        
        foreach (var coin in coins.Where(c => !c.Collected))
        {
            var coinRect = new Rectangle(coin.X, coin.Y, 3, 3);
            
            if (RectsOverlap(marioRect, coinRect))
            {
                coin.Collected = true;
                score += 10;
            }
        }
    }
    
    private static void CheckBlockCollisions(Mario mario, List<Block> blocks, ref int score)
    {
        var marioRect = new Rectangle((int)mario.X, (int)mario.Y, 16, 16);
        
        foreach (var block in blocks.Where(b => b.Type != BlockType.Pipe))
        {
            var blockRect = new Rectangle(block.X, block.Y, 8, 8);
            
            // Check if Mario is hitting block from below
            if (mario.VelocityY < 0 && RectsOverlap(marioRect, blockRect))
            {
                if (block.Type == BlockType.Question && !block.IsHit)
                {
                    block.IsHit = true;
                    block.AnimationOffset = 5;
                    score += 100;
                }
                
                mario.VelocityY = 0;
            }
        }
        
        // Check pipe collision
        var pipe = blocks.FirstOrDefault(b => b.Type == BlockType.Pipe);
        if (pipe != null)
        {
            var pipeRect = new Rectangle(pipe.X - 2, pipe.Y, 8, 4);
            
            // Only check collision if Mario's bottom is touching the pipe area
            if (mario.Y + 16 > pipe.Y && RectsOverlap(marioRect, pipeRect))
            {
                // Check if Mario can land on top of the pipe
                if (mario.VelocityY >= 0 && mario.Y + 16 <= pipe.Y + 4 && 
                    mario.X + 16 > pipe.X - 2 && mario.X < pipe.X + 6)
                {
                    // Land on top of pipe
                    mario.Y = pipe.Y - 16;
                    mario.VelocityY = 0;
                    mario.IsJumping = false;
                }
                // Only block horizontal movement if Mario is not above the pipe
                else if (mario.Y + 16 > pipe.Y + 2)
                {
                    // Horizontal collision - push Mario out
                    if (mario.VelocityX > 0 && mario.X < pipe.X) // Moving right
                    {
                        mario.X = pipe.X - 2 - 16;
                        mario.VelocityX = 0;
                    }
                    else if (mario.VelocityX < 0 && mario.X > pipe.X) // Moving left
                    {
                        mario.X = pipe.X + 6;
                        mario.VelocityX = 0;
                    }
                }
            }
        }
    }
    
    private static bool RectsOverlap(Rectangle a, Rectangle b)
    {
        return a.X < b.X + b.Width &&
               a.X + a.Width > b.X &&
               a.Y < b.Y + b.Height &&
               a.Y + a.Height > b.Y;
    }
    
    private static void DrawHUD(TerminalRenderer renderer, int score, int frameCount, DateTime startTime)
    {
        var hudStyle = Style.Default.WithForegroundColor(Color.White);
        
        // Score
        renderer.DrawText(2, 2, $"SCORE: {score:D6}", hudStyle);
        
        // Time
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        renderer.DrawText(renderer.Width - 15, 2, $"TIME: {elapsed:F0}", hudStyle);
        
        // FPS
        var fps = frameCount / elapsed;
        renderer.DrawText(2, 3, $"FPS: {fps:F1}", hudStyle);
        
        // Controls
        renderer.DrawText(2, renderer.Height - 2, "← → Move   SPACE Jump   ESC Quit", 
            Style.Default.WithForegroundColor(Color.DarkGray));
    }
    
    private struct Rectangle
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}