using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class IInputManagerTests
{
    [Fact]
    public void IInputManager_InterfaceProperties_ShouldExist()
    {
        // This test verifies the interface contract exists
        var interfaceType = typeof(IInputManager);
        
        // Assert required properties exist
        Assert.NotNull(interfaceType.GetProperty("SupportsMouseInput"));
        Assert.NotNull(interfaceType.GetProperty("IsRunning"));
        Assert.NotNull(interfaceType.GetProperty("BufferingEnabled"));
        Assert.NotNull(interfaceType.GetProperty("BufferSize"));
    }
    
    [Fact]
    public void IInputManager_InterfaceMethods_ShouldExist()
    {
        // This test verifies the interface contract methods exist
        var interfaceType = typeof(IInputManager);
        
        // Assert required methods exist
        Assert.NotNull(interfaceType.GetMethod("Start"));
        Assert.NotNull(interfaceType.GetMethod("Stop"));
        Assert.NotNull(interfaceType.GetMethod("Poll"));
        Assert.NotNull(interfaceType.GetMethod("EnableMouseInput"));
        Assert.NotNull(interfaceType.GetMethod("DisableMouseInput"));
        Assert.NotNull(interfaceType.GetMethod("FlushBuffer"));
        Assert.NotNull(interfaceType.GetMethod("ClearBuffer"));
        Assert.NotNull(interfaceType.GetMethod("GetMousePosition"));
    }
    
    [Fact]
    public void IInputManager_InputReceivedEvent_ShouldExist()
    {
        // This test verifies the interface contract events exist
        var interfaceType = typeof(IInputManager);
        
        // Assert required event exists
        Assert.NotNull(interfaceType.GetEvent("InputReceived"));
    }
    
    [Fact]
    public void IInputManager_InheritsFromIDisposable()
    {
        // Assert the interface extends IDisposable
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(IInputManager)));
    }
    
    [Fact]
    public void CrossPlatformInputManager_ImplementsIInputManager()
    {
        // Assert the implementation implements the interface
        Assert.True(typeof(IInputManager).IsAssignableFrom(typeof(CrossPlatformInputManager)));
    }
}