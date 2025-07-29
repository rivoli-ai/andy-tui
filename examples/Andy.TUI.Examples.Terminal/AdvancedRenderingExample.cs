using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Advanced rendering example demonstrating performance optimization techniques.
/// </summary>
public class AdvancedRenderingExample
{
    public static void Run()
    {
        Console.WriteLine("=== Advanced Rendering Example ===");
        Console.WriteLine("This example demonstrates:");
        Console.WriteLine("- Dirty region optimization");
        Console.WriteLine("- Frame rate control");
        Console.WriteLine("- Batch updates");
        Console.WriteLine("- Performance monitoring");
        Console.WriteLine("\nPress any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        try
        {
            renderingSystem.Initialize();
            
            // Configure for high performance
            renderingSystem.Scheduler.TargetFps = 60;
            renderingSystem.Scheduler.MaxBatchWaitMs = 8; // Half frame for batching
            
            RunAdvancedDemo(renderingSystem);
        }
        finally
        {
            renderingSystem.Shutdown();
        }
    }
    
    private static void RunAdvancedDemo(RenderingSystem renderingSystem)
    {
        var terminal = renderingSystem.Terminal;
        var width = terminal.Width;
        var height = terminal.Height;
        
        // Performance tracking
        var frameCount = 0;
        var totalRenderTime = 0.0;
        var minRenderTime = double.MaxValue;
        var maxRenderTime = 0.0;
        var dirtyCellsTotal = 0;
        
        renderingSystem.Scheduler.AfterRender += (s, e) =>
        {
            frameCount++;
            totalRenderTime += e.RenderTimeMs;
            minRenderTime = Math.Min(minRenderTime, e.RenderTimeMs);
            maxRenderTime = Math.Max(maxRenderTime, e.RenderTimeMs);
            dirtyCellsTotal += e.DirtyCellCount;
        };
        
        // Clear and setup
        renderingSystem.Clear();
        
        // Title
        var title = "Advanced Rendering Techniques";
        renderingSystem.WriteText((width - title.Length) / 2, 1, title, 
            new Style { Foreground = Color.Cyan, Bold = true });
        
        // Create different rendering scenarios
        var scenarios = new List<IRenderingScenario>
        {
            new ParticleSystemScenario(5, 5, 30, 15),
            new WaveformScenario(40, 5, 35, 10),
            new TextScrollerScenario(5, 22, 70, 5),
            new ProgressBarsScenario(40, 17, 35, 8)
        };
        
        // Initialize scenarios
        foreach (var scenario in scenarios)
        {
            scenario.Initialize(renderingSystem);
        }
        
        // Main loop
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        
        var updateTask = Task.Run(async () =>
        {
            var stopwatch = Stopwatch.StartNew();
            
            while (!cts.Token.IsCancellationRequested)
            {
                var deltaTime = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                
                // Update all scenarios
                foreach (var scenario in scenarios)
                {
                    scenario.Update(deltaTime);
                }
                
                // Update performance display
                if (frameCount > 0)
                {
                    var avgRenderTime = totalRenderTime / frameCount;
                    var avgDirtyCells = dirtyCellsTotal / frameCount;
                    
                    renderingSystem.WriteText(1, height - 3,
                        $"Performance: FPS: {renderingSystem.Scheduler.ActualFps:F1} | " +
                        $"Avg: {avgRenderTime:F2}ms | " +
                        $"Min: {minRenderTime:F2}ms | " +
                        $"Max: {maxRenderTime:F2}ms | " +
                        $"Cells/Frame: {avgDirtyCells:F0}",
                        new Style { Foreground = Color.Green, Dim = true });
                }
                
                await Task.Delay(16, cts.Token);
            }
        }, cts.Token);
        
        try
        {
            updateTask.Wait();
        }
        catch (OperationCanceledException)
        {
        }
    }
    
    private interface IRenderingScenario
    {
        void Initialize(RenderingSystem renderingSystem);
        void Update(double deltaTime);
    }
    
    private class ParticleSystemScenario : IRenderingScenario
    {
        private readonly int _x, _y, _width, _height;
        private readonly List<Particle> _particles = new();
        private RenderingSystem? _renderingSystem;
        private readonly Random _random = new();
        
        public ParticleSystemScenario(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        
        public void Initialize(RenderingSystem renderingSystem)
        {
            _renderingSystem = renderingSystem;
            
            // Draw border
            renderingSystem.DrawBox(_x - 1, _y - 1, _width + 2, _height + 2,
                new Style { Foreground = Color.Blue }, BoxStyle.Single);
            
            renderingSystem.WriteText(_x, _y - 1, " Particle System ", 
                new Style { Foreground = Color.Blue });
            
            // Create initial particles
            for (int i = 0; i < 20; i++)
            {
                _particles.Add(new Particle
                {
                    X = _random.NextDouble() * _width,
                    Y = _random.NextDouble() * _height,
                    VX = (_random.NextDouble() - 0.5) * 10,
                    VY = (_random.NextDouble() - 0.5) * 10,
                    Life = 1.0,
                    Color = GetRandomColor()
                });
            }
        }
        
        public void Update(double deltaTime)
        {
            if (_renderingSystem == null) return;
            
            _renderingSystem.Scheduler.QueueRender(() =>
            {
                // Clear particle area
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _renderingSystem.Buffer.SetCell(_x + x, _y + y, ' ');
                    }
                }
                
                // Update and draw particles
                for (int i = _particles.Count - 1; i >= 0; i--)
                {
                    var p = _particles[i];
                    
                    // Update position
                    p.X += p.VX * deltaTime;
                    p.Y += p.VY * deltaTime;
                    p.Life -= deltaTime * 0.5;
                    
                    // Bounce off walls
                    if (p.X < 0 || p.X >= _width) p.VX = -p.VX;
                    if (p.Y < 0 || p.Y >= _height) p.VY = -p.VY;
                    
                    // Respawn dead particles
                    if (p.Life <= 0)
                    {
                        p.X = _width / 2;
                        p.Y = _height / 2;
                        p.VX = (_random.NextDouble() - 0.5) * 10;
                        p.VY = (_random.NextDouble() - 0.5) * 10;
                        p.Life = 1.0;
                        p.Color = GetRandomColor();
                    }
                    
                    // Draw particle
                    var px = (int)Math.Clamp(p.X, 0, _width - 1);
                    var py = (int)Math.Clamp(p.Y, 0, _height - 1);
                    var brightness = p.Life;
                    var style = new Style 
                    { 
                        Foreground = p.Color,
                        Dim = brightness < 0.5
                    };
                    
                    _renderingSystem.Buffer.SetCell(_x + px, _y + py, '●', style);
                }
            });
        }
        
        private Color GetRandomColor()
        {
            var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Magenta, Color.Yellow, Color.Cyan };
            return colors[_random.Next(colors.Length)];
        }
        
        private class Particle
        {
            public double X, Y, VX, VY, Life;
            public Color Color;
        }
    }
    
    private class WaveformScenario : IRenderingScenario
    {
        private readonly int _x, _y, _width, _height;
        private RenderingSystem? _renderingSystem;
        private double _time;
        
        public WaveformScenario(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        
        public void Initialize(RenderingSystem renderingSystem)
        {
            _renderingSystem = renderingSystem;
            
            renderingSystem.DrawBox(_x - 1, _y - 1, _width + 2, _height + 2,
                new Style { Foreground = Color.Green }, BoxStyle.Single);
            
            renderingSystem.WriteText(_x, _y - 1, " Waveform Visualizer ", 
                new Style { Foreground = Color.Green });
        }
        
        public void Update(double deltaTime)
        {
            if (_renderingSystem == null) return;
            
            _time += deltaTime;
            
            _renderingSystem.Scheduler.QueueRender(() =>
            {
                // Clear area
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _renderingSystem.Buffer.SetCell(_x + x, _y + y, ' ');
                    }
                }
                
                // Draw multiple waveforms
                for (int wave = 0; wave < 3; wave++)
                {
                    var color = wave switch
                    {
                        0 => Color.Red,
                        1 => Color.Green,
                        _ => Color.Blue
                    };
                    
                    var frequency = 0.5 + wave * 0.3;
                    var phase = wave * Math.PI / 3;
                    
                    for (int x = 0; x < _width; x++)
                    {
                        var t = x / (double)_width * Math.PI * 2;
                        var y = Math.Sin(t * frequency + _time * 2 + phase);
                        var plotY = (int)((y + 1) * 0.5 * (_height - 1));
                        
                        if (plotY >= 0 && plotY < _height)
                        {
                            _renderingSystem.Buffer.SetCell(_x + x, _y + plotY, '█', 
                                new Style { Foreground = color });
                        }
                    }
                }
            });
        }
    }
    
    private class TextScrollerScenario : IRenderingScenario
    {
        private readonly int _x, _y, _width, _height;
        private RenderingSystem? _renderingSystem;
        private double _scrollOffset;
        private readonly string _text = "Welcome to Andy.TUI! This is a demonstration of efficient text scrolling using dirty region tracking. Only the changed portions of the display are redrawn, resulting in optimal performance. ";
        
        public TextScrollerScenario(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        
        public void Initialize(RenderingSystem renderingSystem)
        {
            _renderingSystem = renderingSystem;
            
            renderingSystem.DrawBox(_x - 1, _y - 1, _width + 2, _height + 2,
                new Style { Foreground = Color.Magenta }, BoxStyle.Single);
            
            renderingSystem.WriteText(_x, _y - 1, " Text Scroller ", 
                new Style { Foreground = Color.Magenta });
        }
        
        public void Update(double deltaTime)
        {
            if (_renderingSystem == null) return;
            
            _scrollOffset += deltaTime * 20; // Scroll speed
            if (_scrollOffset > _text.Length) _scrollOffset = 0;
            
            _renderingSystem.Scheduler.QueueRender(() =>
            {
                var offset = (int)_scrollOffset;
                var displayText = _text + _text; // Duplicate for seamless scrolling
                
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        var textIndex = offset + x;
                        if (textIndex < displayText.Length)
                        {
                            var hue = (x / (double)_width + _scrollOffset * 0.01) % 1.0;
                            var (r, g, b) = HsvToRgb(hue, 0.8, 1.0);
                            
                            _renderingSystem.Buffer.SetCell(_x + x, _y + y + 1, 
                                displayText[textIndex],
                                new Style { Foreground = Color.FromRgb((byte)r, (byte)g, (byte)b) });
                        }
                        else
                        {
                            _renderingSystem.Buffer.SetCell(_x + x, _y + y + 1, ' ');
                        }
                    }
                }
            });
        }
        
        private static (int r, int g, int b) HsvToRgb(double h, double s, double v)
        {
            var c = v * s;
            var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
            var m = v - c;
            
            double r, g, b;
            var hi = (int)(h * 6) % 6;
            
            switch (hi)
            {
                case 0: (r, g, b) = (c, x, 0); break;
                case 1: (r, g, b) = (x, c, 0); break;
                case 2: (r, g, b) = (0, c, x); break;
                case 3: (r, g, b) = (0, x, c); break;
                case 4: (r, g, b) = (x, 0, c); break;
                default: (r, g, b) = (c, 0, x); break;
            }
            
            return ((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
        }
    }
    
    private class ProgressBarsScenario : IRenderingScenario
    {
        private readonly int _x, _y, _width, _height;
        private RenderingSystem? _renderingSystem;
        private readonly List<ProgressBar> _bars = new();
        
        public ProgressBarsScenario(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }
        
        public void Initialize(RenderingSystem renderingSystem)
        {
            _renderingSystem = renderingSystem;
            
            renderingSystem.DrawBox(_x - 1, _y - 1, _width + 2, _height + 2,
                new Style { Foreground = Color.Yellow }, BoxStyle.Single);
            
            renderingSystem.WriteText(_x, _y - 1, " Progress Bars ", 
                new Style { Foreground = Color.Yellow });
            
            // Create progress bars
            _bars.Add(new ProgressBar { Label = "Download", Speed = 0.3, Color = Color.Green });
            _bars.Add(new ProgressBar { Label = "Process", Speed = 0.5, Color = Color.Blue });
            _bars.Add(new ProgressBar { Label = "Upload", Speed = 0.2, Color = Color.Magenta });
            _bars.Add(new ProgressBar { Label = "Verify", Speed = 0.4, Color = Color.Cyan });
        }
        
        public void Update(double deltaTime)
        {
            if (_renderingSystem == null) return;
            
            _renderingSystem.Scheduler.QueueRender(() =>
            {
                var barHeight = 1;
                var barSpacing = 1;
                
                for (int i = 0; i < _bars.Count; i++)
                {
                    var bar = _bars[i];
                    bar.Progress += (float)(deltaTime * bar.Speed);
                    if (bar.Progress > 1) bar.Progress = 0;
                    
                    var y = _y + i * (barHeight + barSpacing) + 1;
                    
                    // Draw label
                    _renderingSystem.Buffer.WriteText(_x, y, bar.Label + ":", 
                        new Style { Foreground = Color.White });
                    
                    // Draw progress bar
                    var barX = _x + 10;
                    var barWidth = _width - 12;
                    var filledWidth = (int)(bar.Progress * barWidth);
                    
                    for (int x = 0; x < barWidth; x++)
                    {
                        var ch = x < filledWidth ? '█' : '░';
                        var style = new Style 
                        { 
                            Foreground = x < filledWidth ? bar.Color : Color.DarkGray 
                        };
                        _renderingSystem.Buffer.SetCell(barX + x, y, ch, style);
                    }
                    
                    // Draw percentage
                    var percentage = $"{(int)(bar.Progress * 100)}%";
                    _renderingSystem.Buffer.WriteText(barX + barWidth - percentage.Length, y, 
                        percentage, new Style { Foreground = Color.White, Bold = true });
                }
            });
        }
        
        private class ProgressBar
        {
            public string Label { get; set; } = "";
            public float Progress { get; set; }
            public double Speed { get; set; }
            public Color Color { get; set; }
        }
    }
}