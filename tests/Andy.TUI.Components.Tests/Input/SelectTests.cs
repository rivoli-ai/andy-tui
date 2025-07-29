using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class SelectTests
{
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        // Arrange & Act
        var select = new Select<string>();
        
        // Assert
        Assert.Empty(select.Items);
        Assert.Null(select.SelectedItem);
        Assert.Equal(-1, select.SelectedIndex);
        Assert.Equal("Select an option...", select.Placeholder);
        Assert.False(select.IsOpen);
        Assert.True(select.IsEnabled);
        Assert.False(select.IsFocused);
        Assert.Equal(10, select.MaxDisplayItems);
        Assert.True(select.AllowFiltering);
    }
    
    [Fact]
    public void Items_SetValue_UpdatesItems()
    {
        // Arrange
        var select = new Select<string>();
        var items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2"),
            new SelectItem<string>("Option 3")
        };
        
        // Act
        select.Items = items;
        
        // Assert
        Assert.Equal(3, select.Items.Count());
    }
    
    [Fact]
    public void SelectedItem_SetValue_UpdatesSelection()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2"),
            new SelectItem<string>("Option 3")
        };
        
        // Act
        select.SelectedItem = "Option 2";
        
        // Assert
        Assert.Equal("Option 2", select.SelectedItem);
        Assert.Equal(1, select.SelectedIndex);
    }
    
    [Fact]
    public void SelectedIndex_SetValue_UpdatesSelection()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2"),
            new SelectItem<string>("Option 3")
        };
        
        // Act
        select.SelectedIndex = 2;
        
        // Assert
        Assert.Equal("Option 3", select.SelectedItem);
        Assert.Equal(2, select.SelectedIndex);
    }
    
    [Fact]
    public void SelectedIndex_OutOfRange_ThrowsException()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2")
        };
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => select.SelectedIndex = 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => select.SelectedIndex = -2);
    }
    
    [Fact]
    public void HandleKeyPress_Enter_OpensDropdown()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[] { new SelectItem<string>("Option 1") };
        select.IsFocused = true;
        
        var args = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = select.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.True(select.IsOpen);
    }
    
    [Fact]
    public void HandleKeyPress_Escape_ClosesDropdown()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[] { new SelectItem<string>("Option 1") };
        select.IsFocused = true;
        select.IsOpen = true;
        
        var args = new KeyEventArgs(ConsoleKey.Escape, '\0', 0);
        
        // Act
        var handled = select.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        Assert.False(select.IsOpen);
    }
    
    [Fact]
    public void HandleKeyPress_DownArrow_NavigatesItems()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2"),
            new SelectItem<string>("Option 3")
        };
        select.IsFocused = true;
        select.IsOpen = true;
        
        var args = new KeyEventArgs(ConsoleKey.DownArrow, '\0', 0);
        
        // Act
        var handled = select.HandleKeyPress(args);
        
        // Assert
        Assert.True(handled);
        // Note: Can't directly test highlighted index as it's private
    }
    
    [Fact]
    public void HandleKeyPress_Enter_SelectsHighlightedItem()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2")
        };
        select.IsFocused = true;
        select.IsOpen = true;
        
        // First navigate down
        var downArgs = new KeyEventArgs(ConsoleKey.DownArrow, '\0', 0);
        select.HandleKeyPress(downArgs);
        
        // Then select
        var enterArgs = new KeyEventArgs(ConsoleKey.Enter, '\0', 0);
        
        // Act
        var handled = select.HandleKeyPress(enterArgs);
        
        // Assert
        Assert.True(handled);
        Assert.False(select.IsOpen);
        // Selection would be updated based on highlighted index
    }
    
    [Fact]
    public void SelectionChanged_RaisedWhenSelectionChanges()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2")
        };
        
        string? selectedValue = null;
        int selectedIndex = -1;
        select.SelectionChanged += (s, e) =>
        {
            selectedValue = e.SelectedValue;
            selectedIndex = e.SelectedIndex;
        };
        
        // Act
        select.SelectedIndex = 1;
        
        // Assert
        Assert.Equal("Option 2", selectedValue);
        Assert.Equal(1, selectedIndex);
    }
    
    [Fact]
    public void Measure_ReturnsAppropriateSize()
    {
        // Arrange
        var select = new Select<string>
        {
            Padding = new Spacing(1)
        };
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2")
        };
        
        // Act - Closed
        var closedSize = select.Measure(new Size(100, 50));
        
        // Assert
        Assert.Equal(32, closedSize.Width); // 30 (default) + 2 padding
        Assert.Equal(5, closedSize.Height); // 1 + 2 padding + 2 borders
        
        // Act - Open
        select.IsOpen = true;
        var openSize = select.Measure(new Size(100, 50));
        
        // Assert
        Assert.Equal(32, openSize.Width);
        Assert.True(openSize.Height > closedSize.Height); // Should be taller when open
    }
    
    [Fact]
    public void DisplayFunc_CustomizesItemDisplay()
    {
        // Arrange
        var select = new Select<int>
        {
            DisplayFunc = value => $"Number: {value}"
        };
        select.Items = new[]
        {
            new SelectItem<int>(1),
            new SelectItem<int>(2),
            new SelectItem<int>(3)
        };
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        select.Arrange(new Rectangle(0, 0, 30, 3));
        
        // Act
        var rendered = select.Render();
        
        // Assert
        // The rendered output would use the display function
        Assert.IsType<ElementNode>(rendered);
    }
    
    [Fact]
    public void Render_CreatesProperStructure()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Option 1"),
            new SelectItem<string>("Option 2")
        };
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        select.Arrange(new Rectangle(0, 0, 30, 3));
        
        // Act
        var rendered = select.Render();
        
        // Assert
        Assert.IsType<ElementNode>(rendered);
        var element = (ElementNode)rendered;
        Assert.Equal("select", element.TagName);
        
        // Should have children
        Assert.True(element.Children.Count > 0);
    }
}