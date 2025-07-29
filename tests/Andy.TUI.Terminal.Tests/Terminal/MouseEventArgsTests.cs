using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class MouseEventArgsTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var args = new MouseEventArgs(10, 20, MouseButton.Left, ConsoleModifiers.Control);
        
        Assert.Equal(10, args.X);
        Assert.Equal(20, args.Y);
        Assert.Equal(MouseButton.Left, args.Button);
        Assert.Equal(ConsoleModifiers.Control, args.Modifiers);
        Assert.False(args.Handled);
    }
    
    [Fact]
    public void Constructor_WithDefaultModifiers_Works()
    {
        var args = new MouseEventArgs(5, 15, MouseButton.Right);
        
        Assert.Equal(5, args.X);
        Assert.Equal(15, args.Y);
        Assert.Equal(MouseButton.Right, args.Button);
        Assert.Equal((ConsoleModifiers)0, args.Modifiers);
    }
    
    [Fact]
    public void Handled_CanBeSetAndRetrieved()
    {
        var args = new MouseEventArgs(0, 0, MouseButton.Middle);
        
        Assert.False(args.Handled);
        
        args.Handled = true;
        
        Assert.True(args.Handled);
    }
}

public class MouseWheelEventArgsTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var args = new MouseWheelEventArgs(30, 40, -1, ConsoleModifiers.Shift);
        
        Assert.Equal(30, args.X);
        Assert.Equal(40, args.Y);
        Assert.Equal(-1, args.Delta);
        Assert.Equal(MouseButton.None, args.Button);
        Assert.Equal(ConsoleModifiers.Shift, args.Modifiers);
    }
    
    [Fact]
    public void Constructor_InheritsFromMouseEventArgs()
    {
        var args = new MouseWheelEventArgs(10, 20, 1);
        
        // Should inherit MouseEventArgs properties
        Assert.IsAssignableFrom<MouseEventArgs>(args);
        Assert.Equal(10, args.X);
        Assert.Equal(20, args.Y);
        Assert.Equal(MouseButton.None, args.Button);
    }
    
    [Fact]
    public void Delta_PositiveForScrollUp()
    {
        var args = new MouseWheelEventArgs(0, 0, 1);
        
        Assert.True(args.Delta > 0);
    }
    
    [Fact]
    public void Delta_NegativeForScrollDown()
    {
        var args = new MouseWheelEventArgs(0, 0, -1);
        
        Assert.True(args.Delta < 0);
    }
}