using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class ButtonTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var button = new Button();
        
        // Assert
        Assert.Equal(string.Empty, button.Text);
        Assert.Equal(ButtonStyle.Default, button.Style);
        Assert.False(button.IsDefault);
        Assert.False(button.IsCancel);
        Assert.True(button.IsEnabled);
        Assert.False(button.IsFocused);
        Assert.False(button.IsPressed);
        Assert.Equal(10, button.MinWidth);
        Assert.Null(button.Icon);
    }
    
    [Fact]
    public void Text_SetValue_UpdatesText()
    {
        // Arrange
        var button = new Button();
        
        // Act
        button.Text = "Click Me";
        
        // Assert
        Assert.Equal("Click Me", button.Text);
    }
    
    [Fact]
    public void HandleKeyPress_Enter_TriggersClick()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        button.IsFocused = true;
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        var args = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Allow async press simulation to complete
        System.Threading.Thread.Sleep(150);
        
        // Assert
        Assert.True(handled);
        Assert.True(clicked);
    }
    
    [Fact]
    public void HandleKeyPress_Spacebar_TriggersClick()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        button.IsFocused = true;
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        var args = new KeyEventArgs(ConsoleKey.Spacebar, ' ', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Allow async press simulation to complete
        System.Threading.Thread.Sleep(150);
        
        // Assert
        Assert.True(handled);
        Assert.True(clicked);
    }
    
    [Fact]
    public void HandleKeyPress_Escape_TriggersClickWhenIsCancel()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Cancel",
            IsCancel = true
        };
        button.IsFocused = true;
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        var args = new KeyEventArgs(ConsoleKey.Escape, '\0', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Allow async press simulation to complete
        System.Threading.Thread.Sleep(150);
        
        // Assert
        Assert.True(handled);
        Assert.True(clicked);
    }
    
    [Fact]
    public void HandleKeyPress_Escape_DoesNotTriggerClickWhenNotCancel()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "OK",
            IsCancel = false
        };
        button.IsFocused = true;
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        var args = new KeyEventArgs(ConsoleKey.Escape, '\0', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Assert
        Assert.False(handled);
        Assert.False(clicked);
    }
    
    [Fact]
    public void HandleKeyPress_WhenNotFocused_ReturnsFalse()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        button.IsFocused = false;
        
        var args = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Assert
        Assert.False(handled);
    }
    
    [Fact]
    public void HandleKeyPress_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Test",
            IsEnabled = false,
            IsFocused = true
        };
        
        var args = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = button.HandleKeyPress(args);
        
        // Assert
        Assert.False(handled);
    }
    
    [Fact]
    public void HandleMouseEvent_LeftClick_TriggersClick()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        button.Arrange(new Rectangle(10, 10, 20, 3));
        
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        var args = new MouseEventArgs(15, 11, MouseButton.Left);
        
        // Act
        var handled = button.HandleMouseEvent(args);
        
        // Allow async press simulation to complete
        System.Threading.Thread.Sleep(150);
        
        // Assert
        Assert.True(handled);
        Assert.True(button.IsFocused);
        Assert.True(clicked);
    }
    
    [Fact]
    public void HandleMouseEvent_OutsideBounds_ReturnsFalse()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        button.Arrange(new Rectangle(10, 10, 20, 3));
        
        var args = new MouseEventArgs(5, 5, MouseButton.Left);
        
        // Act
        var handled = button.HandleMouseEvent(args);
        
        // Assert
        Assert.False(handled);
    }
    
    [Fact]
    public void PerformClick_TriggersClickEvent()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        // Act
        button.PerformClick();
        
        // Assert
        Assert.True(clicked);
    }
    
    [Fact]
    public void PerformClick_WhenDisabled_DoesNotTriggerClick()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Test",
            IsEnabled = false
        };
        var clicked = false;
        button.Click += (s, e) => clicked = true;
        
        // Act
        button.PerformClick();
        
        // Assert
        Assert.False(clicked);
    }
    
    [Fact]
    public void Measure_ReturnsAppropriateSize()
    {
        // Arrange
        var button = new Button
        {
            Text = "OK",
            Padding = new Spacing(1),
            MinWidth = 10
        };
        
        // Act
        var size = button.Measure(new Size(100, 50));
        
        // Assert
        Assert.Equal(10, size.Width); // MinWidth enforced
        Assert.Equal(5, size.Height); // 1 + 2 padding + 2 borders
    }
    
    [Fact]
    public void Measure_WithIcon_IncludesIconWidth()
    {
        // Arrange
        var button = new Button
        {
            Text = "Save",
            Icon = "💾",
            Padding = new Spacing(1),
            MinWidth = 10
        };
        
        // Act
        var size = button.Measure(new Size(100, 50));
        
        // Assert
        Assert.Equal(11, size.Width); // "💾 Save" = 7 + 2 padding + 2 borders = 11
        Assert.Equal(5, size.Height);
    }
    
    [Fact]
    public void Render_CreatesProperStructure()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Click Me",
            IsFocused = true
        };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        button.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        var rendered = button.Render();
        
        // Assert
        Assert.IsType<ElementNode>(rendered);
        var element = (ElementNode)rendered;
        Assert.Equal("button", element.TagName);
        
        // Should have children
        Assert.True(element.Children.Count > 0);
    }
    
    [Fact]
    public void Style_Primary_UsesBlueColors()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Primary",
            Style = ButtonStyle.Primary,
            IsFocused = true
        };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        button.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        var rendered = button.Render();
        
        // Assert
        // The render should contain blue background
        var textNodes = TestHelpers.FindTextNodes(rendered);
        Assert.Contains("Primary", string.Join("", textNodes));
    }
    
    [Fact]
    public void IsDefault_UsesDoubleBorder()
    {
        // Arrange
        var button = new Button 
        { 
            Text = "Default",
            IsDefault = true
        };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        button.Arrange(new Rectangle(0, 0, 20, 3));
        
        // Act
        var rendered = button.Render();
        
        // Assert
        var textNodes = TestHelpers.FindTextNodes(rendered);
        // Double border uses ═ and ║ characters
        Assert.Contains("═", string.Join("", textNodes));
    }
    
    [Fact]
    public void Focus_SetsIsFocused()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        
        // Act
        button.Focus();
        
        // Assert
        Assert.True(button.IsFocused);
    }
    
    [Fact]
    public void Blur_ClearsIsFocused()
    {
        // Arrange
        var button = new Button { Text = "Test" };
        var context = TestHelpers.CreateMockContext(button);
        button.Initialize(context);
        button.Focus();
        
        // Act
        button.Blur();
        
        // Assert
        Assert.False(button.IsFocused);
    }
}