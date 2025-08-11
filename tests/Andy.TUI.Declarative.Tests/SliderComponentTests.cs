using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class SliderComponentTests
{
    [Fact]
    public void Slider_CreatesSuccessfully()
    {
        // Arrange
        var binding = new Binding<float>(() => 50f, value => { });

        // Act
        var slider = new Slider(binding);

        // Assert
        Assert.NotNull(slider);
    }

    [Fact]
    public void Slider_CreatesWithAllParameters()
    {
        // Arrange
        var binding = new Binding<float>(() => 25f, value => { });

        // Act
        var slider = new Slider(
            binding,
            minValue: -50f,
            maxValue: 50f,
            step: 5f,
            width: 30,
            orientation: SliderOrientation.Vertical,
            label: "Temperature",
            showValue: false,
            valueFormat: "F1",
            trackChar: '│',
            thumbChar: '●',
            trackColor: Color.Gray,
            thumbColor: Color.Green
        );

        // Assert
        Assert.NotNull(slider);
    }

    [Fact]
    public void SliderInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var binding = new Binding<float>(() => 50f, value => { });
        var slider = new Slider(binding);

        // Act
        var instance = manager.GetOrCreateInstance(slider, "slider1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<SliderInstance>(instance);
    }

    [Fact]
    public void SliderInstance_HandlesKeyboardInput()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var value = 50f;
        var binding = new Binding<float>(() => value, v => value = v);
        var slider = new Slider(binding, 0f, 100f, 10f);
        var instance = manager.GetOrCreateInstance(slider, "slider1") as SliderInstance;
        
        Assert.NotNull(instance);
        instance.OnGotFocus();

        // Act & Assert - Test arrow keys
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false)));
        Assert.Equal(60f, value);

        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false)));
        Assert.Equal(50f, value);

        // Test Home/End
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false)));
        Assert.Equal(0f, value);

        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false)));
        Assert.Equal(100f, value);
    }

    [Fact]
    public void SliderInstance_VerticalKeyboardInput()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var value = 50f;
        var binding = new Binding<float>(() => value, v => value = v);
        var slider = new Slider(binding, 0f, 100f, 5f, orientation: SliderOrientation.Vertical);
        var instance = manager.GetOrCreateInstance(slider, "slider1") as SliderInstance;
        
        Assert.NotNull(instance);
        instance.OnGotFocus();

        // Act & Assert - In vertical mode, up/down arrows work
        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false)));
        Assert.Equal(55f, value);

        Assert.True(instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        Assert.Equal(50f, value);
    }

    [Fact]
    public void SliderInstance_FocusManagement()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var binding = new Binding<float>(() => 50f, value => { });
        var slider = new Slider(binding);
        var instance = manager.GetOrCreateInstance(slider, "slider1") as SliderInstance;
        
        Assert.NotNull(instance);

        // Assert
        Assert.True(instance.CanFocus);
        Assert.False(instance.IsFocused);

        instance.OnGotFocus();
        Assert.True(instance.IsFocused);

        instance.OnLostFocus();
        Assert.False(instance.IsFocused);
    }

    [Fact]
    public void Slider_ShowcaseExamples()
    {
        // Test examples from FinalComponentsShowcase
        
        // Volume control
        var volumeBinding = new Binding<float>(() => 50f, v => { });
        var volumeSlider = new Slider(volumeBinding, 0f, 100f, 5f, label: "Volume", thumbColor: Color.Green);
        Assert.NotNull(volumeSlider);

        // Brightness control with custom characters
        var brightnessBinding = new Binding<float>(() => 75f, v => { });
        var brightnessSlider = new Slider(
            brightnessBinding,
            0f, 100f, 10f,
            label: "Brightness",
            orientation: SliderOrientation.Horizontal,
            trackChar: '▬',
            thumbChar: '●',
            thumbColor: Color.Yellow,
            showValue: true,
            valueFormat: "F0"
        );
        Assert.NotNull(brightnessSlider);
    }

    [Theory]
    [InlineData(-0.5f)]
    [InlineData(0f)]
    [InlineData(0.5f)]
    [InlineData(1f)]
    [InlineData(1.5f)]
    public void Slider_HandlesVariousValues(float value)
    {
        // Arrange
        var binding = new Binding<float>(() => value, v => { });

        // Act
        var slider = new Slider(binding);

        // Assert
        Assert.NotNull(slider);
    }

    [Fact]
    public void Slider_DifferentOrientations()
    {
        // Arrange
        var binding = new Binding<float>(() => 50f, v => { });

        // Test horizontal
        var horizontal = new Slider(binding, orientation: SliderOrientation.Horizontal);
        Assert.NotNull(horizontal);

        // Test vertical
        var vertical = new Slider(binding, orientation: SliderOrientation.Vertical);
        Assert.NotNull(vertical);
    }
}