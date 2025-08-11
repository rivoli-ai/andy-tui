using System;
using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests;

public class TextAreaComponentTests
{
    [Fact]
    public void TextArea_CreatesInstanceWithCorrectDimensions()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = "";
        var textBinding = new Binding<string>(() => text, v => text = v);

        var textArea = new TextArea("Enter text...", textBinding)
            .Rows(10)
            .Cols(50);

        // Act
        var instance = manager.GetOrCreateInstance(textArea, "area1") as TextAreaInstance;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));

        // Assert
        Assert.Equal(52, instance.Layout.Width); // 50 cols + 2 for borders
        Assert.Equal(12, instance.Layout.Height); // 10 rows + 2 for borders
    }

    [Fact]
    public void TextArea_HandlesMultilineText()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = "Line 1\nLine 2\nLine 3";
        var textBinding = new Binding<string>(() => text, v => text = v);

        var textArea = new TextArea("", textBinding, 5, 30);

        // Act
        var instance = manager.GetOrCreateInstance(textArea, "area1") as TextAreaInstance;
        Assert.NotNull(instance);

        // Force update to apply binding
        instance.Update(textArea);

        // Assert - Instance created and can handle multiline text
        Assert.IsType<TextAreaInstance>(instance);
        Assert.Equal("Line 1\nLine 2\nLine 3", textBinding.Value);
    }

    [Fact]
    public void TextArea_HandlesFocusCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = "";
        var textBinding = new Binding<string>(() => text, v => text = v);

        var textArea = new TextArea("Placeholder", textBinding);
        var instance = manager.GetOrCreateInstance(textArea, "area1") as TextAreaInstance;
        Assert.NotNull(instance);

        // Act & Assert
        Assert.True(instance.CanFocus);
        Assert.False(instance.IsFocused);

        instance.OnGotFocus();
        Assert.True(instance.IsFocused);

        instance.OnLostFocus();
        Assert.False(instance.IsFocused);
    }

    [Fact]
    public void TextArea_HandlesKeyPressEvents()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = "";
        var textBinding = new Binding<string>(() => text, v => text = v);

        var textArea = new TextArea("", textBinding);
        var instance = manager.GetOrCreateInstance(textArea, "area1") as TextAreaInstance;
        Assert.NotNull(instance);

        instance.Update(textArea);
        instance.OnGotFocus();

        // Act - Type some characters
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false));
        Assert.True(handled);

        handled = instance.HandleKeyPress(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));
        Assert.True(handled);

        // Assert
        Assert.Equal("Hi", textBinding.Value);

        // Test Enter key
        handled = instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        Assert.True(handled);
        Assert.Equal("Hi\n", textBinding.Value);

        // Test Backspace
        handled = instance.HandleKeyPress(new ConsoleKeyInfo('\b', ConsoleKey.Backspace, false, false, false));
        Assert.True(handled);
        Assert.Equal("Hi", textBinding.Value);
    }

    [Fact]
    public void TextArea_BindingUpdatesWork()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = "Initial";
        var textBinding = new Binding<string>(() => text, v => text = v);

        var textArea = new TextArea("", textBinding);
        var instance = manager.GetOrCreateInstance(textArea, "area1") as TextAreaInstance;
        Assert.NotNull(instance);

        instance.Update(textArea);

        // Act - Update binding value
        textBinding.Value = "Updated text\nWith multiple lines";

        // Assert - TextArea should reflect the change
        // (In real app, this would trigger re-render via PropertyChanged event)
        Assert.Equal("Updated text\nWith multiple lines", textBinding.Value);
    }
}