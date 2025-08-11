using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.ViewInstances;

namespace Andy.TUI.Declarative.Tests;

public class MultiSelectInputComponentTests
{
    [Fact]
    public void MultiSelectInput_CreatesInstanceCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { "Item 1", "Item 2", "Item 3" };
        var selectedSet = new HashSet<string>();
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(items, selectedItems);
        
        // Act
        var instance = manager.GetOrCreateInstance(multiSelect, "m1");
        
        // Assert
        Assert.NotNull(instance);
        Assert.IsType<MultiSelectInputInstance<string>>(instance);
    }
    
    [Fact]
    public void MultiSelectInput_HandlesSelection()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { "Apple", "Banana", "Cherry" };
        var selectedSet = new HashSet<string>();
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(items, selectedItems);
        
        var instance = manager.GetOrCreateInstance(multiSelect, "m1") as MultiSelectInputInstance<string>;
        Assert.NotNull(instance);
        
        // Act - simulate focus and selection
        instance.OnGotFocus();
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert
        Assert.Contains("Apple", selectedItems.Value);
        
        // Act - select another item
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert
        Assert.Contains("Apple", selectedItems.Value);
        Assert.Contains("Banana", selectedItems.Value);
        Assert.Equal(2, selectedItems.Value.Count);
    }
    
    [Fact]
    public void MultiSelectInput_HandlesDeselection()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { "Red", "Green", "Blue" };
        var selectedSet = new HashSet<string> { "Red", "Blue" };
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(items, selectedItems);
        
        var instance = manager.GetOrCreateInstance(multiSelect, "m1") as MultiSelectInputInstance<string>;
        Assert.NotNull(instance);
        
        // Act - deselect first item
        instance.OnGotFocus();
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert
        Assert.DoesNotContain("Red", selectedItems.Value);
        Assert.Contains("Blue", selectedItems.Value);
        Assert.Single(selectedItems.Value);
    }
    
    [Fact]
    public void MultiSelectInput_HandlesKeyboardNavigation()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = Enumerable.Range(1, 10).Select(i => $"Item {i}").ToList();
        var selectedSet = new HashSet<string>();
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(items, selectedItems);
        
        var instance = manager.GetOrCreateInstance(multiSelect, "m1") as MultiSelectInputInstance<string>;
        Assert.NotNull(instance);
        instance.OnGotFocus();
        
        // Act & Assert - test various navigation keys
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false)));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Contains("Item 10", selectedItems.Value);
        
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false)));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Contains("Item 1", selectedItems.Value);
        
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.PageDown, false, false, false)));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Contains("Item 6", selectedItems.Value);
    }
    
    [Fact]
    public void MultiSelectInput_CustomRenderer()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { 1, 2, 3 };
        var selectedSet = new HashSet<int>();
        var selectedItems = new Binding<ISet<int>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<int>(
            items, 
            selectedItems,
            item => $"Number: {item}"
        );
        
        // Act
        var instance = manager.GetOrCreateInstance(multiSelect, "m1");
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Assert
        Assert.NotNull(instance);
        Assert.True(instance.Layout.Width > 0);
        Assert.Equal(3, instance.Layout.Height);
    }
    
    [Fact]
    public void MultiSelectInput_EmptyList()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = Array.Empty<string>();
        var selectedSet = new HashSet<string>();
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(items, selectedItems);
        
        // Act
        var instance = manager.GetOrCreateInstance(multiSelect, "m1");
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Assert
        Assert.NotNull(instance);
        Assert.Equal(1, instance.Layout.Height); // Shows "(No items)"
    }
    
    [Fact]
    public void MultiSelectInput_CustomMarks()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { "A", "B", "C" };
        var selectedSet = new HashSet<string>();
        var selectedItems = new Binding<ISet<string>>(() => selectedSet, v => { selectedSet.Clear(); foreach(var item in v) selectedSet.Add(item); });
        var multiSelect = new MultiSelectInput<string>(
            items, 
            selectedItems,
            checkedMark: "[✓]",
            uncheckedMark: "[•]"
        );
        
        // Act
        var instance = manager.GetOrCreateInstance(multiSelect, "m1");
        
        // Assert
        Assert.NotNull(instance);
        Assert.IsType<MultiSelectInputInstance<string>>(instance);
    }
}