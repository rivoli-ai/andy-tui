using System;
using Xunit;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Focus;

namespace Andy.TUI.Declarative.Tests.Focus;

public class FocusManagerTests
{
    private class TestFocusable : IFocusable
    {
        public string Name { get; }
        public bool CanFocus { get; set; } = true;
        public bool IsFocused { get; private set; }
        public bool GotFocus { get; private set; }
        public bool LostFocus { get; private set; }

        public TestFocusable(string name) { Name = name; }
        public void OnGotFocus() { IsFocused = true; GotFocus = true; }
        public void OnLostFocus() { IsFocused = false; LostFocus = true; }
        public bool HandleKeyPress(ConsoleKeyInfo keyInfo) => false;
        public Andy.TUI.VirtualDom.VirtualNode Render() => new Andy.TUI.VirtualDom.TextNode(Name);
    }

    [Fact]
    public void MoveFocus_CyclesThroughFocusableComponents()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        var c = new TestFocusable("C");

        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);

        // Initial move should focus first
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);

        // Next cycles to B then C then back to A
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(b, fm.FocusedComponent);
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(c, fm.FocusedComponent);
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);

        // Previous cycles backwards
        fm.MoveFocus(FocusDirection.Previous);
        Assert.Same(c, fm.FocusedComponent);
    }

    [Fact]
    public void Unregister_RemovesFocusAndMovesToNext()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        var c = new TestFocusable("C");

        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);

        fm.MoveFocus(FocusDirection.Next); // A
        fm.MoveFocus(FocusDirection.Next); // B
        Assert.Same(b, fm.FocusedComponent);

        fm.UnregisterFocusable(b);
        Assert.Same(c, fm.FocusedComponent);
    }

    [Fact]
    public void SetFocus_RespectsCanFocus()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A") { CanFocus = false };
        var b = new TestFocusable("B") { CanFocus = true };

        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);

        fm.SetFocus(a);
        Assert.Null(fm.FocusedComponent);

        fm.SetFocus(b);
        Assert.Same(b, fm.FocusedComponent);
    }
}
