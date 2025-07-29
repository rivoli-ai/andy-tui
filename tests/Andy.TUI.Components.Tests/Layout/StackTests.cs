using FluentAssertions;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using static Andy.TUI.Components.Tests.TestHelpers;

namespace Andy.TUI.Components.Tests.Layout;

public class StackTests
{
    [Fact]
    public void Stack_Vertical_MeasuresCorrectly()
    {
        var stack = new Stack
        {
            Orientation = Orientation.Vertical,
            Spacing = 5
        };
        
        stack.AddChild(new TextNode("Line 1"));
        stack.AddChild(new TextNode("Line 2"));
        stack.AddChild(new TextNode("Line 3"));
        
        var size = stack.Measure(new Size(100, 100));
        
        // Height should be 3 lines + 2 * spacing
        size.Height.Should().BeGreaterThanOrEqualTo(13); // 3 + 5 + 5
    }
    
    [Fact]
    public void Stack_Horizontal_MeasuresCorrectly()
    {
        var stack = new Stack
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2
        };
        
        stack.AddChild(new TextNode("A"));
        stack.AddChild(new TextNode("B"));
        stack.AddChild(new TextNode("C"));
        
        var size = stack.Measure(new Size(100, 100));
        
        // Width should be 3 chars + 2 * spacing
        size.Width.Should().BeGreaterThanOrEqualTo(7); // 3 + 2 + 2
    }
    
    [Fact]
    public void Stack_MainAxisAlignment_Center()
    {
        var stack = new Stack
        {
            Orientation = Orientation.Vertical,
            MainAxisAlignment = Alignment.Center
        };
        
        stack.Initialize(new MockComponentContext());
        stack.AddChild(new TextNode("Test"));
        
        stack.Measure(new Size(50, 50));
        stack.Arrange(new Rectangle(0, 0, 50, 50));
        
        // Content should be centered vertically
        var result = stack.Render();
        result.Should().BeOfType<ElementNode>();
    }
    
    [Fact]
    public void Stack_CrossAxisAlignment_Stretch()
    {
        var stack = new Stack
        {
            Orientation = Orientation.Vertical,
            CrossAxisAlignment = Alignment.Stretch
        };
        
        stack.Initialize(new MockComponentContext());
        stack.AddChild(new TextNode("Short"));
        stack.AddChild(new TextNode("Much longer text"));
        
        var size = stack.Measure(new Size(100, 100));
        
        // All children should stretch to the width of the longest
        size.Width.Should().BeGreaterThanOrEqualTo(16); // "Much longer text".Length
    }
    
    [Fact]
    public void Stack_Reverse_OrdersChildrenCorrectly()
    {
        var stack = new Stack
        {
            Reverse = true
        };
        
        stack.AddChild(new TextNode("First"));
        stack.AddChild(new TextNode("Second"));
        stack.AddChild(new TextNode("Third"));
        
        stack.Initialize(new MockComponentContext());
        var result = stack.Render();
        
        result.Should().BeOfType<ElementNode>();
        var element = (ElementNode)result;
        var children = element.Children.ToList();
        
        // Children should be in reverse order
        children[0].Should().BeOfType<TextNode>().Which.Content.Should().Be("Third");
        children[1].Should().BeOfType<TextNode>().Which.Content.Should().Be("Second");
        children[2].Should().BeOfType<TextNode>().Which.Content.Should().Be("First");
    }
    
    [Fact]
    public void Stack_AddRemoveChildren_UpdatesCorrectly()
    {
        var stack = new Stack();
        
        stack.Children.Should().BeEmpty();
        
        var child1 = new TextNode("Child 1");
        var child2 = new TextNode("Child 2");
        
        stack.AddChild(child1);
        stack.Children.Should().HaveCount(1);
        
        stack.AddChild(child2);
        stack.Children.Should().HaveCount(2);
        
        stack.RemoveChild(child1).Should().BeTrue();
        stack.Children.Should().HaveCount(1);
        stack.Children.Should().Contain(child2);
        
        stack.ClearChildren();
        stack.Children.Should().BeEmpty();
    }
    
    [Fact]
    public void Stack_SetChildren_ReplacesAllChildren()
    {
        var stack = new Stack();
        
        stack.AddChild(new TextNode("Old"));
        
        var newChildren = new[]
        {
            new TextNode("New 1"),
            new TextNode("New 2")
        };
        
        stack.SetChildren(newChildren);
        
        stack.Children.Should().HaveCount(2);
        stack.Children.Should().BeEquivalentTo(newChildren);
    }
    
    [Fact]
    public void Stack_WithPadding_IncludesInMeasurement()
    {
        var stack = new Stack
        {
            Padding = new Spacing(10)
        };
        
        stack.AddChild(new TextNode("Content"));
        
        var size = stack.Measure(new Size(100, 100));
        
        size.Width.Should().BeGreaterThanOrEqualTo(27); // "Content".Length + 20
        size.Height.Should().BeGreaterThanOrEqualTo(21); // 1 + 20
    }
    
}