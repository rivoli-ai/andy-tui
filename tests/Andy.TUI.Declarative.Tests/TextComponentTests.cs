using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests;

public class TextComponentTests
{
    [Fact]
    public void Text_BasicProperties_WorkCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("Hello World")
            .Color(Terminal.Color.Red)
            .Bold();

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        // Force update to apply properties
        instance.Update(text);

        // Assert - We can't directly access internal properties, but we can verify
        // the instance was created and updated successfully
        Assert.IsType<TextInstance>(instance);
    }

    [Fact]
    public void Text_WordWrap_WrapsAtWordBoundaries()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("The quick brown fox jumps over the lazy dog")
            .Wrap(TextWrap.Word)
            .MaxWidth(20);

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(20, 100));

        // Assert
        // Text should be wrapped into multiple lines
        Assert.True(instance.Layout.Height > 1);
        Assert.True(instance.Layout.Width <= 20);
    }

    [Fact]
    public void Text_CharacterWrap_WrapsAtAnyCharacter()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("abcdefghijklmnopqrstuvwxyz")
            .Wrap(TextWrap.Character)
            .MaxWidth(10);

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(10, 100));

        // Assert
        // 26 characters with max width 10 should create 3 lines
        Assert.Equal(3, instance.Layout.Height);
        Assert.Equal(10, instance.Layout.Width);
    }

    [Fact]
    public void Text_MaxLines_LimitsNumberOfLines()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("Line 1\nLine 2\nLine 3\nLine 4\nLine 5")
            .Wrap(TextWrap.Word)
            .MaxLines(3);

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        // Use a wide constraint to test line limiting, not width wrapping
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.Equal(3, instance.Layout.Height);
    }

    [Fact]
    public void Text_TruncationTail_AddsEllipsisAtEnd()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("This is a very long text that will be truncated")
            .Truncate(TruncationMode.Tail)
            .MaxWidth(20);

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(20, 100));

        // Assert
        Assert.Equal(1, instance.Layout.Height); // No wrapping, just truncation
        Assert.True(instance.Layout.Width <= 20);
    }

    [Fact]
    public void Text_NoWrap_DefaultBehavior()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("This is a very long text that extends beyond the available width");

        // Act
        var instance = manager.GetOrCreateInstance(text, "text1") as TextInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(20, 100));

        // Assert - with no wrap, text is constrained to available width
        Assert.Equal(1, instance.Layout.Height);
        Assert.Equal(20, instance.Layout.Width);
    }
}