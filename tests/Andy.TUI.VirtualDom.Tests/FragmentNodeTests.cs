using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class FragmentNodeTests
{
    [Fact]
    public void Constructor_WithNullChildren_CreatesEmptyFragment()
    {
        var fragment = new FragmentNode((VirtualNode[])null!);

        Assert.Empty(fragment.Children);
        Assert.Equal(VirtualNodeType.Fragment, fragment.Type);
    }

    [Fact]
    public void Constructor_WithNullEnumerable_CreatesEmptyFragment()
    {
        var fragment = new FragmentNode((IEnumerable<VirtualNode>)null!);

        Assert.Empty(fragment.Children);
    }

    [Fact]
    public void Constructor_WithEmptyArray_CreatesEmptyFragment()
    {
        var fragment = new FragmentNode(Array.Empty<VirtualNode>());

        Assert.Empty(fragment.Children);
    }

    [Fact]
    public void Constructor_WithChildren_AddsAllChildren()
    {
        var child1 = new TextNode("1");
        var child2 = new ElementNode("div");
        var child3 = new FragmentNode();

        var fragment = new FragmentNode(child1, child2, child3);

        Assert.Equal(3, fragment.Children.Count);
        Assert.Same(child1, fragment.Children[0]);
        Assert.Same(child2, fragment.Children[1]);
        Assert.Same(child3, fragment.Children[2]);
    }

    [Fact]
    public void Constructor_WithEnumerable_AddsAllChildren()
    {
        var children = new List<VirtualNode>
        {
            new TextNode("1"),
            new ElementNode("div"),
            new FragmentNode()
        };

        var fragment = new FragmentNode(children);

        Assert.Equal(3, fragment.Children.Count);
        Assert.Equal("1", ((TextNode)fragment.Children[0]).Content);
        Assert.Equal("div", ((ElementNode)fragment.Children[1]).TagName);
        Assert.IsType<FragmentNode>(fragment.Children[2]);
    }

    [Fact]
    public void AddChild_WithNull_ThrowsException()
    {
        var fragment = new FragmentNode();

        Assert.Throws<ArgumentNullException>(() => fragment.AddChild(null!));
    }

    [Fact]
    public void RemoveChild_WithNull_ThrowsException()
    {
        var fragment = new FragmentNode();

        Assert.Throws<ArgumentNullException>(() => fragment.RemoveChild(null!));
    }

    [Fact]
    public void RemoveChild_WithNonExistentChild_ReturnsFalse()
    {
        var fragment = new FragmentNode(new TextNode("1"));
        var nonExistentChild = new TextNode("2");

        Assert.False(fragment.RemoveChild(nonExistentChild));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var fragment = new FragmentNode();

        Assert.False(fragment.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentNodeType_ReturnsFalse()
    {
        var fragment = new FragmentNode();
        var element = new ElementNode("div");

        Assert.False(fragment.Equals(element));
    }

    [Fact]
    public void Equals_WithDifferentKey_ReturnsFalse()
    {
        var fragment1 = new FragmentNode() { Key = "key1" };
        var fragment2 = new FragmentNode() { Key = "key2" };

        Assert.False(fragment1.Equals(fragment2));
    }

    [Fact]
    public void Equals_WithDifferentChildCount_ReturnsFalse()
    {
        var fragment1 = new FragmentNode(new TextNode("1"));
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("2"));

        Assert.False(fragment1.Equals(fragment2));
    }

    [Fact]
    public void Equals_WithDifferentChildren_ReturnsFalse()
    {
        var fragment1 = new FragmentNode(new TextNode("1"), new TextNode("2"));
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("3"));

        Assert.False(fragment1.Equals(fragment2));
    }

    [Fact]
    public void Equals_WithSameKeyAndChildren_ReturnsTrue()
    {
        var fragment1 = new FragmentNode(new TextNode("1"), new TextNode("2")) { Key = "key" };
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("2")) { Key = "key" };

        Assert.True(fragment1.Equals(fragment2));
        Assert.Equal(fragment1.GetHashCode(), fragment2.GetHashCode());
    }

    [Fact]
    public void Equals_EmptyFragments_ReturnsTrue()
    {
        var fragment1 = new FragmentNode();
        var fragment2 = new FragmentNode();

        Assert.True(fragment1.Equals(fragment2));
    }

    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var child1 = new TextNode("1");
        var child2 = new ElementNode("div");
        var fragment = new FragmentNode(child1, child2) { Key = "original" };

        var clone = (FragmentNode)fragment.Clone();

        Assert.Equal("original", clone.Key);
        Assert.Equal(2, clone.Children.Count);
        Assert.NotSame(fragment, clone);
        Assert.NotSame(fragment.Children[0], clone.Children[0]);
        Assert.NotSame(fragment.Children[1], clone.Children[1]);
        Assert.Equal(((TextNode)fragment.Children[0]).Content, ((TextNode)clone.Children[0]).Content);
        Assert.Equal(((ElementNode)fragment.Children[1]).TagName, ((ElementNode)clone.Children[1]).TagName);
    }

    [Fact]
    public void Clone_EmptyFragment_CreatesEmptyCopy()
    {
        var fragment = new FragmentNode() { Key = "empty" };

        var clone = (FragmentNode)fragment.Clone();

        Assert.Equal("empty", clone.Key);
        Assert.Empty(clone.Children);
        Assert.NotSame(fragment, clone);
    }

    [Fact]
    public void GetHashCode_DifferentFragments_ProducesDifferentHashes()
    {
        var fragment1 = new FragmentNode(new TextNode("1"));
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("2"));
        var fragment3 = new FragmentNode() { Key = "key" };

        // While hash codes can technically collide, for these simple cases they should be different
        Assert.NotEqual(fragment1.GetHashCode(), fragment2.GetHashCode());
        Assert.NotEqual(fragment1.GetHashCode(), fragment3.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IdenticalFragments_ProducesSameHash()
    {
        var fragment1 = new FragmentNode(new TextNode("1"), new TextNode("2")) { Key = "key" };
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("2")) { Key = "key" };

        Assert.Equal(fragment1.GetHashCode(), fragment2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var fragment1 = new FragmentNode();
        var fragment2 = new FragmentNode(new TextNode("1"), new TextNode("2"));

        Assert.Equal("Fragment: 0 children", fragment1.ToString());
        Assert.Equal("Fragment: 2 children", fragment2.ToString());
    }

    [Fact]
    public void Children_ReturnsReadOnlyList()
    {
        var fragment = new FragmentNode(new TextNode("1"));
        var children = fragment.Children;

        Assert.IsAssignableFrom<IReadOnlyList<VirtualNode>>(children);
        Assert.Throws<NotSupportedException>(() => ((IList<VirtualNode>)children).Add(new TextNode("2")));
    }

    [Fact]
    public void FragmentNode_ComplexEqualityScenario()
    {
        // Create complex nested fragments
        var innerFragment1 = new FragmentNode(new TextNode("inner1"), new TextNode("inner2"));
        var innerFragment2 = new FragmentNode(new TextNode("inner1"), new TextNode("inner2"));

        var fragment1 = new FragmentNode(
            new TextNode("outer1"),
            innerFragment1,
            new ElementNode("div")
        )
        { Key = "complex" };

        var fragment2 = new FragmentNode(
            new TextNode("outer1"),
            innerFragment2,
            new ElementNode("div")
        )
        { Key = "complex" };

        Assert.True(fragment1.Equals(fragment2));
    }
}