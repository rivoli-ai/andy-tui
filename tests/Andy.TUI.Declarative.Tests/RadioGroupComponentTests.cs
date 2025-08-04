using System;
using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class RadioGroupComponentTests
{
    [Fact]
    public void RadioGroup_CreatesSuccessfully()
    {
        // Arrange
        var options = new[] { "Option 1", "Option 2", "Option 3" };
        var binding = new Binding<Optional<string>>(
            () => Optional<string>.None,
            value => { }
        );

        // Act
        var radioGroup = new RadioGroup<string>("Label", options, binding);

        // Assert
        Assert.NotNull(radioGroup);
    }

    [Fact]
    public void RadioGroup_CreatesWithAllParameters()
    {
        // Arrange
        var options = new[] { "A", "B" };
        var binding = new Binding<Optional<string>>(
            () => Optional<string>.None,
            value => { }
        );

        // Act
        var radioGroup = new RadioGroup<string>(
            "Label", 
            options, 
            binding,
            optionRenderer: item => $"Item: {item}",
            selectedMark: "(✓)",
            unselectedMark: "( )",
            vertical: false
        );

        // Assert
        Assert.NotNull(radioGroup);
    }

    [Fact]
    public void RadioGroupInstance_HandlesKeyboardNavigation()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedValue = Optional<string>.Some("Option 2");
        var options = new[] { "Option 1", "Option 2", "Option 3" };
        var binding = new Binding<Optional<string>>(
            () => selectedValue,
            value => selectedValue = value
        );
        var radioGroup = new RadioGroup<string>("Test", options, binding);
        
        // Act
        var instance = manager.GetOrCreateInstance(radioGroup, "radio1") as RadioGroupInstance<string>;
        Assert.NotNull(instance);
        instance.OnGotFocus();

        // Test keyboard navigation - just verify keys are handled
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false)));
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)));
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)));
        
        // Test Home/End
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false)));
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false)));
    }

    [Fact]
    public void RadioGroupInstance_SupportsHorizontalLayout()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var options = new[] { "Small", "Medium", "Large", "X-Large" };
        var binding = new Binding<Optional<string>>(
            () => Optional<string>.None,
            value => { }
        );
        var radioGroup = new RadioGroup<string>("Size", options, binding, vertical: false);
        
        // Act
        var instance = manager.GetOrCreateInstance(radioGroup, "radio1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<RadioGroupInstance<string>>(instance);
    }

    [Fact]
    public void RadioGroup_WithComplexTypes()
    {
        // Arrange
        var countries = new[]
        {
            new Country { Code = "US", Name = "United States", Flag = "🇺🇸" },
            new Country { Code = "UK", Name = "United Kingdom", Flag = "🇬🇧" },
            new Country { Code = "JP", Name = "Japan", Flag = "🇯🇵" }
        };

        var selectedCountry = Optional<Country>.None;
        var binding = new Binding<Optional<Country>>(
            () => selectedCountry,
            value => selectedCountry = value
        );

        // Act
        var radioGroup = new RadioGroup<Country>(
            "Select Country",
            countries,
            binding,
            optionRenderer: c => $"{c.Flag} {c.Name}"
        );

        // Assert
        Assert.NotNull(radioGroup);
    }

    [Fact]
    public void RadioGroupInstance_FocusManagement()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var options = new[] { "Yes", "No" };
        var binding = new Binding<Optional<string>>(
            () => Optional<string>.None,
            value => { }
        );
        var radioGroup = new RadioGroup<string>("Answer", options, binding);
        
        // Act
        var instance = manager.GetOrCreateInstance(radioGroup, "radio1") as RadioGroupInstance<string>;
        Assert.NotNull(instance);

        // Assert
        Assert.True(instance.CanFocus);
        Assert.False(instance.IsFocused);
        
        instance.OnGotFocus();
        Assert.True(instance.IsFocused);
        
        instance.OnLostFocus();
        Assert.False(instance.IsFocused);
    }

    private class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Flag { get; set; } = "";
    }
}