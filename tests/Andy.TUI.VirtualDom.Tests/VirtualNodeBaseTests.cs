using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class VirtualNodeBaseTests
{
    [Fact]
    public void Equals_Object_WithNull_ReturnsFalse()
    {
        VirtualNode node = new TextNode("test");
        
        Assert.False(node.Equals((object?)null));
    }
    
    [Fact]
    public void Equals_Object_WithNonVirtualNode_ReturnsFalse()
    {
        VirtualNode node = new TextNode("test");
        
        Assert.False(node.Equals("not a virtual node"));
        Assert.False(node.Equals(123));
        Assert.False(node.Equals(new object()));
    }
    
    [Fact]
    public void Equals_Object_WithSameNode_ReturnsTrue()
    {
        VirtualNode node = new TextNode("test");
        
        Assert.True(node.Equals((object)node));
    }
    
    [Fact]
    public void Equals_Object_WithEqualNode_DelegatesToTypedEquals()
    {
        VirtualNode node1 = new TextNode("test");
        VirtualNode node2 = new TextNode("test");
        object objNode2 = node2;
        
        // Should delegate to the typed Equals method
        Assert.True(node1.Equals(objNode2));
    }
    
    [Fact]
    public void Equals_Object_WithDifferentNode_ReturnsFalse()
    {
        VirtualNode node1 = new TextNode("test1");
        VirtualNode node2 = new TextNode("test2");
        object objNode2 = node2;
        
        Assert.False(node1.Equals(objNode2));
    }
    
    [Fact]
    public void VirtualNodeType_Enum_HasCorrectValues()
    {
        // Verify enum values
        Assert.Equal(0, (int)VirtualNodeType.Text);
        Assert.Equal(1, (int)VirtualNodeType.Element);
        Assert.Equal(2, (int)VirtualNodeType.Component);
        Assert.Equal(3, (int)VirtualNodeType.Fragment);
        
        // Verify all node types return correct enum value
        Assert.Equal(VirtualNodeType.Text, new TextNode("").Type);
        Assert.Equal(VirtualNodeType.Element, new ElementNode("div").Type);
        Assert.Equal(VirtualNodeType.Component, new ComponentNode(typeof(string)).Type);
        Assert.Equal(VirtualNodeType.Fragment, new FragmentNode().Type);
    }
    
    [Fact]
    public void Props_InitializedAsEmptyDictionary()
    {
        var nodes = new VirtualNode[]
        {
            new TextNode("test"),
            new ElementNode("div"),
            new ComponentNode(typeof(string)),
            new FragmentNode()
        };
        
        foreach (var node in nodes)
        {
            Assert.NotNull(node.Props);
            Assert.Empty(node.Props);
        }
    }
    
    [Fact]
    public void Key_CanBeSetAndRetrieved()
    {
        var nodes = new VirtualNode[]
        {
            new TextNode("test") { Key = "text-key" },
            new ElementNode("div") { Key = "element-key" },
            new ComponentNode(typeof(string)) { Key = "component-key" },
            new FragmentNode() { Key = "fragment-key" }
        };
        
        Assert.Equal("text-key", nodes[0].Key);
        Assert.Equal("element-key", nodes[1].Key);
        Assert.Equal("component-key", nodes[2].Key);
        Assert.Equal("fragment-key", nodes[3].Key);
    }
    
    [Fact]
    public void Children_DefaultImplementation_ReturnsEmptyArray()
    {
        // TextNode overrides Children, but the base implementation returns empty
        VirtualNode node = new MockVirtualNode();
        
        Assert.NotNull(node.Children);
        Assert.Empty(node.Children);
        Assert.Same(Array.Empty<VirtualNode>(), node.Children);
    }
    
    // Mock implementation to test base class behavior
    private class MockVirtualNode : VirtualNode
    {
        public override VirtualNodeType Type => VirtualNodeType.Text;
        
        public override bool Equals(VirtualNode? other) => ReferenceEquals(this, other);
        
        public override int GetHashCode() => 42;
        
        public override VirtualNode Clone() => new MockVirtualNode();
        
        public override void Accept(IVirtualNodeVisitor visitor) { }
    }
}