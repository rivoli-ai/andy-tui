using System;
using System.Linq;
using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class DropdownTests
{
    [Fact]
    public void Dropdown_CreatesCorrectInstance()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var countries = new[] { "USA", "Canada", "UK" };
        var testData = new TestData { Country = "" };
        var binding = testData.Bind(() => testData.Country);
        var dropdown = new Dropdown<string>("Select a country", countries, binding);
        
        // Act
        var instance = manager.GetOrCreateInstance(dropdown, "dropdown1");
        
        // Assert
        Assert.IsType<DropdownInstance<string>>(instance);
    }
    
    [Fact]
    public void Dropdown_RendersPlaceholderWhenNoSelection()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var countries = new[] { "USA", "Canada", "UK" };
        var testData = new TestData { Country = "" };
        var binding = testData.Bind(() => testData.Country);
        var dropdown = new Dropdown<string>("Select a country", countries, binding);
        
        // Act
        var instance = manager.GetOrCreateInstance(dropdown, "dropdown1");
        var rendered = instance.Render();
        
        // Assert
        Assert.IsType<FragmentNode>(rendered);
        var fragment = (FragmentNode)rendered;
        var textNodes = CollectTextNodes(fragment).ToList();
        
        Assert.Contains("Select a country", string.Join(" ", textNodes.Select(t => t.Content)));
    }
    
    [Fact]
    public void Dropdown_KeyboardNavigation_OpensOnEnter()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var countries = new[] { "USA", "Canada", "UK" };
        var testData = new TestData { Country = "" };
        var binding = testData.Bind(() => testData.Country);
        var dropdown = new Dropdown<string>("Select a country", countries, binding);
        
        var instance = manager.GetOrCreateInstance(dropdown, "dropdown1") as DropdownInstance<string>;
        Assert.NotNull(instance);
        
        // Act - Focus and press Enter
        context.FocusManager.RegisterFocusable(instance);
        context.FocusManager.SetFocus(instance);
        instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        // Assert - Dropdown should show items
        var rendered = instance.Render();
        var textContent = string.Join(" ", CollectTextNodes(rendered).Select(t => t.Content));
        
        Assert.Contains("USA", textContent);
        Assert.Contains("Canada", textContent);
        Assert.Contains("UK", textContent);
    }
    
    [Fact]
    public void Dropdown_SelectsItemWithKeyboard()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var countries = new[] { "USA", "Canada", "UK" };
        var testData = new TestData { Country = "" };
        var binding = testData.Bind(() => testData.Country);
        var dropdown = new Dropdown<string>("Select a country", countries, binding);
        
        var instance = manager.GetOrCreateInstance(dropdown, "dropdown1") as DropdownInstance<string>;
        Assert.NotNull(instance);
        
        context.FocusManager.RegisterFocusable(instance);
        context.FocusManager.SetFocus(instance);
        
        // Act - Open dropdown, navigate down, and select
        instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        
        // Assert
        Assert.Equal("Canada", testData.Country);
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
    
    private class TestData
    {
        public string Country { get; set; } = "";
    }
}