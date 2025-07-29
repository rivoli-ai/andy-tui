using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class ElementNodeTests
{
    [Fact]
    public void Constructor_WithNullTagName_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new ElementNode(null!));
    }
    
    [Fact]
    public void Constructor_WithNullChildren_CreatesEmptyChildrenList()
    {
        var node = new ElementNode("div", null, null!);
        
        Assert.Empty(node.Children);
    }
    
    [Fact]
    public void Constructor_WithNullProps_CreatesEmptyPropsDict()
    {
        var node = new ElementNode("div", null);
        
        Assert.NotNull(node.Props);
        Assert.Empty(node.Props);
    }
    
    [Fact]
    public void Constructor_WithPropsAndChildren_SetsAll()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test", ["class"] = "container" };
        var child1 = new TextNode("Hello");
        var child2 = new ElementNode("span");
        
        var node = new ElementNode("div", props, child1, child2);
        
        Assert.Equal("div", node.TagName);
        Assert.Equal(2, node.Props.Count);
        Assert.Equal("test", node.Props["id"]);
        Assert.Equal("container", node.Props["class"]);
        Assert.Equal(2, node.Children.Count);
        Assert.Same(child1, node.Children[0]);
        Assert.Same(child2, node.Children[1]);
    }
    
    [Fact]
    public void Constructor_CopiesPropsDict()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test" };
        var node = new ElementNode("div", props);
        
        // Modify original props
        props["id"] = "modified";
        props["new"] = "value";
        
        // Node's props should be unchanged
        Assert.Equal("test", node.Props["id"]);
        Assert.False(node.Props.ContainsKey("new"));
    }
    
    [Fact]
    public void AddChild_WithNull_ThrowsException()
    {
        var node = new ElementNode("div");
        
        Assert.Throws<ArgumentNullException>(() => node.AddChild(null!));
    }
    
    [Fact]
    public void RemoveChild_WithNull_ThrowsException()
    {
        var node = new ElementNode("div");
        
        Assert.Throws<ArgumentNullException>(() => node.RemoveChild(null!));
    }
    
    [Fact]
    public void RemoveChild_WithNonExistentChild_ReturnsFalse()
    {
        var node = new ElementNode("div", null, new TextNode("existing"));
        var nonExistent = new TextNode("other");
        
        Assert.False(node.RemoveChild(nonExistent));
        Assert.Single(node.Children);
    }
    
    [Fact]
    public void ReplaceChild_WithNull_ThrowsException()
    {
        var node = new ElementNode("div", null, new TextNode("child"));
        
        Assert.Throws<ArgumentNullException>(() => node.ReplaceChild(0, null!));
    }
    
    [Fact]
    public void ReplaceChild_WithInvalidIndex_ThrowsException()
    {
        var node = new ElementNode("div", null, new TextNode("child"));
        var newChild = new TextNode("new");
        
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ReplaceChild(-1, newChild));
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ReplaceChild(1, newChild));
        Assert.Throws<ArgumentOutOfRangeException>(() => node.ReplaceChild(10, newChild));
    }
    
    [Fact]
    public void ReplaceChild_WithValidIndex_ReplacesChild()
    {
        var child1 = new TextNode("1");
        var child2 = new TextNode("2");
        var child3 = new TextNode("3");
        var node = new ElementNode("div", null, child1, child2, child3);
        
        var newChild = new ElementNode("span");
        node.ReplaceChild(1, newChild);
        
        Assert.Equal(3, node.Children.Count);
        Assert.Same(child1, node.Children[0]);
        Assert.Same(newChild, node.Children[1]);
        Assert.Same(child3, node.Children[2]);
    }
    
    [Fact]
    public void Children_ReturnsReadOnlyList()
    {
        var node = new ElementNode("div", null, new TextNode("child"));
        var children = node.Children;
        
        Assert.IsAssignableFrom<IReadOnlyList<VirtualNode>>(children);
        Assert.Throws<NotSupportedException>(() => ((IList<VirtualNode>)children).Add(new TextNode("new")));
    }
    
    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var node = new ElementNode("div");
        
        Assert.False(node.Equals(null));
    }
    
    [Fact]
    public void Equals_WithDifferentNodeType_ReturnsFalse()
    {
        var element = new ElementNode("div");
        var text = new TextNode("text");
        
        Assert.False(element.Equals(text));
    }
    
    [Fact]
    public void Equals_WithDifferentTagName_ReturnsFalse()
    {
        var node1 = new ElementNode("div");
        var node2 = new ElementNode("span");
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentKey_ReturnsFalse()
    {
        var node1 = new ElementNode("div") { Key = "key1" };
        var node2 = new ElementNode("div") { Key = "key2" };
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropsCount_ReturnsFalse()
    {
        var node1 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropKeys_ReturnsFalse()
    {
        var node1 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ElementNode("div", new Dictionary<string, object?> { ["b"] = 1 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropValues_ReturnsFalse()
    {
        var node1 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 2 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithNullPropValue_HandlesCorrectly()
    {
        var node1 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = null });
        var node2 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = null });
        var node3 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = "value" });
        
        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3));
    }
    
    [Fact]
    public void Equals_WithDifferentChildrenCount_ReturnsFalse()
    {
        var node1 = new ElementNode("div", null, new TextNode("1"));
        var node2 = new ElementNode("div", null, new TextNode("1"), new TextNode("2"));
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentChildren_ReturnsFalse()
    {
        var node1 = new ElementNode("div", null, new TextNode("1"), new TextNode("2"));
        var node2 = new ElementNode("div", null, new TextNode("1"), new TextNode("3"));
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithIdenticalNodes_ReturnsTrue()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test", ["class"] = "container" };
        var node1 = new ElementNode("div", props, new TextNode("child"), new ElementNode("span")) { Key = "key" };
        var node2 = new ElementNode("div", props, new TextNode("child"), new ElementNode("span")) { Key = "key" };
        
        Assert.True(node1.Equals(node2));
    }
    
    [Fact]
    public void GetHashCode_SameNodes_ProduceSameHash()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test" };
        var node1 = new ElementNode("div", props, new TextNode("child")) { Key = "key" };
        var node2 = new ElementNode("div", props, new TextNode("child")) { Key = "key" };
        
        Assert.Equal(node1.GetHashCode(), node2.GetHashCode());
    }
    
    [Fact]
    public void GetHashCode_DifferentNodes_UsuallyProduceDifferentHash()
    {
        var node1 = new ElementNode("div");
        var node2 = new ElementNode("span");
        var node3 = new ElementNode("div") { Key = "key" };
        var node4 = new ElementNode("div", new Dictionary<string, object?> { ["a"] = 1 });
        var node5 = new ElementNode("div", null, new TextNode("child"));
        
        // While hash codes can collide, for these simple cases they should be different
        Assert.NotEqual(node1.GetHashCode(), node2.GetHashCode());
        Assert.NotEqual(node1.GetHashCode(), node3.GetHashCode());
        Assert.NotEqual(node1.GetHashCode(), node4.GetHashCode());
        Assert.NotEqual(node1.GetHashCode(), node5.GetHashCode());
    }
    
    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test", ["value"] = 123 };
        var child1 = new TextNode("text");
        var child2 = new ElementNode("span", new Dictionary<string, object?> { ["nested"] = true });
        var original = new ElementNode("div", props, child1, child2) { Key = "original" };
        
        var clone = (ElementNode)original.Clone();
        
        Assert.NotSame(original, clone);
        Assert.Equal(original.TagName, clone.TagName);
        Assert.Equal(original.Key, clone.Key);
        Assert.NotSame(original.Props, clone.Props);
        Assert.Equal(original.Props["id"], clone.Props["id"]);
        Assert.Equal(original.Props["value"], clone.Props["value"]);
        Assert.Equal(original.Children.Count, clone.Children.Count);
        
        // Children are deep cloned
        Assert.NotSame(original.Children[0], clone.Children[0]);
        Assert.NotSame(original.Children[1], clone.Children[1]);
        Assert.Equal(((TextNode)original.Children[0]).Content, ((TextNode)clone.Children[0]).Content);
        Assert.Equal(((ElementNode)original.Children[1]).TagName, ((ElementNode)clone.Children[1]).TagName);
    }
    
    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var props = new Dictionary<string, object?> { ["a"] = 1 };
        var original = new ElementNode("div", props, new TextNode("child"));
        
        var clone = (ElementNode)original.Clone();
        clone.Props["a"] = 2;
        clone.Props["b"] = "new";
        clone.AddChild(new TextNode("new child"));
        
        Assert.Equal(1, original.Props["a"]);
        Assert.False(original.Props.ContainsKey("b"));
        Assert.Single(original.Children);
    }
    
    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var node1 = new ElementNode("div");
        var node2 = new ElementNode("span", null, new TextNode("1"), new TextNode("2"));
        
        Assert.Equal("<div children=0>", node1.ToString());
        Assert.Equal("<span children=2>", node2.ToString());
    }
    
    [Fact]
    public void Accept_CallsCorrectVisitorMethod()
    {
        var node = new ElementNode("div");
        var visitor = new TestVisitor();
        
        node.Accept(visitor);
        
        Assert.True(visitor.VisitedElement);
        Assert.Same(node, visitor.VisitedElementNode);
    }
    
    [Fact]
    public void ComplexScenario_AddRemoveReplaceChildren()
    {
        var node = new ElementNode("div");
        
        // Add children
        var child1 = new TextNode("1");
        var child2 = new ElementNode("span");
        var child3 = new TextNode("3");
        
        node.AddChild(child1);
        node.AddChild(child2);
        node.AddChild(child3);
        
        Assert.Equal(3, node.Children.Count);
        
        // Remove middle child
        Assert.True(node.RemoveChild(child2));
        Assert.Equal(2, node.Children.Count);
        Assert.Same(child1, node.Children[0]);
        Assert.Same(child3, node.Children[1]);
        
        // Replace first child
        var newChild = new FragmentNode();
        node.ReplaceChild(0, newChild);
        
        Assert.Same(newChild, node.Children[0]);
        Assert.Same(child3, node.Children[1]);
    }
    
    private class TestVisitor : IVirtualNodeVisitor
    {
        public bool VisitedElement { get; private set; }
        public ElementNode? VisitedElementNode { get; private set; }
        
        public void VisitText(TextNode node) { }
        public void VisitElement(ElementNode node) 
        { 
            VisitedElement = true;
            VisitedElementNode = node;
        }
        public void VisitComponent(ComponentNode node) { }
        public void VisitFragment(FragmentNode node) { }
    }
}