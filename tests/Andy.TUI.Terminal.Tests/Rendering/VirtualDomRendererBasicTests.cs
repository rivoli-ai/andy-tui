using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal.Rendering;
using Xunit;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Terminal.Tests.Rendering;

/// <summary>
/// Basic tests for VirtualDomRenderer functionality.
/// </summary>
public class VirtualDomRendererBasicTests
{
    [Fact]
    public void Render_SimpleTextElement_WritesAtCorrectPosition()
    {
        // Arrange
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        
        var tree = Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            .WithProp("style", Style.Default)
            .WithChild(new TextNode("Hello World"))
            .Build();
        
        // Act
        renderer.Render(tree);
        
        // Assert
        var writes = mockSystem.GetTextWrites();
        Assert.Single(writes);
        Assert.Equal(10, writes[0].X);
        Assert.Equal(5, writes[0].Y);
        Assert.Equal("Hello World", writes[0].Text);
    }
    
    [Fact]
    public void Render_FragmentWithMultipleElements_WritesAllElements()
    {
        // Arrange
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        
        var tree = Fragment(
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 0)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("First"))
                .Build(),
            Element("text")
                .WithProp("x", 0)
                .WithProp("y", 1)
                .WithProp("style", Style.Default)
                .WithChild(new TextNode("Second"))
                .Build()
        );
        
        // Act
        renderer.Render(tree);
        
        // Assert
        var writes = mockSystem.GetTextWrites();
        Assert.Equal(2, writes.Count);
        Assert.Equal("First", writes[0].Text);
        Assert.Equal("Second", writes[1].Text);
        Assert.Equal(0, writes[0].Y);
        Assert.Equal(1, writes[1].Y);
    }
    
    [Fact]
    public void ApplyPatches_UpdateText_ClearsAndRewritesElement()
    {
        // Arrange
        var mockSystem = new MockRenderingSystem();
        var renderer = new VirtualDomRenderer(mockSystem);
        
        var tree = Element("text")
            .WithProp("x", 5)
            .WithProp("y", 10)
            .WithProp("width", 10)
            .WithProp("height", 1)
            .WithProp("style", Style.Default)
            .WithChild(new TextNode("Original"))
            .Build();
        
        renderer.Render(tree);
        mockSystem.Clear();
        
        // Act
        var patch = new UpdateTextPatch(new[] { 0 }, "Updated");
        renderer.ApplyPatches(new[] { patch });
        
        // Assert
        var clears = mockSystem.GetFillRects();
        var writes = mockSystem.GetTextWrites();
        
        Assert.NotEmpty(clears);
        Assert.Single(writes);
        Assert.Equal("Updated", writes[0].Text);
    }
}