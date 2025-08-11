using System;
using System.Linq;
using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class IntegrationTests
{
    [Fact]
    public void InputExample_ViewInstanceTreeCreatedCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var app = new TestInputApp();
        var ui = app.CreateUI();

        // Act - Create view instances
        var rootInstance = manager.GetOrCreateInstance(ui, "root");

        // Assert - Check the view instance tree structure
        Assert.IsType<VStackInstance>(rootInstance);
        var vstack = (VStackInstance)rootInstance;
        var children = vstack.GetChildInstances();

        // Should have title, spacers, form rows, buttons
        Assert.True(children.Count > 5);

        // First child should be the title text
        Assert.IsType<TextInstance>(children[0]);

        // Find HStack instances (form rows)
        var hstacks = children.OfType<HStackInstance>().ToList();
        Assert.True(hstacks.Count >= 3); // Name, Pass, Country rows at minimum

        // Check that the country HStack contains a dropdown
        var countryHStack = hstacks[2]; // Third HStack should be country
        var countryChildren = countryHStack.GetChildInstances();
        Assert.Equal(2, countryChildren.Count); // Text label and Dropdown
        Assert.IsType<TextInstance>(countryChildren[0]);
        Assert.IsType<DropdownInstance<string>>(countryChildren[1]);
    }

    [Fact]
    public void InputExample_KeyboardInputUpdatesBinding()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var app = new TestInputApp();
        var ui = app.CreateUI();

        // Create view instances
        var rootInstance = manager.GetOrCreateInstance(ui, "root");

        // Find the name TextField instance
        var nameFieldInstance = FindTextFieldInstance(rootInstance, "Enter your name...");
        Assert.NotNull(nameFieldInstance);

        // Act - Type "John"
        context.FocusManager.SetFocus(nameFieldInstance);
        nameFieldInstance.HandleKeyPress(new ConsoleKeyInfo('J', ConsoleKey.J, false, false, false));
        nameFieldInstance.HandleKeyPress(new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false));
        nameFieldInstance.HandleKeyPress(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));
        nameFieldInstance.HandleKeyPress(new ConsoleKeyInfo('n', ConsoleKey.N, false, false, false));

        // Assert
        Assert.Equal("John", app.Name);
    }

    [Fact]
    public void InputExample_TabNavigationWorks()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var app = new TestInputApp();
        var ui = app.CreateUI();

        // Create view instances and register focusables
        var rootInstance = manager.GetOrCreateInstance(ui, "root");
        RegisterFocusables(context, rootInstance);

        // Find the TextField instances
        var nameField = FindTextFieldInstance(rootInstance, "Enter your name...");
        var passField = FindTextFieldInstance(rootInstance, "Enter password...");
        Assert.NotNull(nameField);
        Assert.NotNull(passField);

        // Act - Set initial focus and tab
        context.FocusManager.SetFocus(nameField);
        context.EventRouter.RouteKeyPress(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));

        // Assert - Focus should move to password field
        Assert.False(nameField.IsFocused);
        Assert.True(passField.IsFocused);
    }

    [Fact]
    public void InputExample_RenderingProducesValidDOM()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;

        var app = new TestInputApp();
        var ui = app.CreateUI();

        // Act
        var rootInstance = manager.GetOrCreateInstance(ui, "root");
        var virtualDom = rootInstance.Render();

        // Assert
        Assert.IsType<FragmentNode>(virtualDom);
        var fragment = (FragmentNode)virtualDom;

        // Check that elements are properly positioned
        var elements = CollectElements(fragment).ToList();
        Assert.True(elements.Count > 0);

        // Verify text content
        var textNodes = CollectTextNodes(fragment).ToList();
        var textContent = string.Join(" ", textNodes.Select(t => t.Content));

        Assert.Contains("🚀 Andy.TUI Input Components Demo", textContent);
        Assert.Contains("Name:", textContent);
        Assert.Contains("Enter your name...", textContent); // Placeholder might not have brackets in text nodes
    }

    private void RegisterFocusables(DeclarativeContext context, ViewInstance instance)
    {
        if (instance is IFocusable focusable)
        {
            context.FocusManager.RegisterFocusable(focusable);
        }

        if (instance is VStackInstance vstack)
        {
            foreach (var child in vstack.GetChildInstances())
            {
                RegisterFocusables(context, child);
            }
        }
        else if (instance is HStackInstance hstack)
        {
            foreach (var child in hstack.GetChildInstances())
            {
                RegisterFocusables(context, child);
            }
        }
    }

    private static IEnumerable<ElementNode> CollectElements(VirtualNode node)
    {
        if (node is ElementNode element)
        {
            yield return element;
            foreach (var child in element.Children)
            {
                foreach (var childElement in CollectElements(child))
                {
                    yield return childElement;
                }
            }
        }
        else if (node is FragmentNode fragment)
        {
            foreach (var child in fragment.Children)
            {
                foreach (var childElement in CollectElements(child))
                {
                    yield return childElement;
                }
            }
        }
    }

    private static IEnumerable<TextNode> CollectTextNodes(VirtualNode node)
    {
        if (node is TextNode text)
        {
            yield return text;
        }
        else if (node is ElementNode element)
        {
            foreach (var child in element.Children)
            {
                foreach (var textNode in CollectTextNodes(child))
                {
                    yield return textNode;
                }
            }
        }
        else if (node is FragmentNode fragment)
        {
            foreach (var child in fragment.Children)
            {
                foreach (var textNode in CollectTextNodes(child))
                {
                    yield return textNode;
                }
            }
        }
    }

    private TextFieldInstance? FindTextFieldInstance(ViewInstance instance, string placeholder)
    {
        if (instance is TextFieldInstance textField)
        {
            // We need to check the placeholder property
            var node = textField.Render();
            if (node is ElementNode element && element.Children.FirstOrDefault() is TextNode text)
            {
                if (text.Content.Contains(placeholder))
                    return textField;
            }
        }

        if (instance is VStackInstance vstack)
        {
            foreach (var child in vstack.GetChildInstances())
            {
                var found = FindTextFieldInstance(child, placeholder);
                if (found != null) return found;
            }
        }
        else if (instance is HStackInstance hstack)
        {
            foreach (var child in hstack.GetChildInstances())
            {
                var found = FindTextFieldInstance(child, placeholder);
                if (found != null) return found;
            }
        }

        return null;
    }

    private class TestInputApp
    {
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
        public string Country { get; set; } = "";

        private readonly string[] countries = { "USA", "Canada", "UK" };

        public ISimpleComponent CreateUI()
        {
            return new VStack(spacing: 1) {
                new Text("🚀 Andy.TUI Input Components Demo").Title().Color(Color.Cyan),
                " ",
                new HStack(spacing: 2) {
                    new Text("  Name:").Bold().Color(Color.White),
                    new TextField("Enter your name...", this.Bind(() => Name))
                },
                new HStack(spacing: 2) {
                    new Text("  Pass:").Bold().Color(Color.White),
                    new TextField("Enter password...", this.Bind(() => Password)).Secure()
                },
                new HStack(spacing: 2) {
                    new Text("Country:").Bold().Color(Color.White),
                    new Dropdown<string>("Select a country...", countries, this.Bind(() => Country))
                        .Color(Color.White)
                        .PlaceholderColor(Color.Gray)
                },
                " ",
                new HStack(spacing: 3) {
                    new Button("Submit", () => { }).Primary(),
                    new Button("Cancel", () => { }).Secondary()
                }
            };
        }
    }
}