using System;
using Xunit;
using Andy.TUI.Declarative.Hooks;

namespace Andy.TUI.Declarative.Tests.Hooks;

public class HookContextTests
{
    [Fact]
    public void HookContext_ShouldCreateWithComponentId()
    {
        // Arrange & Act
        var context = new HookContext("TestComponent");

        // Assert
        Assert.Equal("TestComponent", context.ComponentId);
        Assert.False(context.IsInitialized);
    }

    [Fact]
    public void BeginRender_EndRender_ShouldMarkAsInitialized()
    {
        // Arrange
        var context = new HookContext("TestComponent");

        // Act
        context.BeginRender();
        Assert.False(context.IsInitialized);
        
        context.EndRender();

        // Assert
        Assert.True(context.IsInitialized);
    }

    [Fact]
    public void UseHook_FirstRender_ShouldCreateNewHook()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        context.BeginRender();

        // Act
        var hook = context.UseHook(() => new TestHook());

        // Assert
        Assert.NotNull(hook);
        Assert.IsType<TestHook>(hook);
    }

    [Fact]
    public void UseHook_SubsequentRenders_ShouldReturnSameHook()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        
        // First render
        context.BeginRender();
        var hook1 = context.UseHook(() => new TestHook());
        context.EndRender();

        // Second render
        context.BeginRender();
        var hook2 = context.UseHook(() => new TestHook());
        context.EndRender();

        // Assert
        Assert.Same(hook1, hook2);
    }

    [Fact]
    public void UseHook_DifferentTypesAtSameIndex_ShouldThrow()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        
        // First render - create TestHook
        context.BeginRender();
        context.UseHook(() => new TestHook());
        context.EndRender();

        // Second render - try to use different hook type
        context.BeginRender();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            context.UseHook(() => new AnotherTestHook()));
        Assert.Contains("Hook type mismatch", ex.Message);
    }

    [Fact]
    public void ValidateHookOrder_MismatchedCount_ShouldThrow()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        
        // First render - create 2 hooks
        context.BeginRender();
        context.UseHook(() => new TestHook());
        context.UseHook(() => new TestHook());
        context.EndRender();

        // Second render - only use 1 hook
        context.BeginRender();
        context.UseHook(() => new TestHook());

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            context.ValidateHookOrder());
        Assert.Contains("Hook count mismatch", ex.Message);
    }

    [Fact]
    public void UseHook_ConditionalHook_ShouldThrow()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        
        // First render - no hooks
        context.BeginRender();
        context.EndRender();

        // Second render - try to add hook
        context.BeginRender();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            context.UseHook(() => new TestHook()));
        Assert.Contains("Hook count mismatch", ex.Message);
    }

    [Fact]
    public void Dispose_ShouldDisposeAllHooks()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        context.BeginRender();
        var hook1 = context.UseHook(() => new TestHook());
        var hook2 = context.UseHook(() => new TestHook());
        context.EndRender();

        // Act
        context.Dispose();

        // Assert
        Assert.True(hook1.IsDisposed);
        Assert.True(hook2.IsDisposed);
    }

    [Fact]
    public void RequestUpdate_ShouldTriggerScheduleUpdate()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        bool updateRequested = false;
        context.ScheduleUpdate = action => updateRequested = true;

        // Act
        context.RequestUpdate();

        // Assert
        Assert.True(updateRequested);
    }

    private class TestHook : IHook
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private class AnotherTestHook : IHook
    {
        public void Dispose() { }
    }
}