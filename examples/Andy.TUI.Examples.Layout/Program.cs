using System;
using System.Threading;
using Andy.TUI.Components;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.TUI.Examples.Layout;

class Program
{
    static void Main(string[] args)
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
            renderingSystem.Clear();
            
            // Title
            renderingSystem.WriteText(2, 1, "Andy.TUI Layout Components Demo", 
                Style.Default.WithForegroundColor(Color.Cyan).WithBold());
            renderingSystem.WriteText(2, 2, "===============================", 
                Style.Default.WithForegroundColor(Color.Cyan));
            
            // Create Box examples
            renderingSystem.WriteText(2, 4, "Box Components:", Style.Default.WithBold());
            
            // Simple box
            var simpleBox = new Box
            {
                Border = new Border(BorderStyle.Single),
                Padding = new Spacing(1, 2),
                Content = new TextNode("Simple Box with Border")
            };
            var simpleContext = new ComponentContext(simpleBox, services, themeProvider, sharedStateManager);
            simpleBox.Initialize(simpleContext);
            simpleBox.Arrange(new Rectangle(2, 6, 30, 5));
            
            // Colored box
            var coloredBox = new Box
            {
                Border = new Border(BorderStyle.Double),
                Padding = new Spacing(1),
                BorderColor = Color.Green,
                BackgroundColor = Color.DarkBlue,
                ForegroundColor = Color.White,
                Content = new TextNode("Colored Box")
            };
            var coloredContext = new ComponentContext(coloredBox, services, themeProvider, sharedStateManager);
            coloredBox.Initialize(coloredContext);
            coloredBox.Arrange(new Rectangle(35, 6, 25, 5));
            
            // Stack example
            renderingSystem.WriteText(2, 12, "Stack Component:", Style.Default.WithBold());
            
            var stack = new Stack
            {
                Orientation = Orientation.Vertical,
                Spacing = 1
            };
            var stackContext = new ComponentContext(stack, services, themeProvider, sharedStateManager);
            stack.Initialize(stackContext);
            stack.Arrange(new Rectangle(2, 14, 30, 10));
            
            // Grid example - simplified
            renderingSystem.WriteText(35, 12, "Grid Component:", Style.Default.WithBold());
            
            var grid = new Grid();
            
            // Add columns
            grid.AddColumn(new ColumnDefinition { Width = GridLength.Absolute(15) });
            grid.AddColumn(new ColumnDefinition { Width = GridLength.Star(1) });
            
            // Add rows
            grid.AddRow(new RowDefinition { Height = GridLength.Auto });
            grid.AddRow(new RowDefinition { Height = GridLength.Star(1) });
            
            var gridContext = new ComponentContext(grid, services, themeProvider, sharedStateManager);
            grid.Initialize(gridContext);
            grid.Arrange(new Rectangle(35, 14, 35, 10));
            
            // Instructions
            renderingSystem.WriteText(2, 25, "Press Q to quit", 
                Style.Default.WithForegroundColor(Color.Gray));
            
            var running = true;
            
            // Input handling
            var inputHandler = new ConsoleInputHandler();
            inputHandler.KeyPressed += (sender, args) =>
            {
                if (args.Key == ConsoleKey.Q)
                {
                    running = false;
                }
            };
            
            inputHandler.Start();
            
            // Render components
            void RenderComponents()
            {
                RenderNode(simpleBox.Render(), 0, 0);
                RenderNode(coloredBox.Render(), 0, 0);
                
                // For stack, we'll just show a placeholder since it needs children
                renderingSystem.DrawBox(2, 14, 30, 8, Style.Default, BoxStyle.Single);
                renderingSystem.WriteText(3, 15, "Stack Item 1", Style.Default);
                renderingSystem.WriteText(3, 17, "Stack Item 2", Style.Default);
                renderingSystem.WriteText(3, 19, "Stack Item 3", Style.Default);
                
                // For grid, show a simple layout
                renderingSystem.DrawBox(35, 14, 35, 8, Style.Default, BoxStyle.Single);
                renderingSystem.WriteText(36, 15, "Header (colspan=2)", Style.Default);
                renderingSystem.WriteText(36, 17, "Left | Right Content", Style.Default);
            }
            
            void RenderNode(VirtualNode node, int baseX, int baseY)
            {
                if (node is ElementNode element)
                {
                    // Handle different element types
                    if (element.TagName == "box" || element.TagName == "stack" || element.TagName == "grid")
                    {
                        baseX = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                        baseY = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                        
                        // Render all children
                        foreach (var child in element.Children)
                        {
                            RenderNode(child, baseX, baseY);
                        }
                    }
                    else if (element.TagName == "rect")
                    {
                        // Handle background fill
                        var x = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                        var y = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                        var width = element.Props.TryGetValue("width", out var wObj) && wObj is int w ? w : 0;
                        var height = element.Props.TryGetValue("height", out var hObj) && hObj is int h ? h : 0;
                        var fill = element.Props.TryGetValue("fill", out var fillObj) && fillObj is Color fillColor ? fillColor : (Color?)null;
                        
                        if (fill.HasValue)
                        {
                            var bgStyle = Style.Default.WithBackgroundColor(fill.Value);
                            renderingSystem.FillRect(x, y, width, height, ' ', bgStyle);
                        }
                    }
                    else if (element.TagName == "text")
                    {
                        // Text nodes from box already have absolute positioning
                        var x = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                        var y = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                        var style = element.Props.TryGetValue("style", out var styleObj) && styleObj is Style s ? s : Style.Default;
                        
                        foreach (var child in element.Children)
                        {
                            if (child is TextNode text)
                            {
                                renderingSystem.WriteText(x, y, text.Content, style);
                            }
                        }
                    }
                    else if (element.TagName == "content")
                    {
                        // Content wrapper also has absolute positioning from the box
                        var x = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                        var y = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                        var color = element.Props.TryGetValue("color", out var colorObj) && colorObj is Color c ? c : (Color?)null;
                        
                        foreach (var child in element.Children)
                        {
                            if (child is TextNode text)
                            {
                                var style = color.HasValue ? Style.Default.WithForegroundColor(color.Value) : Style.Default;
                                renderingSystem.WriteText(x, y, text.Content, style);
                            }
                            else
                            {
                                RenderNode(child, x, y);
                            }
                        }
                    }
                    else
                    {
                        // For other elements, recursively render children
                        foreach (var child in element.Children)
                        {
                            RenderNode(child, baseX, baseY);
                        }
                    }
                }
                else if (node is TextNode text)
                {
                    // Direct text node
                    renderingSystem.WriteText(baseX, baseY, text.Content, Style.Default);
                }
                else if (node is FragmentNode fragment)
                {
                    // Handle fragment nodes - render all children
                    foreach (var child in fragment.Children)
                    {
                        RenderNode(child, baseX, baseY);
                    }
                }
            }
            
            RenderComponents();
            
            // Main loop
            while (running)
            {
                Thread.Sleep(100);
            }
            
            inputHandler.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}