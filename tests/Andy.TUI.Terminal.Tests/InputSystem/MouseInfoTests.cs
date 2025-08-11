using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class MouseInfoTests
{
    [Fact]
    public void MouseInfo_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var mouseInfo = new MouseInfo(
            x: 15,
            y: 25,
            button: MouseButton.Right,
            modifiers: ConsoleModifiers.Control | ConsoleModifiers.Shift,
            wheelDelta: -1,
            isDrag: true,
            dragStart: (10, 20),
            command: true
        );

        // Assert
        Assert.Equal(15, mouseInfo.X);
        Assert.Equal(25, mouseInfo.Y);
        Assert.Equal(MouseButton.Right, mouseInfo.Button);
        Assert.Equal(ConsoleModifiers.Control | ConsoleModifiers.Shift, mouseInfo.Modifiers);
        Assert.Equal(-1, mouseInfo.WheelDelta);
        Assert.True(mouseInfo.IsDrag);
        Assert.Equal((10, 20), mouseInfo.DragStart);
        Assert.True(mouseInfo.Command);
    }

    [Fact]
    public void MouseInfo_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var mouseInfo = new MouseInfo(10, 20, MouseButton.Left);

        // Assert
        Assert.Equal(10, mouseInfo.X);
        Assert.Equal(20, mouseInfo.Y);
        Assert.Equal(MouseButton.Left, mouseInfo.Button);
        Assert.Equal((ConsoleModifiers)0, mouseInfo.Modifiers);
        Assert.Equal(0, mouseInfo.WheelDelta);
        Assert.False(mouseInfo.IsDrag);
        Assert.Null(mouseInfo.DragStart);
        Assert.False(mouseInfo.Command);
    }

    [Fact]
    public void MouseInfo_ModifierProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var mouseInfo = new MouseInfo(0, 0, MouseButton.None, ConsoleModifiers.Control | ConsoleModifiers.Alt);

        // Act & Assert
        Assert.True(mouseInfo.Control);
        Assert.True(mouseInfo.Alt);
        Assert.False(mouseInfo.Shift);
    }

    [Fact]
    public void MouseInfo_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var mouseInfo = new MouseInfo(15, 25, MouseButton.Left, ConsoleModifiers.Shift);

        // Act
        var result = mouseInfo.ToString();

        // Assert
        Assert.Contains("Shift", result);
        Assert.Contains("LeftButton", result);
        Assert.Contains("(15,25)", result);
    }

    [Fact]
    public void MouseInfo_ToString_WithWheelDelta_ShouldIncludeWheelInfo()
    {
        // Arrange
        var mouseInfo = new MouseInfo(10, 20, MouseButton.None, wheelDelta: 3);

        // Act
        var result = mouseInfo.ToString();

        // Assert
        Assert.Contains("Wheel:3", result);
        Assert.Contains("(10,20)", result);
    }

    [Fact]
    public void MouseInfo_ToString_WithDrag_ShouldIncludeDragInfo()
    {
        // Arrange
        var mouseInfo = new MouseInfo(15, 25, MouseButton.Left, isDrag: true);

        // Act
        var result = mouseInfo.ToString();

        // Assert
        Assert.Contains("Drag", result);
        Assert.Contains("LeftButton", result);
        Assert.Contains("(15,25)", result);
    }

    [Fact]
    public void MouseInfo_ToString_WithCommandKey_ShouldIncludeCmd()
    {
        // Arrange
        var mouseInfo = new MouseInfo(10, 20, MouseButton.Right, command: true);

        // Act
        var result = mouseInfo.ToString();

        // Assert
        Assert.Contains("Cmd", result);
        Assert.Contains("RightButton", result);
    }
}