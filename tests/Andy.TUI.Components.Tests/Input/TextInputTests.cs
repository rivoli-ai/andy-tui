using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class TextInputTests
{
    [Fact]
    public void Constructor_InitializesWithEmptyText()
    {
        // Arrange & Act
        var textInput = new TextInput();
        
        // Assert
        Assert.Equal(string.Empty, textInput.Text);
        Assert.Equal(0, textInput.CursorPosition);
        Assert.False(textInput.HasSelection);
        Assert.True(textInput.IsEnabled);
        Assert.False(textInput.IsReadOnly);
        Assert.False(textInput.IsFocused);
    }
    
    [Fact]
    public void Text_SetValue_UpdatesTextAndCursorPosition()
    {
        // Arrange
        var textInput = new TextInput();
        textInput.Text = "Hello World";
        textInput.CursorPosition = 5;
        
        // Act
        textInput.Text = "Hello";
        
        // Assert
        Assert.Equal("Hello", textInput.Text);
        Assert.Equal(5, textInput.CursorPosition); // Cursor clamps to text length
    }
    
    [Fact]
    public void Text_SetValue_ClearsPreviousSelection()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello World" };
        textInput.SelectAll();
        Assert.True(textInput.HasSelection);
        
        // Act
        textInput.Text = "New Text";
        
        // Assert
        Assert.False(textInput.HasSelection);
    }
    
    [Fact]
    public void CursorPosition_ClampsToBounds()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        
        // Act & Assert
        textInput.CursorPosition = -5;
        Assert.Equal(0, textInput.CursorPosition);
        
        textInput.CursorPosition = 10;
        Assert.Equal(5, textInput.CursorPosition);
    }
    
    [Fact]
    public void InsertText_InsertsAtCursor()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 2;
        
        // Act
        textInput.InsertText("ABC");
        
        // Assert
        Assert.Equal("HeABCllo", textInput.Text);
        Assert.Equal(5, textInput.CursorPosition);
    }
    
    [Fact]
    public void InsertText_RespectsMaxLength()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "Hello",
            MaxLength = 8
        };
        textInput.CursorPosition = 5;
        
        // Act
        textInput.InsertText("World!");
        
        // Assert
        Assert.Equal("HelloWor", textInput.Text);
        Assert.Equal(8, textInput.CursorPosition);
    }
    
    [Fact]
    public void InsertText_IgnoresWhenReadOnly()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "Hello",
            IsReadOnly = true
        };
        
        // Act
        textInput.InsertText("World");
        
        // Assert
        Assert.Equal("Hello", textInput.Text);
    }
    
    [Fact]
    public void InsertText_ReplacesSelection()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello World" };
        textInput.SelectAll();
        
        // Act
        textInput.InsertText("Goodbye");
        
        // Assert
        Assert.Equal("Goodbye", textInput.Text);
        Assert.Equal(7, textInput.CursorPosition);
        Assert.False(textInput.HasSelection);
    }
    
    [Fact]
    public void DeleteBackward_RemovesPreviousCharacter()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 3;
        
        // Act
        textInput.DeleteBackward();
        
        // Assert
        Assert.Equal("Helo", textInput.Text);
        Assert.Equal(2, textInput.CursorPosition);
    }
    
    [Fact]
    public void DeleteBackward_RemovesSelection()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello World" };
        textInput.CursorPosition = 5;
        textInput.SelectAll();
        
        // Act
        textInput.DeleteBackward();
        
        // Assert
        Assert.Equal(string.Empty, textInput.Text);
        Assert.Equal(0, textInput.CursorPosition);
    }
    
    [Fact]
    public void DeleteForward_RemovesNextCharacter()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 2;
        
        // Act
        textInput.DeleteForward();
        
        // Assert
        Assert.Equal("Helo", textInput.Text);
        Assert.Equal(2, textInput.CursorPosition);
    }
    
    [Fact]
    public void SelectAll_SelectsEntireText()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello World" };
        
        // Act
        textInput.SelectAll();
        
        // Assert
        Assert.True(textInput.HasSelection);
        Assert.Equal("Hello World", textInput.SelectedText);
        Assert.Equal(11, textInput.CursorPosition);
    }
    
    [Fact]
    public void ClearSelection_RemovesSelection()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.SelectAll();
        
        // Act
        textInput.ClearSelection();
        
        // Assert
        Assert.False(textInput.HasSelection);
        Assert.Equal(string.Empty, textInput.SelectedText);
    }
    
    [Fact]
    public void Focus_SetsIsFocused()
    {
        // Arrange
        var textInput = new TextInput();
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        
        // Act
        textInput.Focus();
        
        // Assert
        Assert.True(textInput.IsFocused);
    }
    
    [Fact]
    public void Blur_ClearsIsFocused()
    {
        // Arrange
        var textInput = new TextInput();
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Focus();
        
        // Act
        textInput.Blur();
        
        // Assert
        Assert.False(textInput.IsFocused);
    }
    
    [Fact]
    public void HandleKeyPress_LeftArrow_MovesCursorLeft()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 3;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.LeftArrow, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal(2, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_RightArrow_MovesCursorRight()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 2;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.RightArrow, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal(3, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_ShiftLeftArrow_ExtendsSelection()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 3;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.LeftArrow, '\0', ConsoleModifiers.Shift);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal(2, textInput.CursorPosition);
        Assert.True(textInput.HasSelection);
        Assert.Equal("l", textInput.SelectedText);
    }
    
    [Fact]
    public void HandleKeyPress_Home_MovesCursorToStart()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 3;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.Home, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal(0, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_End_MovesCursorToEnd()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 2;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.End, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal(5, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_Backspace_DeletesBackward()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 3;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.Backspace, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal("Helo", textInput.Text);
        Assert.Equal(2, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_Delete_DeletesForward()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 2;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.Delete, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal("Helo", textInput.Text);
        Assert.Equal(2, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_Enter_RaisesSubmittedEvent()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.IsFocused = true;
        var submitted = false;
        textInput.Submitted += (s, e) => submitted = true;
        
        var args = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.True(submitted);
    }
    
    [Fact]
    public void HandleKeyPress_CtrlA_SelectsAll()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.A, 'a', ConsoleModifiers.Control);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.True(textInput.HasSelection);
        Assert.Equal("Hello", textInput.SelectedText);
    }
    
    [Fact]
    public void HandleKeyPress_Character_InsertsText()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.CursorPosition = 5;
        textInput.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.A, 'a', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.Equal("Helloa", textInput.Text);
        Assert.Equal(6, textInput.CursorPosition);
    }
    
    [Fact]
    public void HandleKeyPress_WhenNotFocused_ReturnsFalse()
    {
        // Arrange
        var textInput = new TextInput { Text = "Hello" };
        textInput.IsFocused = false;
        
        var args = new KeyEventArgs(ConsoleKey.A, 'a', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.False(handled);
        Assert.Equal("Hello", textInput.Text);
    }
    
    [Fact]
    public void HandleKeyPress_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "Hello",
            IsEnabled = false,
            IsFocused = true
        };
        
        var args = new KeyEventArgs(ConsoleKey.A, 'a', 0);
        
        // Act
        var handled = textInput.HandleKeyPress(args);
        
        // Assert
        Assert.False(handled);
        Assert.Equal("Hello", textInput.Text);
    }
    
    [Fact]
    public void HandleMouseEvent_LeftClick_RequestsFocus()
    {
        // Arrange
        var textInput = new TextInput();
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(10, 10, 20, 3));
        
        var args = new MouseEventArgs(15, 11, MouseButton.Left);
        
        // Act
        var handled = textInput.HandleMouseEvent(args);
        
        // Assert
        Assert.True(handled);
        Assert.True(textInput.IsFocused);
    }
    
    [Fact]
    public void HandleMouseEvent_OutsideBounds_ReturnsFalse()
    {
        // Arrange
        var textInput = new TextInput();
        textInput.Arrange(new Rectangle(10, 10, 20, 3));
        
        var args = new MouseEventArgs(5, 5, MouseButton.Left);
        
        // Act
        var handled = textInput.HandleMouseEvent(args);
        
        // Assert
        Assert.False(handled);
    }
    
    [Fact]
    public void TextChanged_RaisedWhenTextChanges()
    {
        // Arrange
        var textInput = new TextInput();
        string? newText = null;
        textInput.TextChanged += (s, e) => newText = e.Text;
        
        // Act
        textInput.Text = "Hello";
        
        // Assert
        Assert.Equal("Hello", newText);
    }
    
    [Fact]
    public void IsValid_UsesValidatorFunction()
    {
        // Arrange
        var textInput = new TextInput
        {
            Validator = text => text.Length >= 5
        };
        
        // Act & Assert
        textInput.Text = "Hi";
        Assert.False(textInput.IsValid);
        
        textInput.Text = "Hello";
        Assert.True(textInput.IsValid);
    }
    
    [Fact]
    public void IsValid_TrueWhenNoValidator()
    {
        // Arrange
        var textInput = new TextInput { Text = "Any text" };
        
        // Act & Assert
        Assert.True(textInput.IsValid);
    }
    
    [Fact]
    public void PasswordChar_MasksDisplayText()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "secret",
            PasswordChar = '*'
        };
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        
        // Act
        var rendered = textInput.Render();
        
        // Assert
        // The rendered output should contain asterisks, not the actual text
        var textNodes = TestHelpers.FindTextNodes(rendered);
        Assert.DoesNotContain("secret", string.Join("", textNodes));
    }
    
    [Fact]
    public void Placeholder_ShowsWhenEmpty()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Placeholder = "Enter text...",
            IsFocused = false // Ensure not focused so placeholder shows
        };
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3)); // Need to arrange to set bounds
        
        // Act
        var rendered = textInput.Render();
        
        // Assert
        var textNodes = TestHelpers.FindTextNodes(rendered);
        Assert.Contains("Enter text...", string.Join("", textNodes));
    }
    
    [Fact]
    public void Placeholder_HiddenWhenTextExists()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "Hello",
            Placeholder = "Enter text..."
        };
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        
        // Act
        var rendered = textInput.Render();
        
        // Assert
        var textNodes = TestHelpers.FindTextNodes(rendered);
        Assert.DoesNotContain("Enter text...", string.Join("", textNodes));
    }
    
    [Fact]
    public void Measure_ReturnsAppropriateSize()
    {
        // Arrange
        var textInput = new TextInput
        {
            Padding = new Spacing(2)
        };
        
        // Act
        var size = textInput.Measure(new Size(100, 50));
        
        // Assert
        Assert.Equal(54, size.Width); // 50 (default) + 4 padding (2 left + 2 right)
        Assert.Equal(5, size.Height);  // 1 + 4 padding (2 top + 2 bottom)
    }
    
    [Fact]
    public void Render_CreatesProperStructure()
    {
        // Arrange
        var textInput = new TextInput 
        { 
            Text = "Hello",
            IsFocused = true
        };
        var context = TestHelpers.CreateMockContext(textInput);
        textInput.Initialize(context);
        textInput.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        var rendered = textInput.Render();
        
        // Assert
        Assert.IsType<ElementNode>(rendered);
        var element = (ElementNode)rendered;
        Assert.Equal("textinput", element.TagName);
        
        // Should have background, text, and border
        Assert.True(element.Children.Count > 0);
    }
    
    [Fact]
    public void IsEnabled_DisablingClearsFocus()
    {
        // Arrange
        var textInput = new TextInput();
        textInput.IsFocused = true;
        
        // Act
        textInput.IsEnabled = false;
        
        // Assert
        Assert.False(textInput.IsFocused);
        Assert.False(textInput.IsEnabled);
    }
}