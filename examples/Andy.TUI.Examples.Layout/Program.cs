using Andy.TUI.Components;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Examples.Layout;

class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Andy.TUI Layout Components Examples");
        Console.WriteLine("===================================");
        Console.WriteLine();
        Console.WriteLine("1. Box Component Demo");
        Console.WriteLine("2. Stack Component Demo");
        Console.WriteLine("3. Grid Component Demo");
        Console.WriteLine("4. ScrollView Component Demo");
        Console.WriteLine("5. Complex Layout Demo");
        Console.WriteLine();
        Console.Write("Select a demo (1-5): ");
        
        var choice = Console.ReadLine();
        Console.Clear();
        
        switch (choice)
        {
            case "1":
                BoxDemo();
                break;
            case "2":
                StackDemo();
                break;
            case "3":
                GridDemo();
                break;
            case "4":
                ScrollViewDemo();
                break;
            case "5":
                ComplexLayoutDemo();
                break;
            default:
                Console.WriteLine("Invalid choice!");
                break;
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
    
    static void BoxDemo()
    {
        Console.WriteLine("Box Component Demo\n");
        
        // Simple box with border
        var box1 = new Box
        {
            Border = Border.Single,
            Padding = new Spacing(2, 4),
            Content = Text("Simple Box with Single Border")
        };
        
        RenderComponent(box1, 0, 3);
        
        // Box with background color
        var box2 = new Box
        {
            Border = Border.Double,
            BorderColor = Color.Cyan,
            BackgroundColor = Color.DarkBlue,
            ForegroundColor = Color.White,
            Padding = new Spacing(1, 3),
            Content = Text("Colored Box with Double Border")
        };
        
        RenderComponent(box2, 0, 10);
        
        // Box with rounded border and alignment
        var box3 = new Box
        {
            Border = Border.Rounded,
            BorderColor = Color.Green,
            Padding = new Spacing(2, 5),
            ContentHorizontalAlignment = Alignment.Center,
            Content = Text("Centered Content\nin\nRounded Box")
        };
        
        RenderComponent(box3, 40, 3);
    }
    
    static void StackDemo()
    {
        Console.WriteLine("Stack Component Demo\n");
        
        // Vertical stack
        var vStack = new Stack
        {
            Orientation = Orientation.Vertical,
            Spacing = 1,
            Padding = new Spacing(1, 2)
        };
        
        vStack.AddChild(CreateColoredBox("Item 1", Color.Red));
        vStack.AddChild(CreateColoredBox("Item 2", Color.Green));
        vStack.AddChild(CreateColoredBox("Item 3", Color.Blue));
        
        Console.WriteLine("Vertical Stack:");
        RenderComponent(vStack, 0, 4);
        
        // Horizontal stack
        var hStack = new Stack
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Padding = new Spacing(1, 2)
        };
        
        hStack.AddChild(CreateColoredBox("A", Color.Yellow));
        hStack.AddChild(CreateColoredBox("B", Color.Magenta));
        hStack.AddChild(CreateColoredBox("C", Color.Cyan));
        
        Console.WriteLine("\nHorizontal Stack:");
        RenderComponent(hStack, 0, 12);
        
        // Stack with different alignments
        var alignedStack = new Stack
        {
            Orientation = Orientation.Vertical,
            CrossAxisAlignment = Alignment.Center,
            MainAxisAlignment = Alignment.Center,
            Spacing = 1
        };
        
        alignedStack.AddChild(Text("Short"));
        alignedStack.AddChild(Text("Medium Length"));
        alignedStack.AddChild(Text("This is a longer text"));
        
        Console.WriteLine("\n\nCentered Stack:");
        RenderComponent(alignedStack, 40, 4);
    }
    
    static void GridDemo()
    {
        Console.WriteLine("Grid Component Demo\n");
        
        // Simple 2x3 grid
        var grid = new Grid
        {
            RowGap = 1,
            ColumnGap = 2,
            Padding = new Spacing(1)
        };
        
        grid.SetColumns(GridLength.Absolute(15), GridLength.Absolute(15), GridLength.Absolute(15));
        grid.SetRows(GridLength.Absolute(3), GridLength.Absolute(3));
        
        grid.AddChild(CreateColoredBox("Cell [0,0]", Color.Red), 0, 0);
        grid.AddChild(CreateColoredBox("Cell [0,1]", Color.Green), 0, 1);
        grid.AddChild(CreateColoredBox("Cell [0,2]", Color.Blue), 0, 2);
        grid.AddChild(CreateColoredBox("Cell [1,0]", Color.Yellow), 1, 0);
        grid.AddChild(CreateColoredBox("Cell [1,1]", Color.Magenta), 1, 1);
        grid.AddChild(CreateColoredBox("Cell [1,2]", Color.Cyan), 1, 2);
        
        Console.WriteLine("Fixed Size Grid:");
        RenderComponent(grid, 0, 4);
        
        // Grid with star sizing
        var starGrid = new Grid
        {
            RowGap = 1,
            ColumnGap = 2
        };
        
        starGrid.SetColumns(GridLength.Star(1), GridLength.Star(2), GridLength.Star(1));
        starGrid.SetRows(GridLength.Auto, GridLength.Star(1));
        
        starGrid.AddChild(Text("Header spanning 3 columns"), 0, 0, 1, 3);
        starGrid.AddChild(CreateColoredBox("Left", Color.DarkGray), 1, 0);
        starGrid.AddChild(CreateColoredBox("Center (2x)", Color.Gray), 1, 1);
        starGrid.AddChild(CreateColoredBox("Right", Color.DarkGray), 1, 2);
        
        Console.WriteLine("\n\nStar-Sized Grid:");
        RenderComponent(starGrid, 0, 16);
    }
    
    static void ScrollViewDemo()
    {
        Console.WriteLine("ScrollView Component Demo\n");
        Console.WriteLine("Use arrow keys to scroll, 'q' to quit\n");
        
        // Create large content
        var contentLines = new List<string>();
        for (int i = 0; i < 50; i++)
        {
            contentLines.Add($"Line {i + 1}: " + new string('=', 100));
        }
        
        var scrollView = new ScrollView
        {
            ShowVerticalScrollbar = true,
            ShowHorizontalScrollbar = true,
            ScrollbarStyle = ScrollbarStyle.Simple,
            Content = Text(string.Join("\n", contentLines))
        };
        
        var container = new Box
        {
            Border = Border.Single,
            BorderColor = Color.White,
            Width = 60,
            Height = 20,
            Content = Component(scrollView)
        };
        
        // Initial render
        RenderComponent(container, 0, 4);
        Console.SetCursorPosition(0, 26);
        Console.WriteLine($"Scroll Position: X={scrollView.ScrollX}, Y={scrollView.ScrollY}");
        Console.WriteLine("Use arrow keys to scroll, 'q' to quit");
        
        // Handle scrolling
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    scrollView.ScrollBy(0, -1);
                    break;
                case ConsoleKey.DownArrow:
                    scrollView.ScrollBy(0, 1);
                    break;
                case ConsoleKey.LeftArrow:
                    scrollView.ScrollBy(-5, 0);
                    break;
                case ConsoleKey.RightArrow:
                    scrollView.ScrollBy(5, 0);
                    break;
                case ConsoleKey.PageUp:
                    scrollView.ScrollBy(0, -10);
                    break;
                case ConsoleKey.PageDown:
                    scrollView.ScrollBy(0, 10);
                    break;
                case ConsoleKey.Home:
                    scrollView.ScrollToTop();
                    scrollView.ScrollToLeft();
                    break;
                case ConsoleKey.End:
                    scrollView.ScrollToBottom();
                    break;
            }
            
            // Re-render
            RenderComponent(container, 0, 4);
            Console.SetCursorPosition(0, 26);
            Console.WriteLine($"Scroll Position: X={scrollView.ScrollX}, Y={scrollView.ScrollY}  ");
            
        } while (key.KeyChar != 'q' && key.KeyChar != 'Q');
    }
    
    static void ComplexLayoutDemo()
    {
        Console.WriteLine("Complex Layout Demo - Dashboard\n");
        
        // Main container
        var mainGrid = new Grid
        {
            RowGap = 1,
            ColumnGap = 2
        };
        
        mainGrid.SetColumns(GridLength.Absolute(20), GridLength.Star(1));
        mainGrid.SetRows(GridLength.Absolute(3), GridLength.Star(1), GridLength.Absolute(3));
        
        // Header
        var header = new Box
        {
            Border = Border.Double,
            BorderColor = Color.Cyan,
            BackgroundColor = Color.DarkBlue,
            ForegroundColor = Color.White,
            ContentHorizontalAlignment = Alignment.Center,
            Padding = new Spacing(0, 2),
            Content = Text("Dashboard Header")
        };
        mainGrid.AddChild(Component(header), 0, 0, 1, 2);
        
        // Sidebar
        var sidebar = new Box
        {
            Border = Border.Single,
            BorderColor = Color.Gray,
            Padding = new Spacing(1),
            Content = Component(CreateSidebar())
        };
        mainGrid.AddChild(Component(sidebar), 1, 0);
        
        // Main content area
        var contentArea = new ScrollView
        {
            ShowVerticalScrollbar = true,
            Content = Component(CreateMainContent())
        };
        
        var contentBox = new Box
        {
            Border = Border.Single,
            BorderColor = Color.White,
            Padding = new Spacing(1),
            Content = Component(contentArea)
        };
        mainGrid.AddChild(Component(contentBox), 1, 1);
        
        // Footer
        var footer = new Box
        {
            Border = Border.Single,
            BorderColor = Color.DarkGray,
            ContentHorizontalAlignment = Alignment.Center,
            Content = Text("Status: Ready | Items: 42 | Memory: 128MB")
        };
        mainGrid.AddChild(Component(footer), 2, 0, 1, 2);
        
        // Render the entire layout
        RenderComponent(mainGrid, 0, 3);
    }
    
    static Stack CreateSidebar()
    {
        var sidebar = new Stack
        {
            Orientation = Orientation.Vertical,
            Spacing = 1
        };
        
        sidebar.AddChild(Text("Menu"));
        sidebar.AddChild(Text("────────────"));
        sidebar.AddChild(Text("• Dashboard"));
        sidebar.AddChild(Text("• Reports"));
        sidebar.AddChild(Text("• Analytics"));
        sidebar.AddChild(Text("• Settings"));
        sidebar.AddChild(Text("• Help"));
        
        return sidebar;
    }
    
    static Grid CreateMainContent()
    {
        var content = new Grid
        {
            RowGap = 2,
            ColumnGap = 3
        };
        
        content.SetColumns(GridLength.Star(1), GridLength.Star(1));
        content.SetRows(GridLength.Auto, GridLength.Auto);
        
        // Add some widgets
        content.AddChild(CreateWidget("CPU Usage", "45%", Color.Green), 0, 0);
        content.AddChild(CreateWidget("Memory", "2.1 GB", Color.Yellow), 0, 1);
        content.AddChild(CreateWidget("Network", "1.2 MB/s", Color.Cyan), 1, 0);
        content.AddChild(CreateWidget("Storage", "120 GB", Color.Magenta), 1, 1);
        
        return content;
    }
    
    static VirtualNode CreateWidget(string title, string value, Color color)
    {
        var widget = new Box
        {
            Border = Border.Rounded,
            BorderColor = color,
            Padding = new Spacing(1, 3),
            Content = Element("div", 
                Element("div", Attributes(("style", "font-weight:bold")), Text(title)),
                Element("div", Attributes(("style", "font-size:large")), Text(value))
            )
        };
        
        return Component(widget);
    }
    
    static VirtualNode CreateColoredBox(string text, Color color)
    {
        var box = new Box
        {
            Border = Border.Single,
            BorderColor = color,
            Padding = new Spacing(0, 1),
            Content = Text(text)
        };
        
        return Component(box);
    }
    
    static void RenderComponent(LayoutComponent component, int x, int y)
    {
        // Initialize component
        component.Initialize(new SimpleComponentContext());
        
        // Measure and arrange
        var availableSize = new Size(Console.WindowWidth - x, Console.WindowHeight - y);
        component.Measure(availableSize);
        component.Arrange(new Rectangle(x, y, 
            Math.Min(component.Width ?? availableSize.Width, availableSize.Width),
            Math.Min(component.Height ?? availableSize.Height, availableSize.Height)));
        
        // Render to virtual DOM
        var vdom = component.Render();
        
        // Simple rendering to console (in a real app, this would use the rendering system)
        RenderNode(vdom);
    }
    
    static void RenderNode(VirtualNode node)
    {
        if (node is TextNode textNode)
        {
            Console.Write(textNode.Text);
        }
        else if (node is ElementNode elementNode)
        {
            // Handle specific element types
            if (elementNode.Tag == "text" && 
                elementNode.Attributes.TryGetValue("x", out var x) &&
                elementNode.Attributes.TryGetValue("y", out var y))
            {
                Console.SetCursorPosition((int)x!, (int)y!);
                
                if (elementNode.Attributes.TryGetValue("style", out var style) && style is Style s)
                {
                    if (s.Foreground.HasValue)
                        Console.ForegroundColor = ToConsoleColor(s.Foreground.Value);
                }
                
                foreach (var child in elementNode.Children)
                {
                    RenderNode(child);
                }
                
                Console.ResetColor();
            }
            else if (elementNode.Tag == "rect" &&
                elementNode.Attributes.TryGetValue("fill", out var fill) && fill is Color fillColor)
            {
                var rx = (int)elementNode.Attributes["x"]!;
                var ry = (int)elementNode.Attributes["y"]!;
                var rw = (int)elementNode.Attributes["width"]!;
                var rh = (int)elementNode.Attributes["height"]!;
                
                Console.BackgroundColor = ToConsoleColor(fillColor);
                for (int row = 0; row < rh; row++)
                {
                    Console.SetCursorPosition(rx, ry + row);
                    Console.Write(new string(' ', rw));
                }
                Console.ResetColor();
            }
            else
            {
                foreach (var child in elementNode.Children)
                {
                    RenderNode(child);
                }
            }
        }
        else if (node is FragmentNode fragmentNode)
        {
            foreach (var child in fragmentNode.Children)
            {
                RenderNode(child);
            }
        }
        else if (node is ComponentNode componentNode)
        {
            var rendered = componentNode.Component.Render();
            RenderNode(rendered);
        }
    }
    
    static ConsoleColor ToConsoleColor(Color color)
    {
        return color switch
        {
            Color.Black => ConsoleColor.Black,
            Color.Red => ConsoleColor.Red,
            Color.Green => ConsoleColor.Green,
            Color.Yellow => ConsoleColor.Yellow,
            Color.Blue => ConsoleColor.Blue,
            Color.Magenta => ConsoleColor.Magenta,
            Color.Cyan => ConsoleColor.Cyan,
            Color.White => ConsoleColor.White,
            Color.Gray => ConsoleColor.Gray,
            Color.DarkGray => ConsoleColor.DarkGray,
            Color.DarkRed => ConsoleColor.DarkRed,
            Color.DarkGreen => ConsoleColor.DarkGreen,
            Color.DarkYellow => ConsoleColor.DarkYellow,
            Color.DarkBlue => ConsoleColor.DarkBlue,
            Color.DarkMagenta => ConsoleColor.DarkMagenta,
            Color.DarkCyan => ConsoleColor.DarkCyan,
            _ => ConsoleColor.White
        };
    }
    
    class SimpleComponentContext : IComponentContext
    {
        public IComponent? Parent => null;
        public IEventHandler EventHandler { get; } = new SimpleEventHandler();
        public ISharedStateManager SharedState { get; } = new SharedStateManager();
        public IThemeProvider Theme { get; } = new ThemeProvider();
        
        class SimpleEventHandler : IEventHandler
        {
            public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class { }
            public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class { }
            public void Publish<TEvent>(TEvent eventData) where TEvent : class { }
            public void Clear() { }
            public void Dispose() { }
        }
    }
}