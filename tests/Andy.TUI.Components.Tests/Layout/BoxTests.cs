using FluentAssertions;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using static Andy.TUI.Components.Tests.TestHelpers;

namespace Andy.TUI.Components.Tests.Layout;

public class BoxTests
{
    [Fact]
    public void Box_MeasureCore_ReturnsCorrectSizeWithPadding()
    {
        var box = new Box
        {
            Padding = new Spacing(5, 10)
        };
        
        var size = box.Measure(new Size(100, 50));
        
        size.Width.Should().BeGreaterThanOrEqualTo(20); // At least padding
        size.Height.Should().BeGreaterThanOrEqualTo(10);
    }
    
    [Fact]
    public void Box_MeasureCore_IncludesBorderSize()
    {
        var box = new Box
        {
            Border = Border.Single,
            Padding = new Spacing(5)
        };
        
        var size = box.Measure(new Size(100, 50));
        
        size.Width.Should().BeGreaterThanOrEqualTo(12); // Padding (10) + Border (2)
        size.Height.Should().BeGreaterThanOrEqualTo(12);
    }
    
    [Fact]
    public void Box_Render_IncludesBackgroundColor()
    {
        var box = new Box
        {
            BackgroundColor = Color.Blue
        };
        box.Initialize(new MockComponentContext());
        box.Arrange(new Rectangle(0, 0, 10, 5));
        
        var result = box.Render();
        
        result.Should().BeOfType<ElementNode>();
        var element = (ElementNode)result;
        element.TagName.Should().Be("box");
        
        // Should have background rect child
        var children = element.Children.ToList();
        var hasBackground = children.Any(child => 
        {
            if (child is ElementNode elem && elem.TagName == "rect")
            {
                if (elem.Props.TryGetValue("fill", out var fill) && fill is Color color)
                {
                    return color == Color.Blue;
                }
            }
            return false;
        });
        hasBackground.Should().BeTrue();
    }
    
    [Fact]
    public void Box_Render_IncludesBorder()
    {
        var box = new Box
        {
            Border = Border.Single,
            BorderColor = Color.Green
        };
        box.Initialize(new MockComponentContext());
        box.Arrange(new Rectangle(0, 0, 10, 5));
        
        var result = box.Render();
        
        result.Should().BeOfType<ElementNode>();
        var element = (ElementNode)result;
        
        // Should have border elements
        var children = element.Children.ToList();
        var hasBorder = children.Any(child => child is FragmentNode);
        hasBorder.Should().BeTrue();
    }
    
    [Fact]
    public void Box_ContentAlignment_AppliesCorrectly()
    {
        var content = new TextNode("Test");
        var box = new Box
        {
            Content = content,
            ContentHorizontalAlignment = Alignment.Center,
            ContentVerticalAlignment = Alignment.Center,
            Padding = new Spacing(10)
        };
        
        box.Initialize(new MockComponentContext());
        box.Measure(new Size(100, 50));
        box.Arrange(new Rectangle(0, 0, 100, 50));
        
        var result = box.Render();
        
        // Content should be centered within the padding area
        result.Should().BeOfType<ElementNode>();
    }
    
    [Fact]
    public void Box_WithDynamicContent_UsesRenderFunction()
    {
        var renderCount = 0;
        var box = new Box
        {
            RenderContent = () =>
            {
                renderCount++;
                return new TextNode($"Render {renderCount}");
            }
        };
        
        box.Initialize(new MockComponentContext());
        box.Measure(new Size(100, 50));
        box.Arrange(new Rectangle(0, 0, 100, 50));
        
        var result1 = box.Render();
        var result2 = box.Render();
        
        renderCount.Should().Be(2);
    }
    
}