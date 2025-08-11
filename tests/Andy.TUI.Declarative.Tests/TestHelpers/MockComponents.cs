using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.ViewInstances;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

/// <summary>
/// A component with fixed dimensions for testing.
/// </summary>
public class FixedSizeComponent : ISimpleComponent
{
    public float Width { get; set; } = 100;
    public float Height { get; set; } = 50;
    public string Id { get; set; } = "fixed";

    public ISimpleComponent Copy() => new FixedSizeComponent
    {
        Width = Width,
        Height = Height,
        Id = Id
    };

    public VirtualNode Render() => Fragment(); // Components don't render themselves
}

/// <summary>
/// ViewInstance for FixedSizeComponent.
/// </summary>
public class FixedSizeInstance : ViewInstance
{
    private FixedSizeComponent _component = null!;

    public FixedSizeInstance(string id) : base(id) { }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        _component = viewDeclaration as FixedSizeComponent
            ?? throw new ArgumentException("Expected FixedSizeComponent");
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        return new LayoutBox
        {
            Width = constraints.ConstrainWidth(_component.Width),
            Height = constraints.ConstrainHeight(_component.Height)
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        return Fragment(); // Empty render for testing
    }
}

/// <summary>
/// A component that sizes itself based on content.
/// </summary>
public class AutoSizeComponent : ISimpleComponent
{
    public float? PreferredWidth { get; set; }
    public float? PreferredHeight { get; set; }
    public float MinWidth { get; set; } = 0;
    public float MinHeight { get; set; } = 0;
    public string Id { get; set; } = "auto";

    public ISimpleComponent Copy() => new AutoSizeComponent
    {
        PreferredWidth = PreferredWidth,
        PreferredHeight = PreferredHeight,
        MinWidth = MinWidth,
        MinHeight = MinHeight,
        Id = Id
    };

    public VirtualNode Render() => Fragment(); // Components don't render themselves
}

/// <summary>
/// ViewInstance for AutoSizeComponent.
/// </summary>
public class AutoSizeInstance : ViewInstance
{
    private AutoSizeComponent _component = null!;

    public AutoSizeInstance(string id) : base(id) { }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        _component = viewDeclaration as AutoSizeComponent
            ?? throw new ArgumentException("Expected AutoSizeComponent");
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Use preferred size if available, otherwise use minimum
        var width = _component.PreferredWidth ?? _component.MinWidth;
        var height = _component.PreferredHeight ?? _component.MinHeight;

        // If constraints are infinite, use preferred size
        if (float.IsPositiveInfinity(constraints.MaxWidth))
        {
            width = _component.PreferredWidth ?? _component.MinWidth;
        }

        if (float.IsPositiveInfinity(constraints.MaxHeight))
        {
            height = _component.PreferredHeight ?? _component.MinHeight;
        }

        return new LayoutBox
        {
            Width = constraints.ConstrainWidth(width),
            Height = constraints.ConstrainHeight(height)
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        return Fragment(); // Empty render for testing
    }
}

/// <summary>
/// A test container that can hold children.
/// </summary>
public class TestContainer : ISimpleComponent
{
    public List<ISimpleComponent> Children { get; set; } = new();
    public string Id { get; set; } = "container";

    public ISimpleComponent Copy() => new TestContainer
    {
        Children = Children.Select(c => c).ToList(), // Shallow copy for testing
        Id = Id
    };

    public VirtualNode Render() => Fragment(); // Components don't render themselves
}

/// <summary>
/// ViewInstance for TestContainer that implements IContainerInstance.
/// </summary>
public class TestContainerInstance : ViewInstance, IContainerInstance
{
    private TestContainer _component = null!;
    private readonly List<ViewInstance> _childInstances = new();

    public TestContainerInstance(string id) : base(id) { }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        _component = viewDeclaration as TestContainer
            ?? throw new ArgumentException("Expected TestContainer");

        // Update child instances
        var testContext = Context as TestDeclarativeContext;
        if (testContext != null)
        {
            _childInstances.Clear();
            for (int i = 0; i < _component.Children.Count; i++)
            {
                var childPath = $"{Id}/{i}";
                var childInstance = testContext.GetTestInstance(_component.Children[i], childPath);
                _childInstances.Add(childInstance);
            }
        }
        else if (Context != null)
        {
            // Fallback for non-test contexts
            _childInstances.Clear();
            for (int i = 0; i < _component.Children.Count; i++)
            {
                var childPath = $"{Id}/{i}";
                var childInstance = Context.ViewInstanceManager.GetOrCreateInstance(_component.Children[i], childPath);
                _childInstances.Add(childInstance);
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();
        float maxWidth = 0;
        float totalHeight = 0;

        // Simple vertical stacking for testing
        foreach (var child in _childInstances)
        {
            child.CalculateLayout(constraints);
            child.Layout.Y = totalHeight;

            maxWidth = Math.Max(maxWidth, child.Layout.Width);
            totalHeight += child.Layout.Height;
        }

        layout.Width = constraints.ConstrainWidth(maxWidth);
        layout.Height = constraints.ConstrainHeight(totalHeight);

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var childNodes = new List<VirtualNode>();

        foreach (var child in _childInstances)
        {
            // Update absolute positions
            child.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(child.Layout.X);
            child.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(child.Layout.Y);

            childNodes.Add(child.Render());
        }

        return Fragment(childNodes.ToArray());
    }

    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}

/// <summary>
/// A component that reports extreme values for testing edge cases.
/// </summary>
public class ExtremeValueComponent : ISimpleComponent
{
    public enum SizeMode
    {
        Zero,
        Infinite,
        NaN,
        Normal
    }

    public SizeMode WidthMode { get; set; } = SizeMode.Normal;
    public SizeMode HeightMode { get; set; } = SizeMode.Normal;
    public float NormalWidth { get; set; } = 100;
    public float NormalHeight { get; set; } = 50;
    public string Id { get; set; } = "extreme";

    public ISimpleComponent Copy() => new ExtremeValueComponent
    {
        WidthMode = WidthMode,
        HeightMode = HeightMode,
        NormalWidth = NormalWidth,
        NormalHeight = NormalHeight,
        Id = Id
    };

    public VirtualNode Render() => Fragment(); // Components don't render themselves
}

/// <summary>
/// ViewInstance for ExtremeValueComponent.
/// </summary>
public class ExtremeValueInstance : ViewInstance
{
    private ExtremeValueComponent _component = null!;

    public ExtremeValueInstance(string id) : base(id) { }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        _component = viewDeclaration as ExtremeValueComponent
            ?? throw new ArgumentException("Expected ExtremeValueComponent");
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var width = GetSizeValue(_component.WidthMode, _component.NormalWidth);
        var height = GetSizeValue(_component.HeightMode, _component.NormalHeight);

        // Handle NaN by converting to 0
        if (float.IsNaN(width)) width = 0;
        if (float.IsNaN(height)) height = 0;

        return new LayoutBox
        {
            Width = constraints.ConstrainWidth(width),
            Height = constraints.ConstrainHeight(height)
        };
    }

    private float GetSizeValue(ExtremeValueComponent.SizeMode mode, float normalValue)
    {
        return mode switch
        {
            ExtremeValueComponent.SizeMode.Zero => 0,
            ExtremeValueComponent.SizeMode.Infinite => float.PositiveInfinity,
            ExtremeValueComponent.SizeMode.NaN => float.NaN,
            _ => normalValue
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        return Fragment(); // Empty render for testing
    }
}

/// <summary>
/// Factory for creating mock ViewInstances for testing.
/// </summary>
public static class MockViewInstanceFactory
{
    public static void RegisterMockComponents(ViewInstanceManager manager)
    {
        // ViewInstanceManager doesn't have RegisterFactory, we need to handle this differently
        // For now, we'll modify the GetOrCreateInstance calls to handle our mock types
    }
}