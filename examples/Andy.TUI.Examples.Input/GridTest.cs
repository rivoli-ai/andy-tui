using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Examples.Input;

class GridTest
{
    static void Main(string[] args)
    {
        var app = new GridTestApp();
        app.Run();
    }
}

class GridTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        renderingSystem.Initialize();
        terminal.Clear();
        
        // Create UI demonstrating Grid layout
        var ui = new VStack(spacing: 2) {
            new Text("Grid Layout Demo").Bold().Color(Color.Cyan),
            
            // Example 1: Basic 3-column grid
            new Text("Basic 3-column grid (1fr each):").Bold(),
            new Grid(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
                .WithGap(1)
            {
                new Text("Cell 1").Color(Color.Green),
                new Text("Cell 2").Color(Color.Yellow),
                new Text("Cell 3").Color(Color.Red),
                new Text("Cell 4").Color(Color.Blue),
                new Text("Cell 5").Color(Color.Magenta),
                new Text("Cell 6").Color(Color.Cyan)
            },
            
            new Spacer(minLength: 2),
            
            // Example 2: Mixed column sizes
            new Text("Mixed column sizes (100px, 2fr, 1fr):").Bold(),
            new Grid()
                .WithColumns(GridTrackSize.Pixels(15), GridTrackSize.Fr(2), GridTrackSize.Fr(1))
                .WithRows(GridTrackSize.Auto, GridTrackSize.Auto)
                .WithGap(2)
            {
                new Text("Fixed").BackgroundColor(Color.DarkGray),
                new Text("2fr - takes more space").BackgroundColor(Color.DarkGray),
                new Text("1fr").BackgroundColor(Color.DarkGray),
                new Text("Row 2").Color(Color.Green),
                new Text("Flexible content here").Color(Color.Yellow),
                new Text("Less").Color(Color.Red)
            },
            
            new Spacer(minLength: 2),
            
            // Example 3: Grid with spanning cells
            new Text("Grid with spanning cells:").Bold(),
            new Grid()
                .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
                .WithRows(GridTrackSize.Auto, GridTrackSize.Auto, GridTrackSize.Auto)
                .WithGap(1)
            {
                // Header spans all columns
                new Text("Header - Spans 3 columns").Bold().Color(Color.Cyan)
                    .GridArea(1, 1, rowSpan: 1, columnSpan: 3),
                
                // Sidebar spans 2 rows
                new Box { new Text("Sidebar") }
                    .WithPadding(1)
                    .GridArea(2, 1, rowSpan: 2, columnSpan: 1),
                
                // Main content
                new Box { new Text("Main Content Area") }
                    .WithPadding(1)
                    .GridArea(2, 2, rowSpan: 1, columnSpan: 2),
                
                // Footer in bottom right
                new Text("Footer").Color(Color.Gray)
                    .GridArea(3, 2, rowSpan: 1, columnSpan: 2)
            },
            
            new Spacer(minLength: 2),
            
            // Example 4: Responsive-like grid
            new Text("Grid with alignment:").Bold(),
            new Grid()
                .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1))
                .WithGap(2)
                .JustifyItems(JustifyContent.Center)
                .AlignItems(AlignItems.Center)
            {
                new Box { new Text("Centered 1") }.WithWidth(10).WithHeight(3),
                new Box { new Text("Centered 2") }.WithWidth(10).WithHeight(3),
                new Box { new Text("Centered 3") }.WithWidth(10).WithHeight(3),
                new Box { new Text("Centered 4") }.WithWidth(10).WithHeight(3)
            }
        };
        
        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        
        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}