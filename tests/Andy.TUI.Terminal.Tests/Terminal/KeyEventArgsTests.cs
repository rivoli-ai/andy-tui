using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class KeyEventArgsTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var args = new KeyEventArgs(ConsoleKey.A, 'a', ConsoleModifiers.Control);

        Assert.Equal(ConsoleKey.A, args.Key);
        Assert.Equal('a', args.KeyChar);
        Assert.Equal(ConsoleModifiers.Control, args.Modifiers);
        Assert.False(args.Handled);
    }

    [Fact]
    public void Constructor_WithConsoleKeyInfo_SetsPropertiesCorrectly()
    {
        var keyInfo = new ConsoleKeyInfo('X', ConsoleKey.X, shift: true, alt: false, control: false);
        var args = new KeyEventArgs(keyInfo);

        Assert.Equal(ConsoleKey.X, args.Key);
        Assert.Equal('X', args.KeyChar);
        Assert.Equal(ConsoleModifiers.Shift, args.Modifiers);
    }

    [Fact]
    public void ModifierProperties_ReturnCorrectValues()
    {
        var args1 = new KeyEventArgs(ConsoleKey.A, 'a', ConsoleModifiers.Shift);
        Assert.True(args1.Shift);
        Assert.False(args1.Alt);
        Assert.False(args1.Control);

        var args2 = new KeyEventArgs(ConsoleKey.B, 'b', ConsoleModifiers.Alt);
        Assert.False(args2.Shift);
        Assert.True(args2.Alt);
        Assert.False(args2.Control);

        var args3 = new KeyEventArgs(ConsoleKey.C, 'c', ConsoleModifiers.Control);
        Assert.False(args3.Shift);
        Assert.False(args3.Alt);
        Assert.True(args3.Control);

        var args4 = new KeyEventArgs(ConsoleKey.D, 'd',
            ConsoleModifiers.Shift | ConsoleModifiers.Alt | ConsoleModifiers.Control);
        Assert.True(args4.Shift);
        Assert.True(args4.Alt);
        Assert.True(args4.Control);
    }

    [Fact]
    public void Handled_CanBeSetAndRetrieved()
    {
        var args = new KeyEventArgs(ConsoleKey.Enter, '\r', 0);

        Assert.False(args.Handled);

        args.Handled = true;

        Assert.True(args.Handled);
    }
}