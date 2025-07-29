using FluentAssertions;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using static Andy.TUI.Components.Tests.TestHelpers;

namespace Andy.TUI.Components.Tests.Layout;

public class ScrollViewTests
{
    [Fact]
    public void ScrollView_ScrollPosition_ClampedToValidRange()
    {
        var scrollView = new ScrollView
        {
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("Long line of text", 100)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        // Try to scroll beyond bounds
        scrollView.ScrollY = 1000;
        scrollView.ScrollY.Should().Be(scrollView.MaxScrollY);
        
        scrollView.ScrollX = -10;
        scrollView.ScrollX.Should().Be(0);
    }
    
    [Fact]
    public void ScrollView_ViewportSize_AccountsForScrollbars()
    {
        var scrollView = new ScrollView
        {
            ShowVerticalScrollbar = true,
            ShowHorizontalScrollbar = true,
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("x", 100)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        // Viewport should be smaller when scrollbars are shown
        scrollView.ViewportWidth.Should().BeLessThan(50);
        scrollView.ViewportHeight.Should().BeLessThan(20);
    }
    
    [Fact]
    public void ScrollView_ScrollToArea_ScrollsToMakeAreaVisible()
    {
        var scrollView = new ScrollView
        {
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("x".PadRight(200), 200)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        // Scroll to area that's off screen
        scrollView.ScrollToArea(new Rectangle(100, 50, 10, 5));
        
        // Should have scrolled to make area visible
        scrollView.ScrollX.Should().BeGreaterThan(0);
        scrollView.ScrollY.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void ScrollView_ScrollBy_UpdatesPosition()
    {
        var scrollView = new ScrollView
        {
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("x".PadRight(200), 200)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        var initialX = scrollView.ScrollX;
        var initialY = scrollView.ScrollY;
        
        scrollView.ScrollBy(10, 5);
        
        scrollView.ScrollX.Should().Be(initialX + 10);
        scrollView.ScrollY.Should().Be(initialY + 5);
    }
    
    [Fact]
    public void ScrollView_ScrollToPositions_WorkCorrectly()
    {
        var scrollView = new ScrollView
        {
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("x".PadRight(200), 200)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        scrollView.ScrollToTop();
        scrollView.ScrollY.Should().Be(0);
        
        scrollView.ScrollToBottom();
        scrollView.ScrollY.Should().Be(scrollView.MaxScrollY);
        
        scrollView.ScrollToLeft();
        scrollView.ScrollX.Should().Be(0);
        
        scrollView.ScrollToRight();
        scrollView.ScrollX.Should().Be(scrollView.MaxScrollX);
    }
    
    [Fact]
    public void ScrollView_DisabledScrolling_PreventsScrolling()
    {
        var scrollView = new ScrollView
        {
            HorizontalScrollEnabled = false,
            VerticalScrollEnabled = false,
            Content = new TextNode(string.Join("\n", Enumerable.Repeat("x".PadRight(200), 200)))
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        scrollView.ScrollBy(10, 10);
        
        scrollView.ScrollX.Should().Be(0);
        scrollView.ScrollY.Should().Be(0);
    }
    
    [Fact]
    public void ScrollView_Render_IncludesViewportAndScrollbars()
    {
        var scrollView = new ScrollView
        {
            Content = new TextNode("Content"),
            ShowVerticalScrollbar = true,
            ShowHorizontalScrollbar = true
        };
        
        scrollView.Initialize(new MockComponentContext());
        scrollView.Measure(new Size(50, 20));
        scrollView.Arrange(new Rectangle(0, 0, 50, 20));
        
        var result = scrollView.Render();
        
        result.Should().BeOfType<ElementNode>();
        var element = (ElementNode)result;
        element.TagName.Should().Be("scrollview");
        
        // Should have viewport child
        var children = element.Children.ToList();
        var hasViewport = children.Any(child => child is ElementNode elem && elem.TagName == "viewport");
        hasViewport.Should().BeTrue();
    }
    
    [Fact]
    public void ScrollView_MeasureCore_WithoutContent_ReturnsMinimumSize()
    {
        var scrollView = new ScrollView
        {
            Padding = new Spacing(5)
        };
        
        var size = scrollView.Measure(new Size(100, 100));
        
        size.Width.Should().Be(10);
        size.Height.Should().Be(10);
    }
    
}