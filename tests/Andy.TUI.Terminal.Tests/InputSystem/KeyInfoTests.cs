using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Terminal.Tests.InputSystem;

public class KeyInfoTests
{
    [Fact]
    public void KeyInfo_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var keyInfo = new KeyInfo(ConsoleKey.A, 'a', ConsoleModifiers.Control | ConsoleModifiers.Shift, "\x1b[1;6A", true);
        
        // Assert
        Assert.Equal(ConsoleKey.A, keyInfo.Key);
        Assert.Equal('a', keyInfo.KeyChar);
        Assert.Equal(ConsoleModifiers.Control | ConsoleModifiers.Shift, keyInfo.Modifiers);
        Assert.Equal("\x1b[1;6A", keyInfo.EscapeSequence);
        Assert.True(keyInfo.Command);
    }
    
    [Fact]
    public void KeyInfo_ConsoleKeyInfoConstructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var consoleKeyInfo = new ConsoleKeyInfo('b', ConsoleKey.B, shift: true, alt: false, control: false);
        
        // Act
        var keyInfo = new KeyInfo(consoleKeyInfo);
        
        // Assert
        Assert.Equal(ConsoleKey.B, keyInfo.Key);
        Assert.Equal('b', keyInfo.KeyChar);
        Assert.Equal(ConsoleModifiers.Shift, keyInfo.Modifiers);
        Assert.Null(keyInfo.EscapeSequence);
        Assert.False(keyInfo.Command);
    }
    
    [Fact]
    public void KeyInfo_ModifierProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.C, 'c', ConsoleModifiers.Control | ConsoleModifiers.Alt);
        
        // Act & Assert
        Assert.True(keyInfo.Control);
        Assert.True(keyInfo.Alt);
        Assert.False(keyInfo.Shift);
    }
    
    [Fact]
    public void KeyInfo_IsSpecialKey_ShouldReturnTrueWhenEscapeSequenceExists()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.UpArrow, '\0', 0, "\x1b[A");
        
        // Act & Assert
        Assert.True(keyInfo.IsSpecialKey);
    }
    
    [Fact]
    public void KeyInfo_IsSpecialKey_ShouldReturnFalseWhenNoEscapeSequence()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.A, 'a', 0);
        
        // Act & Assert
        Assert.False(keyInfo.IsSpecialKey);
    }
    
    [Fact]
    public void KeyInfo_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.A, 'a', ConsoleModifiers.Control | ConsoleModifiers.Shift);
        
        // Act
        var result = keyInfo.ToString();
        
        // Assert
        Assert.Equal("Ctrl+Alt+Shift+A", result);
    }
    
    [Fact]
    public void KeyInfo_ToString_WithNoModifiers_ShouldFormatCorrectly()
    {
        // Arrange
        var keyInfo = new KeyInfo(ConsoleKey.Enter, '\r', 0);
        
        // Act
        var result = keyInfo.ToString();
        
        // Assert
        Assert.Equal("Enter", result);
    }
}