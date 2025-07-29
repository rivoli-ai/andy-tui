using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a classic Snake game with score tracking and increasing speed.
/// </summary>
public class SnakeGameExample
{
    private class Snake
    {
        public List<Point> Body { get; private set; }
        public Direction Direction { get; set; }
        public bool HasGrown { get; set; }
        
        public Snake(int startX, int startY)
        {
            Body = new List<Point>
            {
                new Point(startX, startY),
                new Point(startX - 1, startY),
                new Point(startX - 2, startY)
            };
            Direction = Direction.Right;
        }
        
        public Point Head => Body[0];
        
        public void Move(int stepSize = 1)
        {
            var newHead = Direction switch
            {
                Direction.Up => new Point(Head.X, Head.Y - stepSize),
                Direction.Down => new Point(Head.X, Head.Y + stepSize),
                Direction.Left => new Point(Head.X - stepSize, Head.Y),
                Direction.Right => new Point(Head.X + stepSize, Head.Y),
                _ => Head
            };
            
            Body.Insert(0, newHead);
            
            if (!HasGrown)
            {
                Body.RemoveAt(Body.Count - 1);
            }
            else
            {
                HasGrown = false;
            }
        }
        
        public bool IsMovingHorizontally()
        {
            return Direction == Direction.Left || Direction == Direction.Right;
        }
        
        public void Grow()
        {
            HasGrown = true;
        }
        
        public bool CollidesWithSelf()
        {
            return Body.Skip(1).Any(segment => segment.Equals(Head));
        }
        
        public bool CollidesWithWalls(int width, int height)
        {
            return Head.X < 1 || Head.X > width - 2 || 
                   Head.Y < 1 || Head.Y > height - 2;
        }
    }
    
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    private struct Point
    {
        public int X { get; }
        public int Y { get; }
        
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is Point other && X == other.X && Y == other.Y;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
    
    private class Food
    {
        public Point Position { get; set; }
        public FoodType Type { get; set; }
        public int Value { get; set; }
        public Color Color { get; set; }
        public char Character { get; set; }
        public int TimeLeft { get; set; }
        
        public static Food CreateRegular(Point position)
        {
            return new Food
            {
                Position = position,
                Type = FoodType.Regular,
                Value = 10,
                Color = Color.Red,
                Character = '●',
                TimeLeft = -1 // Never expires
            };
        }
        
        public static Food CreateBonus(Point position, Random random)
        {
            var bonusTypes = new[]
            {
                (FoodType.Cherry, 50, Color.FromRgb(255, 0, 100), '@', 200),
                (FoodType.Apple, 30, Color.FromRgb(255, 100, 0), '@', 150),
                (FoodType.Banana, 25, Color.Yellow, '@', 180),
                (FoodType.Grape, 40, Color.FromRgb(128, 0, 128), '@', 120)
            };
            
            var (type, value, color, character, duration) = bonusTypes[random.Next(bonusTypes.Length)];
            
            return new Food
            {
                Position = position,
                Type = type,
                Value = value,
                Color = color,
                Character = character,
                TimeLeft = duration
            };
        }
        
        public void Update()
        {
            if (TimeLeft > 0)
                TimeLeft--;
        }
        
        public bool IsExpired => TimeLeft == 0;
    }
    
    private enum FoodType
    {
        Regular,
        Cherry,
        Apple,
        Banana,
        Grape
    }
    
    public static void Run()
    {
        Console.WriteLine("=== Snake Game ===");
        Console.WriteLine("Use arrow keys to control the snake");
        Console.WriteLine("Eat food to grow and increase your score!");
        Console.WriteLine("Starting game...");
        
        using var terminal = new AnsiTerminal();
        var renderer = new TerminalRenderer(terminal);
        
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
        
        // Game initialization
        var random = new Random();
        var gameWidth = renderer.Width;
        var gameHeight = renderer.Height - 5; // Leave space for UI
        
        var snake = new Snake(gameWidth / 2, gameHeight / 2);
        var foods = new List<Food>();
        var score = 0;
        var level = 1;
        var gameSpeed = 150; // Initial delay in ms for snake movement
        var gameOver = false;
        var paused = false;
        
        // Create initial food
        foods.Add(GenerateFood(random, gameWidth, gameHeight, snake));
        
        var lastMoveTime = DateTime.Now;
        var frameCount = 0;
        var startTime = DateTime.Now;
        var targetFps = 60.0;
        var targetFrameTime = TimeSpan.FromMilliseconds(1000.0 / targetFps);
        var nextFrameTime = DateTime.Now;
        
        while (!exit && !gameOver)
        {
            renderer.BeginFrame();
            
            if (!paused)
            {
                // Handle input
                HandleInput(pressedKeys, snake, ref paused);
                
                // Move snake based on game speed
                if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= gameSpeed)
                {
                    // Use different step sizes for horizontal vs vertical movement
                    int stepSize = snake.IsMovingHorizontally() ? 2 : 1;
                    snake.Move(stepSize);
                    lastMoveTime = DateTime.Now;
                    
                    // Check collisions after moving
                    if (snake.CollidesWithWalls(gameWidth, gameHeight) || snake.CollidesWithSelf())
                    {
                        gameOver = true;
                    }
                    
                    // Check food collisions (including intermediate positions for horizontal moves)
                    for (int i = foods.Count - 1; i >= 0; i--)
                    {
                        var food = foods[i];
                        bool foodEaten = false;
                        
                        // Check collision with final head position
                        if (food.Position.X == snake.Head.X && food.Position.Y == snake.Head.Y)
                        {
                            foodEaten = true;
                        }
                        // For horizontal moves, also check intermediate position
                        else if (snake.IsMovingHorizontally() && stepSize == 2)
                        {
                            var intermediateX = snake.Direction == Direction.Right ? snake.Head.X - 1 : snake.Head.X + 1;
                            if (food.Position.X == intermediateX && food.Position.Y == snake.Head.Y)
                            {
                                foodEaten = true;
                            }
                        }
                        
                        if (foodEaten)
                        {
                            snake.Grow();
                            score += food.Value;
                            foods.RemoveAt(i);
                            
                            // Increase speed more frequently and aggressively
                            if (score % 30 == 0) // Speed up every 3 foods instead of 5
                            {
                                level++;
                                gameSpeed = Math.Max(30, gameSpeed - 20); // Reduce by 20ms instead of 10ms
                            }
                            
                            // Generate new food
                            foods.Add(GenerateFood(random, gameWidth, gameHeight, snake));
                            
                            // Occasionally add bonus food
                            if (random.NextDouble() < 0.3 && foods.Count < 3)
                            {
                                var bonusFood = GenerateBonusFood(random, gameWidth, gameHeight, snake);
                                if (bonusFood != null)
                                    foods.Add(bonusFood);
                            }
                        }
                    }
                    
                    // Update food timers
                    for (int i = foods.Count - 1; i >= 0; i--)
                    {
                        foods[i].Update();
                        if (foods[i].IsExpired)
                        {
                            foods.RemoveAt(i);
                        }
                    }
                }
            }
            else
            {
                // Handle pause input
                if (pressedKeys.Contains(ConsoleKey.Spacebar))
                {
                    paused = false;
                    pressedKeys.Remove(ConsoleKey.Spacebar);
                }
            }
            
            // Draw game
            DrawGame(renderer, snake, foods, gameWidth, gameHeight, score, level, paused, frameCount, startTime);
            
            renderer.EndFrame();
            
            frameCount++;
            
            // Precise frame timing for consistent 60 FPS
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
        
        // Game over screen
        if (gameOver)
        {
            ShowGameOver(renderer, score, level);
            Console.ReadKey(true);
        }
        
        // Restore cursor
        terminal.CursorVisible = true;
        
        Console.Clear();
        Console.WriteLine($"\nGame Over! Final Score: {score}");
        Console.WriteLine($"Level Reached: {level}");
    }
    
    private static void HandleInput(HashSet<ConsoleKey> pressedKeys, Snake snake, ref bool paused)
    {
        if (pressedKeys.Contains(ConsoleKey.Spacebar))
        {
            paused = !paused;
            pressedKeys.Remove(ConsoleKey.Spacebar);
        }
        
        // Check if snake has more than one segment to prevent reversing
        bool canReverse = snake.Body.Count <= 2;
        
        if (pressedKeys.Contains(ConsoleKey.UpArrow) && (snake.Direction != Direction.Down || canReverse))
        {
            snake.Direction = Direction.Up;
            pressedKeys.Remove(ConsoleKey.UpArrow);
        }
        else if (pressedKeys.Contains(ConsoleKey.DownArrow) && (snake.Direction != Direction.Up || canReverse))
        {
            snake.Direction = Direction.Down;
            pressedKeys.Remove(ConsoleKey.DownArrow);
        }
        else if (pressedKeys.Contains(ConsoleKey.LeftArrow) && (snake.Direction != Direction.Right || canReverse))
        {
            snake.Direction = Direction.Left;
            pressedKeys.Remove(ConsoleKey.LeftArrow);
        }
        else if (pressedKeys.Contains(ConsoleKey.RightArrow) && (snake.Direction != Direction.Left || canReverse))
        {
            snake.Direction = Direction.Right;
            pressedKeys.Remove(ConsoleKey.RightArrow);
        }
    }
    
    private static Food GenerateFood(Random random, int width, int height, Snake snake)
    {
        Point position;
        do
        {
            position = new Point(
                random.Next(1, width - 1),
                random.Next(1, height - 1)
            );
        } while (snake.Body.Any(segment => segment.X == position.X && segment.Y == position.Y));
        
        return Food.CreateRegular(position);
    }
    
    private static Food? GenerateBonusFood(Random random, int width, int height, Snake snake)
    {
        Point position;
        int attempts = 0;
        do
        {
            position = new Point(
                random.Next(1, width - 1),
                random.Next(1, height - 1)
            );
            attempts++;
        } while (snake.Body.Any(segment => segment.X == position.X && segment.Y == position.Y) && attempts < 50);
        
        return attempts < 50 ? Food.CreateBonus(position, random) : null;
    }
    
    private static void DrawGame(TerminalRenderer renderer, Snake snake, List<Food> foods, 
        int gameWidth, int gameHeight, int score, int level, bool paused, int frameCount, DateTime startTime)
    {
        // Clear game area with dark background
        var bgStyle = Style.Default.WithBackgroundColor(Color.FromRgb(10, 20, 10));
        for (int y = 0; y < gameHeight; y++)
        {
            for (int x = 0; x < gameWidth; x++)
            {
                renderer.DrawChar(x, y, ' ', bgStyle);
            }
        }
        
        // Draw walls
        DrawWalls(renderer, gameWidth, gameHeight);
        
        // Draw snake
        DrawSnake(renderer, snake);
        
        // Draw food
        foreach (var food in foods)
        {
            DrawFood(renderer, food);
        }
        
        // Draw UI
        DrawUI(renderer, gameWidth, gameHeight, score, level, paused, frameCount, startTime);
        
        // Draw pause overlay
        if (paused)
        {
            DrawPauseOverlay(renderer, gameWidth, gameHeight);
        }
    }
    
    private static void DrawWalls(TerminalRenderer renderer, int width, int height)
    {
        var wallStyle = Style.Default.WithForegroundColor(Color.FromRgb(100, 100, 100));
        
        // Horizontal walls
        for (int x = 0; x < width; x++)
        {
            renderer.DrawChar(x, 0, '█', wallStyle);
            renderer.DrawChar(x, height - 1, '█', wallStyle);
        }
        
        // Vertical walls
        for (int y = 0; y < height; y++)
        {
            renderer.DrawChar(0, y, '█', wallStyle);
            renderer.DrawChar(width - 1, y, '█', wallStyle);
        }
    }
    
    private static void DrawSnake(TerminalRenderer renderer, Snake snake)
    {
        var headStyle = Style.Default.WithForegroundColor(Color.FromRgb(0, 255, 0));
        var bodyStyle = Style.Default.WithForegroundColor(Color.FromRgb(0, 200, 0));
        var tailStyle = Style.Default.WithForegroundColor(Color.FromRgb(0, 150, 0));
        
        for (int i = 0; i < snake.Body.Count; i++)
        {
            var segment = snake.Body[i];
            
            if (i == 0)
            {
                // Head
                var headChar = snake.Direction switch
                {
                    Direction.Up => '▲',
                    Direction.Down => '▼',
                    Direction.Left => '◄',
                    Direction.Right => '►',
                    _ => '●'
                };
                renderer.DrawChar(segment.X, segment.Y, headChar, headStyle);
            }
            else if (i == snake.Body.Count - 1)
            {
                // Tail
                renderer.DrawChar(segment.X, segment.Y, '○', tailStyle);
            }
            else
            {
                // Body
                renderer.DrawChar(segment.X, segment.Y, '●', bodyStyle);
            }
        }
    }
    
    private static void DrawFood(TerminalRenderer renderer, Food food)
    {
        var style = Style.Default.WithForegroundColor(food.Color);
        
        // Add blinking effect for bonus food
        if (food.Type != FoodType.Regular && food.TimeLeft < 50 && food.TimeLeft % 10 < 5)
        {
            style = style.WithBold();
        }
        
        renderer.DrawChar(food.Position.X, food.Position.Y, food.Character, style);
    }
    
    private static void DrawUI(TerminalRenderer renderer, int gameWidth, int gameHeight, 
        int score, int level, bool paused, int frameCount, DateTime startTime)
    {
        var uiY = gameHeight + 1;
        var uiStyle = Style.Default.WithForegroundColor(Color.White);
        var highlightStyle = Style.Default.WithForegroundColor(Color.Yellow);
        
        // Score and level
        renderer.DrawText(2, uiY, $"Score: {score}", highlightStyle);
        renderer.DrawText(15, uiY, $"Level: {level}", highlightStyle);
        
        // FPS
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        var fps = frameCount / elapsed;
        renderer.DrawText(25, uiY, $"FPS: {fps:F1}", uiStyle);
        
        // Controls
        renderer.DrawText(2, uiY + 1, "Controls: ← → ↑ ↓ Move | SPACE Pause | ESC/Q Quit", uiStyle);
        
        // Game info
        var speedText = level switch
        {
            1 => "Slow",
            <= 3 => "Normal",
            <= 6 => "Fast",
            <= 10 => "Very Fast",
            _ => "Lightning"
        };
        renderer.DrawText(gameWidth - 20, uiY, $"Speed: {speedText}", uiStyle);
    }
    
    private static void DrawPauseOverlay(TerminalRenderer renderer, int gameWidth, int gameHeight)
    {
        var overlayStyle = Style.Default
            .WithForegroundColor(Color.White)
            .WithBackgroundColor(Color.Black);
        
        var centerX = gameWidth / 2;
        var centerY = gameHeight / 2;
        
        // Draw pause box
        for (int y = centerY - 3; y <= centerY + 3; y++)
        {
            for (int x = centerX - 10; x <= centerX + 10; x++)
            {
                renderer.DrawChar(x, y, ' ', overlayStyle);
            }
        }
        
        // Draw pause text
        var pauseStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        renderer.DrawText(centerX - 4, centerY - 1, "PAUSED", pauseStyle);
        renderer.DrawText(centerX - 9, centerY + 1, "Press SPACE to continue", 
            Style.Default.WithForegroundColor(Color.White));
    }
    
    private static void ShowGameOver(TerminalRenderer renderer, int score, int level)
    {
        renderer.BeginFrame();
        
        var centerX = renderer.Width / 2;
        var centerY = renderer.Height / 2;
        
        // Draw game over box
        var boxStyle = Style.Default.WithBackgroundColor(Color.FromRgb(50, 0, 0));
        for (int y = centerY - 5; y <= centerY + 5; y++)
        {
            for (int x = centerX - 20; x <= centerX + 20; x++)
            {
                renderer.DrawChar(x, y, ' ', boxStyle);
            }
        }
        
        // Draw text
        var titleStyle = Style.Default.WithForegroundColor(Color.Red).WithBold();
        var textStyle = Style.Default.WithForegroundColor(Color.White);
        
        renderer.DrawText(centerX - 5, centerY - 3, "GAME OVER", titleStyle);
        renderer.DrawText(centerX - 8, centerY - 1, $"Final Score: {score}", textStyle);
        renderer.DrawText(centerX - 8, centerY, $"Level Reached: {level}", textStyle);
        renderer.DrawText(centerX - 10, centerY + 2, "Press any key to exit", textStyle);
        
        renderer.EndFrame();
    }
}