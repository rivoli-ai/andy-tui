using System.Linq;
using Andy.TUI.Components.Input;
using Andy.TUI.Core;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Input;

public class SelectHighlightingTests
{
    [Fact]
    public void Select_HighlightedIndex_ShouldBeAccessible()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Item 1"),
            new SelectItem<string>("Item 2"),
            new SelectItem<string>("Item 3")
        };
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        
        // Act & Assert - Initially not open, highlighted index should be -1
        Assert.Equal(-1, select.HighlightedIndex);
        
        // Open the dropdown
        select.IsOpen = true;
        
        // Should highlight the first item (or selected item if any)
        Assert.Equal(0, select.HighlightedIndex);
    }
    
    [Fact]
    public void Select_HighlightedIndex_ShouldStartAtSelectedItem()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Item 1"),
            new SelectItem<string>("Item 2"),
            new SelectItem<string>("Item 3")
        };
        select.SelectedIndex = 1; // Select second item
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        
        // Act
        select.IsOpen = true;
        
        // Assert - Should highlight the selected item
        Assert.Equal(1, select.HighlightedIndex);
    }
    
    [Fact]
    public void Select_NavigatingDown_ShouldUpdateHighlightedIndex()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Item 1"),
            new SelectItem<string>("Item 2"),
            new SelectItem<string>("Item 3")
        };
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        select.IsFocused = true;
        select.IsOpen = true;
        
        // Act - Navigate down
        var args = new KeyEventArgs(ConsoleKey.DownArrow, '\0', ConsoleModifiers.None);
        select.HandleKeyPress(args);
        
        // Assert
        Assert.Equal(1, select.HighlightedIndex);
        
        // Navigate down again
        select.HandleKeyPress(args);
        Assert.Equal(2, select.HighlightedIndex);
    }
    
    [Fact]
    public void Select_NavigatingUp_ShouldUpdateHighlightedIndex()
    {
        // Arrange
        var select = new Select<string>();
        select.Items = new[]
        {
            new SelectItem<string>("Item 1"),
            new SelectItem<string>("Item 2"),
            new SelectItem<string>("Item 3")
        };
        
        var context = TestHelpers.CreateMockContext(select);
        select.Initialize(context);
        select.IsFocused = true;
        select.IsOpen = true;
        
        // Navigate to last item first
        var downArgs = new KeyEventArgs(ConsoleKey.DownArrow, '\0', ConsoleModifiers.None);
        select.HandleKeyPress(downArgs);
        select.HandleKeyPress(downArgs);
        
        // Act - Navigate up
        var upArgs = new KeyEventArgs(ConsoleKey.UpArrow, '\0', ConsoleModifiers.None);
        select.HandleKeyPress(upArgs);
        
        // Assert
        Assert.Equal(1, select.HighlightedIndex);
    }
}