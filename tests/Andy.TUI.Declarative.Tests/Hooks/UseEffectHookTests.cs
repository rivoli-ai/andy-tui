using System;
using Xunit;
using Andy.TUI.Declarative.Hooks;

namespace Andy.TUI.Declarative.Tests.Hooks;

public class UseEffectHookTests
{
    private HookContext CreateTestContext()
    {
        var context = new HookContext("test");
        context.BeginRender();
        return context;
    }
    [Fact]
    public void SetEffect_FirstCall_ShouldRunEffect()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        bool effectRan = false;

        // Act
        hook.SetEffect(() => effectRan = true);

        // Effects run after render completes
        Assert.False(effectRan);
        context.EndRender();

        // Assert
        Assert.True(effectRan);
    }

    [Fact]
    public void SetEffect_WithCleanup_ShouldReturnCleanup()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        bool cleanupRan = false;

        // Act
        hook.SetEffect(() =>
        {
            return () => cleanupRan = true;
        });
        context.EndRender();

        // Cleanup should not run immediately
        Assert.False(cleanupRan);

        // Start new render cycle
        context.BeginRender();
        // Run effect again to trigger cleanup
        hook.SetEffect(() => null);
        context.EndRender();

        // Assert
        Assert.True(cleanupRan);
    }

    [Fact]
    public void SetEffect_NoDependencies_ShouldRunEveryTime()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        int runCount = 0;

        // Act - First render
        hook.SetEffect(() => runCount++);
        context.EndRender();
        Assert.Equal(1, runCount);

        // Second render
        context.BeginRender();
        hook.SetEffect(() => runCount++);
        context.EndRender();
        Assert.Equal(2, runCount);

        // Third render
        context.BeginRender();
        hook.SetEffect(() => runCount++);
        context.EndRender();

        // Assert
        Assert.Equal(3, runCount);
    }

    [Fact]
    public void SetEffect_SameDependencies_ShouldNotRerun()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        int runCount = 0;
        var deps = new object[] { "test", 42 };

        // Act - First render
        hook.SetEffect(() => runCount++, deps);
        context.EndRender();
        Assert.Equal(1, runCount);

        // Second render with same dependencies
        context.BeginRender();
        hook.SetEffect(() => runCount++, new object[] { "test", 42 });
        context.EndRender();

        // Assert
        Assert.Equal(1, runCount); // Should not have run again
    }

    [Fact]
    public void SetEffect_DifferentDependencies_ShouldRerun()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        int runCount = 0;

        // Act - First render
        hook.SetEffect(() => runCount++, new object[] { "test", 42 });
        context.EndRender();
        Assert.Equal(1, runCount);

        // Second render with different dependencies
        context.BeginRender();
        hook.SetEffect(() => runCount++, new object[] { "test", 43 });
        context.EndRender();

        // Assert
        Assert.Equal(2, runCount);
    }

    [Fact]
    public void SetEffect_DifferentDependencyCount_ShouldRerun()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        int runCount = 0;

        // Act - First render
        hook.SetEffect(() => runCount++, new object[] { "test" });
        context.EndRender();
        Assert.Equal(1, runCount);

        // Second render with different number of dependencies
        context.BeginRender();
        hook.SetEffect(() => runCount++, new object[] { "test", "extra" });
        context.EndRender();

        // Assert
        Assert.Equal(2, runCount);
    }

    [Fact]
    public void Dispose_ShouldRunCleanup()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        bool cleanupRan = false;

        hook.SetEffect(() =>
        {
            return () => cleanupRan = true;
        });
        context.EndRender();

        // Act
        hook.Dispose();

        // Assert
        Assert.True(cleanupRan);
    }

    [Fact]
    public void SetEffect_ChangingDependencies_ShouldRunPreviousCleanup()
    {
        // Arrange
        var context = CreateTestContext();
        var hook = new UseEffectHook();
        hook.SetContext(context);
        string cleanupMessage = "";
        int effectCount = 0;

        // First effect with cleanup
        hook.SetEffect(() =>
        {
            effectCount++;
            int currentCount = effectCount;
            return () => cleanupMessage = $"Cleanup {currentCount}";
        }, new object[] { 1 });
        context.EndRender();

        Assert.Equal(1, effectCount);
        Assert.Equal("", cleanupMessage);

        // Act - change dependencies in new render
        context.BeginRender();
        hook.SetEffect(() =>
        {
            effectCount++;
            int currentCount = effectCount;
            return () => cleanupMessage = $"Cleanup {currentCount}";
        }, new object[] { 2 });
        context.EndRender();

        // Assert
        Assert.Equal(2, effectCount);
        Assert.Equal("Cleanup 1", cleanupMessage); // Previous cleanup should have run
    }
}