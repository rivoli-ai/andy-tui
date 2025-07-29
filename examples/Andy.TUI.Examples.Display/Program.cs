using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Andy.TUI.Components;
using Andy.TUI.Components.Display;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.TUI.Examples.Display;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var terminal = new AnsiTerminal();
            using var renderingSystem = new RenderingSystem(terminal);
            
            // Setup services
            var services = new ServiceCollection()
                .AddSingleton<IThemeProvider, ThemeProvider>()
                .AddSingleton<ISharedStateManager, SharedStateManager>()
                .BuildServiceProvider();
            
            var themeProvider = services.GetRequiredService<IThemeProvider>();
            var sharedStateManager = services.GetRequiredService<ISharedStateManager>();
            
            // Initialize rendering
            renderingSystem.Initialize();
            
            // Create Table
            var table = new Table<Person>
            {
                ShowHeader = true,
                ShowBorder = true,
                AllowSelection = true
            };
            table.Columns = new List<TableColumn<Person>>
            {
                new() { Header = "ID", Width = 5, ValueGetter = p => p.Id.ToString(), Alignment = TableAlignment.Right },
                new() { Header = "Name", Width = 20, ValueGetter = p => p.Name },
                new() { Header = "Age", Width = 5, ValueGetter = p => p.Age.ToString(), Alignment = TableAlignment.Right },
                new() { Header = "Department", Width = 15, ValueGetter = p => p.Department }
            };
            table.Items = new List<Person>
            {
                new() { Id = 1, Name = "Alice Johnson", Age = 28, Department = "Engineering" },
                new() { Id = 2, Name = "Bob Smith", Age = 34, Department = "Marketing" },
                new() { Id = 3, Name = "Carol Williams", Age = 29, Department = "Sales" },
                new() { Id = 4, Name = "David Brown", Age = 41, Department = "Engineering" },
                new() { Id = 5, Name = "Eve Davis", Age = 35, Department = "HR" }
            };
            
            var tableContext = new ComponentContext(table, services, themeProvider, sharedStateManager);
            table.Initialize(tableContext);
            // Set table to show all rows: 1 border top + 1 header + 1 separator + 5 data rows + 1 border bottom = 9
            table.Arrange(new Rectangle(4, 5, 50, 9));
            
            // Create ProgressBar
            var progressBar = new ProgressBar
            {
                Label = "Processing:",
                Style = ProgressBarStyle.Blocks,
                ShowPercentage = true,
                FillColor = Color.Green
            };
            var progressContext = new ComponentContext(progressBar, services, themeProvider, sharedStateManager);
            progressBar.Initialize(progressContext);
            progressBar.Arrange(new Rectangle(4, 16, 50, 2));
            
            // Create multiple spinners to show different styles
            var spinnerStyles = new[] 
            { 
                SpinnerStyle.Dots, 
                SpinnerStyle.Line, 
                SpinnerStyle.Arrow, 
                SpinnerStyle.Circle,
                SpinnerStyle.Dots2,
                SpinnerStyle.SimpleDots,
                SpinnerStyle.Bar,
                SpinnerStyle.Square,
                SpinnerStyle.Bounce,
                SpinnerStyle.Pipe,
                SpinnerStyle.GrowingDots,
                SpinnerStyle.Flip
            };
            var spinners = new List<Spinner>();
            
            for (int i = 0; i < spinnerStyles.Length; i++)
            {
                var spinner = new Spinner
                {
                    Style = spinnerStyles[i],
                    Text = spinnerStyles[i].ToString(),
                    Color = Color.Cyan,
                    AnimationSpeed = 100
                };
                var spinnerContext = new ComponentContext(spinner, services, themeProvider, sharedStateManager);
                spinner.Initialize(spinnerContext);
                
                // Arrange spinners in 3 rows of 4
                int col = i % 4;
                int row = i / 4;
                spinner.Arrange(new Rectangle(4 + col * 18, 20 + row * 2, 16, 1));
                spinner.Start();
                spinners.Add(spinner);
            }
            
            var running = true;
            var cts = new CancellationTokenSource();
            var needsFullRender = true;
            var lastProgressValue = -1.0;
            
            // Progress animation task
            var progressTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    progressBar.Increment(2);
                    if (progressBar.IsComplete)
                    {
                        progressBar.Value = 0;
                    }
                    await Task.Delay(100, cts.Token);
                }
            }, cts.Token);
            
            // Input handling
            var inputHandler = new ConsoleInputHandler();
            inputHandler.KeyPressed += (sender, args) =>
            {
                switch (args.Key)
                {
                    case ConsoleKey.Q:
                        running = false;
                        cts.Cancel();
                        break;
                        
                    case ConsoleKey.UpArrow:
                        table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
                        needsFullRender = true;
                        break;
                        
                    case ConsoleKey.DownArrow:
                        table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
                        needsFullRender = true;
                        break;
                        
                    case ConsoleKey.Spacebar:
                        // Cycle through progress bar styles
                        progressBar.Style = progressBar.Style switch
                        {
                            ProgressBarStyle.Blocks => ProgressBarStyle.Line,
                            ProgressBarStyle.Line => ProgressBarStyle.Dots,
                            ProgressBarStyle.Dots => ProgressBarStyle.Gradient,
                            ProgressBarStyle.Gradient => ProgressBarStyle.Blocks,
                            _ => ProgressBarStyle.Blocks
                        };
                        needsFullRender = true;
                        break;
                        
                    case ConsoleKey.S:
                        // Cycle through different spinner style sets
                        var allStyles = Enum.GetValues<SpinnerStyle>().Where(s => 
                            s != SpinnerStyle.Clock && // Skip emoji styles that may not render
                            s != SpinnerStyle.Earth && 
                            s != SpinnerStyle.Moon && 
                            s != SpinnerStyle.Runner &&
                            s != SpinnerStyle.Pong // Skip Pong as it's very wide
                        ).ToArray();
                        
                        for (int i = 0; i < spinners.Count && i < allStyles.Length; i++)
                        {
                            var currentIndex = Array.IndexOf(allStyles, spinners[i].Style);
                            spinners[i].Style = allStyles[(currentIndex + spinners.Count) % allStyles.Length];
                            spinners[i].Text = spinners[i].Style.ToString();
                        }
                        needsFullRender = true;
                        break;
                        
                    case ConsoleKey.T:
                        // Toggle all spinners
                        foreach (var spinner in spinners)
                        {
                            if (spinner.IsAnimating)
                            {
                                spinner.Stop();
                            }
                            else
                            {
                                spinner.Start();
                            }
                        }
                        needsFullRender = true;
                        break;
                }
            };
            
            inputHandler.Start();
            
            void RenderHeader()
            {
                renderingSystem.WriteText(2, 1, "Andy.TUI Display Components Demo", Style.Default.WithForegroundColor(Color.Cyan).WithBold());
                renderingSystem.WriteText(2, 2, "=================================", Style.Default.WithForegroundColor(Color.Cyan));
            }
            
            void RenderSectionHeaders()
            {
                // Clear the lines where headers will be to avoid overlaps
                renderingSystem.FillRect(0, 4, 80, 1, ' ');
                renderingSystem.FillRect(0, 15, 80, 1, ' ');
                renderingSystem.FillRect(0, 19, 80, 1, ' ');
                
                renderingSystem.WriteText(2, 4, "Table Component:", Style.Default.WithBold());
                renderingSystem.WriteText(2, 15, "ProgressBar Component:", Style.Default.WithBold());
                renderingSystem.WriteText(2, 19, "Spinner Components (various styles):", Style.Default.WithBold());
            }
            
            void RenderInstructions()
            {
                renderingSystem.WriteText(2, 27, "Up/Down: Navigate | Space: Progress style | S: Cycle spinners | T: Toggle spinners | Q: Quit",
                    Style.Default.WithForegroundColor(Color.Gray));
            }
            
            void RenderTable()
            {
                // Clear the table area first to avoid artifacts
                renderingSystem.FillRect(table.Bounds.X, table.Bounds.Y, table.Bounds.Width, table.Bounds.Height, ' ');
                var node = table.Render();
                RenderNode(node, 0, 0); // Use absolute positioning from the node
            }
            
            void RenderProgressBar()
            {
                // Clear the progress bar area first
                renderingSystem.FillRect(progressBar.Bounds.X, progressBar.Bounds.Y, progressBar.Bounds.Width, progressBar.Bounds.Height, ' ');
                var node = progressBar.Render();
                RenderNode(node, 0, 0); // Use absolute positioning from the node
            }
            
            void RenderSpinners()
            {
                foreach (var spinner in spinners)
                {
                    // Clear the spinner area first
                    renderingSystem.FillRect(spinner.Bounds.X, spinner.Bounds.Y, spinner.Bounds.Width, spinner.Bounds.Height, ' ');
                    var node = spinner.Render();
                    RenderNode(node, 0, 0); // Use absolute positioning from the node
                }
            }
            
            void RenderNode(VirtualNode node, int baseX, int baseY)
            {
                if (node is ElementNode element)
                {
                    // For table, progressbar, and spinner nodes, use their absolute positioning
                    if (element.TagName == "table" || element.TagName == "progressbar" || element.TagName == "spinner")
                    {
                        baseX = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                        baseY = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                    }
                    
                    foreach (var child in element.Children)
                    {
                        if (child is TextNode text)
                        {
                            var x = baseX + (element.TagName == "text" && element.Props.TryGetValue("x", out var childX) && childX is int cx ? cx : 0);
                            var y = baseY + (element.TagName == "text" && element.Props.TryGetValue("y", out var childY) && childY is int cy ? cy : 0);
                            var style = element.Props.TryGetValue("style", out var styleObj) && styleObj is Style s ? s : Style.Default;
                            renderingSystem.WriteText(x, y, text.Content, style);
                        }
                        else if (child is ElementNode childElement)
                        {
                            RenderNode(child, baseX, baseY);
                        }
                    }
                }
            }
            
            // Initial render
            renderingSystem.Clear();
            RenderHeader();
            RenderSectionHeaders();
            RenderInstructions();
            RenderTable();
            RenderProgressBar();
            RenderSpinners();
            
            // Render loop
            var renderTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (needsFullRender)
                        {
                            needsFullRender = false;
                            RenderSectionHeaders(); // Re-render headers to avoid label overlap
                            RenderTable();
                            RenderProgressBar();
                            RenderSpinners();
                        }
                        else
                        {
                            // Check if we need to update specific components
                            if (Math.Abs(progressBar.Value - lastProgressValue) > 0.01)
                            {
                                lastProgressValue = progressBar.Value;
                                RenderProgressBar();
                            }
                            
                            // Always update spinners if any are animating
                            if (spinners.Any(s => s.IsAnimating))
                            {
                                RenderSpinners();
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    
                    await Task.Delay(50, cts.Token);
                }
            }, cts.Token);
            
            // Wait for exit
            while (running)
            {
                Thread.Sleep(100);
            }
            
            inputHandler.Stop();
            
            // Stop all spinners
            foreach (var spinner in spinners)
            {
                spinner.Stop();
                spinner.Dispose();
            }
            
            await Task.WhenAll(progressTask, renderTask);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Department { get; set; } = string.Empty;
}