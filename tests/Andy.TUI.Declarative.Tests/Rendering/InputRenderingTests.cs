using System;
using System.Linq;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.VirtualDom;

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

        // Initial render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 1));
        var tree = instance.Render();
        var renderer = new VirtualDomRenderer(renderingSystem);
        renderer.Render(tree);
        renderingSystem.Buffer.SwapBuffers();
        Assert.False(renderingSystem.Buffer.IsDirty);

        // Act: simulate first keypress 'A'
        var tf = Assert.IsType<TextFieldInstance>(instance);
        var keyInfo = new ConsoleKeyInfo('A', ConsoleKey.A, false, false, false);
        Assert.True(tf.HandleKeyPress(keyInfo));
        binding.Value = name; // ensure binding side-effect stays consistent

        // Re-render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 1));
        var tree2 = instance.Render();
        var patches = new DiffEngine().Diff(tree, tree2);
        renderer.ApplyPatches(patches);
        renderingSystem.Render();

        // Assert: buffer should be dirty due to the typed character
        Assert.True(renderingSystem.Buffer.IsDirty, "TextField should render on first keypress");
    }

    [Fact]
    public void Dropdown_ArrowNavigation_ShouldRepaintHighlight()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var items = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        string selected = string.Empty;
        var selection = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select a country...", items, selection)
            .DisplayText(s => s);

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "test-dropdown");

        // Initial render (closed)
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var tree = instance.Render();
        var renderer = new VirtualDomRenderer(renderingSystem);
        renderer.Render(tree);
        renderingSystem.Buffer.SwapBuffers();
        Assert.False(renderingSystem.Buffer.IsDirty);

        // Open the dropdown with Space (HandleKeyPress via instance)
        var dd = Assert.IsType<DropdownInstance<string>>(instance);
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)));

        // Render open state
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var openTree = instance.Render();
        var openPatches = new DiffEngine().Diff(tree, openTree);
        renderer.ApplyPatches(openPatches);
        renderingSystem.Render();
        Assert.True(renderingSystem.Buffer.IsDirty, "Opening dropdown should mark buffer dirty");
        renderingSystem.Buffer.SwapBuffers();
        Assert.False(renderingSystem.Buffer.IsDirty);

        // Act: move highlight down one item
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var navTree = instance.Render();
        var navPatches = new DiffEngine().Diff(openTree, navTree);
        renderer.ApplyPatches(navPatches);
        renderingSystem.Render();

        // Assert: highlight change should repaint (dirty)
        Assert.True(renderingSystem.Buffer.IsDirty, "Dropdown highlight movement should repaint");
    }
    [Fact]
    public void TextArea_KeyPress_ShouldMarkBufferDirty()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var content = "";
        var binding = new Binding<string>(() => content, v => content = v);
        var textArea = new TextArea("Enter text", binding);

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(textArea, "test-textarea");

        // Initial render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var virtualDom = instance.Render();

        var renderer = new VirtualDomRenderer(renderingSystem);
        renderer.Render(virtualDom);

        // Clear any initial dirty state by swapping buffers
        renderingSystem.Buffer.SwapBuffers();
        Assert.False(renderingSystem.Buffer.IsDirty, "Buffer should not be dirty after initial render");

        // Act - Simulate key press through the instance
        // TextArea components handle input through their instances, not directly
        var textAreaInstance = instance as TextAreaInstance;
        Assert.NotNull(textAreaInstance);

        var keyInfo = new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
        textAreaInstance.HandleKeyPress(keyInfo);
        content = "a"; // Simulate the binding update

        // Re-render after key press
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var updatedVirtualDom = instance.Render();

        var patches = new DiffEngine().Diff(virtualDom, updatedVirtualDom);
        renderer.ApplyPatches(patches);

        // Force the rendering system to process the changes
        renderingSystem.Render();

        // Assert
        Assert.True(renderingSystem.Buffer.IsDirty, "Buffer should be dirty after key press and render");
    }

    [Fact]
    public void TabView_ArrowKey_ShouldMarkBufferDirty()
    {
        // Arrange
        var terminal = new TestTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview");

        // Initial render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var virtualDom = instance.Render();

        var renderer = new VirtualDomRenderer(renderingSystem);
        renderer.Render(virtualDom);

        // Clear any initial dirty state
        renderingSystem.Buffer.SwapBuffers();
        Assert.False(renderingSystem.Buffer.IsDirty, "Buffer should not be dirty after initial render");

        // Act - Simulate arrow key press through the instance
        // TabViewInstance is internal, so we test through the IFocusable interface
        var focusable = instance as IFocusable;
        Assert.NotNull(focusable);

        var keyInfo = new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false);
        focusable.HandleKeyPress(keyInfo);

        // Re-render after key press
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var updatedVirtualDom = instance.Render();

        var patches = new DiffEngine().Diff(virtualDom, updatedVirtualDom);
        renderer.ApplyPatches(patches);

        // Force the rendering system to process
        renderingSystem.Render();

        // Assert
        Assert.True(renderingSystem.Buffer.IsDirty, "Buffer should be dirty after arrow key press");
    }

    private class TestTerminal : ITerminal
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
}