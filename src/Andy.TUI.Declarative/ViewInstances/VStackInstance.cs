using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a VStack view with child management.
/// VStack is a simplified Box with flexDirection: column.
/// </summary>
public class VStackInstance : ViewInstance
{
    private int _spacing;
    private readonly List<ViewInstance> _childInstances = new();
    
    public VStackInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not VStack vstack)
            throw new ArgumentException("Expected VStack declaration");
        
        _spacing = vstack.GetSpacing();
        
        // Update child instances
        var children = vstack.GetChildren();
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
        
        // First pass: calculate sizes for non-spacer children and identify spacers
        float totalFixedHeight = 0;
        float maxWidth = 0;
        var spacerIndices = new List<int>();
        var childLayouts = new List<LayoutBox>();
        
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            
            if (child is SpacerInstance spacer)
            {
                spacerIndices.Add(i);
                // Give spacer minimum size for now
                var minLength = spacer.MinLength?.ToPixels(constraints.MaxHeight) ?? 0;
                childLayouts.Add(new LayoutBox { X = 0, Y = 0, Width = 0, Height = minLength });
                totalFixedHeight += minLength;
            }
            else
            {
                // For VStack, non-spacer children get constrained width but unconstrained height
                var childConstraints = new LayoutConstraints(
                    constraints.MinWidth, constraints.MaxWidth,
                    0, float.PositiveInfinity
                );
                
                child.CalculateLayout(childConstraints);
                var childLayout = child.Layout;
                childLayouts.Add(childLayout);
                
                totalFixedHeight += childLayout.Height;
                maxWidth = Math.Max(maxWidth, childLayout.Width);
            }
        }
        
        // Add spacing
        float totalSpacing = 0;
        if (_childInstances.Count > 1)
        {
            totalSpacing = _spacing * (_childInstances.Count - 1);
            totalFixedHeight += totalSpacing;
        }
        
        // Calculate available space for spacers
        float availableHeight = constraints.MaxHeight;
        float remainingSpace = Math.Max(0, availableHeight - totalFixedHeight);
        
        // Distribute remaining space among spacers
        if (spacerIndices.Count > 0 && remainingSpace > 0)
        {
            float spacePerSpacer = remainingSpace / spacerIndices.Count;
            foreach (var index in spacerIndices)
            {
                childLayouts[index].Height += spacePerSpacer;
            }
        }
        
        // Calculate final dimensions
        float finalHeight = totalFixedHeight + remainingSpace;
        layout.Width = constraints.ConstrainWidth(maxWidth);
        layout.Height = constraints.ConstrainHeight(finalHeight);
        
        // Position children
        float currentY = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var childLayout = childLayouts[i];
            
            // Update child's layout with final size
            child.Layout.Width = childLayout.Width;
            child.Layout.Height = childLayout.Height;
            
            // Left-align children by default
            child.Layout.X = 0;
            child.Layout.Y = currentY;
            
            currentY += childLayout.Height;
            if (i < _childInstances.Count - 1)
            {
                currentY += _spacing;
            }
        }
        
        return layout;
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_childInstances.Count == 0)
            return Fragment();
        
        var childElements = new List<VirtualNode>();
        
        foreach (var child in _childInstances)
        {
            // Update child's absolute position
            child.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(child.Layout.X);
            child.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(child.Layout.Y);
            
            // Render child with its layout
            var childNode = child.Render();
            childElements.Add(childNode);
        }
        
        return Fragment(childElements.ToArray());
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}