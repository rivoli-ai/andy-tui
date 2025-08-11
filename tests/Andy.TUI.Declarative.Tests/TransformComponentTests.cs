using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class TransformComponentTests
{
    [Fact]
    public void Transform_AppliesUppercaseCorrectly()
    {
        // Arrange
        var text = "hello world";

        // Act
        var result = Transform.ApplyTransform(text, TextTransform.Uppercase);

        // Assert
        Assert.Equal("HELLO WORLD", result);
    }

    [Fact]
    public void Transform_AppliesLowercaseCorrectly()
    {
        // Arrange
        var text = "HELLO WORLD";

        // Act
        var result = Transform.ApplyTransform(text, TextTransform.Lowercase);

        // Assert
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void Transform_AppliesCapitalizeCorrectly()
    {
        // Arrange
        var text = "hello world from transform";

        // Act
        var result = Transform.ApplyTransform(text, TextTransform.Capitalize);

        // Assert
        Assert.Equal("Hello World From Transform", result);
    }

    [Fact]
    public void Transform_AppliesCapitalizeFirstCorrectly()
    {
        // Arrange
        var text = "hello WORLD";

        // Act
        var result = Transform.ApplyTransform(text, TextTransform.CapitalizeFirst);

        // Assert
        Assert.Equal("Hello world", result);
    }

    [Fact]
    public void Transform_NoneTransformLeavesTextUnchanged()
    {
        // Arrange
        var text = "Hello World 123!";

        // Act
        var result = Transform.ApplyTransform(text, TextTransform.None);

        // Assert
        Assert.Equal("Hello World 123!", result);
    }

    [Fact]
    public void Transform_CreatesInstanceCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var transform = new Transform("test text").Uppercase().Color(Color.Red);

        // Act
        var instance = manager.GetOrCreateInstance(transform, "t1") as TransformInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.True(instance.Layout.Width > 0);
        Assert.Equal(1, instance.Layout.Height);
    }

    [Fact]
    public void Transform_FluentAPIWorks()
    {
        // Arrange
        var transform = new Transform("test")
            .Uppercase()
            .Bold()
            .Color(Color.Blue);

        // Assert
        Assert.NotNull(transform);
        Assert.IsType<Transform>(transform);
    }

    [Fact]
    public void Transform_HandlesEmptyString()
    {
        // Arrange & Act
        var uppercase = Transform.ApplyTransform("", TextTransform.Uppercase);
        var capitalize = Transform.ApplyTransform("", TextTransform.Capitalize);

        // Assert
        Assert.Equal("", uppercase);
        Assert.Equal("", capitalize);
    }
}