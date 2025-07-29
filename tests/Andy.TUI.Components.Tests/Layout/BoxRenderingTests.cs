using System.Linq;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Layout;

public class BoxRenderingTests
{
    [Fact]
    public void Box_WithSimpleTextContent_RendersCorrectly()
    {
        // Arrange
        var box = new Box
        {
            Border = new Border(BorderStyle.Single),
            Padding = new Spacing(1, 2),
            Content = new TextNode("Hello Box!")
        };
        
        var context = TestHelpers.CreateMockContext(box);
        box.Initialize(context);
        box.Arrange(new Rectangle(0, 0, 20, 5));
        
        // Act
        var node = box.Render();
        
        // Assert
        Assert.NotNull(node);
        Assert.IsType<ElementNode>(node);
        
        var element = (ElementNode)node;
        Assert.Equal("box", element.TagName);
        
        // Check that we have children (border fragments and content)
        Assert.NotEmpty(element.Children);
        
        // Find the content element
        var contentElement = FindContentElement(element);
        Assert.NotNull(contentElement);
        
        // Content should be at position (3, 2) - border (1) + padding (2)
        Assert.Equal(3, contentElement.Props["x"]);
        Assert.Equal(2, contentElement.Props["y"]);
        
        // Content should have our text
        var textNode = contentElement.Children.FirstOrDefault() as TextNode;
        Assert.NotNull(textNode);
        Assert.Equal("Hello Box!", textNode.Content);
    }
    
    [Fact]
    public void Box_WithColoredBorder_RendersWithCorrectStyle()
    {
        // Arrange
        var box = new Box
        {
            Border = new Border(BorderStyle.Double),
            BorderColor = Color.Green,
            Content = new TextNode("Colored Border")
        };
        
        var context = TestHelpers.CreateMockContext(box);
        box.Initialize(context);
        box.Arrange(new Rectangle(0, 0, 20, 5));
        
        // Act
        var node = box.Render();
        
        // Assert
        var element = (ElementNode)node;
        
        // Find border text elements
        var borderTexts = FindAllTextElements(element).ToList();
        Assert.NotEmpty(borderTexts);
        
        // Border elements should have the border color style
        var borderElement = borderTexts.First();
        Assert.True(borderElement.Props.ContainsKey("style"));
        var styleObj = borderElement.Props["style"];
        Assert.IsType<Style>(styleObj);
        var style = (Style)styleObj;
        Assert.Equal(Color.Green, style.Foreground);
    }
    
    [Fact]
    public void Box_WithBackgroundColor_RendersBackgroundRect()
    {
        // Arrange
        var box = new Box
        {
            BackgroundColor = Color.DarkBlue,
            Content = new TextNode("Background")
        };
        
        var context = TestHelpers.CreateMockContext(box);
        box.Initialize(context);
        box.Arrange(new Rectangle(0, 0, 20, 5));
        
        // Act
        var node = box.Render();
        
        // Assert
        var element = (ElementNode)node;
        
        // Should have a rect element for background
        var rectElement = FindElementByTagName(element, "rect");
        Assert.NotNull(rectElement);
        Assert.Equal(Color.DarkBlue, rectElement.Props["fill"]);
    }
    
    [Fact]
    public void Box_AtNonZeroPosition_RendersAtCorrectLocation()
    {
        // Arrange
        var box = new Box
        {
            Border = new Border(BorderStyle.Single),
            Content = new TextNode("Positioned")
        };
        
        var context = TestHelpers.CreateMockContext(box);
        box.Initialize(context);
        box.Arrange(new Rectangle(10, 5, 20, 3));
        
        // Act
        var node = box.Render();
        
        // Assert
        var element = (ElementNode)node;
        Assert.Equal(10, element.Props["x"]);
        Assert.Equal(5, element.Props["y"]);
        
        // Border elements should be positioned relative to box position
        var borderTexts = FindAllTextElements(element).ToList();
        Assert.NotEmpty(borderTexts);
        
        // Top-left corner should be at (10, 5)
        var topLeftCorner = borderTexts.FirstOrDefault(t => 
            t.Children.FirstOrDefault() is TextNode tn && tn.Content == "┌");
        Assert.NotNull(topLeftCorner);
        Assert.Equal(10, topLeftCorner.Props["x"]);
        Assert.Equal(5, topLeftCorner.Props["y"]);
    }
    
    private ElementNode? FindContentElement(ElementNode parent)
    {
        foreach (var child in parent.Children)
        {
            if (child is ElementNode element)
            {
                if (element.TagName == "content")
                    return element;
                
                var found = FindContentElement(element);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
    
    private ElementNode? FindElementByTagName(ElementNode parent, string tagName)
    {
        foreach (var child in parent.Children)
        {
            if (child is ElementNode element)
            {
                if (element.TagName == tagName)
                    return element;
                
                var found = FindElementByTagName(element, tagName);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
    
    private System.Collections.Generic.IEnumerable<ElementNode> FindAllTextElements(ElementNode parent)
    {
        foreach (var child in parent.Children)
        {
            if (child is ElementNode element)
            {
                if (element.TagName == "text")
                    yield return element;
                
                foreach (var nested in FindAllTextElements(element))
                    yield return nested;
            }
            else if (child is FragmentNode fragment)
            {
                foreach (var fragChild in fragment.Children)
                {
                    if (fragChild is ElementNode fragElement)
                    {
                        if (fragElement.TagName == "text")
                            yield return fragElement;
                        
                        foreach (var nested in FindAllTextElements(fragElement))
                            yield return nested;
                    }
                }
            }
        }
    }
}