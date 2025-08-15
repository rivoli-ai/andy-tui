using System;
using System.Linq;
using Xunit;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.VirtualDom.Tests;

/// <summary>
/// Test to verify that the DiffEngine doesn't generate duplicate patches when DOM structure changes.
/// This was causing issues with SelectInput arrow navigation.
/// </summary>
public class DiffEngineDuplicatePatchTest
{
    [Fact]
    public void DiffEngine_WithStructuralChanges_ShouldNotGenerateDuplicatePatches()
    {
        // This test reproduces the issue where SelectInput would switch between:
        // 1. Single element for normal/highlighted items
        // 2. Fragment with 3 elements for selected but not highlighted items
        
        var diffEngine = new DiffEngine();
        
        // Create initial tree with simple elements
        var oldTree = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Line 1")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("Line 2")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 2).WithChild(new TextNode("Line 3")).Build()
        );
        
        // Create new tree where middle element becomes a fragment with multiple children
        // This simulates what was happening in SelectInput when an item became selected
        var newTree = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Line 1")).Build(),
            Fragment(
                Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("│")).Build(),
                Element("text").WithProp("x", 1).WithProp("y", 1).WithChild(new TextNode("Line 2")).Build(),
                Element("text").WithProp("x", 7).WithProp("y", 1).WithChild(new TextNode("│")).Build()
            ),
            Element("text").WithProp("x", 0).WithProp("y", 2).WithChild(new TextNode("Line 3")).Build()
        );
        
        // Act - This should not throw DiffInvariantViolationException
        var patches = diffEngine.Diff(oldTree, newTree);
        
        // Assert
        Assert.NotNull(patches);
        Assert.True(patches.Count > 0, "Should generate some patches");
        
        // Verify no duplicate patches for same path and type
        var patchKeys = patches.Select(p => $"{p.Type}:{string.Join(",", p.Path ?? Array.Empty<int>())}").ToList();
        var distinctKeys = patchKeys.Distinct().Count();
        Assert.Equal(distinctKeys, patchKeys.Count);
    }
    
    [Fact]
    public void DiffEngine_RapidStructuralChanges_ShouldHandleCorrectly()
    {
        // Test rapid back-and-forth changes like arrow navigation
        var diffEngine = new DiffEngine();
        
        // Start with all simple elements
        var tree1 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Item 1")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("Item 2")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 2).WithChild(new TextNode("Item 3")).Build()
        );
        
        // Item 2 becomes selected (fragment)
        var tree2 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Item 1")).Build(),
            Fragment(
                Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("→ Item 2")).Build()
            ),
            Element("text").WithProp("x", 0).WithProp("y", 2).WithChild(new TextNode("Item 3")).Build()
        );
        
        // Item 3 becomes highlighted (back to simple)
        var tree3 = Fragment(
            Element("text").WithProp("x", 0).WithProp("y", 0).WithChild(new TextNode("Item 1")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 1).WithChild(new TextNode("Item 2 [selected]")).Build(),
            Element("text").WithProp("x", 0).WithProp("y", 2).WithChild(new TextNode("▶ Item 3")).Build()
        );
        
        // Apply all diffs - none should throw
        var patches1 = diffEngine.Diff(tree1, tree2);
        var patches2 = diffEngine.Diff(tree2, tree3);
        var patches3 = diffEngine.Diff(tree3, tree1); // Back to original
        
        Assert.NotNull(patches1);
        Assert.NotNull(patches2);
        Assert.NotNull(patches3);
    }
    
    [Fact]
    public void DiffEngine_NestedFragments_ShouldNotCauseDuplicates()
    {
        var diffEngine = new DiffEngine();
        
        // Test nested fragments which could cause path conflicts
        var oldTree = Fragment(
            Element("div").WithChild(
                Fragment(
                    Element("text").WithProp("x", 0).WithChild(new TextNode("A")).Build(),
                    Element("text").WithProp("x", 1).WithChild(new TextNode("B")).Build()
                )
            ).Build()
        );
        
        var newTree = Fragment(
            Element("div").WithChild(
                Fragment(
                    Element("text").WithProp("x", 0).WithChild(new TextNode("A")).Build(),
                    Fragment(
                        Element("text").WithProp("x", 1).WithChild(new TextNode("B1")).Build(),
                        Element("text").WithProp("x", 2).WithChild(new TextNode("B2")).Build()
                    ),
                    Element("text").WithProp("x", 3).WithChild(new TextNode("C")).Build()
                )
            ).Build()
        );
        
        // Should not throw duplicate patch exception
        var patches = diffEngine.Diff(oldTree, newTree);
        Assert.NotNull(patches);
    }
}