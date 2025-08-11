using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class VirtualNodeTests
{
    [Fact]
    public void TextNode_Creation_SetsProperties()
    {
        var node = new TextNode("Hello World");

        Assert.Equal(VirtualNodeType.Text, node.Type);
        Assert.Equal("Hello World", node.Content);
        Assert.Null(node.Key);
        Assert.Empty(node.Children);
        Assert.Empty(node.Props);
    }

    [Fact]
    public void TextNode_Equality_WorksCorrectly()
    {
        var node1 = new TextNode("Hello");
        var node2 = new TextNode("Hello");
        var node3 = new TextNode("World");
        var node4 = new TextNode("Hello") { Key = "key1" };

        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3));
        Assert.False(node1.Equals(node4));
        Assert.Equal(node1.GetHashCode(), node2.GetHashCode());
    }

    [Fact]
    public void TextNode_Clone_CreatesIdenticalCopy()
    {
        var original = new TextNode("Test") { Key = "key1" };
        var clone = (TextNode)original.Clone();

        Assert.Equal(original.Content, clone.Content);
        Assert.Equal(original.Key, clone.Key);
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ElementNode_Creation_SetsProperties()
    {
        var props = new Dictionary<string, object?> { ["class"] = "container" };
        var child1 = new TextNode("Child 1");
        var child2 = new TextNode("Child 2");

        var node = new ElementNode("div", props, child1, child2);

        Assert.Equal(VirtualNodeType.Element, node.Type);
        Assert.Equal("div", node.TagName);
        Assert.Equal("container", node.Props["class"]);
        Assert.Equal(2, node.Children.Count);
    }

    [Fact]
    public void ElementNode_AddRemoveChildren_WorksCorrectly()
    {
        var node = new ElementNode("div");
        var child1 = new TextNode("Child 1");
        var child2 = new TextNode("Child 2");

        node.AddChild(child1);
        node.AddChild(child2);
        Assert.Equal(2, node.Children.Count);

        Assert.True(node.RemoveChild(child1));
        Assert.Single(node.Children);
        Assert.Equal(child2, node.Children[0]);

        Assert.False(node.RemoveChild(child1)); // Already removed
    }

    [Fact]
    public void ElementNode_ReplaceChild_WorksCorrectly()
    {
        var node = new ElementNode("div", null, new TextNode("Old"));
        var newChild = new TextNode("New");

        node.ReplaceChild(0, newChild);

        Assert.Single(node.Children);
        Assert.Equal(newChild, node.Children[0]);
    }

    [Fact]
    public void ElementNode_Equality_ComparesAllProperties()
    {
        var props1 = new Dictionary<string, object?> { ["id"] = "1", ["class"] = "test" };
        var props2 = new Dictionary<string, object?> { ["id"] = "1", ["class"] = "test" };
        var props3 = new Dictionary<string, object?> { ["id"] = "2", ["class"] = "test" };

        var child1 = new TextNode("Child");
        var child2 = new TextNode("Child");

        var node1 = new ElementNode("div", props1, child1);
        var node2 = new ElementNode("div", props2, child2);
        var node3 = new ElementNode("div", props3, child1.Clone());
        var node4 = new ElementNode("span", props1, child1.Clone());

        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3)); // Different props
        Assert.False(node1.Equals(node4)); // Different tag
    }

    [Fact]
    public void ElementNode_Clone_DeepCopiesChildrenAndProps()
    {
        var props = new Dictionary<string, object?> { ["id"] = "test" };
        var child = new TextNode("Child");
        var original = new ElementNode("div", props, child) { Key = "key1" };

        var clone = (ElementNode)original.Clone();

        Assert.Equal(original.TagName, clone.TagName);
        Assert.Equal(original.Key, clone.Key);
        Assert.Equal(original.Props["id"], clone.Props["id"]);
        Assert.NotSame(original.Props, clone.Props);
        Assert.Equal(original.Children.Count, clone.Children.Count);
        Assert.NotSame(original.Children[0], clone.Children[0]);
    }

    [Fact]
    public void FragmentNode_Creation_AcceptsChildren()
    {
        var child1 = new TextNode("1");
        var child2 = new TextNode("2");
        var child3 = new TextNode("3");

        var fragment1 = new FragmentNode(child1, child2, child3);
        var fragment2 = new FragmentNode(new[] { child1, child2, child3 });

        Assert.Equal(VirtualNodeType.Fragment, fragment1.Type);
        Assert.Equal(3, fragment1.Children.Count);
        Assert.Equal(3, fragment2.Children.Count);
    }

    [Fact]
    public void FragmentNode_AddRemoveChildren_WorksCorrectly()
    {
        var fragment = new FragmentNode();
        var child = new TextNode("Child");

        fragment.AddChild(child);
        Assert.Single(fragment.Children);

        Assert.True(fragment.RemoveChild(child));
        Assert.Empty(fragment.Children);
    }

    [Fact]
    public void ComponentNode_Creation_SetsProperties()
    {
        var props = new Dictionary<string, object?> { ["name"] = "Test" };
        var node = new ComponentNode(typeof(string), props);

        Assert.Equal(VirtualNodeType.Component, node.Type);
        Assert.Equal(typeof(string), node.ComponentType);
        Assert.Equal("Test", node.Props["name"]);
        Assert.Null(node.ComponentInstance);
        Assert.Null(node.RenderedContent);
    }

    [Fact]
    public void ComponentNode_GenericCreate_WorksCorrectly()
    {
        var props = new Dictionary<string, object?> { ["value"] = 42 };
        var node = ComponentNode.Create<int>(props);

        Assert.Equal(typeof(int), node.ComponentType);
        Assert.Equal(42, node.Props["value"]);
    }

    [Fact]
    public void ComponentNode_Equality_ComparesTypeAndProps()
    {
        var props1 = new Dictionary<string, object?> { ["id"] = "1" };
        var props2 = new Dictionary<string, object?> { ["id"] = "1" };
        var props3 = new Dictionary<string, object?> { ["id"] = "2" };

        var node1 = new ComponentNode(typeof(string), props1);
        var node2 = new ComponentNode(typeof(string), props2);
        var node3 = new ComponentNode(typeof(string), props3);
        var node4 = new ComponentNode(typeof(int), props1);

        Assert.True(node1.Equals(node2));
        Assert.False(node1.Equals(node3)); // Different props
        Assert.False(node1.Equals(node4)); // Different type
    }

    [Fact]
    public void VirtualNode_Visitor_VisitsCorrectMethods()
    {
        var visitor = new TestVisitor();

        var text = new TextNode("Test");
        var element = new ElementNode("div");
        var fragment = new FragmentNode();
        var component = ComponentNode.Create<string>();

        text.Accept(visitor);
        element.Accept(visitor);
        fragment.Accept(visitor);
        component.Accept(visitor);

        Assert.True(visitor.VisitedText);
        Assert.True(visitor.VisitedElement);
        Assert.True(visitor.VisitedFragment);
        Assert.True(visitor.VisitedComponent);
    }

    private class TestVisitor : IVirtualNodeVisitor
    {
        public bool VisitedText { get; private set; }
        public bool VisitedElement { get; private set; }
        public bool VisitedFragment { get; private set; }
        public bool VisitedComponent { get; private set; }

        public void VisitText(TextNode node) => VisitedText = true;
        public void VisitElement(ElementNode node) => VisitedElement = true;
        public void VisitComponent(ComponentNode node) => VisitedComponent = true;
        public void VisitFragment(FragmentNode node) => VisitedFragment = true;
        public void VisitClipping(ClippingNode node) { }
        public void VisitEmpty(EmptyNode node) { }
    }
}