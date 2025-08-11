using System;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

namespace Andy.TUI.Declarative.Tests.Rendering;

public class DropdownBehaviorTests
{
    private static (DropdownInstance<string> Instance, RenderingSystem RS, VirtualDomRenderer Renderer, DiffEngine Diff, Func<string> GetSelected)
        Setup(string placeholder, string[] items)
    {
        string selected = string.Empty;
        var selection = new Binding<string>(() => selected, v => selected = v);
        var dropdown = new Dropdown<string>(placeholder, items, selection, s => s);

        var terminal = new MockTerminal(80, 24);
        var rs = new RenderingSystem(terminal);
        rs.Initialize();
        var renderer = new VirtualDomRenderer(rs);
        var diff = new DiffEngine();

        var context = new DeclarativeContext(() => { });
        var instance = (DropdownInstance<string>)context.ViewInstanceManager.GetOrCreateInstance(dropdown, "dropdown-under-test");

        // initial render
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var t0 = instance.Render();
        renderer.Render(t0);
        rs.Render();

        return (instance, rs, renderer, diff, () => selected);
    }

    private static VirtualNode Apply(RenderingSystem rs, VirtualDomRenderer renderer, DiffEngine diff, DropdownInstance<string> inst, VirtualNode prevTree)
    {
        inst.CalculateLayout(LayoutConstraints.Loose(80, 24));
        var next = inst.Render();
        var patches = diff.Diff(prevTree, next);
        if (patches.Any(p => p.Type is PatchType.Insert or PatchType.Remove or PatchType.Replace or PatchType.Reorder))
        {
            renderer.Render(next);
        }
        else
        {
            renderer.ApplyPatches(patches);
        }
        rs.Render();
        return next;
    }

    private static string BufferToString(Andy.TUI.Terminal.Buffer buffer)
    {
        var sb = new StringBuilder();
        for (int y = 0; y < buffer.Height; y++)
        {
            for (int x = 0; x < buffer.Width; x++)
            {
                sb.Append(buffer[x, y].Character);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static bool SpinWaitFor(Func<bool> condition, int timeoutMs = 500, int intervalMs = 20)
    {
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
        {
            if (condition()) return true;
            Thread.Sleep(intervalMs);
        }
        return condition();
    }

    [Fact]
    public void Enter_OnOpen_ShouldSelectHighlighted_CloseMenu_UpdateTrigger()
    {
        var items = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        var (dd, rs, renderer, diff, getSelected) = Setup("Select a country...", items);

        dd.OnGotFocus();
        var tClosed = dd.Render();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = Apply(rs, renderer, diff, dd, tClosed);
        // Move highlight to second item
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t2 = Apply(rs, renderer, diff, dd, t1);

        // Press Enter to select and close
        dd.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        var t3 = Apply(rs, renderer, diff, dd, t2);

        Assert.Equal("Canada", getSelected());
        Assert.True(SpinWaitFor(() => BufferToString(rs.Buffer.GetFrontBuffer()).Contains("▶ Canada")));
        var afterSelect = BufferToString(rs.Buffer.GetFrontBuffer());
        Assert.DoesNotContain("  United States", afterSelect);
        Assert.DoesNotContain("  Canada\n", afterSelect); // no menu lines remain
        Assert.DoesNotContain("  United Kingdom", afterSelect);
        Assert.DoesNotContain("  Germany", afterSelect);
        Assert.DoesNotContain("  France", afterSelect);
    }

    [Fact]
    public void Escape_OnOpen_ShouldCloseMenu_KeepSelection()
    {
        var items = new[] { "United States", "Canada" };
        var (dd, rs, renderer, diff, getSelected) = Setup("Select a country...", items);

        dd.OnGotFocus();
        var tClosed = dd.Render();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = Apply(rs, renderer, diff, dd, tClosed);
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)); // move
        var t2 = Apply(rs, renderer, diff, dd, t1);

        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false)); // close
        var t3 = Apply(rs, renderer, diff, dd, t2);

        Assert.Equal(string.Empty, getSelected());
        var buf = BufferToString(rs.Buffer.GetFrontBuffer());
        Assert.Contains("▶ Select a country...", buf);
        Assert.DoesNotContain("  United States", buf);
        Assert.DoesNotContain("  Canada", buf);
    }

    [Fact]
    public void Navigation_ShouldClampAtBounds()
    {
        var items = new[] { "A", "B" };
        var (dd, rs, renderer, diff, _) = Setup("Select...", items);

        dd.OnGotFocus();
        var tClosed = dd.Render();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = Apply(rs, renderer, diff, dd, tClosed);

        // Up at first should stay at first
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        var t2 = Apply(rs, renderer, diff, dd, t1);
        Assert.Contains("  A", BufferToString(rs.Buffer.GetFrontBuffer()));

        // Down twice should stop at last
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t3 = Apply(rs, renderer, diff, dd, t2);
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t4 = Apply(rs, renderer, diff, dd, t3);
        Assert.Contains("  B", BufferToString(rs.Buffer.GetFrontBuffer()));
    }

    [Fact]
    public void Reopen_ShouldHighlightSelectedItem()
    {
        var items = new[] { "A", "B", "C" };
        var (dd, rs, renderer, diff, getSelected) = Setup("Select...", items);

        dd.OnGotFocus();
        var tClosed = dd.Render();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = Apply(rs, renderer, diff, dd, tClosed);
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false)); // highlight B
        var t2 = Apply(rs, renderer, diff, dd, t1);
        dd.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false)); // select B
        var t3 = Apply(rs, renderer, diff, dd, t2);
        Assert.Equal("B", getSelected());

        // Reopen
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));
        var t4 = Apply(rs, renderer, diff, dd, t3);

        // Expect highlighted row equals selected item (B)
        var vdom = t4;
        static bool IsHighlightedText(VirtualNode node)
        {
            if (node is ElementNode el && el.TagName.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                if (el.Props.TryGetValue("style", out var styleObj) && styleObj is Style style)
                {
                    return style.Background.ConsoleColor.HasValue && style.Background.ConsoleColor.Value == ConsoleColor.White;
                }
            }
            return false;
        }
        static string ConcatText(VirtualNode node)
        {
            if (node is TextNode tn) return tn.Content;
            return string.Concat(node.Children.Select(ConcatText));
        }
        string? highlightedText = null;
        void Find(VirtualNode n)
        {
            if (highlightedText != null) return;
            if (IsHighlightedText(n)) { highlightedText = ConcatText(n); return; }
            foreach (var c in n.Children) Find(c);
        }
        Find(vdom);
        Assert.NotNull(highlightedText);
        Assert.Contains("  B", highlightedText!);
    }

    [Fact]
    public void LostFocus_ShouldCloseMenu()
    {
        var items = new[] { "A", "B" };
        var (dd, rs, renderer, diff, _) = Setup("Select...", items);

        dd.OnGotFocus();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var tClosed2 = dd.Render();
        var t1 = Apply(rs, renderer, diff, dd, tClosed2);
        dd.OnLostFocus();
        var t2 = Apply(rs, renderer, diff, dd, t1);

        var buf = BufferToString(rs.Buffer.GetFrontBuffer());
        Assert.DoesNotContain("  A", buf);
        Assert.DoesNotContain("  B", buf);
    }

    [Fact]
    public void MovingHighlight_ShouldUpdateTriggerWhileOpen()
    {
        var items = new[] { "United States", "Canada", "United Kingdom" };
        var (dd, rs, renderer, diff, _) = Setup("Select a country...", items);

        dd.OnGotFocus();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = dd.Render();

        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t2 = Apply(rs, renderer, diff, dd, t1);
        Assert.True(SpinWaitFor(() => BufferToString(rs.Buffer.GetFrontBuffer()).Contains("▼ Canada")));

        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t3 = Apply(rs, renderer, diff, dd, t2);
        Assert.True(SpinWaitFor(() => BufferToString(rs.Buffer.GetFrontBuffer()).Contains("▼ United Kingdom")));
    }

    [Fact]
    public void Open_Move_Dropdown_ShouldShowAllItems_NotJustTwo()
    {
        var items = new[] { "United States", "Canada", "United Kingdom", "Germany", "France" };
        var (dd, rs, renderer, diff, _) = Setup("Select a country...", items);

        dd.OnGotFocus();
        var tClosed = dd.Render();
        dd.HandleKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false)); // open
        var t1 = Apply(rs, renderer, diff, dd, tClosed);

        // Verify all items visible on open
        var buf1 = BufferToString(rs.Buffer.GetFrontBuffer());
        foreach (var it in items)
            Assert.Contains(it, buf1);

        // Move down, should still see all items
        dd.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        var t2 = Apply(rs, renderer, diff, dd, t1);
        var buf2 = BufferToString(rs.Buffer.GetFrontBuffer());
        foreach (var it in items)
            Assert.Contains(it, buf2);
    }
}


