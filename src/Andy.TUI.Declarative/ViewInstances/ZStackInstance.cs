using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance of a ZStack view that overlays children.
/// </summary>
public class ZStackInstance : ViewInstance
{
    private AlignItems _alignment = AlignItems.Center;
    private readonly List<ViewInstance> _childInstances = new();

    public ZStackInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not ZStack zstack)
            throw new ArgumentException("Expected ZStack declaration");

        _alignment = zstack.GetAlignment();

        // Update child instances
        var children = zstack.GetChildren();
        var manager = Context?.ViewInstanceManager;

        if (manager != null)
        {
            _childInstances.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                var childPath = $"{Id}/{i}";
                var childInstance = manager.GetOrCreateInstance(children[i], childPath);
                _childInstances.Add(childInstance);
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();

        if (_childInstances.Count == 0)
        {
            layout.Width = 0;
            layout.Height = 0;
            return layout;
        }

        // ZStack sizes itself to fit the largest child
        float maxWidth = 0;
        float maxHeight = 0;

        // Calculate size of each child and find the maximum
        foreach (var child in _childInstances)
        {
            child.CalculateLayout(constraints);
            maxWidth = Math.Max(maxWidth, child.Layout.Width);
            maxHeight = Math.Max(maxHeight, child.Layout.Height);
        }

        // Set our size to the largest child
        layout.Width = constraints.ConstrainWidth(maxWidth);
        layout.Height = constraints.ConstrainHeight(maxHeight);

        // Position children according to alignment
        foreach (var child in _childInstances)
        {
            // Calculate position based on alignment
            float x = 0;
            float y = 0;

            switch (_alignment)
            {
                case AlignItems.FlexStart:
                    // Top-left alignment (default position)
                    x = 0;
                    y = 0;
                    break;

                case AlignItems.FlexEnd:
                    // Bottom-right alignment
                    x = layout.Width - child.Layout.Width;
                    y = layout.Height - child.Layout.Height;
                    break;

                case AlignItems.Center:
                    // Center alignment
                    x = (layout.Width - child.Layout.Width) / 2;
                    y = (layout.Height - child.Layout.Height) / 2;
                    break;

                case AlignItems.Stretch:
                    // Stretch to fill the container
                    x = 0;
                    y = 0;
                    // Recalculate child with tight constraints to fill container
                    var stretchConstraints = LayoutConstraints.Tight(layout.Width, layout.Height);
                    child.CalculateLayout(stretchConstraints);
                    break;
            }

            child.Layout.X = x;
            child.Layout.Y = y;
        }

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_childInstances.Count == 0)
            return Fragment();

        var elements = new List<VirtualNode>();

        // Render children in order (later children appear on top)
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];

            // Update child's absolute position
            child.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(child.Layout.X);
            child.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(child.Layout.Y);

            // Render child with z-index based on order
            var childNode = child.Render();

            // Apply z-index to ensure proper layering
            if (childNode is ElementNode element)
            {
                var props = new Dictionary<string, object?>(element.Props)
                {
                    ["z-index"] = i // Later children have higher z-index
                };
                childNode = new ElementNode(element.TagName, props, element.Children.ToArray());
            }

            elements.Add(childNode);
        }

        return Fragment(elements.ToArray());
    }

    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}