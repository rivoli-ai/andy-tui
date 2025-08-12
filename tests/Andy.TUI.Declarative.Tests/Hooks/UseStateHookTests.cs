using System;
using Xunit;
using Andy.TUI.Declarative.Hooks;

namespace Andy.TUI.Declarative.Tests.Hooks;

public class UseStateHookTests
{
    [Fact]
    public void UseStateHook_ShouldInitializeWithValue()
    {
        // Arrange
        var context = new HookContext("TestComponent");

        // Act
        var hook = new UseStateHook<int>(context, 42);

        // Assert
        Assert.Equal(42, hook.Value);
    }

    [Fact]
    public void SetValue_ShouldUpdateValue()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        var hook = new UseStateHook<string>(context, "initial");

        // Act
        hook.SetValue("updated");

        // Assert
        Assert.Equal("updated", hook.Value);
    }

    [Fact]
    public void SetValue_WithFunction_ShouldUpdateBasedOnCurrent()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        var hook = new UseStateHook<int>(context, 10);

        // Act
        hook.SetValue(current => current + 5);

        // Assert
        Assert.Equal(15, hook.Value);
    }

    [Fact]
    public void SetValue_ShouldTriggerUpdate()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        bool updateTriggered = false;
        context.ScheduleUpdate = _ => updateTriggered = true;
        var hook = new UseStateHook<int>(context, 0);

        // Act
        hook.SetValue(1);

        // Assert
        Assert.True(updateTriggered);
    }

    [Fact]
    public void SetValue_SameValue_ShouldNotTriggerUpdate()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        bool updateTriggered = false;
        context.ScheduleUpdate = _ => updateTriggered = true;
        var hook = new UseStateHook<int>(context, 42);

        // Act
        hook.SetValue(42);

        // Assert
        Assert.False(updateTriggered);
    }

    [Fact]
    public void StateAccessor_ShouldProvideConvenientAPI()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        var hook = new UseStateHook<string>(context, "test");
        var accessor = new StateAccessor<string>(hook);

        // Act & Assert
        Assert.Equal("test", accessor.Value);

        accessor.SetValue("new value");
        Assert.Equal("new value", accessor.Value);

        accessor.UpdateValue(v => v.ToUpper());
        Assert.Equal("NEW VALUE", accessor.Value);
    }

    [Fact]
    public void StateAccessor_Deconstruct_ShouldWork()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        var hook = new UseStateHook<int>(context, 100);
        var accessor = new StateAccessor<int>(hook);

        // Act
        var (value, setValue) = accessor;

        // Assert
        Assert.Equal(100, value);
        setValue(200);
        Assert.Equal(200, accessor.Value);
    }

    [Fact]
    public void StateAccessor_ImplicitConversion_ShouldWork()
    {
        // Arrange
        var context = new HookContext("TestComponent");
        var hook = new UseStateHook<double>(context, 3.14);
        var accessor = new StateAccessor<double>(hook);

        // Act
        double value = accessor;

        // Assert
        Assert.Equal(3.14, value);
    }
}