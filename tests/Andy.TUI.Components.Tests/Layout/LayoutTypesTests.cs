using FluentAssertions;
using Andy.TUI.Components.Layout;

namespace Andy.TUI.Components.Tests.Layout;

public class LayoutTypesTests
{
    [Fact]
    public void Spacing_Constructor_CreatesUniformSpacing()
    {
        var spacing = new Spacing(10);
        
        spacing.Top.Should().Be(10);
        spacing.Right.Should().Be(10);
        spacing.Bottom.Should().Be(10);
        spacing.Left.Should().Be(10);
        spacing.Horizontal.Should().Be(20);
        spacing.Vertical.Should().Be(20);
    }
    
    [Fact]
    public void Spacing_Constructor_CreatesVerticalHorizontalSpacing()
    {
        var spacing = new Spacing(5, 10);
        
        spacing.Top.Should().Be(5);
        spacing.Right.Should().Be(10);
        spacing.Bottom.Should().Be(5);
        spacing.Left.Should().Be(10);
        spacing.Horizontal.Should().Be(20);
        spacing.Vertical.Should().Be(10);
    }
    
    [Fact]
    public void Spacing_None_ReturnsZeroSpacing()
    {
        var spacing = Spacing.None;
        
        spacing.Top.Should().Be(0);
        spacing.Right.Should().Be(0);
        spacing.Bottom.Should().Be(0);
        spacing.Left.Should().Be(0);
        spacing.Horizontal.Should().Be(0);
        spacing.Vertical.Should().Be(0);
    }
    
    [Fact]
    public void Border_None_ReturnsBorderWithNoSides()
    {
        var border = Border.None;
        
        border.Style.Should().Be(BorderStyle.None);
        border.Top.Should().BeFalse();
        border.Right.Should().BeFalse();
        border.Bottom.Should().BeFalse();
        border.Left.Should().BeFalse();
    }
    
    [Fact]
    public void Border_Single_ReturnsBorderWithAllSides()
    {
        var border = Border.Single;
        
        border.Style.Should().Be(BorderStyle.Single);
        border.Top.Should().BeTrue();
        border.Right.Should().BeTrue();
        border.Bottom.Should().BeTrue();
        border.Left.Should().BeTrue();
    }
    
    [Fact]
    public void Rectangle_Properties_CalculateCorrectly()
    {
        var rect = new Rectangle(10, 20, 30, 40);
        
        rect.X.Should().Be(10);
        rect.Y.Should().Be(20);
        rect.Width.Should().Be(30);
        rect.Height.Should().Be(40);
        rect.Right.Should().Be(40);
        rect.Bottom.Should().Be(60);
    }
    
    [Fact]
    public void Rectangle_Inset_AppliesSpacingCorrectly()
    {
        var rect = new Rectangle(10, 20, 100, 80);
        var spacing = new Spacing(5, 10, 15, 20);
        
        var inset = rect.Inset(spacing);
        
        inset.X.Should().Be(30); // 10 + 20
        inset.Y.Should().Be(25); // 20 + 5
        inset.Width.Should().Be(70); // 100 - 20 - 10
        inset.Height.Should().Be(60); // 80 - 5 - 15
    }
    
    [Fact]
    public void Size_Unlimited_ReturnsMaxValues()
    {
        var size = Size.Unlimited;
        
        size.Width.Should().Be(int.MaxValue);
        size.Height.Should().Be(int.MaxValue);
    }
    
    [Fact]
    public void Size_Zero_ReturnsZeroSize()
    {
        var size = Size.Zero;
        
        size.Width.Should().Be(0);
        size.Height.Should().Be(0);
    }
}