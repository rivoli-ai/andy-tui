using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.ViewInstances;

namespace Andy.TUI.Declarative.Tests;

public class CheckboxComponentTests
{
    [Fact]
    public void Checkbox_CreatesInstanceCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        var checkbox = new Checkbox("Accept terms", binding);
        
        // Act
        var instance = manager.GetOrCreateInstance(checkbox, "c1");
        
        // Assert
        Assert.NotNull(instance);
        Assert.IsType<CheckboxInstance>(instance);
    }
    
    [Fact]
    public void Checkbox_TogglesValue()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        var checkbox = new Checkbox("Enable notifications", binding);
        
        var instance = manager.GetOrCreateInstance(checkbox, "c1") as CheckboxInstance;
        Assert.NotNull(instance);
        
        // Act - simulate focus and toggle
        instance.OnGotFocus();
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert
        Assert.True(isChecked);
        Assert.True(binding.Value);
        
        // Act - toggle again
        instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        
        // Assert
        Assert.False(isChecked);
        Assert.False(binding.Value);
    }
    
    [Fact]
    public void Checkbox_HandlesEnterKey()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        var checkbox = new Checkbox("Confirm", binding);
        
        var instance = manager.GetOrCreateInstance(checkbox, "c1") as CheckboxInstance;
        Assert.NotNull(instance);
        
        // Act
        instance.OnGotFocus();
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        // Assert
        Assert.True(handled);
        Assert.True(isChecked);
    }
    
    [Fact]
    public void Checkbox_CustomMarks()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        var checkbox = new Checkbox(
            "Custom marks",
            binding,
            checkedMark: "[✓]",
            uncheckedMark: "[•]"
        );
        
        // Act
        var instance = manager.GetOrCreateInstance(checkbox, "c1");
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Assert
        Assert.NotNull(instance);
        Assert.True(instance.Layout.Width > 0);
        Assert.Equal(1, instance.Layout.Height);
    }
    
    [Fact]
    public void Checkbox_LabelPosition()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        
        // Test label first (default)
        var checkbox1 = new Checkbox("Label first", binding, labelFirst: true);
        var instance1 = manager.GetOrCreateInstance(checkbox1, "c1");
        instance1.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Test mark first
        var checkbox2 = new Checkbox("Mark first", binding, labelFirst: false);
        var instance2 = manager.GetOrCreateInstance(checkbox2, "c2");
        instance2.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Assert
        Assert.NotNull(instance1);
        Assert.NotNull(instance2);
        Assert.True(instance1.Layout.Width > 0);
        Assert.True(instance2.Layout.Width > 0);
    }
    
    [Fact]
    public void Checkbox_FocusHandling()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isChecked = false;
        var binding = new Binding<bool>(() => isChecked, v => isChecked = v);
        var checkbox = new Checkbox("Focus test", binding);
        
        var instance = manager.GetOrCreateInstance(checkbox, "c1") as CheckboxInstance;
        Assert.NotNull(instance);
        
        // Assert initial state
        Assert.False(instance.IsFocused);
        Assert.True(instance.CanFocus);
        
        // Act - focus
        instance.OnGotFocus();
        
        // Assert
        Assert.True(instance.IsFocused);
        
        // Act - blur
        instance.OnLostFocus();
        
        // Assert
        Assert.False(instance.IsFocused);
    }
}