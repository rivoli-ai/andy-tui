using System;
using Xunit;
using Andy.TUI.Declarative.Hooks;

namespace Andy.TUI.Declarative.Tests.Hooks;

public class UseMemoHookTests
{
    [Fact]
    public void GetValue_FirstCall_ShouldCompute()
    {
        // Arrange
        var hook = new UseMemoHook<int>();
        int computeCount = 0;

        // Act
        var result = hook.GetValue(() =>
        {
            computeCount++;
            return 42;
        }, null);

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(1, computeCount);
    }

    [Fact]
    public void GetValue_SameDependencies_ShouldReturnCached()
    {
        // Arrange
        var hook = new UseMemoHook<string>();
        int computeCount = 0;
        var deps = new object[] { "key", 123 };

        // Act
        var result1 = hook.GetValue(() =>
        {
            computeCount++;
            return "computed";
        }, deps);

        var result2 = hook.GetValue(() =>
        {
            computeCount++;
            return "recomputed";
        }, new object[] { "key", 123 });

        // Assert
        Assert.Equal("computed", result1);
        Assert.Equal("computed", result2); // Should return cached value
        Assert.Equal(1, computeCount); // Should only compute once
    }

    [Fact]
    public void GetValue_DifferentDependencies_ShouldRecompute()
    {
        // Arrange
        var hook = new UseMemoHook<int>();
        int computeCount = 0;

        // Act
        var result1 = hook.GetValue(() =>
        {
            computeCount++;
            return computeCount * 10;
        }, new object[] { "a" });

        var result2 = hook.GetValue(() =>
        {
            computeCount++;
            return computeCount * 10;
        }, new object[] { "b" });

        // Assert
        Assert.Equal(10, result1);
        Assert.Equal(20, result2);
        Assert.Equal(2, computeCount);
    }

    [Fact]
    public void GetValue_NullDependencies_ShouldComputeOnce()
    {
        // Arrange
        var hook = new UseMemoHook<double>();
        int computeCount = 0;

        // Act
        var result1 = hook.GetValue(() =>
        {
            computeCount++;
            return Math.PI;
        }, null);

        var result2 = hook.GetValue(() =>
        {
            computeCount++;
            return Math.E;
        }, null);

        // Assert
        Assert.Equal(Math.PI, result1);
        Assert.Equal(Math.PI, result2); // Should return cached value
        Assert.Equal(1, computeCount);
    }

    [Fact]
    public void GetValue_ChangingDependencyCount_ShouldRecompute()
    {
        // Arrange
        var hook = new UseMemoHook<string>();
        int computeCount = 0;

        // Act
        var result1 = hook.GetValue(() =>
        {
            computeCount++;
            return $"compute{computeCount}";
        }, new object[] { 1 });

        var result2 = hook.GetValue(() =>
        {
            computeCount++;
            return $"compute{computeCount}";
        }, new object[] { 1, 2 });

        // Assert
        Assert.Equal("compute1", result1);
        Assert.Equal("compute2", result2);
        Assert.Equal(2, computeCount);
    }

    [Fact]
    public void Dispose_WithDisposableValue_ShouldDispose()
    {
        // Arrange
        var hook = new UseMemoHook<DisposableValue>();
        var value = hook.GetValue(() => new DisposableValue(), null);

        // Act
        hook.Dispose();

        // Assert
        Assert.True(value.IsDisposed);
    }

    [Fact]
    public void UseCallbackHook_ShouldMemoizeFunction()
    {
        // Arrange
        var hook = new UseCallbackHook<Func<int, int>>();
        int invocationCount = 0;

        Func<int, int> CreateCallback() => x =>
        {
            invocationCount++;
            return x * 2;
        };

        // Act
        var callback1 = hook.GetValue(CreateCallback, new object[] { "dep1" });
        var result1 = callback1(5);

        var callback2 = hook.GetValue(CreateCallback, new object[] { "dep1" });
        var result2 = callback2(5);

        // Assert
        Assert.Equal(10, result1);
        Assert.Equal(10, result2);
        Assert.Same(callback1, callback2); // Should be the same function reference
    }

    private class DisposableValue : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }
}