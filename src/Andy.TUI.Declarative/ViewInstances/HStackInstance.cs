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
        
        // Calculate total width needed
        float totalWidth = 0;
        float maxHeight = 0;
        
        // First pass: calculate child sizes
        var childLayouts = new List<LayoutBox>();
        foreach (var child in _childInstances)
        {
            // For HStack, children get unconstrained width but constrained height
            var childConstraints = new LayoutConstraints(
                0, float.PositiveInfinity,
                constraints.MinHeight, constraints.MaxHeight
            );
            
            child.CalculateLayout(childConstraints);
            var childLayout = child.Layout;
            childLayouts.Add(childLayout);
            
            totalWidth += childLayout.Width;
            maxHeight = Math.Max(maxHeight, childLayout.Height);
        }
        
        // Add spacing
        if (_childInstances.Count > 1)
        {
            totalWidth += _spacing * (_childInstances.Count - 1);
        }
        
        // Constrain to available space
        layout.Width = constraints.ConstrainWidth(totalWidth);
        layout.Height = constraints.ConstrainHeight(maxHeight);
        
        // Position children
        float currentX = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var childLayout = child.Layout;
            
            // Center children vertically if they're smaller than the container
            childLayout.X = currentX;
            childLayout.Y = (layout.Height - childLayout.Height) / 2;
            
            currentX += childLayout.Width + _spacing;
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