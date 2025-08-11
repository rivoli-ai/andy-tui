using Xunit;
using Andy.TUI.Core.Rendering;

namespace Andy.TUI.Core.Tests.Rendering;

public class IZIndexAwareTests
{
    private class TestZIndexAware : IZIndexAware
    {
        public int RelativeZIndex { get; set; }
        public int AbsoluteZIndex { get; private set; }
        public bool CreatesStackingContext { get; set; }

        public void UpdateAbsoluteZIndex(ZIndexContext context)
        {
            AbsoluteZIndex = context.ResolveAbsolute(RelativeZIndex);
        }
    }

    private class ComplexZIndexAware : IZIndexAware
    {
        public int RelativeZIndex { get; set; }
        public int AbsoluteZIndex { get; private set; }
        public bool CreatesStackingContext { get; set; }
        public string Name { get; set; } = "";

        public ComplexZIndexAware(int relativeZIndex, string name, bool createsStackingContext = false)
        {
            RelativeZIndex = relativeZIndex;
            Name = name;
            CreatesStackingContext = createsStackingContext;
        }

        public void UpdateAbsoluteZIndex(ZIndexContext context)
        {
            AbsoluteZIndex = context.ResolveAbsolute(RelativeZIndex);
        }
    }

    [Fact]
    public void IZIndexAware_CanBeImplemented()
    {
        var instance = new TestZIndexAware { RelativeZIndex = 10 };

        Assert.Equal(10, instance.RelativeZIndex);

        // Test interface reference
        IZIndexAware aware = instance;
        Assert.Equal(10, aware.RelativeZIndex);
    }

    [Fact]
    public void RelativeZIndex_CanBeModified()
    {
        var instance = new TestZIndexAware { RelativeZIndex = 5 };

        Assert.Equal(5, instance.RelativeZIndex);

        instance.RelativeZIndex = 15;
        Assert.Equal(15, instance.RelativeZIndex);

        instance.RelativeZIndex = -10;
        Assert.Equal(-10, instance.RelativeZIndex);
    }

    [Fact]
    public void MultipleImplementations_MaintainIndependentZIndices()
    {
        var first = new TestZIndexAware { RelativeZIndex = 100 };
        var second = new TestZIndexAware { RelativeZIndex = 200 };
        var third = new ComplexZIndexAware(300, "Third");

        Assert.Equal(100, first.RelativeZIndex);
        Assert.Equal(200, second.RelativeZIndex);
        Assert.Equal(300, third.RelativeZIndex);

        first.RelativeZIndex = 150;
        Assert.Equal(150, first.RelativeZIndex);
        Assert.Equal(200, second.RelativeZIndex);
        Assert.Equal(300, third.RelativeZIndex);
    }

    [Fact]
    public void ZIndexAwareCollection_CanBeSorted()
    {
        var items = new IZIndexAware[]
        {
            new TestZIndexAware { RelativeZIndex = 5 },
            new ComplexZIndexAware(1, "First"),
            new TestZIndexAware { RelativeZIndex = 10 },
            new ComplexZIndexAware(3, "Third"),
            new TestZIndexAware { RelativeZIndex = -1 }
        };

        var sorted = items.OrderBy(x => x.RelativeZIndex).ToArray();

        Assert.Equal(-1, sorted[0].RelativeZIndex);
        Assert.Equal(1, sorted[1].RelativeZIndex);
        Assert.Equal(3, sorted[2].RelativeZIndex);
        Assert.Equal(5, sorted[3].RelativeZIndex);
        Assert.Equal(10, sorted[4].RelativeZIndex);
    }

    [Fact]
    public void UpdateAbsoluteZIndex_UpdatesBasedOnContext()
    {
        var context = new ZIndexContext();
        var aware = new TestZIndexAware { RelativeZIndex = 5 };

        context.EnterComponent(10, "Parent");
        aware.UpdateAbsoluteZIndex(context);

        Assert.Equal(15, aware.AbsoluteZIndex);

        context.EnterComponent(20, "Child");
        aware.UpdateAbsoluteZIndex(context);

        Assert.Equal(35, aware.AbsoluteZIndex);
    }

    [Fact]
    public void RelativeZIndex_DefaultValueIsZero()
    {
        var instance = new TestZIndexAware();

        Assert.Equal(0, instance.RelativeZIndex);
        Assert.Equal(0, instance.AbsoluteZIndex);
    }

    [Fact]
    public void ComplexHierarchy_WithMultipleZIndexAware()
    {
        var context = new ZIndexContext();

        var components = new[]
        {
            new ComplexZIndexAware(100, "Background"),
            new ComplexZIndexAware(200, "Content"),
            new ComplexZIndexAware(300, "Overlay"),
            new ComplexZIndexAware(1000, "Modal")
        };

        // Simulate entering a nested component hierarchy
        context.EnterComponent(1000, "App");

        // Update absolute z-indices for all components
        foreach (var component in components)
        {
            component.UpdateAbsoluteZIndex(context);
        }

        var sorted = components.OrderBy(c => c.AbsoluteZIndex).ToArray();

        Assert.Equal("Background", sorted[0].Name);
        Assert.Equal(1100, sorted[0].AbsoluteZIndex);

        Assert.Equal("Content", sorted[1].Name);
        Assert.Equal(1200, sorted[1].AbsoluteZIndex);

        Assert.Equal("Overlay", sorted[2].Name);
        Assert.Equal(1300, sorted[2].AbsoluteZIndex);

        Assert.Equal("Modal", sorted[3].Name);
        Assert.Equal(2000, sorted[3].AbsoluteZIndex);
    }

    [Fact]
    public void NegativeZIndex_PlacesBehind()
    {
        var behind = new TestZIndexAware { RelativeZIndex = -10 };
        var normal = new TestZIndexAware { RelativeZIndex = 0 };
        var front = new TestZIndexAware { RelativeZIndex = 10 };

        var items = new[] { front, behind, normal };
        var sorted = items.OrderBy(x => x.RelativeZIndex).ToArray();

        Assert.Same(behind, sorted[0]);
        Assert.Same(normal, sorted[1]);
        Assert.Same(front, sorted[2]);
    }

    [Fact]
    public void CreatesStackingContext_PropertyWorks()
    {
        var normalComponent = new TestZIndexAware { CreatesStackingContext = false };
        var stackingComponent = new TestZIndexAware { CreatesStackingContext = true };

        Assert.False(normalComponent.CreatesStackingContext);
        Assert.True(stackingComponent.CreatesStackingContext);

        var complexStacking = new ComplexZIndexAware(100, "Stacking", true);
        Assert.True(complexStacking.CreatesStackingContext);
    }

    [Fact]
    public void AbsoluteZIndex_UpdatesCorrectlyWithNestedContexts()
    {
        var context = new ZIndexContext();
        var component = new TestZIndexAware { RelativeZIndex = 5 };

        // Start with no context
        component.UpdateAbsoluteZIndex(context);
        Assert.Equal(5, component.AbsoluteZIndex);

        // Enter first level
        context.EnterComponent(100, "Level1");
        component.UpdateAbsoluteZIndex(context);
        Assert.Equal(105, component.AbsoluteZIndex);

        // Enter second level
        context.EnterComponent(50, "Level2");
        component.UpdateAbsoluteZIndex(context);
        Assert.Equal(155, component.AbsoluteZIndex);

        // Exit back to first level
        context.ExitComponent();
        component.UpdateAbsoluteZIndex(context);
        Assert.Equal(105, component.AbsoluteZIndex);

        // Exit completely
        context.ExitComponent();
        component.UpdateAbsoluteZIndex(context);
        Assert.Equal(5, component.AbsoluteZIndex);
    }
}