using Xunit;
using Andy.TUI.Core.Rendering;
using System;

namespace Andy.TUI.Core.Tests.Rendering;

public class ZIndexContextTests
{
    [Fact]
    public void NewContext_HasZeroAbsoluteZ()
    {
        var context = new ZIndexContext();
        
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
    }
    
    [Fact]
    public void EnterComponent_AddsToAbsoluteZ()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(100, "Modal");
        
        Assert.Equal(100, context.CurrentAbsoluteZ);
        Assert.Equal(1, context.Depth);
        Assert.Equal("Modal", context.GetComponentPath());
    }
    
    [Fact]
    public void NestedComponents_AccumulateZIndex()
    {
        var context = new ZIndexContext();
        
        // Modal at z=1000
        context.EnterComponent(1000, "Modal");
        Assert.Equal(1000, context.CurrentAbsoluteZ);
        
        // TabView inside modal at relative z=0
        context.EnterComponent(0, "TabView");
        Assert.Equal(1000, context.CurrentAbsoluteZ);
        
        // Selected tab at relative z=2
        context.EnterComponent(2, "Tab1");
        Assert.Equal(1002, context.CurrentAbsoluteZ);
        Assert.Equal(3, context.Depth);
        Assert.Equal("Modal > TabView > Tab1", context.GetComponentPath());
    }
    
    [Fact]
    public void ExitComponent_RestoresPreviousContext()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(1000, "Modal");
        context.EnterComponent(5, "Content");
        Assert.Equal(1005, context.CurrentAbsoluteZ);
        
        context.ExitComponent();
        Assert.Equal(1000, context.CurrentAbsoluteZ);
        Assert.Equal("Modal", context.GetComponentPath());
        
        context.ExitComponent();
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal("", context.GetComponentPath());
    }
    
    [Fact]
    public void ExitComponent_ThrowsWhenEmpty()
    {
        var context = new ZIndexContext();
        
        Assert.Throws<InvalidOperationException>(() => context.ExitComponent());
    }
    
    [Fact]
    public void ResolveAbsolute_AddsToCurrentContext()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(100);
        
        Assert.Equal(105, context.ResolveAbsolute(5));
        Assert.Equal(110, context.ResolveAbsolute(10));
        Assert.Equal(90, context.ResolveAbsolute(-10));
    }
    
    [Fact]
    public void Reset_ClearsAllState()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(100, "A");
        context.EnterComponent(200, "B");
        context.EnterComponent(300, "C");
        
        context.Reset();
        
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
        Assert.Equal("", context.GetComponentPath());
    }
    
    [Fact]
    public void Snapshot_CapturesCurrentState()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(100, "A");
        context.EnterComponent(200, "B");
        
        var snapshot = context.CreateSnapshot();
        
        // Modify context
        context.EnterComponent(300, "C");
        Assert.Equal(600, context.CurrentAbsoluteZ);
        
        // Restore snapshot
        context.RestoreSnapshot(snapshot);
        Assert.Equal(300, context.CurrentAbsoluteZ);
        Assert.Equal("A > B", context.GetComponentPath());
    }
    
    [Fact]
    public void ComplexHierarchy_TabViewInModal()
    {
        var context = new ZIndexContext();
        
        // Background content
        context.EnterComponent(0, "Root");
        context.EnterComponent(10, "BackgroundPanel");
        context.EnterComponent(5, "Button");
        Assert.Equal(15, context.CurrentAbsoluteZ);
        context.ExitComponent(); // Exit Button
        context.ExitComponent(); // Exit BackgroundPanel
        
        // Modal overlay
        context.EnterComponent(1000, "Modal");
        Assert.Equal(1000, context.CurrentAbsoluteZ);
        
        // TabView inside modal
        context.EnterComponent(0, "TabView");
        
        // Tab headers
        context.EnterComponent(1, "TabHeader1");
        Assert.Equal(1001, context.CurrentAbsoluteZ);
        context.ExitComponent();
        
        context.EnterComponent(1, "TabHeader2");
        Assert.Equal(1001, context.CurrentAbsoluteZ);
        context.ExitComponent();
        
        // Selected tab content
        context.EnterComponent(2, "SelectedTabContent");
        Assert.Equal(1002, context.CurrentAbsoluteZ);
        
        // Nested modal inside tab
        context.EnterComponent(100, "NestedModal");
        Assert.Equal(1102, context.CurrentAbsoluteZ);
    }
    
    [Fact]
    public void NegativeZIndex_ReducesAbsolute()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(100, "Parent");
        context.EnterComponent(-10, "BehindParent");
        
        Assert.Equal(90, context.CurrentAbsoluteZ);
    }
}