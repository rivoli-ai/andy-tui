using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Layout;

namespace Andy.TUI.Examples.Input;

class GridTestApp
{
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        renderingSystem.Initialize();
        terminal.Clear();

        // Create grids separately to add children
        var grid1 = new Grid(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
            .WithGap(1);
        grid1.Add(new Text("Cell 1").Color(Color.Green));
        grid1.Add(new Text("Cell 2").Color(Color.Yellow));
        grid1.Add(new Text("Cell 3").Color(Color.Red));
        grid1.Add(new Text("Cell 4").Color(Color.Blue));
        grid1.Add(new Text("Cell 5").Color(Color.Magenta));
        grid1.Add(new Text("Cell 6").Color(Color.Cyan));

        var grid2 = new Grid()
            .WithColumns(GridTrackSize.Pixels(15), GridTrackSize.Fr(2), GridTrackSize.Fr(1))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto)
            .WithGap(2);
        grid2.Add(new Text("Fixed"));
        grid2.Add(new Text("2fr - takes more space"));
        grid2.Add(new Text("1fr"));
        grid2.Add(new Text("Row 2").Color(Color.Green));
        grid2.Add(new Text("Flexible content here").Color(Color.Yellow));
        grid2.Add(new Text("Less").Color(Color.Red));

        var grid3 = new Grid()
            .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto, GridTrackSize.Auto)
            .WithGap(1);
        grid3.Add(new Text("Header - Spans 3 columns").Bold().Color(Color.Cyan)
            .GridArea(1, 1, rowSpan: 1, columnSpan: 3));
        grid3.Add(new Box { new Text("Sidebar") }
            .WithPadding(1)
            .GridArea(2, 1, rowSpan: 2, columnSpan: 1));
        grid3.Add(new Box { new Text("Main Content Area") }
            .WithPadding(1)
            .GridArea(2, 2, rowSpan: 1, columnSpan: 2));
        grid3.Add(new Text("Footer").Color(Color.Gray)
            .GridArea(3, 2, rowSpan: 1, columnSpan: 2));

        var grid4 = new Grid()
            .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1))
            .WithGap(2)
            .WithJustifyItems(JustifyContent.Center)
            .WithAlignItems(AlignItems.Center);
        grid4.Add(new Box { new Text("Centered 1") }.WithWidth(10).WithHeight(3));
        grid4.Add(new Box { new Text("Centered 2") }.WithWidth(10).WithHeight(3));
        grid4.Add(new Box { new Text("Centered 3") }.WithWidth(10).WithHeight(3));
        grid4.Add(new Box { new Text("Centered 4") }.WithWidth(10).WithHeight(3));

        // Create UI demonstrating Grid layout
        var ui = new VStack(spacing: 2) {
            new Text("Grid Layout Demo").Bold().Color(Color.Cyan),
            
            // Example 1: Basic 3-column grid
            new Text("Basic 3-column grid (1fr each):").Bold(),
            grid1,

            new Spacer(minLength: 2),
            
            // Example 2: Mixed column sizes
            new Text("Mixed column sizes (100px, 2fr, 1fr):").Bold(),
            grid2,

            new Spacer(minLength: 2),
            
            // Example 3: Grid with spanning cells
            new Text("Grid with spanning cells:").Bold(),
            grid3,

            new Spacer(minLength: 2),
            
            // Example 4: Responsive-like grid
            new Text("Grid with alignment:").Bold(),
            grid4
        };

        // Render
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);

        Console.SetCursorPosition(0, terminal.Height - 2);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}