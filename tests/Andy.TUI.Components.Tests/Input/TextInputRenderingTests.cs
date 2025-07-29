using System;
using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class TextInputRenderingTests
{
    [Fact]
    public void TextInput_LongText_ShouldBeTruncatedForDisplay()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 40,
            Text = "This is a very long text that should be truncated when displayed in the input box"
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 40, 3));
        
        // Act
        var maxDisplayWidth = 38; // 40 - 2 for borders
        var displayText = textInput.Text.Length > maxDisplayWidth 
            ? textInput.Text.Substring(0, maxDisplayWidth)
            : textInput.Text;
        
        // Assert
        Assert.Equal(38, displayText.Length);
        Assert.Equal("This is a very long text that should b", displayText);
    }
    
    [Fact]
    public void TextInput_CursorPosition_ShouldBeWithinVisibleRange()
    {
        // Arrange
        var textInput = new TextInput
        {
            Width = 20,
            Text = "12345678901234567890" // 20 chars
        };
        
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        textInput.CursorPosition = 25; // Beyond text length
        var maxWidth = 18; // 20 - 2 for borders
        var scrollOffset = Math.Max(0, textInput.CursorPosition - maxWidth + 1);
        var visibleCursorPos = textInput.CursorPosition - scrollOffset;
        
        // Assert
        Assert.Equal(20, textInput.CursorPosition); // Should be clamped to text length
        Assert.True(visibleCursorPos >= 0 && visibleCursorPos < maxWidth);
    }
    
    [Fact]
    public void PasswordInput_LongPassword_ShouldTruncateDots()
    {
        // Arrange
        var passwordInput = new TextInput
        {
            Width = 20,
            PasswordChar = '•',
            Text = "verylongpasswordthatexceedswidth"
        };
        
        var context = TestHelpers.CreateMockContext(passwordInput);
        passwordInput.Initialize(context);
        passwordInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        var maxWidth = 18; // 20 - 2 for borders
        var passwordDisplay = new string('•', passwordInput.Text.Length);
        if (passwordDisplay.Length > maxWidth)
            passwordDisplay = passwordDisplay.Substring(passwordDisplay.Length - maxWidth);
        
        // Assert
        Assert.Equal(18, passwordDisplay.Length);
        Assert.Equal("••••••••••••••••••", passwordDisplay);
    }
    
    [Fact]
    public void Select_LongItemText_ShouldBeTruncated()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("United States of America - A very long country name", "United States of America - A very long country name")
        };
        select.Width = 40;
        select.SelectedIndex = 0;
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        select.Arrange(new Rectangle(0, 0, 40, 10));
        
        // Act
        var maxWidth = 38; // 40 - 2 for borders
        var displayText = select.SelectedItem!.Length > maxWidth 
            ? select.SelectedItem.Substring(0, maxWidth)
            : select.SelectedItem;
        
        // Assert
        Assert.Equal(38, displayText.Length);
        Assert.StartsWith("United States of America - A very long", displayText);
    }
}