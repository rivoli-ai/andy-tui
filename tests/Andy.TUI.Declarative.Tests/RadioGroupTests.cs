using System;
using System.Linq;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Extensions;
using Xunit;

namespace Andy.TUI.Declarative.Tests;

public class RadioGroupTests
{
    private class TestHost
    {
        public Optional<string> SelectedValue { get; set; } = Optional<string>.None;
    }

    [Fact]
    public void RadioGroup_InitialState_ShouldHaveNoSelection()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "A", "B", "C" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");

        // Act
        instance.Update(radioGroup);

        // Assert
        Assert.False(binding.Value.HasValue);
        Assert.Equal("None", binding.Value.ToString());
        Assert.True(instance.CanFocus);
    }

    [Fact]
    public void RadioGroup_SpacebarSelection_ShouldUpdateBinding()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "A", "B", "C" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");

        instance.Update(radioGroup);
        instance.OnGotFocus();

        // Act - Select first option with spacebar
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        // Assert
        Assert.True(handled);
        Assert.True(binding.Value.HasValue);
        Assert.Equal("A", binding.Value.Value);
        Assert.Equal("A", host.SelectedValue.Value);
        Assert.Equal("A", binding.Value.ToString());
    }

    [Fact]
    public void RadioGroup_ArrowNavigation_AfterSelection_ShouldStillWork()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "A", "B", "C" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");

        instance.Update(radioGroup);
        instance.OnGotFocus();

        // Act - Select first option, then navigate to second, then select
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Equal("A", binding.Value.Value); // First selection worked
        
        // Update should be called after binding changes (simulating PropertyChanged event)
        instance.Update(radioGroup);
        
        // Try to navigate down after selection - this is where the bug was
        var downHandled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        
        // Select the new option
        var spaceHandled = instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        // Assert
        Assert.True(downHandled, "Down arrow should be handled after initial selection");
        Assert.True(spaceHandled, "Spacebar should work for second selection");
        Assert.True(binding.Value.HasValue);
        Assert.Equal("B", binding.Value.Value);
        Assert.Equal("B", host.SelectedValue.Value);
    }

    [Fact]
    public void RadioGroup_MultipleSelections_ShouldUpdateCorrectly()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "First", "Second", "Third" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");

        instance.Update(radioGroup);
        instance.OnGotFocus();

        // Act - Navigate and select multiple times
        // Select first option
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Equal("First", binding.Value.Value);

        // Navigate to second option
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Equal("Second", binding.Value.Value);

        // Navigate to third option
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert final state
        Assert.Equal("Third", binding.Value.Value);
        Assert.Equal("Third", host.SelectedValue.Value);
    }

    [Fact]
    public void RadioGroup_BindingPropertyChangedEvents_ShouldTriggerUpdates()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "A", "B", "C" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");
        // Hook into InvalidateView calls (this would be done by the rendering system)
        instance.Update(radioGroup);

        // Act - Set binding value programmatically
        binding.Value = Optional<string>.Some("B");

        // The binding should have notified about the change
        // We can verify the instance reflects the new state
        instance.Update(radioGroup); // This would be called by the renderer when PropertyChanged fires

        // Assert
        // After update, the instance should reflect the bound value
        Assert.True(binding.Value.HasValue);
        Assert.Equal("B", binding.Value.Value);
        Assert.Equal("B", host.SelectedValue.Value);
    }

    [Fact]
    public void RadioGroup_VerticalNavigation_ShouldWorkInBothDirections()
    {
        // Arrange
        var host = new TestHost();
        var options = new[] { "Top", "Middle", "Bottom" };
        var binding = host.Bind(() => host.SelectedValue);
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        var instance = new RadioGroupInstance<string>("test");

        instance.Update(radioGroup);
        instance.OnGotFocus();

        // Act - Navigate down then up
        // Start at index 0, move down to index 1
        var down1 = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Equal("Middle", binding.Value.Value);

        // Move down to index 2
        var down2 = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        Assert.Equal("Bottom", binding.Value.Value);

        // Move back up to index 1
        var up1 = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

        // Assert
        Assert.True(down1);
        Assert.True(down2);
        Assert.True(up1);
        Assert.Equal("Middle", binding.Value.Value);
        Assert.Equal("Middle", host.SelectedValue.Value);
    }

    [Fact]
    public void RadioGroup_OptionalToString_ShouldDisplayCorrectly()
    {
        // Arrange & Act
        var none = Optional<string>.None;
        var some = Optional<string>.Some("Test");

        // Assert
        Assert.Equal("None", none.ToString());
        Assert.Equal("Test", some.ToString());
    }
}