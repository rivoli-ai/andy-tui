using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class ResizeInfoTests
{
    [Fact]
    public void ResizeInfo_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var resizeInfo = new ResizeInfo(width: 120, height: 40, previousWidth: 80, previousHeight: 25);
        
        // Assert
        Assert.Equal(120, resizeInfo.Width);
        Assert.Equal(40, resizeInfo.Height);
        Assert.Equal(80, resizeInfo.PreviousWidth);
        Assert.Equal(25, resizeInfo.PreviousHeight);
    }
    
    [Fact]
    public void ResizeInfo_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var resizeInfo = new ResizeInfo(100, 30, 80, 25);
        
        // Act
        var result = resizeInfo.ToString();
        
        // Assert
        Assert.Equal("Resize: 80x25 -> 100x30", result);
    }
    
    [Fact]
    public void ResizeInfo_WithSameDimensions_ShouldFormatCorrectly()
    {
        // Arrange
        var resizeInfo = new ResizeInfo(80, 25, 80, 25);
        
        // Act
        var result = resizeInfo.ToString();
        
        // Assert
        Assert.Equal("Resize: 80x25 -> 80x25", result);
    }
}