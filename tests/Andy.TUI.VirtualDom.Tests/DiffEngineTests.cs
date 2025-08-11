using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class DiffEngineTests
{
    private readonly DiffEngine _diffEngine = new();

    [Fact]
    public void Diff_BothNull_ReturnsNoPatches()
    {
        var patches = _diffEngine.Diff(null, null);

        Assert.Empty(patches);
    }

    [Fact]
    public void Diff_OldNullNewExists_ReturnsInsertPatch()
    {
        var newNode = new TextNode("Hello");

        var patches = _diffEngine.Diff(null, newNode);

        Assert.Single(patches);
        var patch = Assert.IsType<InsertPatch>(patches[0]);
        Assert.Equal(newNode, patch.Node);
        Assert.Equal(0, patch.Index);
    }

    [Fact]
    public void Diff_OldExistsNewNull_ReturnsRemovePatch()
    {
        var oldNode = new TextNode("Hello");

        var patches = _diffEngine.Diff(oldNode, null);

        Assert.Single(patches);
        var patch = Assert.IsType<RemovePatch>(patches[0]);
        Assert.Equal(0, patch.Index);
    }

    [Fact]
    public void Diff_DifferentNodeTypes_ReturnsReplacePatch()
    {
        var oldNode = new TextNode("Hello");
        var newNode = new ElementNode("div");

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        var patch = Assert.IsType<ReplacePatch>(patches[0]);
        Assert.Equal(newNode, patch.NewNode);
    }

    [Fact]
    public void Diff_TextNodes_DifferentContent_ReturnsUpdateTextPatch()
    {
        var oldNode = new TextNode("Hello");
        var newNode = new TextNode("World");

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        var patch = Assert.IsType<UpdateTextPatch>(patches[0]);
        Assert.Equal("World", patch.NewText);
    }

    [Fact]
    public void Diff_TextNodes_SameContent_ReturnsNoPatches()
    {
        var oldNode = new TextNode("Hello");
        var newNode = new TextNode("Hello");

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Empty(patches);
    }

    [Fact]
    public void Diff_ElementNodes_DifferentProps_ReturnsUpdatePropsPatch()
    {
        var oldNode = new ElementNode("div", new Dictionary<string, object?>
        {
            ["id"] = "old",
            ["class"] = "container",
            ["removed"] = "value"
        });

        var newNode = new ElementNode("div", new Dictionary<string, object?>
        {
            ["id"] = "new",
            ["class"] = "container",
            ["added"] = "value"
        });

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        var patch = Assert.IsType<UpdatePropsPatch>(patches[0]);
        Assert.Equal("new", patch.PropsToSet["id"]);
        Assert.Equal("value", patch.PropsToSet["added"]);
        Assert.Contains("removed", patch.PropsToRemove);
        Assert.DoesNotContain("class", patch.PropsToSet.Keys); // Unchanged
    }

    [Fact]
    public void Diff_ElementNodes_DifferentChildren_ReturnsChildPatches()
    {
        var oldNode = new ElementNode("div", null,
            new TextNode("Old Child 1"),
            new TextNode("Shared Child")
        );

        var newNode = new ElementNode("div", null,
            new TextNode("New Child 1"),
            new TextNode("Shared Child"),
            new TextNode("New Child 3")
        );

        var patches = _diffEngine.Diff(oldNode, newNode);

        // Should have patches for: update text of first child, insert third child
        Assert.Equal(2, patches.Count);
        Assert.IsType<UpdateTextPatch>(patches[0]);
        Assert.IsType<InsertPatch>(patches[1]);
    }

    [Fact]
    public void Diff_WithKeys_DetectsReorder()
    {
        var oldNode = new ElementNode("div", null,
            new ElementNode("span") { Key = "a" },
            new ElementNode("span") { Key = "b" },
            new ElementNode("span") { Key = "c" }
        );

        var newNode = new ElementNode("div", null,
            new ElementNode("span") { Key = "c" },
            new ElementNode("span") { Key = "a" },
            new ElementNode("span") { Key = "b" }
        );

        var patches = _diffEngine.Diff(oldNode, newNode);

        // Should contain a reorder patch
        var reorderPatch = patches.OfType<ReorderPatch>().FirstOrDefault();
        Assert.NotNull(reorderPatch);
        Assert.NotEmpty(reorderPatch.Moves);
    }

    [Fact]
    public void Diff_WithKeys_DetectsRemovalAndAddition()
    {
        var oldNode = new ElementNode("div", null,
            new ElementNode("span") { Key = "a" },
            new ElementNode("span") { Key = "b" },
            new ElementNode("span") { Key = "c" }
        );

        var newNode = new ElementNode("div", null,
            new ElementNode("span") { Key = "a" },
            new ElementNode("span") { Key = "d" },
            new ElementNode("span") { Key = "c" }
        );

        var patches = _diffEngine.Diff(oldNode, newNode);

        // Should have remove and insert patches
        Assert.Contains(patches, p => p is RemovePatch);
        Assert.Contains(patches, p => p is InsertPatch);
    }

    [Fact]
    public void Diff_FragmentNodes_DiffsChildren()
    {
        var oldNode = new FragmentNode(
            new TextNode("1"),
            new TextNode("2")
        );

        var newNode = new FragmentNode(
            new TextNode("1"),
            new TextNode("2"),
            new TextNode("3")
        );

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        Assert.IsType<InsertPatch>(patches[0]);
    }

    [Fact]
    public void Diff_ComponentNodes_DifferentProps_ReturnsUpdatePropsPatch()
    {
        var oldNode = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["value"] = "old" });
        var newNode = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["value"] = "new" });

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        var patch = Assert.IsType<UpdatePropsPatch>(patches[0]);
        Assert.Equal("new", patch.PropsToSet["value"]);
    }

    [Fact]
    public void Diff_ComponentNodes_DifferentTypes_ReturnsReplacePatch()
    {
        var oldNode = ComponentNode.Create<string>();
        var newNode = ComponentNode.Create<int>();

        var patches = _diffEngine.Diff(oldNode, newNode);

        Assert.Single(patches);
        Assert.IsType<ReplacePatch>(patches[0]);
    }

    [Fact]
    public void Diff_ComplexTree_GeneratesCorrectPatches()
    {
        var oldTree = new ElementNode("div", null,
            new ElementNode("header", new Dictionary<string, object?> { ["class"] = "old-header" },
                new TextNode("Old Title")
            ),
            new ElementNode("main", null,
                new ElementNode("p") { Key = "p1" },
                new ElementNode("p") { Key = "p2" }
            )
        );

        var newTree = new ElementNode("div", null,
            new ElementNode("header", new Dictionary<string, object?> { ["class"] = "new-header" },
                new TextNode("New Title")
            ),
            new ElementNode("main", null,
                new ElementNode("p") { Key = "p2" },
                new ElementNode("p") { Key = "p1" },
                new ElementNode("p") { Key = "p3" }
            )
        );

        var patches = _diffEngine.Diff(oldTree, newTree);

        // Should have patches for:
        // 1. Update header class prop
        // 2. Update header text
        // 3. Reorder paragraphs
        // 4. Insert new paragraph
        Assert.True(patches.Count >= 4);
        Assert.Contains(patches, p => p is UpdatePropsPatch);
        Assert.Contains(patches, p => p is UpdateTextPatch);
        Assert.Contains(patches, p => p is ReorderPatch);
        Assert.Contains(patches, p => p is InsertPatch);
    }

    [Fact]
    public void Diff_PathsAreCorrect()
    {
        var oldTree = new ElementNode("div", null,
            new ElementNode("span", null,
                new TextNode("Text")
            )
        );

        var newTree = new ElementNode("div", null,
            new ElementNode("span", null,
                new TextNode("Changed")
            )
        );

        var patches = _diffEngine.Diff(oldTree, newTree);

        Assert.Single(patches);
        var patch = Assert.IsType<UpdateTextPatch>(patches[0]);
        Assert.Equal(new[] { 0, 0 }, patch.Path); // First child of first child
    }
}