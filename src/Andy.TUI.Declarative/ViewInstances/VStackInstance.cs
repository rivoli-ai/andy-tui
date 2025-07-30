using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;
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
        
        // Calculate total height needed
        float totalHeight = 0;
        float maxWidth = 0;
        
        // First pass: calculate child sizes
        var childLayouts = new List<LayoutBox>();
        foreach (var child in _childInstances)
        {
            // For VStack, children get constrained width but unconstrained height
            var childConstraints = new LayoutConstraints(
                constraints.MinWidth, constraints.MaxWidth,
                0, float.PositiveInfinity
            );
            
            child.CalculateLayout(childConstraints);
            var childLayout = child.Layout;
            childLayouts.Add(childLayout);
            
            totalHeight += childLayout.Height;
            maxWidth = Math.Max(maxWidth, childLayout.Width);
        }
        
        // Add spacing
        if (_childInstances.Count > 1)
        {
            totalHeight += _spacing * (_childInstances.Count - 1);
        }
        
        // Constrain to available space
        layout.Width = constraints.ConstrainWidth(maxWidth);
        layout.Height = constraints.ConstrainHeight(totalHeight);
        
        // Position children
        float currentY = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var childLayout = child.Layout;
            
            // Left-align children by default
            childLayout.X = 0;
            childLayout.Y = currentY;
            
            currentY += childLayout.Height + _spacing;
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