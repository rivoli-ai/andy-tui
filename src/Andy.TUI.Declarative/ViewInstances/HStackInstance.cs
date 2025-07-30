using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of an HStack view with child management.
/// HStack is a simplified Box with flexDirection: row.
/// </summary>
public class HStackInstance : ViewInstance
{
    private int _spacing;
    private readonly List<ViewInstance> _childInstances = new();
    
    public HStackInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not HStack hstack)
            throw new ArgumentException("Expected HStack declaration");
        
        _spacing = hstack.GetSpacing();
        
        // Update child instances
        var children = hstack.GetChildren();
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
        float totalFixedWidth = 0;
        float maxHeight = 0;
        var spacerIndices = new List<int>();
        var childLayouts = new List<LayoutBox>();
        
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            
            if (child is SpacerInstance spacer)
            {
                spacerIndices.Add(i);
                // Give spacer minimum size for now
                var minLength = spacer.MinLength?.Resolve(constraints.MaxWidth) ?? 0;
                childLayouts.Add(new LayoutBox { Width = minLength, Height = 0 });
                totalFixedWidth += minLength;
            }
            else
            {
                // For HStack, non-spacer children get unconstrained width but constrained height
                var childConstraints = new LayoutConstraints(
                    0, float.PositiveInfinity,
                    constraints.MinHeight, constraints.MaxHeight
                );
                
                child.CalculateLayout(childConstraints);
                var childLayout = child.Layout;
                childLayouts.Add(childLayout);
                
                totalFixedWidth += childLayout.Width;
                maxHeight = Math.Max(maxHeight, childLayout.Height);
            }
        }
        
        // Add spacing
        float totalSpacing = 0;
        if (_childInstances.Count > 1)
        {
            totalSpacing = _spacing * (_childInstances.Count - 1);
            totalFixedWidth += totalSpacing;
        }
        
        // Calculate available space for spacers
        float availableWidth = constraints.MaxWidth;
        float remainingSpace = Math.Max(0, availableWidth - totalFixedWidth);
        
        // Distribute remaining space among spacers
        if (spacerIndices.Count > 0 && remainingSpace > 0)
        {
            float spacePerSpacer = remainingSpace / spacerIndices.Count;
            foreach (var index in spacerIndices)
            {
                childLayouts[index].Width += spacePerSpacer;
            }
        }
        
        // Calculate final dimensions
        float finalWidth = totalFixedWidth + remainingSpace;
        layout.Width = constraints.ConstrainWidth(finalWidth);
        layout.Height = constraints.ConstrainHeight(maxHeight);
        
        // Position children
        float currentX = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var childLayout = childLayouts[i];
            
            // Update child's layout with final size
            child.Layout.Width = childLayout.Width;
            child.Layout.Height = childLayout.Height;
            
            // Center children vertically if they're smaller than the container
            child.Layout.X = currentX;
            child.Layout.Y = (layout.Height - childLayout.Height) / 2;
            
            currentX += childLayout.Width;
            if (i < _childInstances.Count - 1)
            {
                currentX += _spacing;
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