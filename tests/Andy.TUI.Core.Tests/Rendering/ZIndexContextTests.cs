using System;
using Xunit;
using Andy.TUI.Core.Rendering;

namespace Andy.TUI.Core.Tests.Rendering;

public class ZIndexContextTests
{
    [Fact]
    public void CurrentAbsoluteZ_EmptyStack_ReturnsZero()
    {
        var context = new ZIndexContext();
        Assert.Equal(0, context.CurrentAbsoluteZ);
    }

    [Fact]
    public void CurrentAbsoluteZ_SingleComponent_ReturnsComponentZIndex()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "TestComponent");
        
        Assert.Equal(5, context.CurrentAbsoluteZ);
    }

    [Fact]
    public void CurrentAbsoluteZ_NestedComponents_ReturnsSumOfZIndices()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "Parent");
        context.EnterComponent(3, "Child");
        context.EnterComponent(2, "Grandchild");
        
        Assert.Equal(10, context.CurrentAbsoluteZ);
    }

    [Fact]
    public void Depth_EmptyStack_ReturnsZero()
    {
        var context = new ZIndexContext();
        Assert.Equal(0, context.Depth);
    }

    [Fact]
    public void Depth_AfterEnteringComponents_ReturnsCorrectDepth()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(1, "First");
        Assert.Equal(1, context.Depth);
        
        context.EnterComponent(2, "Second");
        Assert.Equal(2, context.Depth);
        
        context.EnterComponent(3, "Third");
        Assert.Equal(3, context.Depth);
    }

    [Fact]
    public void ExitComponent_RemovesFromStack()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "Parent");
        context.EnterComponent(3, "Child");
        
        Assert.Equal(8, context.CurrentAbsoluteZ);
        Assert.Equal(2, context.Depth);
        
        context.ExitComponent();
        
        Assert.Equal(5, context.CurrentAbsoluteZ);
        Assert.Equal(1, context.Depth);
    }

    [Fact]
    public void ExitComponent_EmptyStack_ThrowsException()
    {
        var context = new ZIndexContext();
        
        Assert.Throws<InvalidOperationException>(() => context.ExitComponent());
    }

    [Fact]
    public void ResolveAbsolute_EmptyStack_ReturnsRelativeValue()
    {
        var context = new ZIndexContext();
        
        Assert.Equal(7, context.ResolveAbsolute(7));
    }

    [Fact]
    public void ResolveAbsolute_WithContext_AddsToCurrentAbsolute()
    {
        var context = new ZIndexContext();
        context.EnterComponent(10, "Base");
        context.EnterComponent(5, "Middle");
        
        Assert.Equal(20, context.ResolveAbsolute(5));
        Assert.Equal(25, context.ResolveAbsolute(10));
        Assert.Equal(10, context.ResolveAbsolute(-5));
    }

    [Fact]
    public void GetComponentPath_EmptyStack_ReturnsEmptyString()
    {
        var context = new ZIndexContext();
        
        Assert.Equal("", context.GetComponentPath());
    }

    [Fact]
    public void GetComponentPath_SingleComponent_ReturnsComponentName()
    {
        var context = new ZIndexContext();
        context.EnterComponent(1, "RootComponent");
        
        Assert.Equal("RootComponent", context.GetComponentPath());
    }

    [Fact]
    public void GetComponentPath_NestedComponents_ReturnsHierarchy()
    {
        var context = new ZIndexContext();
        context.EnterComponent(1, "Root");
        context.EnterComponent(2, "Child");
        context.EnterComponent(3, "Grandchild");
        
        Assert.Equal("Root > Child > Grandchild", context.GetComponentPath());
    }

    [Fact]
    public void GetComponentPath_WithNullNames_UsesDefaultNames()
    {
        var context = new ZIndexContext();
        context.EnterComponent(1, null);
        context.EnterComponent(2, null);
        
        Assert.Equal("Component0 > Component1", context.GetComponentPath());
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "First");
        context.EnterComponent(3, "Second");
        
        Assert.Equal(8, context.CurrentAbsoluteZ);
        Assert.Equal(2, context.Depth);
        Assert.Equal("First > Second", context.GetComponentPath());
        
        context.Reset();
        
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
        Assert.Equal("", context.GetComponentPath());
    }

    [Fact]
    public void CreateSnapshot_CapturesCurrentState()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "Parent");
        context.EnterComponent(3, "Child");
        
        var snapshot = context.CreateSnapshot();
        
        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.ZIndexStack.Length);
        // Stack.ToArray() returns elements in LIFO order
        Assert.Equal(3, snapshot.ZIndexStack[0]); // Child (last pushed)
        Assert.Equal(5, snapshot.ZIndexStack[1]); // Parent (first pushed)
        Assert.Equal(2, snapshot.ComponentStack.Length);
        Assert.Equal("Child", snapshot.ComponentStack[0]); // Child (last pushed)
        Assert.Equal("Parent", snapshot.ComponentStack[1]); // Parent (first pushed)
    }

    [Fact]
    public void RestoreSnapshot_RestoresState()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "Parent");
        context.EnterComponent(3, "Child");
        
        var snapshot = context.CreateSnapshot();
        
        context.Reset();
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
        
        context.RestoreSnapshot(snapshot);
        
        Assert.Equal(8, context.CurrentAbsoluteZ);
        Assert.Equal(2, context.Depth);
        Assert.Equal("Parent > Child", context.GetComponentPath());
    }

    [Fact]
    public void RestoreSnapshot_OverwritesCurrentState()
    {
        var context = new ZIndexContext();
        context.EnterComponent(5, "Original");
        
        var snapshot = context.CreateSnapshot();
        
        context.Reset();
        context.EnterComponent(10, "New");
        context.EnterComponent(20, "Different");
        
        Assert.Equal(30, context.CurrentAbsoluteZ);
        
        context.RestoreSnapshot(snapshot);
        
        Assert.Equal(5, context.CurrentAbsoluteZ);
        Assert.Equal(1, context.Depth);
        Assert.Equal("Original", context.GetComponentPath());
    }

    [Fact]
    public void ComplexNestedScenario_HandlesCorrectly()
    {
        var context = new ZIndexContext();
        
        // Build a complex hierarchy
        context.EnterComponent(100, "App");
        Assert.Equal(100, context.CurrentAbsoluteZ);
        
        context.EnterComponent(10, "MainView");
        Assert.Equal(110, context.CurrentAbsoluteZ);
        
        context.EnterComponent(5, "Header");
        Assert.Equal(115, context.CurrentAbsoluteZ);
        Assert.Equal(120, context.ResolveAbsolute(5));
        
        context.ExitComponent(); // Exit Header
        Assert.Equal(110, context.CurrentAbsoluteZ);
        
        context.EnterComponent(20, "Modal");
        Assert.Equal(130, context.CurrentAbsoluteZ);
        
        var modalSnapshot = context.CreateSnapshot();
        
        context.EnterComponent(2, "ModalContent");
        Assert.Equal(132, context.CurrentAbsoluteZ);
        
        // Exit all the way back
        context.ExitComponent(); // Exit ModalContent
        context.ExitComponent(); // Exit Modal
        context.ExitComponent(); // Exit MainView
        context.ExitComponent(); // Exit App
        
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
        
        // Restore to modal state
        context.RestoreSnapshot(modalSnapshot);
        Assert.Equal(130, context.CurrentAbsoluteZ);
        Assert.Equal("App > MainView > Modal", context.GetComponentPath());
    }

    [Fact]
    public void NegativeZIndex_HandledCorrectly()
    {
        var context = new ZIndexContext();
        
        context.EnterComponent(10, "Base");
        context.EnterComponent(-5, "Negative");
        
        Assert.Equal(5, context.CurrentAbsoluteZ);
        Assert.Equal(2, context.ResolveAbsolute(-3));
        
        context.EnterComponent(-10, "MoreNegative");
        Assert.Equal(-5, context.CurrentAbsoluteZ);
    }
}

public class ZIndexContextSnapshotTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var zStack = new[] { 1, 2, 3 };
        var cStack = new[] { "A", "B", "C" };
        
        var snapshot = new ZIndexContextSnapshot(zStack, cStack);
        
        Assert.Equal(zStack, snapshot.ZIndexStack);
        Assert.Equal(cStack, snapshot.ComponentStack);
    }

    [Fact]
    public void EmptySnapshot_HandledCorrectly()
    {
        var snapshot = new ZIndexContextSnapshot(Array.Empty<int>(), Array.Empty<string>());
        
        Assert.Empty(snapshot.ZIndexStack);
        Assert.Empty(snapshot.ComponentStack);
        
        var context = new ZIndexContext();
        context.EnterComponent(5, "Test");
        context.RestoreSnapshot(snapshot);
        
        Assert.Equal(0, context.CurrentAbsoluteZ);
        Assert.Equal(0, context.Depth);
    }
}