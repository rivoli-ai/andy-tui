using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Layout;

public class LayoutExampleRenderingTests
{
    [Fact]
    public void Box_RenderingStructure_ContainsFragmentWithBorderElements()
    {
        // Arrange
        var box = new Box
        {
            Border = new Border(BorderStyle.Single),
            Padding = new Spacing(1),
            Content = new TextNode("Test")
        };
        
        var context = TestHelpers.CreateMockContext(box);
        box.Initialize(context);
        box.Arrange(new Rectangle(0, 0, 10, 5));
        
        // Act
        var node = box.Render();
        
        // Assert
        Assert.IsType<ElementNode>(node);
        var element = (ElementNode)node;
        Assert.Equal("box", element.TagName);
        
        // Should have at least 2 children: Fragment (for borders) and content element
        Assert.True(element.Children.Count >= 2);
        
        // First child should be rect (if background) or fragment (borders)
        var hasFragment = element.Children.Any(c => c is FragmentNode);
        Assert.True(hasFragment);
        
        // Find the fragment
        var fragment = element.Children.OfType<FragmentNode>().First();
        Assert.NotEmpty(fragment.Children);
        
        // Fragment should contain text elements for borders
        var borderTexts = fragment.Children.OfType<ElementNode>()
            .Where(e => e.TagName == "text")
            .ToList();
        Assert.NotEmpty(borderTexts);
        
        // Should have border characters
        var borderChars = borderTexts
            .SelectMany(e => e.Children.OfType<TextNode>())
            .Select(t => t.Content)
            .ToList();
        
        // Should contain various border characters
        Assert.Contains("─", borderChars);
        Assert.Contains("│", borderChars);
        Assert.Contains("┌", borderChars);
        Assert.Contains("┐", borderChars);
        Assert.Contains("└", borderChars);
        Assert.Contains("┘", borderChars);
    }
    
    [Fact]
    public void FragmentNode_ShouldRenderAllChildren()
    {
        // This test verifies the rendering logic handles fragments correctly
        var mockRenderer = new MockRenderer();
        
        // Create a fragment with multiple text elements
        var fragment = new FragmentNode(
            new ElementNode("text", new Dictionary<string, object?> { ["x"] = 0, ["y"] = 0 }, 
                new TextNode("A")),
            new ElementNode("text", new Dictionary<string, object?> { ["x"] = 1, ["y"] = 0 }, 
                new TextNode("B")),
            new ElementNode("text", new Dictionary<string, object?> { ["x"] = 2, ["y"] = 0 }, 
                new TextNode("C"))
        );
        
        // Simulate rendering the fragment
        mockRenderer.RenderNode(fragment, 0, 0);
        
        // All three text nodes should be rendered
        Assert.Equal(3, mockRenderer.RenderedTexts.Count);
        Assert.Contains(mockRenderer.RenderedTexts, t => t.Text == "A" && t.X == 0);
        Assert.Contains(mockRenderer.RenderedTexts, t => t.Text == "B" && t.X == 1);
        Assert.Contains(mockRenderer.RenderedTexts, t => t.Text == "C" && t.X == 2);
    }
    
    private class MockRenderer
    {
        public List<(int X, int Y, string Text, Style Style)> RenderedTexts { get; } = new();
        
        public void RenderNode(VirtualNode node, int baseX, int baseY)
        {
            if (node is ElementNode element)
            {
                if (element.TagName == "text")
                {
                    var x = element.Props.TryGetValue("x", out var xObj) && xObj is int xVal ? xVal : 0;
                    var y = element.Props.TryGetValue("y", out var yObj) && yObj is int yVal ? yVal : 0;
                    var style = element.Props.TryGetValue("style", out var styleObj) && styleObj is Style s ? s : Style.Default;
                    
                    foreach (var child in element.Children)
                    {
                        if (child is TextNode text)
                        {
                            RenderedTexts.Add((x, y, text.Content, style));
                        }
                    }
                }
                else
                {
                    foreach (var child in element.Children)
                    {
                        RenderNode(child, baseX, baseY);
                    }
                }
            }
            else if (node is FragmentNode fragment)
            {
                foreach (var child in fragment.Children)
                {
                    RenderNode(child, baseX, baseY);
                }
            }
        }
    }
}