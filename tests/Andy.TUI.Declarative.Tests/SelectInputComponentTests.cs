using System;
using System.Collections.Generic;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests;

public class SelectInputComponentTests
{
    [Fact]
    public void SelectInput_CreatesInstanceWithCorrectProperties()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<string>.None;
        var binding = new Binding<Optional<string>>(() => selectedItem, v => selectedItem = v);
        var items = new[] { "Item 1", "Item 2", "Item 3" };
        
        var selectInput = new SelectInput<string>(items, binding)
            .VisibleItems(3)
            .Placeholder("Choose...");
        
        // Act
        var instance = manager.GetOrCreateInstance(selectInput, "select1") as SelectInputInstance<string>;
        Assert.NotNull(instance);
        
        // Assert
        Assert.IsType<SelectInputInstance<string>>(instance);
        Assert.True(instance.CanFocus);
    }
    
    [Fact]
    public void SelectInput_HandlesCustomItemRenderer()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedPerson = Optional<Person>.None;
        var binding = new Binding<Optional<Person>>(() => selectedPerson, v => selectedPerson = v);
        
        var people = new[]
        {
            new Person { Id = 1, Name = "Alice" },
            new Person { Id = 2, Name = "Bob" }
        };
        
        var selectInput = new SelectInput<Person>(
            people,
            binding,
            person => $"#{person.Id}: {person.Name}"
        );
        
        // Act
        var instance = manager.GetOrCreateInstance(selectInput, "select1") as SelectInputInstance<Person>;
        Assert.NotNull(instance);
        
        instance.CalculateLayout(LayoutConstraints.Loose(50, 20));
        
        // Assert - the layout should accommodate the custom renderer
        Assert.True(instance.Layout.Width > 10); // Should have reasonable width
    }
    
    [Fact]
    public void SelectInput_HandlesFocusCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<int>.None;
        var binding = new Binding<Optional<int>>(() => selectedItem, v => selectedItem = v);
        var items = new[] { 1, 2, 3, 4, 5 };
        
        var selectInput = new SelectInput<int>(items, binding);
        var instance = manager.GetOrCreateInstance(selectInput, "select1") as SelectInputInstance<int>;
        Assert.NotNull(instance);
        
        // Act & Assert
        Assert.False(instance.IsFocused);
        
        instance.OnGotFocus();
        Assert.True(instance.IsFocused);
        
        instance.OnLostFocus();
        Assert.False(instance.IsFocused);
    }
    
    [Fact]
    public void SelectInput_HandlesKeyboardNavigation()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<string>.None;
        var binding = new Binding<Optional<string>>(() => selectedItem, v => selectedItem = v);
        var items = new[] { "A", "B", "C", "D", "E" };
        
        var selectInput = new SelectInput<string>(items, binding);
        var instance = manager.GetOrCreateInstance(selectInput, "select1") as SelectInputInstance<string>;
        Assert.NotNull(instance);
        
        instance.Update(selectInput);
        instance.OnGotFocus();
        
        // Act - Navigate down
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        Assert.True(handled);
        
        // Select item
        handled = instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        Assert.True(handled);
        
        // Assert
        Assert.True(binding.Value.TryGetValue(out var selected));
        Assert.Equal("B", selected); // Should have selected second item
    }
    
    [Fact]
    public void SelectInput_BindingUpdatesWork()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<string>.Some("Initial");
        var binding = new Binding<Optional<string>>(() => selectedItem, v => selectedItem = v);
        var items = new[] { "Initial", "Updated", "Final" };
        
        var selectInput = new SelectInput<string>(items, binding);
        var instance = manager.GetOrCreateInstance(selectInput, "select1") as SelectInputInstance<string>;
        Assert.NotNull(instance);
        
        instance.Update(selectInput);
        
        // Act - Update binding value
        binding.Value = Optional<string>.Some("Updated");
        
        // Assert
        Assert.True(selectedItem.TryGetValue(out var value));
        Assert.Equal("Updated", value);
        Assert.True(binding.Value.TryGetValue(out var bindingValue));
        Assert.Equal("Updated", bindingValue);
    }
    
    // Test data class
    private class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}