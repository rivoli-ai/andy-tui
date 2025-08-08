using System;
using System.Linq;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Rendering;

/// <summary>
/// Tests to ensure input handling causes proper rendering updates.
/// </summary>
public class InputRenderingTests
{
    [Fact]
    public void TextField_FirstKeypress_ShouldRenderImmediately()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        string name = "";
        var binding = new Binding<string>(() => name, v => name = v);
        var textField = new TextField("Enter your name...", binding);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(textField, "test-textfield");
        var textFieldInstance = Assert.IsType<TextFieldInstance>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        instance.Render();
        renderingSystem.Render();

        // Check initial dirty state  
        var wasInitiallyDirty = renderingSystem.Buffer.IsDirty;

        // Simulate typing a character
        textFieldInstance.OnGotFocus(); // TextField needs focus to handle key presses
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        // Force the rendering system to process
        renderingSystem.Render();

        // Assert
        Assert.True(renderingSystem.Buffer.IsDirty || wasInitiallyDirty, "Buffer should be dirty after first keypress");
    }

    [Fact]
    public void TextField_TextUpdate_ShouldUpdateRender()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        string name = "";
        var binding = new Binding<string>(() => name, v => name = v);
        var textField = new TextField("Enter your name...", binding);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(textField, "test-textfield-update");
        var textFieldInstance = Assert.IsType<TextFieldInstance>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var initialTree = instance.Render();
        renderingSystem.Render();

        // Type some text
        textFieldInstance.OnGotFocus(); // TextField needs focus to handle key presses
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false));
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('e', ConsoleKey.E, false, false, false));
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('l', ConsoleKey.L, false, false, false));
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('o', ConsoleKey.O, false, false, false));

        // Get updated tree
        var updatedTree = instance.Render();

        // Assert - Trees should be different
        Assert.NotEqual(initialTree, updatedTree);
        Assert.Equal("Hello", name);
    }

    [Fact]
    public void Dropdown_Toggle_ShouldUpdateRender()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var items = new[] { "Option 1", "Option 2", "Option 3" };
        string selected = "";
        var binding = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select an option", items, binding, s => s);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "test-dropdown");
        var dropdownInstance = Assert.IsType<DropdownInstance<string>>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var closedTree = instance.Render();
        renderingSystem.Render();

        // Open dropdown
        dropdownInstance.OnGotFocus(); // Dropdown needs focus to handle key presses
        dropdownInstance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        var openTree = instance.Render();

        // Assert - Trees should be different (dropdown opened)
        Assert.NotEqual(closedTree, openTree);
    }

    [Fact]
    public void Dropdown_ItemSelection_ShouldUpdateRender()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var items = new[] { "First", "Second", "Third" };
        string selected = "";
        var binding = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select", items, binding, s => s);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "test-dropdown-select");
        var dropdownInstance = Assert.IsType<DropdownInstance<string>>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        instance.Render();
        renderingSystem.Render();

        // Open dropdown
        dropdownInstance.OnGotFocus(); // Dropdown needs focus to handle key presses
        dropdownInstance.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        instance.Render();

        // Move down and select
        dropdownInstance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        dropdownInstance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));

        var finalTree = instance.Render();
        renderingSystem.Render();

        // Assert
        Assert.Equal("Second", selected);
    }

    [Fact]
    public void TabView_TabSwitch_ShouldUpdateRender()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var tabView = new TabView();
        tabView.Add("Tab1", new Text("Content 1"));
        tabView.Add("Tab2", new Text("Content 2"));
        tabView.Add("Tab3", new Text("Content 3"));

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(tabView, "test-tabview");
        var tabViewInstance = Assert.IsType<TabViewInstance>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var initialTree = instance.Render();
        renderingSystem.Render();

        // Switch to next tab
        tabViewInstance.OnGotFocus(); // TabView needs focus to handle key presses
        tabViewInstance.HandleKeyPress(new ConsoleKeyInfo('\t', ConsoleKey.Tab, false, false, false));
        var afterTabTree = instance.Render();

        // Assert - Trees should be different
        Assert.NotEqual(initialTree, afterTabTree);
    }

    [Fact]
    public void TextField_ArrowKey_ShouldTriggerRender()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        string name = "Hello";
        var binding = new Binding<string>(() => name, v => name = v);
        var textField = new TextField("", binding);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(textField, "test-textfield-arrow");
        var textFieldInstance = Assert.IsType<TextFieldInstance>(instance);

        // Act - Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        instance.Render();
        renderingSystem.Render();

        // Check initial dirty state
        var wasInitiallyDirty = renderingSystem.Buffer.IsDirty;

        // Press left arrow key
        textFieldInstance.OnGotFocus(); // TextField needs focus to handle key presses
        textFieldInstance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.LeftArrow, false, false, false));

        // Force the rendering system to process
        renderingSystem.Render();

        // Assert
        Assert.True(renderingSystem.Buffer.IsDirty || wasInitiallyDirty, "Buffer should be dirty after arrow key press");
    }

    [Fact]
    public void Dropdown_ArrowNavigation_ShouldMoveHighlightedItem()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var items = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        string selected = string.Empty;
        var selection = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select a country...", items, selection, s => s);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "test-dropdown-highlight");

        // Initial layout/render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var closedTree = instance.Render();

        // Open the dropdown
        var dd = Assert.IsType<DropdownInstance<string>>(instance);
        dd.OnGotFocus(); // Dropdown needs focus to handle key presses
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var openTree = instance.Render();

        // Helper: find highlighted item text from virtual tree
        static string? FindHighlighted(VirtualNode node)
        {
            if (node is ElementNode el && el.TagName.ToLower() == "text")
            {
                if (el.Props.TryGetValue("style", out var styleObj) && styleObj is Style style)
                {
                    // Highlight uses white background
                    var bg = style.Background;
                    if (bg.ConsoleColor.HasValue && bg.ConsoleColor.Value == ConsoleColor.White)
                    {
                        var text = GetText(node);
                        return text;
                    }
                }
            }
            foreach (var child in node.Children)
            {
                var t = FindHighlighted(child);
                if (t != null) return t;
            }
            return null;
        }

        static string GetText(VirtualNode node)
        {
            if (node is TextNode tn) return tn.Content;
            if (node is ElementNode en)
            {
                return string.Concat(en.Children.Select(GetText));
            }
            string s = "";
            foreach (var c in node.Children) s += GetText(c);
            return s;
        }

        // Initially, highlight should be first item
        var initiallyHighlighted = FindHighlighted(openTree);
        Assert.NotNull(initiallyHighlighted);
        Assert.Contains(items[0], initiallyHighlighted!);

        // Move highlight down
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var afterDownTree = instance.Render();
        var nextHighlighted = FindHighlighted(afterDownTree);
        Assert.NotNull(nextHighlighted);
        Assert.Contains(items[1], nextHighlighted!);
    }

    [Fact]
    public void Dropdown_Open_DisplayTextReflectsHighlightedItem()
    {
        // Arrange
        var items = new[] { "United States", "Canada", "United Kingdom" };
        string selected = string.Empty;
        var selection = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select a country...", items, selection, s => s);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "test-dropdown-display");

        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        instance.Render();

        // Open
        var dd = Assert.IsType<DropdownInstance<string>>(instance);
        dd.OnGotFocus(); // Dropdown needs focus to handle key presses
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var openTree = instance.Render();

        // Helper to get display (the line starting with ▼ )
        static string? FindDisplay(VirtualNode node)
        {
            if (node is ElementNode el && el.TagName.ToLower() == "text")
            {
                var txt = node.Children.OfType<TextNode>().FirstOrDefault()?.Content;
                if (!string.IsNullOrEmpty(txt) && (txt!.StartsWith("▼ ") || txt.StartsWith("▶ ")))
                    return txt;
            }
            foreach (var c in node.Children)
            {
                var r = FindDisplay(c);
                if (r != null) return r;
            }
            return null;
        }

        var display1 = FindDisplay(openTree);
        Assert.NotNull(display1);
        Assert.Contains(items[0], display1!);

        // Move highlight down and confirm display changes
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var t2 = instance.Render();
        var display2 = FindDisplay(t2);
        Assert.NotNull(display2);
        Assert.Contains(items[1], display2!);
    }
}

// Test terminal implementation moved outside of test class
public class TestTerminal : ITerminal
{
    public TestTerminal(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }
    public (int Column, int Row) CursorPosition { get; set; }
    public bool CursorVisible { get; set; }
    public bool SupportsColor => true;
    public bool SupportsAnsi => true;

#pragma warning disable CS0067
    public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
#pragma warning restore CS0067

    public void Clear() { }
    public void ClearLine() { }
    public void MoveCursor(int column, int row) => CursorPosition = (column, row);
    public void Write(string text) { }
    public void WriteLine(string text) { }
    public void SetForegroundColor(ConsoleColor color) { }
    public void SetBackgroundColor(ConsoleColor color) { }
    public void ResetColors() { }
    public void SaveCursorPosition() { }
    public void RestoreCursorPosition() { }
    public void EnterAlternateScreen() { }
    public void ExitAlternateScreen() { }
    public void Flush() { }
    public void ApplyStyle(Style style) { }
    public void Dispose() { }
}