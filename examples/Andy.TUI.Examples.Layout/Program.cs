using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Components;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Layout;

class Program
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Andy.TUI Layout Components Examples");
        Console.WriteLine("===================================");
        Console.WriteLine();
        
        // Demonstrate all layout components
        Console.WriteLine("1. Box Component Demo");
        Console.WriteLine("---------------------");
        ShowBoxDemo();
        
        Console.WriteLine("\n2. Stack Component Demo");
        Console.WriteLine("-----------------------");
        ShowStackDemo();
        
        Console.WriteLine("\n3. Grid Component Demo");
        Console.WriteLine("----------------------");
        ShowGridDemo();
        
        Console.WriteLine("\n4. ScrollView Component Demo");
        Console.WriteLine("----------------------------");
        ShowScrollViewDemo();
        
        Console.WriteLine("\n5. Complex Layout Demo");
        Console.WriteLine("----------------------");
        ShowComplexLayoutDemo();
        
        Console.WriteLine("\nAll demonstrations completed.");
    }
    
    static void ShowBoxDemo()
    {
        Console.WriteLine("  - Box with border and padding");
        Console.WriteLine("  - Box with background color");
        Console.WriteLine("  - Box with rounded border");
        
        // Just show the component types and their properties
        var box1 = new Box { Content = new TextNode("Simple Box"), Padding = new Spacing(2), Border = Border.Single };
        var box2 = new Box { Content = new TextNode("Colored Box"), Padding = new Spacing(2), BackgroundColor = Color.Blue, Border = Border.Double };
        var box3 = new Box { Content = new TextNode("Rounded Box"), Padding = new Spacing(3, 5), Border = Border.Rounded };
        
        Console.WriteLine($"    Box 1: {box1.Border.Style} border, padding={box1.Padding}");
        Console.WriteLine($"    Box 2: {box2.Border.Style} border, background={box2.BackgroundColor}");
        Console.WriteLine($"    Box 3: {box3.Border.Style} border, padding={box3.Padding}");
    }
    
    static void ShowStackDemo()
    {
        Console.WriteLine("  - Vertical stack with spacing");
        Console.WriteLine("  - Horizontal stack with spacing");
        Console.WriteLine("  - Alignment options (start, center, end, stretch)");
        
        var vStack = new Stack { Orientation = Orientation.Vertical, Spacing = 1, Padding = new Spacing(1) };
        vStack.AddChild(new TextNode("Item 1"));
        vStack.AddChild(new TextNode("Item 2"));
        vStack.AddChild(new TextNode("Item 3"));
        
        var hStack = new Stack { Orientation = Orientation.Horizontal, Spacing = 3 };
        hStack.AddChild(new TextNode("Button1"));
        hStack.AddChild(new TextNode("Button2"));
        
        Console.WriteLine($"    Vertical Stack: {vStack.Children.Count} items, spacing={vStack.Spacing}");
        Console.WriteLine($"    Horizontal Stack: {hStack.Children.Count} items, spacing={hStack.Spacing}");
    }
    
    static void ShowGridDemo()
    {
        Console.WriteLine("  - Row and column definitions");
        Console.WriteLine("  - Star sizing, absolute sizing, and auto sizing");
        Console.WriteLine("  - Row and column spans");
        Console.WriteLine("  - Gap between cells");
        
        var grid = new Grid { RowGap = 1, ColumnGap = 2, Padding = new Spacing(1) };
        grid.SetColumns(GridLength.Absolute(15), GridLength.Star(1), GridLength.Absolute(10));
        grid.SetRows(GridLength.Auto, GridLength.Absolute(3), GridLength.Auto);
        
        Console.WriteLine($"    Grid: {grid.Columns.Count} columns x {grid.Rows.Count} rows");
        Console.WriteLine($"    Column types: Absolute(15), Star(1), Absolute(10)");
        Console.WriteLine($"    Row gap={grid.RowGap}, Column gap={grid.ColumnGap}");
    }
    
    static void ShowScrollViewDemo()
    {
        Console.WriteLine("  - Viewport management");
        Console.WriteLine("  - Horizontal and vertical scrolling");
        Console.WriteLine("  - Scrollbar rendering");
        Console.WriteLine("  - Content larger than viewport");
        
        var scrollView = new ScrollView
        {
            Content = new TextNode("Large content...\n" + string.Join("\n", Enumerable.Range(1, 50).Select(i => $"Line {i}"))),
            ShowVerticalScrollbar = true,
            ShowHorizontalScrollbar = true
        };
        
        Console.WriteLine($"    ScrollView: Content with 50 lines");
        Console.WriteLine($"    Scrollbars: V={scrollView.ShowVerticalScrollbar}, H={scrollView.ShowHorizontalScrollbar}");
        Console.WriteLine($"    Features: ScrollToArea, ScrollBy, ScrollToTop/Bottom/Left/Right");
    }
    
    static void ShowComplexLayoutDemo()
    {
        Console.WriteLine("  - Grid as main container");
        Console.WriteLine("  - Box components for header, content, and sidebar");
        Console.WriteLine("  - Stack for menu items");
        Console.WriteLine("  - Nested layout components");
        
        Console.WriteLine("\n  Layout structure:");
        Console.WriteLine("    Grid (2 columns, 2 rows)");
        Console.WriteLine("    ├─ Header Box (spans 2 columns)");
        Console.WriteLine("    ├─ Main Content Box (with border)");
        Console.WriteLine("    └─ Sidebar Box");
        Console.WriteLine("        └─ Stack (vertical menu items)");
    }
}