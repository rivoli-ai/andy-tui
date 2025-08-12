using System;
using System.Collections.Generic;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Hooks;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Hooks;

public class HookableComponentTests
{
    [Fact]
    public void HookableComponent_UseState_ShouldMaintainState()
    {
        // Arrange
        var component = new TestStatefulComponent();

        // Act - First render
        component.Render();
        var count1 = component.GetCount();

        // Simulate state change
        component.TriggerIncrement();

        // Second render
        component.Render();
        var count2 = component.GetCount();

        // Assert
        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public void HookableComponent_UseEffect_ShouldRunAfterRender()
    {
        // Arrange
        var component = new TestEffectComponent();

        // Act
        component.Render();

        // Assert
        Assert.Equal(1, component.EffectRunCount);

        // Render again
        component.Render();
        Assert.Equal(2, component.EffectRunCount);
    }

    [Fact]
    public void HookableComponent_UseMemo_ShouldMemoizeValue()
    {
        // Arrange
        var component = new TestMemoComponent();

        // Act - First render
        component.Render();
        Assert.Equal(1, component.ComputeCount);

        // Second render with same dependency
        component.Render();
        Assert.Equal(1, component.ComputeCount); // Should not recompute

        // Change dependency
        component.ChangeDependency();
        component.Render();
        Assert.Equal(2, component.ComputeCount); // Should recompute
    }

    [Fact]
    public void HookableComponent_ConditionalHook_ShouldThrow()
    {
        // Arrange
        var component = new TestConditionalHookComponent();

        // Act - First render (no hooks)
        component.Render();

        // Change condition
        component.UseHook = true;

        // Assert - Second render with hook should throw
        Assert.Throws<InvalidOperationException>(() => component.Render());
    }

    [Fact]
    public void HookableComponent_UseRef_ShouldPersistAcrossRenders()
    {
        // Arrange
        var component = new TestRefComponent();

        // Act - First render
        component.Render();
        component.SetRefValue("test");

        // Second render
        component.Render();
        var value = component.GetRefValue();

        // Assert
        Assert.Equal("test", value);
    }

    private class TestStatefulComponent : HookableComponent
    {
        private Action? _increment;
        private int _currentCount;

        protected override ISimpleComponent Body()
        {
            var (count, setCount) = UseState(0);
            _currentCount = count;
            _increment = () => setCount(count + 1);

            return new TestRenderableComponent($"Count: {count}");
        }

        public void TriggerIncrement() => _increment?.Invoke();
        public int GetCount() => _currentCount;
    }

    private class TestEffectComponent : HookableComponent
    {
        public int EffectRunCount { get; private set; }

        protected override ISimpleComponent Body()
        {
            UseEffect(() => EffectRunCount++);
            return new TestRenderableComponent("Effect Test");
        }
    }

    private class TestMemoComponent : HookableComponent
    {
        public int ComputeCount { get; private set; }
        private int _dependency = 1;

        protected override ISimpleComponent Body()
        {
            var expensiveValue = UseMemo(() =>
            {
                ComputeCount++;
                return _dependency * 100;
            }, new object[] { _dependency });

            return new TestRenderableComponent($"Value: {expensiveValue}");
        }

        public void ChangeDependency() => _dependency++;
    }

    private class TestConditionalHookComponent : HookableComponent
    {
        public bool UseHook { get; set; }

        protected override ISimpleComponent Body()
        {
            if (UseHook)
            {
                var (value, _) = UseState(0);
                return new TestRenderableComponent($"Value: {value}");
            }
            return new TestRenderableComponent("No hook");
        }
    }

    private class TestRefComponent : HookableComponent
    {
        private RefObject<string>? _ref;

        protected override ISimpleComponent Body()
        {
            _ref = UseRef<string>();
            return new TestRenderableComponent("Ref Test");
        }

        public void SetRefValue(string value)
        {
            if (_ref != null)
                _ref.Current = value;
        }

        public string? GetRefValue() => _ref?.Current;
    }

    // Simple component that can actually render for testing
    private class TestRenderableComponent : ISimpleComponent
    {
        private readonly string _text;

        public TestRenderableComponent(string text)
        {
            _text = text;
        }

        public VirtualNode Render()
        {
            return VirtualDomBuilder.Fragment(new TextNode(_text));
        }
    }
}