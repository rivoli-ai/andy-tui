using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Tests;

/// <summary>
/// Comprehensive tests for the diff engine's ability to handle content movement,
/// resizing, and overlapping scenarios. Tests are ordered from simple to complex.
/// </summary>
public class DiffEngineMovementTests
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
        
        public void WriteText(int x, int y, string text, Style style) => _writes.Add((x, y, text, style));
        public void FillRect(int x, int y, int width, int height, char fill, Style style) => _fills.Add((x, y, width, height, fill, style));
        public void DrawBox(int x, int y, int width, int height, Style style, BoxStyle boxStyle) { }
        public void Initialize() { }
        public void SetCursorPosition(int x, int y) { }
        public void ShowCursor() { }
        public void HideCursor() { }
        public void Render() { }
        public void Dispose() => IsDisposed = true;
    }

    #region Test Case 1: Basic Property Change Detection
    
    [Fact]
    public void DiffEngine_PropertyChange_ShouldGenerateUpdatePropsPatch()
    {
        // Test: Verify diff engine detects property changes
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text").WithProp("x", 5).WithProp("y", 2).Build();
        var tree2 = Element("text").WithProp("x", 10).WithProp("y", 2).Build();
        
        var patches = diffEngine.Diff(tree1, tree2);
        
        Assert.Single(patches);
        Assert.IsType<UpdatePropsPatch>(patches[0]);
        var patch = (UpdatePropsPatch)patches[0];
        Assert.True(patch.PropsToSet.ContainsKey("x"));
        Assert.Equal(10, patch.PropsToSet["x"]);
    }

    #endregion

    #region Test Case 2: Simple Position Movement

    [Fact] 
    public void SimpleMovement_TextMoveRight_ShouldClearOldAndDrawNew()
    {
        // Test: Single text component moves right
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello")).Build();
        
        var tree2 = Element("text")
            .WithProp("x", 10).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello")).Build();
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear old position (5,2) and draw at new position (10,2)
        AssertCleared(mockSystem, 5, 2, 5, 1); // "Hello" is 5 characters wide
        AssertWritten(mockSystem, 10, 2, "Hello");
    }

    [Fact]
    public void SimpleMovement_TextMoveDown_ShouldClearOldAndDrawNew()
    {
        // Test: Single text component moves down
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Test")).Build();
        
        var tree2 = Element("text")
            .WithProp("x", 5).WithProp("y", 4).WithProp("style", Style.Default)
            .WithChild(new TextNode("Test")).Build();
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        AssertCleared(mockSystem, 5, 2, 4, 1); // "Test" is 4 characters wide
        AssertWritten(mockSystem, 5, 4, "Test");
    }

    #endregion

    #region Test Case 3: Content Size Changes

    [Fact]
    public void ContentExpansion_TextGetsLonger_ShouldClearOldAreaAndDrawNew()
    {
        // Test: Text content expands in place
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hi")).Build();
        
        var tree2 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello World")).Build();
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear old area and draw new content
        AssertCleared(mockSystem, 5, 2, 2, 1); // "Hi" was 2 characters
        AssertWritten(mockSystem, 5, 2, "Hello World");
    }

    [Fact]
    public void ContentShrinking_TextGetsShorter_ShouldClearExtraArea()
    {
        // Test: Text content shrinks, need to clear the extra area
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello World")).Build();
        
        var tree2 = Element("text")
            .WithProp("x", 5).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hi")).Build();
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear the full old area (11 chars) and draw new content (2 chars)
        AssertCleared(mockSystem, 5, 2, 11, 1); // "Hello World" was 11 characters
        AssertWritten(mockSystem, 5, 2, "Hi");
    }

    #endregion

    #region Test Case 4: Overlapping Scenarios

    [Fact]
    public void OverlappingMovement_TextMovesLeftOverlapping_ShouldHandleOverlap()
    {
        // Test: Text moves left and overlaps with its previous position
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Element("text")
            .WithProp("x", 10).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello")).Build();
        
        var tree2 = Element("text")
            .WithProp("x", 7).WithProp("y", 2).WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello")).Build();
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear old position (10-14) and draw at new position (7-11)
        // Note: positions 10-11 overlap, so clearing strategy is important
        AssertCleared(mockSystem, 10, 2, 5, 1);
        AssertWritten(mockSystem, 7, 2, "Hello");
    }

    #endregion

    #region Test Case 5: Multi-Component Column Shifts

    [Fact]
    public void ColumnShift_ThreeColumnsExpandFirst_ShouldShiftAllFollowing()
    {
        // Test: MultiSelectInput scenario - first column expands, others shift right
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Col1")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 0).WithChild(new TextNode("Col2")).Build(),
            Element("text").WithProp("x", 40).WithProp("y", 0).WithChild(new TextNode("Col3")).Build()
        );
        
        var tree2 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Col1_Expanded")).Build(),
            Element("text").WithProp("x", 30).WithProp("y", 0).WithChild(new TextNode("Col2")).Build(), // Moved right
            Element("text").WithProp("x", 50).WithProp("y", 0).WithChild(new TextNode("Col3")).Build()  // Moved right
        );
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear old positions of Col2 and Col3
        AssertCleared(mockSystem, 20, 0, 4, 1); // "Col2" old position
        AssertCleared(mockSystem, 40, 0, 4, 1); // "Col3" old position
        
        // Should write new content
        AssertWritten(mockSystem, 0, 0, "Col1_Expanded");
        AssertWritten(mockSystem, 30, 0, "Col2");
        AssertWritten(mockSystem, 50, 0, "Col3");
    }

    #endregion

    #region Test Case 6: Complex Multi-Row Multi-Column

    [Fact]
    public void ComplexGrid_MultipleRowsAndColumns_ShouldHandleAllMovements()
    {
        // Test: Complex grid where multiple elements move simultaneously
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        var diffEngine = new DiffEngine();
        
        var tree1 = Fragment(
            // Row 1
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("A")).Build(),
            Element("text").WithProp("x", 10).WithProp("y", 0).WithChild(new TextNode("B")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 0).WithChild(new TextNode("C")).Build(),
            // Row 2
            Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("D")).Build(),
            Element("text").WithProp("x", 10).WithProp("y", 1).WithChild(new TextNode("E")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 1).WithChild(new TextNode("F")).Build()
        );
        
        var tree2 = Fragment(
            // Row 1 - all shifted right by different amounts
            Element("text").WithProp("x", 5).WithProp("y", 0).WithChild(new TextNode("A_expanded")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 0).WithChild(new TextNode("B")).Build(),
            Element("text").WithProp("x", 30).WithProp("y", 0).WithChild(new TextNode("C")).Build(),
            // Row 2 - same shifts
            Element("text").WithProp("x", 5).WithProp("y", 1).WithChild(new TextNode("D_expanded")).Build(),
            Element("text").WithProp("x", 20).WithProp("y", 1).WithChild(new TextNode("E")).Build(),
            Element("text").WithProp("x", 30).WithProp("y", 1).WithChild(new TextNode("F")).Build()
        );
        
        renderer.Render(tree1);
        ClearMockSystem(mockSystem);
        
        var patches = diffEngine.Diff(tree1, tree2);
        renderer.ApplyPatches(patches);
        
        // Should clear all old positions that moved
        AssertCleared(mockSystem, 10, 0, 1, 1); // B old position
        AssertCleared(mockSystem, 20, 0, 1, 1); // C old position  
        AssertCleared(mockSystem, 10, 1, 1, 1); // E old position
        AssertCleared(mockSystem, 20, 1, 1, 1); // F old position
        
        // Should write new content at new positions
        AssertWritten(mockSystem, 5, 0, "A_expanded");
        AssertWritten(mockSystem, 20, 0, "B");
        AssertWritten(mockSystem, 30, 0, "C");
        AssertWritten(mockSystem, 5, 1, "D_expanded");
        AssertWritten(mockSystem, 20, 1, "E");
        AssertWritten(mockSystem, 30, 1, "F");
    }

    #endregion

    #region Helper Methods

    private static void ClearMockSystem(MockRenderingSystem mockSystem)
    {
        mockSystem.Writes.Clear();
        mockSystem.Fills.Clear();
    }

    private static void AssertCleared(MockRenderingSystem mockSystem, int x, int y, int width, int height)
    {
        Assert.Contains(mockSystem.Fills, f => 
            f.x == x && f.y == y && f.width == width && f.height == height && f.fill == ' ');
    }

    private static void AssertWritten(MockRenderingSystem mockSystem, int x, int y, string text)
    {
        Assert.Contains(mockSystem.Writes, w => 
            w.x == x && w.y == y && w.text == text);
    }

    #endregion
}