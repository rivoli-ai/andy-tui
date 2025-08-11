using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Focus;

/// <summary>
/// Thorough tests for FocusManager functionality
/// </summary>
public class FocusManagerThoroughTests
{
    private readonly ITestOutputHelper _output;
    
    public FocusManagerThoroughTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    private class TestFocusable : IFocusable
    {
        public string Name { get; }
        public bool CanFocus { get; set; } = true;
        public bool IsFocused { get; private set; }
        public List<string> Events { get; } = new();
        public int KeyPressCount { get; private set; }
        
        public TestFocusable(string name) 
        { 
            Name = name;
        }
        
        public void OnGotFocus() 
        { 
            IsFocused = true;
            Events.Add($"GotFocus");
        }
        
        public void OnLostFocus() 
        { 
            IsFocused = false;
            Events.Add($"LostFocus");
        }
        
        public bool HandleKeyPress(ConsoleKeyInfo keyInfo) 
        {
            KeyPressCount++;
            Events.Add($"KeyPress:{keyInfo.KeyChar}");
            return true;
        }
        
        public VirtualNode Render() => new TextNode(Name);
    }
    
    [Fact]
    public void FocusManager_InitialState_NoFocus()
    {
        var fm = new FocusManager();
        
        Assert.Null(fm.FocusedComponent);
    }
    
    [Fact]
    public void FocusManager_RegisterSingle_CanFocus()
    {
        var fm = new FocusManager();
        var component = new TestFocusable("A");
        
        fm.RegisterFocusable(component);
        
        // Component is registered but not focused yet
        Assert.Null(fm.FocusedComponent);
        
        // Move focus should select it
        fm.MoveFocus(FocusDirection.Next);
        
        Assert.Same(component, fm.FocusedComponent);
        Assert.True(component.IsFocused);
        Assert.Contains("GotFocus", component.Events);
    }
    
    [Fact]
    public void FocusManager_RegisterMultiple_FocusInOrder()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        var c = new TestFocusable("C");
        
        // Register in order A, B, C
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);
        
        // First TAB should focus A
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);
        _output.WriteLine($"First TAB: Focused {a.Name}");
        
        // Second TAB should focus B
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(b, fm.FocusedComponent);
        _output.WriteLine($"Second TAB: Focused {b.Name}");
        
        // Third TAB should focus C
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(c, fm.FocusedComponent);
        _output.WriteLine($"Third TAB: Focused {c.Name}");
        
        // Fourth TAB should cycle back to A
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);
        _output.WriteLine($"Fourth TAB: Cycled back to {a.Name}");
    }
    
    [Fact]
    public void FocusManager_MovePrevious_ReverseOrder()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        var c = new TestFocusable("C");
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);
        
        // Start by focusing A
        fm.SetFocus(a);
        Assert.Same(a, fm.FocusedComponent);
        
        // Move previous should go to C (wrap around)
        fm.MoveFocus(FocusDirection.Previous);
        Assert.Same(c, fm.FocusedComponent);
        
        // Previous again should go to B
        fm.MoveFocus(FocusDirection.Previous);
        Assert.Same(b, fm.FocusedComponent);
        
        // Previous again should go to A
        fm.MoveFocus(FocusDirection.Previous);
        Assert.Same(a, fm.FocusedComponent);
    }
    
    [Fact]
    public void FocusManager_UnregisterFocused_MovesToNext()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        var c = new TestFocusable("C");
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);
        
        // Focus B
        fm.SetFocus(b);
        Assert.Same(b, fm.FocusedComponent);
        
        // Unregister B while it's focused
        fm.UnregisterFocusable(b);
        
        // Focus should move to next available (C)
        Assert.Same(c, fm.FocusedComponent);
        Assert.False(b.IsFocused);
        Assert.True(c.IsFocused);
    }
    
    [Fact]
    public void FocusManager_UnregisterLast_ClearsAllFocus()
    {
        var fm = new FocusManager();
        var component = new TestFocusable("A");
        
        fm.RegisterFocusable(component);
        fm.SetFocus(component);
        
        Assert.Same(component, fm.FocusedComponent);
        
        // Unregister the only component
        fm.UnregisterFocusable(component);
        
        Assert.Null(fm.FocusedComponent);
        Assert.False(component.IsFocused);
    }
    
    [Fact]
    public void FocusManager_CanFocus_RespectedDuringNavigation()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A") { CanFocus = true };
        var b = new TestFocusable("B") { CanFocus = false };
        var c = new TestFocusable("C") { CanFocus = true };
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.RegisterFocusable(c);
        
        // First TAB should focus A
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);
        
        // Second TAB should skip B and focus C
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(c, fm.FocusedComponent);
        Assert.False(b.IsFocused); // B should never get focus
        
        // Third TAB should cycle back to A
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(a, fm.FocusedComponent);
    }
    
    [Fact]
    public void FocusManager_SetFocus_RespectsCanFocus()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A") { CanFocus = false };
        var b = new TestFocusable("B") { CanFocus = true };
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        
        // Try to focus A (should fail)
        fm.SetFocus(a);
        Assert.Null(fm.FocusedComponent);
        Assert.False(a.IsFocused);
        
        // Focus B (should succeed)
        fm.SetFocus(b);
        Assert.Same(b, fm.FocusedComponent);
        Assert.True(b.IsFocused);
    }
    
    [Fact]
    public void FocusManager_FocusedComponent_RoutesToFocused()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        
        // Focus A
        fm.SetFocus(a);
        
        // Send key press
        var keyInfo = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
        var handled = fm.FocusedComponent?.HandleKeyPress(keyInfo) ?? false;
        
        Assert.True(handled);
        Assert.Equal(1, a.KeyPressCount);
        Assert.Equal(0, b.KeyPressCount);
        Assert.Contains("KeyPress:x", a.Events);
    }
    
    [Fact]
    public void FocusManager_FocusedComponent_NoFocus_ReturnsFalse()
    {
        var fm = new FocusManager();
        var component = new TestFocusable("A");
        
        fm.RegisterFocusable(component);
        // Don't focus anything
        
        var keyInfo = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);
        var handled = fm.FocusedComponent?.HandleKeyPress(keyInfo) ?? false;
        
        Assert.False(handled);
        Assert.Equal(0, component.KeyPressCount);
    }
    
    [Fact]
    public void FocusManager_SetFocusNull_RemovesAllFocus()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        fm.SetFocus(a);
        
        Assert.Same(a, fm.FocusedComponent);
        Assert.True(a.IsFocused);
        
        // Clear focus
        fm.SetFocus(null);
        
        Assert.Null(fm.FocusedComponent);
        Assert.False(a.IsFocused);
        Assert.Contains("LostFocus", a.Events);
    }
    
    [Fact]
    public void FocusManager_FocusEvents_ProperlyFired()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        
        // Focus A
        fm.SetFocus(a);
        Assert.Contains("GotFocus", a.Events);
        Assert.Empty(b.Events);
        
        // Switch to B
        fm.SetFocus(b);
        Assert.Contains("LostFocus", a.Events);
        Assert.Contains("GotFocus", b.Events);
        
        // Clear focus
        fm.SetFocus(null);
        Assert.Contains("LostFocus", b.Events);
    }
    
    [Fact]
    public void FocusManager_RegisterDuplicate_HandledGracefully()
    {
        var fm = new FocusManager();
        var component = new TestFocusable("A");
        
        fm.RegisterFocusable(component);
        fm.RegisterFocusable(component); // Register again
        
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(component, fm.FocusedComponent);
        
        // Should only need one TAB to cycle back
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(component, fm.FocusedComponent);
    }
    
    [Fact]
    public void FocusManager_UnregisterNonRegistered_HandledGracefully()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A");
        var b = new TestFocusable("B");
        
        fm.RegisterFocusable(a);
        fm.SetFocus(a);
        
        // Try to unregister B which was never registered
        fm.UnregisterFocusable(b);
        
        // A should still be focused
        Assert.Same(a, fm.FocusedComponent);
    }
    
    [Fact]
    public void FocusManager_AllComponentsCannotFocus_NoFocusSet()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A") { CanFocus = false };
        var b = new TestFocusable("B") { CanFocus = false };
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        
        // Try to move focus
        fm.MoveFocus(FocusDirection.Next);
        
        // No component should be focused
        Assert.Null(fm.FocusedComponent);
        Assert.False(a.IsFocused);
        Assert.False(b.IsFocused);
    }
    
    [Fact]
    public void FocusManager_DynamicCanFocusChange_UpdatesDuringNavigation()
    {
        var fm = new FocusManager();
        var a = new TestFocusable("A") { CanFocus = true };
        var b = new TestFocusable("B") { CanFocus = true };
        
        fm.RegisterFocusable(a);
        fm.RegisterFocusable(b);
        
        fm.SetFocus(a);
        Assert.Same(a, fm.FocusedComponent);
        
        // Disable B's ability to focus
        b.CanFocus = false;
        
        // Try to move to B
        fm.MoveFocus(FocusDirection.Next);
        
        // Should stay on A since B can't focus
        Assert.Same(a, fm.FocusedComponent);
        
        // Re-enable B
        b.CanFocus = true;
        
        // Now should be able to move to B
        fm.MoveFocus(FocusDirection.Next);
        Assert.Same(b, fm.FocusedComponent);
    }
}