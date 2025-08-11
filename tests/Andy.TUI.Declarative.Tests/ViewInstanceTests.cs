using System;
using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;

namespace Andy.TUI.Declarative.Tests;

public class ViewInstanceTests
{
    [Fact]
    public void ViewInstanceManager_CreatesInstancesForComponents()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var text = new Text("Hello");
        var button = new Button("Click", () => { });

        // Act
        var textInstance = manager.GetOrCreateInstance(text, "text1");
        var buttonInstance = manager.GetOrCreateInstance(button, "button1");

        // Assert
        Assert.IsType<TextInstance>(textInstance);
        Assert.IsType<ButtonInstance>(buttonInstance);
        Assert.Equal("text1:Text", textInstance.Id);
        Assert.Equal("button1:Button", buttonInstance.Id);
    }

    [Fact]
    public void ViewInstanceManager_ReusesExistingInstances()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var text = new Text("Hello");

        // Act
        var instance1 = manager.GetOrCreateInstance(text, "text1");
        var instance2 = manager.GetOrCreateInstance(text, "text1");

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void TextFieldInstance_HandlesKeyboardInput()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var testData = new TestData { Value = "" };
        var binding = testData.Bind(() => testData.Value);
        var textField = new TextField("Enter text", binding);
        var instance = manager.GetOrCreateInstance(textField, "field1") as TextFieldInstance;
        Assert.NotNull(instance);

        // Act - Type "Hello"
        instance.HandleKeyPress(new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false));

        // Assert
        Assert.Equal("Hello", testData.Value);
    }

    [Fact]
    public void TextFieldInstance_HandlesBackspace()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var testData = new TestData { Value = "Hello" };
        var binding = testData.Bind(() => testData.Value);
        var textField = new TextField("Enter text", binding);
        var instance = manager.GetOrCreateInstance(textField, "field1") as TextFieldInstance;
        Assert.NotNull(instance);

        // Move cursor to end
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false));

        // Act - Delete last character
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Backspace, false, false, false));

        // Assert
        Assert.Equal("Hell", testData.Value);
    }

    private class TestData
    {
        public string Value { get; set; } = "";
    }

    [Fact]
    public void VStackInstance_CreatesChildInstances()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var testData = new TestData();
        var vstack = new VStack(spacing: 1) {
            new Text("Title"),
            new Button("Click", () => { }),
            new TextField("Input", testData.Bind(() => testData.Value))
        };

        // Act
        var instance = manager.GetOrCreateInstance(vstack, "stack1") as VStackInstance;
        Assert.NotNull(instance);

        // Assert
        var children = instance.GetChildInstances();
        Assert.Equal(3, children.Count);
        Assert.IsType<TextInstance>(children[0]);
        Assert.IsType<ButtonInstance>(children[1]);
        Assert.IsType<TextFieldInstance>(children[2]);
    }

    [Fact]
    public void HStackInstance_PositionsChildrenHorizontally()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var testData = new TestData();
        var hstack = new HStack(spacing: 2) {
            new Text("Label:"),
            new TextField("Input", testData.Bind(() => testData.Value))
        };

        // Act
        var instance = manager.GetOrCreateInstance(hstack, "hstack1") as HStackInstance;
        Assert.NotNull(instance);
        var rendered = instance.Render();

        // Assert
        Assert.IsType<FragmentNode>(rendered);
        var fragment = (FragmentNode)rendered;

        // Check that children are positioned with horizontal spacing
        var children = fragment.Children.ToArray();
        Assert.Equal(2, children.Length);
    }

    [Fact]
    public void ViewInstance_UpdatesWhenStateChanges()
    {
        // Arrange
        var renderCount = 0;
        var context = new DeclarativeContext(() => renderCount++);
        var manager = context.ViewInstanceManager;

        var testData = new TestData { Value = "initial" };
        var binding = testData.Bind(() => testData.Value);
        var textField = new TextField("Enter text", binding);

        // Act
        var instance = manager.GetOrCreateInstance(textField, "field1");
        renderCount = 0; // Reset after initial creation
        binding.Value = "updated";

        // Assert
        Assert.Equal(1, renderCount); // Should request render on change
        Assert.Equal("updated", testData.Value);
    }
}