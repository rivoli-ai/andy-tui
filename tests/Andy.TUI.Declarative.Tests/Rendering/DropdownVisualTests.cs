using System;
using System.Linq;
using Xunit;
using System.Text;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Rendering;

public class DropdownVisualTests
{
    [Fact]
    public void Dropdown_Example1_VisualUpdatesOnArrowNavigation()
    {
        // Arrange: mirror Example 1 (countries list)
        var items = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        string selected = string.Empty;
        var selection = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>("Select a country...", items, selection, s => s);

        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        var context = new DeclarativeContext(() => { });
        var instance = context.ViewInstanceManager.GetOrCreateInstance(dropdown, "dropdown-example1");
        var dd = Assert.IsType<DropdownInstance<string>>(instance);
        var renderer = new VirtualDomRenderer(renderingSystem);
        var diff = new DiffEngine();

        // Initial layout and render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var initialTree = instance.Render();
        renderer.Render(initialTree);
        renderingSystem.Render();

        // Open the dropdown
        dd.OnGotFocus();
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var openTree = instance.Render();
        var openPatches = diff.Diff(initialTree, openTree);
        renderer.ApplyPatches(openPatches);
        renderingSystem.Render();

        // Helpers
        static (int index, string text)? FindHighlighted(VirtualNode node)
        {
            if (node is ElementNode el && el.TagName.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                if (el.Props.TryGetValue("style", out var styleObj) && styleObj is Style style)
                {
                    // Highlight uses white background in DropdownInstance
                    if (style.Background.ConsoleColor.HasValue && style.Background.ConsoleColor.Value == ConsoleColor.White)
                    {
                        var text = GetText(node);
                        var idx = ExtractIndex(text);
                        return (idx, text);
                    }
                }
            }
            foreach (var c in node.Children)
            {
                var r = FindHighlighted(c);
                if (r != null) return r;
            }
            return null;
        }

        static string GetText(VirtualNode node)
        {
            if (node is TextNode tn) return tn.Content;
            if (node is ElementNode en)
                return string.Concat(en.Children.Select(GetText));
            return string.Concat(node.Children.Select(GetText));
        }

        static int ExtractIndex(string text)
        {
            // Text is rendered as "  {itemText}"; we infer index by matching against known items in assertions
            return -1;
        }

        // Assert: first item highlighted on open
        var h1 = FindHighlighted(openTree);
        Assert.NotNull(h1);
        Assert.Contains(items[0], h1!.Value.text);

        // Act: move highlight down once
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var treeAfterDown1 = instance.Render();
        var patches1 = diff.Diff(openTree, treeAfterDown1);
        renderer.ApplyPatches(patches1);
        renderingSystem.Render();

        // Assert: second item highlighted and trigger text reflects it (▼ Canada)
        var h2 = FindHighlighted(treeAfterDown1);
        Assert.NotNull(h2);
        Assert.Contains(items[1], h2!.Value.text);
        Assert.Contains($"▼ {items[1]}", GetText(treeAfterDown1));
        // Also verify the rendered line buffer contains the trigger text
        // Inspect the terminal buffer content directly
        Assert.True(SpinWaitFor(() => BufferToString(renderingSystem.Buffer.GetFrontBuffer()).Contains($"▼ {items[1]}")),
            "Trigger line should reflect highlighted item after first Down");

        // Act: move highlight down again
        Assert.True(dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)));
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var treeAfterDown2 = instance.Render();
        var patches2 = diff.Diff(treeAfterDown1, treeAfterDown2);
        renderer.ApplyPatches(patches2);
        renderingSystem.Render();

        // Assert: third item highlighted and trigger text reflects it (▼ United Kingdom)
        var h3 = FindHighlighted(treeAfterDown2);
        Assert.NotNull(h3);
        Assert.Contains(items[2], h3!.Value.text);
        Assert.Contains($"▼ {items[2]}", GetText(treeAfterDown2));
        Assert.True(SpinWaitFor(() => BufferToString(renderingSystem.Buffer.GetFrontBuffer()).Contains($"▼ {items[2]}")),
            "Trigger line should reflect highlighted item after second Down");

        static string BufferToString(Andy.TUI.Terminal.Buffer buffer)
        {
            var sb = new StringBuilder();
            for (int y = 0; y < buffer.Height; y++)
            {
                for (int x = 0; x < buffer.Width; x++)
                {
                    var cell = buffer[x, y];
                    sb.Append(cell.Character);
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        static bool SpinWaitFor(Func<bool> condition, int timeoutMs = 500, int intervalMs = 20)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (condition()) return true;
                Thread.Sleep(intervalMs);
            }
            return condition();
        }
    }
}


