using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class CrossPlatformInputManagerTests : IDisposable
{
    private readonly CrossPlatformInputManager _inputManager;

    public CrossPlatformInputManagerTests()
    {
        _inputManager = new CrossPlatformInputManager();
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.False(_inputManager.IsRunning);
        Assert.True(_inputManager.BufferingEnabled);
        Assert.Equal(1000, _inputManager.BufferSize);
        // SupportsMouseInput will vary by environment, so we just check it's a boolean
        Assert.True(_inputManager.SupportsMouseInput == true || _inputManager.SupportsMouseInput == false);
    }

    [Fact]
    public void BufferingEnabled_ShouldBeSettable()
    {
        // Act
        _inputManager.BufferingEnabled = false;

        // Assert
        Assert.False(_inputManager.BufferingEnabled);
    }

    [Fact]
    public void BufferSize_ShouldBeSettable()
    {
        // Act
        _inputManager.BufferSize = 500;

        // Assert
        Assert.Equal(500, _inputManager.BufferSize);
    }

    [Fact]
    public void Start_ShouldSetIsRunningToTrue()
    {
        // Act
        _inputManager.Start();

        // Assert
        Assert.True(_inputManager.IsRunning);

        // Cleanup
        _inputManager.Stop();
    }

    [Fact]
    public void Stop_ShouldSetIsRunningToFalse()
    {
        // Arrange
        _inputManager.Start();
        Assert.True(_inputManager.IsRunning);

        // Act
        _inputManager.Stop();

        // Assert
        Assert.False(_inputManager.IsRunning);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_ShouldNotThrow()
    {
        // Arrange
        _inputManager.Start();

        // Act & Assert - Should not throw
        _inputManager.Start();

        // Cleanup
        _inputManager.Stop();
    }

    [Fact]
    public void Stop_WhenNotRunning_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _inputManager.Stop();
    }

    [Fact]
    public void FlushBuffer_WhenEmpty_ShouldReturnEmptyArray()
    {
        // Act
        var events = _inputManager.FlushBuffer();

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public void ClearBuffer_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _inputManager.ClearBuffer();
    }

    [Fact]
    public void GetMousePosition_Initially_ShouldReturnNull()
    {
        // Act
        var position = _inputManager.GetMousePosition();

        // Assert
        Assert.Null(position);
    }

    [Fact]
    public void EnableMouseInput_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _inputManager.EnableMouseInput();

        // Cleanup
        _inputManager.DisableMouseInput();
    }

    [Fact]
    public void DisableMouseInput_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _inputManager.DisableMouseInput();
    }

    [Fact]
    public void Poll_ShouldNotThrow()
    {
        // Act & Assert - Should not throw
        _inputManager.Poll();
    }

    [Fact]
    public void InputReceived_EventCanBeSubscribed()
    {
        // Arrange
        var eventFired = false;
        _inputManager.InputReceived += (sender, e) => eventFired = true;

        // Act - We can't easily trigger a real input event in tests, but we can verify subscription works

        // Assert - Just verify the event handler was added without exception
        Assert.False(eventFired); // No events should have been fired yet
    }

    public void Dispose()
    {
        _inputManager?.Dispose();
    }
}