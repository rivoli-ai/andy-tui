using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class ComponentNodeTests
{
    [Fact]
    public void Constructor_WithNullComponentType_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new ComponentNode(null!, null));
    }
    
    [Fact]
    public void Constructor_WithNullProps_CreatesEmptyPropsDict()
    {
        var node = new ComponentNode(typeof(string), null);
        
        Assert.NotNull(node.Props);
        Assert.Empty(node.Props);
    }
    
    [Fact]
    public void Constructor_WithProps_CopiesProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["value"] = 123,
            ["enabled"] = true
        };
        
        var node = new ComponentNode(typeof(string), props);
        
        Assert.Equal(3, node.Props.Count);
        Assert.Equal("Test", node.Props["name"]);
        Assert.Equal(123, node.Props["value"]);
        Assert.Equal(true, node.Props["enabled"]);
        
        // Verify it's a copy, not the same reference
        props["name"] = "Modified";
        Assert.Equal("Test", node.Props["name"]);
    }
    
    [Fact]
    public void Children_WithoutRenderedContent_ReturnsEmpty()
    {
        var node = new ComponentNode(typeof(string));
        
        Assert.Empty(node.Children);
    }
    
    [Fact]
    public void Children_WithRenderedContent_ReturnsContent()
    {
        var node = new ComponentNode(typeof(string));
        var content = new TextNode("Rendered");
        node.RenderedContent = content;
        
        Assert.Single(node.Children);
        Assert.Same(content, node.Children[0]);
    }
    
    [Fact]
    public void ComponentInstance_CanBeSetAndRetrieved()
    {
        var node = new ComponentNode(typeof(string));
        var instance = "Component Instance";
        
        node.ComponentInstance = instance;
        
        Assert.Same(instance, node.ComponentInstance);
    }
    
    [Fact]
    public void Create_Generic_CreatesCorrectType()
    {
        var node1 = ComponentNode.Create<string>();
        var node2 = ComponentNode.Create<int>();
        var node3 = ComponentNode.Create<ComponentNodeTests>();
        
        Assert.Equal(typeof(string), node1.ComponentType);
        Assert.Equal(typeof(int), node2.ComponentType);
        Assert.Equal(typeof(ComponentNodeTests), node3.ComponentType);
    }
    
    [Fact]
    public void Create_GenericWithProps_SetsProps()
    {
        var props = new Dictionary<string, object?>
        {
            ["test"] = "value"
        };
        
        var node = ComponentNode.Create<string>(props);
        
        Assert.Equal("value", node.Props["test"]);
    }
    
    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var node = new ComponentNode(typeof(string));
        
        Assert.False(node.Equals(null));
    }
    
    [Fact]
    public void Equals_WithDifferentNodeType_ReturnsFalse()
    {
        var component = new ComponentNode(typeof(string));
        var element = new ElementNode("div");
        
        Assert.False(component.Equals(element));
    }
    
    [Fact]
    public void Equals_WithDifferentComponentType_ReturnsFalse()
    {
        var node1 = new ComponentNode(typeof(string));
        var node2 = new ComponentNode(typeof(int));
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentKey_ReturnsFalse()
    {
        var node1 = new ComponentNode(typeof(string)) { Key = "key1" };
        var node2 = new ComponentNode(typeof(string)) { Key = "key2" };
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropsCount_ReturnsFalse()
    {
        var node1 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 1, ["b"] = 2 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropKeys_ReturnsFalse()
    {
        var node1 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["b"] = 1 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithDifferentPropValues_ReturnsFalse()
    {
        var node1 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 1 });
        var node2 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 2 });
        
        Assert.False(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_WithNullPropValue_HandlesCorrectly()
    {
        var node1 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = null });
        var node2 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = null });
        var node3 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = "value" });
        
        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3));
    }
    
    [Fact]
    public void Equals_WithIdenticalNodes_ReturnsTrue()
    {
        var props = new Dictionary<string, object?> { ["a"] = 1, ["b"] = "test" };
        var node1 = new ComponentNode(typeof(string), props) { Key = "key" };
        var node2 = new ComponentNode(typeof(string), props) { Key = "key" };
        
        Assert.True(node1.Equals(node2));
    }
    
    [Fact]
    public void Equals_IgnoresComponentInstance_AndRenderedContent()
    {
        var node1 = new ComponentNode(typeof(string)) 
        { 
            ComponentInstance = "instance1",
            RenderedContent = new TextNode("content1")
        };
        
        var node2 = new ComponentNode(typeof(string)) 
        { 
            ComponentInstance = "instance2",
            RenderedContent = new TextNode("content2")
        };
        
        Assert.True(node1.Equals(node2));
    }
    
    [Fact]
    public void GetHashCode_SameNodes_ProduceSameHash()
    {
        var props = new Dictionary<string, object?> { ["a"] = 1 };
        var node1 = new ComponentNode(typeof(string), props) { Key = "key" };
        var node2 = new ComponentNode(typeof(string), props) { Key = "key" };
        
        Assert.Equal(node1.GetHashCode(), node2.GetHashCode());
    }
    
    [Fact]
    public void GetHashCode_DifferentNodes_UsuallyProduceDifferentHash()
    {
        var node1 = new ComponentNode(typeof(string));
        var node2 = new ComponentNode(typeof(int));
        var node3 = new ComponentNode(typeof(string)) { Key = "key" };
        var node4 = new ComponentNode(typeof(string), new Dictionary<string, object?> { ["a"] = 1 });
        
        // While hash codes can collide, for these simple cases they should be different
        Assert.NotEqual(node1.GetHashCode(), node2.GetHashCode());
        Assert.NotEqual(node1.GetHashCode(), node3.GetHashCode());
        Assert.NotEqual(node1.GetHashCode(), node4.GetHashCode());
    }
    
    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var props = new Dictionary<string, object?> { ["a"] = 1, ["b"] = "test" };
        var original = new ComponentNode(typeof(string), props) 
        { 
            Key = "original",
            ComponentInstance = "instance",
            RenderedContent = new TextNode("content")
        };
        
        var clone = (ComponentNode)original.Clone();
        
        Assert.NotSame(original, clone);
        Assert.Equal(original.ComponentType, clone.ComponentType);
        Assert.Equal(original.Key, clone.Key);
        Assert.Same(original.ComponentInstance, clone.ComponentInstance); // Instance is copied by reference
        Assert.NotSame(original.Props, clone.Props); // Props dictionary is cloned
        Assert.Equal(original.Props["a"], clone.Props["a"]);
        Assert.Equal(original.Props["b"], clone.Props["b"]);
        
        // Rendered content is cloned
        Assert.NotSame(original.RenderedContent, clone.RenderedContent);
        Assert.Equal(((TextNode)original.RenderedContent).Content, ((TextNode)clone.RenderedContent!).Content);
    }
    
    [Fact]
    public void Clone_WithNullRenderedContent_HandlesCorrectly()
    {
        var original = new ComponentNode(typeof(string)) { Key = "test" };
        
        var clone = (ComponentNode)original.Clone();
        
        Assert.Null(clone.RenderedContent);
        Assert.Equal(original.Key, clone.Key);
    }
    
    [Fact]
    public void Clone_ModifyingClone_DoesNotAffectOriginal()
    {
        var props = new Dictionary<string, object?> { ["a"] = 1 };
        var original = new ComponentNode(typeof(string), props);
        
        var clone = (ComponentNode)original.Clone();
        clone.Props["a"] = 2;
        clone.Props["b"] = "new";
        
        Assert.Equal(1, original.Props["a"]);
        Assert.False(original.Props.ContainsKey("b"));
    }
    
    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        var node1 = new ComponentNode(typeof(string));
        var node2 = new ComponentNode(typeof(Dictionary<string, object>));
        
        Assert.Equal("Component: String", node1.ToString());
        Assert.Equal("Component: Dictionary`2", node2.ToString());
    }
    
    [Fact]
    public void Accept_CallsCorrectVisitorMethod()
    {
        var node = new ComponentNode(typeof(string));
        var visitor = new TestVisitor();
        
        node.Accept(visitor);
        
        Assert.True(visitor.VisitedComponent);
        Assert.Same(node, visitor.VisitedComponentNode);
    }
    
    private class TestVisitor : IVirtualNodeVisitor
    {
        public bool VisitedComponent { get; private set; }
        public ComponentNode? VisitedComponentNode { get; private set; }
        
        public void VisitText(TextNode node) { }
        public void VisitElement(ElementNode node) { }
        public void VisitComponent(ComponentNode node) 
        { 
            VisitedComponent = true;
            VisitedComponentNode = node;
        }
        public void VisitFragment(FragmentNode node) { }
        public void VisitClipping(ClippingNode node) { }
        public void VisitEmpty(EmptyNode node) { }
    }
}