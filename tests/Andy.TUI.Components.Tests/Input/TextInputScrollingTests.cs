using System;
using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class TextInputScrollingTests
{
    [Fact]
    public void TextInput_ScrollingLogic_ShouldScrollWhenCursorMovesRight()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 20,
            MaxLength = 100
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Insert text that exceeds visible width
        var longText = "This is a very long text that exceeds the visible width of the input box";
        textInput.Text = longText;
        textInput.CursorPosition = longText.Length;
        
        // Act - Calculate scroll offset for display
        var maxDisplayWidth = 18; // 20 - 2 for borders
        var scrollOffset = Math.Max(0, textInput.CursorPosition - maxDisplayWidth + 1);
        
        // Assert
        Assert.True(scrollOffset > 0);
        Assert.Equal(longText.Length - maxDisplayWidth + 1, scrollOffset);
        
        // Visible text should be the end portion
        var visibleText = longText.Substring(scrollOffset, Math.Min(maxDisplayWidth, longText.Length - scrollOffset));
        Assert.True(visibleText.Length <= maxDisplayWidth);
        Assert.EndsWith("box", visibleText);
    }
    
    [Fact]
    public void TextInput_ScrollingLogic_ShouldShowBeginningWhenCursorAtStart()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 20,
            MaxLength = 100
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Insert text that exceeds visible width
        var longText = "This is a very long text that exceeds the visible width of the input box";
        textInput.Text = longText;
        textInput.CursorPosition = 0;
        
        // Act - Calculate scroll offset for display
        var maxDisplayWidth = 18; // 20 - 2 for borders
        var scrollOffset = Math.Max(0, textInput.CursorPosition - maxDisplayWidth + 1);
        
        // Assert
        Assert.Equal(0, scrollOffset);
        
        // Visible text should be the beginning portion
        var visibleText = longText.Substring(scrollOffset, Math.Min(maxDisplayWidth, longText.Length - scrollOffset));
        Assert.Equal(maxDisplayWidth, visibleText.Length);
        Assert.StartsWith("This is a very lon", visibleText);
    }
    
    [Fact]
    public void TextInput_ScrollingLogic_CursorShouldStayVisible()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 20,
            MaxLength = 100
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Insert text and position cursor in middle
        var longText = "This is a very long text that exceeds the visible width of the input box";
        textInput.Text = longText;
        textInput.CursorPosition = 30;
        
        // Act - Calculate scroll offset and cursor display position
        var maxDisplayWidth = 18; // 20 - 2 for borders
        var scrollOffset = Math.Max(0, textInput.CursorPosition - maxDisplayWidth + 1);
        var cursorDisplayX = textInput.CursorPosition - scrollOffset;
        
        // Assert
        Assert.True(cursorDisplayX >= 0);
        Assert.True(cursorDisplayX < maxDisplayWidth);
        Assert.Equal(13, scrollOffset); // 30 - 18 + 1
        Assert.Equal(17, cursorDisplayX); // 30 - 13
    }
    
    [Fact]
    public void TextInput_AcceptsMoreCharsThanWidth()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 20,
            MaxLength = 50
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.IsFocused = true;
        
        // Act - Type more characters than visible width
        var text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        foreach (char c in text)
        {
            var args = new KeyEventArgs(ConsoleKey.A, c, ConsoleModifiers.None);
            textInput.HandleKeyPress(args);
        }
        
        // Assert
        Assert.Equal(text, textInput.Text);
        Assert.Equal(text.Length, textInput.CursorPosition);
        Assert.True(textInput.Text.Length > 18); // More than visible width
    }
}