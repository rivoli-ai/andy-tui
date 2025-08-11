using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;

namespace Andy.TUI.Declarative.Tests;

public class NewlineComponentTests
{
    [Fact]
    public void Newline_DefaultsToSingleLine()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var newline = new Newline();

        // Act
        var instance = manager.GetOrCreateInstance(newline, "nl1") as NewlineInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.Equal(0, instance.Layout.Width); // No width
        Assert.Equal(1, instance.Layout.Height); // Single line height
    }

    [Fact]
    public void Newline_SupportsMultipleLines()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var newline = new Newline(3);

        // Act
        var instance = manager.GetOrCreateInstance(newline, "nl1") as NewlineInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.Equal(0, instance.Layout.Width); // No width
        Assert.Equal(3, instance.Layout.Height); // Three lines height
    }

    [Fact]
    public void Newline_ImplicitConversionFromInt()
    {
        // Arrange
        Newline newline = 5; // Implicit conversion

        // Assert
        Assert.NotNull(newline);
        // Conversion should work without exceptions
        Assert.IsType<Newline>(newline);
    }

    [Fact]
    public void Newline_ClampsMinimuValueToOne()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var newline = new Newline(0); // Should be clamped to 1

        // Act
        var instance = manager.GetOrCreateInstance(newline, "nl1") as NewlineInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.Equal(1, instance.Layout.Height); // Clamped to minimum of 1
    }
}