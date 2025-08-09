using System;
using Xunit;
using Andy.TUI.Layout;

namespace Andy.TUI.Layout.Tests;

public class LayoutBoxTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var box = new LayoutBox();
        
        Assert.Equal(0, box.X);
        Assert.Equal(0, box.Y);
        Assert.Equal(0, box.Width);
        Assert.Equal(0, box.Height);
        // Padding and Margin are value types, always initialized
    }
    
    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var box = new LayoutBox
        {
            X = 10,
            Y = 20,
            Width = 100,
            Height = 50,
            Padding = new Spacing(5),
            Margin = new Spacing(10)
        };
        
        Assert.Equal(10, box.X);
        Assert.Equal(20, box.Y);
        Assert.Equal(100, box.Width);
        Assert.Equal(50, box.Height);
        Assert.Equal(5, box.Padding.Top.Value);
        Assert.Equal(10, box.Margin.Top.Value);
    }
    
    [Fact]
    public void OuterWidth_IncludesPadding()
    {
        var box = new LayoutBox
        {
            Width = 100,
            Padding = new Spacing(0, 15, 0, 10) // Top=0, Right=15, Bottom=0, Left=10
        };
        
        Assert.Equal(125, box.OuterWidth); // 100 + 10 + 15
    }
    
    [Fact]
    public void OuterHeight_IncludesPadding()
    {
        var box = new LayoutBox
        {
            Height = 50,
            Padding = new Spacing(5, 0, 8, 0) // Top=5, Right=0, Bottom=8, Left=0
        };
        
        Assert.Equal(63, box.OuterHeight); // 50 + 5 + 8
    }
    
    [Fact]
    public void OuterWidth_WithPercentagePadding_CalculatesCorrectly()
    {
        var box = new LayoutBox
        {
            Width = 200,
            Padding = new Spacing(
                Length.Pixels(0),
                Length.Percentage(5),   // 5% of 200 = 10
                Length.Pixels(0),
                Length.Percentage(10)   // 10% of 200 = 20
            )
        };
        
        Assert.Equal(230, box.OuterWidth); // 200 + 20 + 10
    }
    
    [Fact]
    public void OuterHeight_WithPercentagePadding_CalculatesCorrectly()
    {
        var box = new LayoutBox
        {
            Height = 100,
            Padding = new Spacing(
                Length.Percentage(20),   // 20% of 100 = 20
                Length.Pixels(0),
                Length.Percentage(10),   // 10% of 100 = 10
                Length.Pixels(0)
            )
        };
        
        Assert.Equal(130, box.OuterHeight); // 100 + 20 + 10
    }
    
    [Fact]
    public void AbsolutePosition_CanBeSetAndRetrieved()
    {
        var box = new LayoutBox
        {
            AbsoluteX = 100,
            AbsoluteY = 50
        };
        
        Assert.Equal(100, box.AbsoluteX);
        Assert.Equal(50, box.AbsoluteY);
    }
    
    [Fact]
    public void ContentX_IncludesLeftPadding()
    {
        var box = new LayoutBox
        {
            AbsoluteX = 10,
            Width = 100,
            Padding = Spacing.OnlyLeft(5)
        };
        
        Assert.Equal(15, box.ContentX); // 10 + 5
    }
    
    [Fact]
    public void ContentY_IncludesTopPadding()
    {
        var box = new LayoutBox
        {
            AbsoluteY = 20,
            Height = 50,
            Padding = Spacing.OnlyTop(8)
        };
        
        Assert.Equal(28, box.ContentY); // 20 + 8
    }
    
    [Fact]
    public void ContentWidth_ReturnsRoundedWidth()
    {
        var box = new LayoutBox { Width = 100.7f };
        Assert.Equal(101, box.ContentWidth);
        
        box.Width = 100.3f;
        Assert.Equal(100, box.ContentWidth);
    }
    
    [Fact]
    public void ContentHeight_ReturnsRoundedHeight()
    {
        var box = new LayoutBox { Height = 50.6f };
        Assert.Equal(51, box.ContentHeight);
        
        box.Height = 50.4f;
        Assert.Equal(50, box.ContentHeight);
    }
    
    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var box = new LayoutBox
        {
            AbsoluteX = 10,
            AbsoluteY = 20,
            Width = 100,
            Height = 50,
            Padding = new Spacing(0)
        };
        
        Assert.True(box.Contains(10, 20));   // Top-left corner
        Assert.True(box.Contains(50, 40));   // Middle
        Assert.True(box.Contains(109, 69));  // Bottom-right corner (exclusive)
    }
    
    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var box = new LayoutBox
        {
            AbsoluteX = 10,
            AbsoluteY = 20,
            Width = 100,
            Height = 50,
            Padding = new Spacing(0)
        };
        
        Assert.False(box.Contains(9, 20));    // Just left
        Assert.False(box.Contains(10, 19));   // Just above
        Assert.False(box.Contains(110, 40));  // Just right
        Assert.False(box.Contains(50, 70));   // Just below
    }
    
    [Fact]
    public void Clone_CreatesDeepCopy()
    {
        var original = new LayoutBox
        {
            X = 10,
            Y = 20,
            Width = 100,
            Height = 50,
            Padding = new Spacing(5),
            Margin = new Spacing(10)
        };
        
        var clone = original.Clone();
        
        Assert.NotSame(original, clone);
        Assert.Equal(original.X, clone.X);
        Assert.Equal(original.Y, clone.Y);
        Assert.Equal(original.Width, clone.Width);
        Assert.Equal(original.Height, clone.Height);
        Assert.Equal(original.Padding.Top.Value, clone.Padding.Top.Value);
        Assert.Equal(original.Margin.Top.Value, clone.Margin.Top.Value);
        
        // Verify deep copy by modifying clone
        clone.X = 999;
        Assert.NotEqual(original.X, clone.X);
    }
    
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var box = new LayoutBox
        {
            X = 10,
            Y = 20,
            Width = 100,
            Height = 50
        };
        
        var result = box.ToString();
        
        Assert.Contains("10", result);
        Assert.Contains("20", result);
        Assert.Contains("100", result);
        Assert.Contains("50", result);
    }
}