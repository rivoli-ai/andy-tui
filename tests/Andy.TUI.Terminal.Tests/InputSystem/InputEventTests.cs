using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class InputEventTests
{
    [Fact]
    public void KeyboardInputEvent_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.A, 'a', ConsoleModifiers.Control);

        // Act
        var inputEvent = new InputEvent(InputEventType.KeyPress, keyInfo);

        // Assert
        Assert.Equal(InputEventType.KeyPress, inputEvent.Type);
        Assert.Equal(keyInfo, inputEvent.Key);
        Assert.Null(inputEvent.Mouse);
        Assert.Null(inputEvent.Resize);
        Assert.False(inputEvent.Handled);
        Assert.True(inputEvent.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void MouseInputEvent_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var mouseInfo = new MouseInfo(10, 20, MouseButton.Left, ConsoleModifiers.Shift);

        // Act
        var inputEvent = new InputEvent(InputEventType.MousePress, mouseInfo);

        // Assert
        Assert.Equal(InputEventType.MousePress, inputEvent.Type);
        Assert.Equal(mouseInfo, inputEvent.Mouse);
        Assert.Null(inputEvent.Key);
        Assert.Null(inputEvent.Resize);
        Assert.False(inputEvent.Handled);
        Assert.True(inputEvent.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void ResizeInputEvent_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var resizeInfo = new ResizeInfo(80, 25, 100, 30);

        // Act
        var inputEvent = new InputEvent(resizeInfo);

        // Assert
        Assert.Equal(InputEventType.Resize, inputEvent.Type);
        Assert.Equal(resizeInfo, inputEvent.Resize);
        Assert.Null(inputEvent.Key);
        Assert.Null(inputEvent.Mouse);
        Assert.False(inputEvent.Handled);
        Assert.True(inputEvent.Timestamp <= DateTime.UtcNow);
    }

    [Fact]
    public void InputEvent_HandledProperty_ShouldBeSettable()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.Enter, '\r', 0);
        var inputEvent = new InputEvent(InputEventType.KeyPress, keyInfo);

        // Act
        inputEvent.Handled = true;

        // Assert
        Assert.True(inputEvent.Handled);
    }
}