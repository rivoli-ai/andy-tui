using Xunit;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Tests;

/// <summary>
/// Tests for the diff engine's ability to handle overlapping content scenarios
/// where components shift position and need to properly redraw over previous content.
/// </summary>
public class DiffEngineOverlapTests
{
    private class MockRenderingSystem : IRenderingSystem
    {
        private readonly List<(int x, int y, string text, Style style)> _writes = new();
        private readonly List<(int x, int y, int width, int height, char fill, Style style)> _fills = new();
        
        public int Width { get; set; } = 80;
        public int Height { get; set; } = 24;
        public bool IsDisposed { get; private set; }
        
        public List<(int x, int y, string text, Style style)> Writes => _writes;
        public List<(int x, int y, int width, int height, char fill, Style style)> Fills => _fills;
        
        public void WriteText(int x, int y, string text, Style style)
        {
            _writes.Add((x, y, text, style));
        }
        
        public void FillRect(int x, int y, int width, int height, char fill, Style style)
        {
            _fills.Add((x, y, width, height, fill, style));
        }
        
        public void DrawBox(int x, int y, int width, int height, Style style, BoxStyle boxStyle) { }
        public void Initialize() { }
        public void SetCursorPosition(int x, int y) { }
        public void ShowCursor() { }
        public void HideCursor() { }
        public void Render() { }
        public void Dispose() => IsDisposed = true;
    }
    
    [Fact]
    public void SimpleOverlap_TwoTextComponents_ShouldClearOverlappedArea()
    {
        // Arrange
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        // Initial tree: single text at position (5, 2)
        var tree1 = Element("text")
            .WithProp("x", 5)
            .WithProp("y", 2)
            .WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello"))
            .Build();
        
        // Modified tree: text moved to position (3, 2) - overlaps with previous position
        var tree2 = Element("text")
            .WithProp("x", 3)
            .WithProp("y", 2)
            .WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello World"))
            .Build();
        
        // Act
        renderer.Render(tree1);
        mockSystem.Writes.Clear();
        mockSystem.Fills.Clear();
        
        var patches = diffEngine.Diff(tree1, tree2);
        
        // Debug: Let's see what patches are generated
        var patchesDebug = string.Join(", ", patches.Select(p => p.GetType().Name));
        Assert.True(patches.Count > 0, $"Should generate patches. Generated: {patches.Count}, Types: [{patchesDebug}]");
        
        renderer.ApplyPatches(patches);
        
        // Debug: Let's see what operations were performed
        var fillsDebug = string.Join(", ", mockSystem.Fills.Select(f => $"({f.x},{f.y},{f.width}x{f.height})"));
        var writesDebug = string.Join(", ", mockSystem.Writes.Select(w => $"({w.x},{w.y}:'{w.text}')"));
        
        // Assert
        // Should have cleared the old text area (position 5-9, row 2) with spaces
        Assert.True(mockSystem.Fills.Count > 0, $"Should have clearing operations. Fills: [{fillsDebug}], Writes: [{writesDebug}]");
        Assert.Contains(mockSystem.Fills, f => 
            f.x == 5 && f.y == 2 && f.width >= 5 && f.fill == ' ');
        
        // Should have written new text at new position (3, 2)
        Assert.Contains(mockSystem.Writes, w => 
            w.x == 3 && w.y == 2 && w.text == "Hello World");
    }
    
    [Fact]
    public void ColumnWidthExpansion_ShouldClearOverlappedColumns()
    {
        // Arrange - simulate the MultiSelectInput scenario
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        // Initial tree: three columns at fixed positions
        var tree1 = Fragment(
            // Column 1: Programming Languages
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Languages:"))
                .Build(),
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("C#"))
                .Build(),
            
            // Column 2: Colors
            Element("text")
                .WithProp("x", 20)
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Colors:"))
                .Build(),
            Element("text")
                .WithProp("x", 20)
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Red"))
                .Build(),
                
            // Column 3: Numbers  
            Element("text")
                .WithProp("x", 40)
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Numbers:"))
                .Build(),
            Element("text")
                .WithProp("x", 40)
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("1"))
                .Build()
        );
        
        // Modified tree: Column 1 expands, pushing columns 2 and 3 to the right
        var tree2 = Fragment(
            // Column 1: Expanded
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Languages:"))
                .Build(),
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("C#, JavaScript, Python"))
                .Build(),
            
            // Column 2: Moved right due to expansion
            Element("text")
                .WithProp("x", 30)  // Moved from 20 to 30
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Colors:"))
                .Build(),
            Element("text")
                .WithProp("x", 30)  // Moved from 20 to 30
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Red"))
                .Build(),
                
            // Column 3: Moved further right
            Element("text")
                .WithProp("x", 50)  // Moved from 40 to 50
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Numbers:"))
                .Build(),
            Element("text")
                .WithProp("x", 50)  // Moved from 40 to 50
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("1"))
                .Build()
        );
        
        // Act
        renderer.Render(tree1);
        mockSystem.Writes.Clear();
        mockSystem.Fills.Clear();
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Assert
        // Should clear the old positions of columns 2 and 3
        Assert.Contains(mockSystem.Fills, f => 
            f.x == 20 && f.y == 0 && f.fill == ' '); // Old column 2 header
        Assert.Contains(mockSystem.Fills, f => 
            f.x == 20 && f.y == 1 && f.fill == ' '); // Old column 2 content
        Assert.Contains(mockSystem.Fills, f => 
            f.x == 40 && f.y == 0 && f.fill == ' '); // Old column 3 header  
        Assert.Contains(mockSystem.Fills, f => 
            f.x == 40 && f.y == 1 && f.fill == ' '); // Old column 3 content
        
        // Should write new content at new positions
        Assert.Contains(mockSystem.Writes, w => 
            w.x == 0 && w.y == 1 && w.text == "C#, JavaScript, Python");
        Assert.Contains(mockSystem.Writes, w => 
            w.x == 30 && w.y == 0 && w.text == "Colors:");
        Assert.Contains(mockSystem.Writes, w => 
            w.x == 50 && w.y == 0 && w.text == "Numbers:");
    }
    
    [Fact]
    public void ComplexLayoutShift_MultipleMoves_ShouldHandleAllOverlaps()
    {
        // Arrange - more complex scenario with multiple components shifting
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        // Initial: A grid of text components
        var tree1 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("A")).Build(),
            Element("text").WithProp("x", 5).WithProp("y", 0).WithChild(new TextNode("B")).Build(),
            Element("text").WithProp("x", 10).WithProp("y", 0).WithChild(new TextNode("C")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("D")).Build(),
            Element("text").WithProp("x", 5).WithProp("y", 1).WithChild(new TextNode("E")).Build(),
            Element("text").WithProp("x", 10).WithProp("y", 1).WithChild(new TextNode("F")).Build()
        );
        
        // Modified: Shift everything right by different amounts
        var tree2 = Fragment(
            Element("text").WithProp("x", 2).WithProp("y", 0).WithChild(new TextNode("A_expanded")).Build(),
            Element("text").WithProp("x", 15).WithProp("y", 0).WithChild(new TextNode("B")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 0).WithChild(new TextNode("C")).Build(),
            Element("text").WithProp("x", 2).WithProp("y", 1).WithChild(new TextNode("D_expanded")).Build(),
            Element("text").WithProp("x", 15).WithProp("y", 1).WithChild(new TextNode("E")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 1).WithChild(new TextNode("F")).Build()
        );
        
        // Act
        renderer.Render(tree1);
        mockSystem.Writes.Clear();
        mockSystem.Fills.Clear();
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Assert - should clear all old positions and write to new positions
        // This is a complex scenario that tests the diff engine's ability to handle
        // multiple simultaneous position changes and overlaps
        
        // Should have clearing operations for old positions
        Assert.True(mockSystem.Fills.Count > 0, "Should have clearing operations for old content");
        
        // Should have new content written at new positions
        Assert.Contains(mockSystem.Writes, w => w.text == "A_expanded");
        Assert.Contains(mockSystem.Writes, w => w.text == "D_expanded");
        Assert.Contains(mockSystem.Writes, w => w.x == 15 && w.text == "B");
        Assert.Contains(mockSystem.Writes, w => w.x == 20 && w.text == "C");
    }
}