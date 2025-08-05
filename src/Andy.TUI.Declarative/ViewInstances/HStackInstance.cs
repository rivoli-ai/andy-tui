using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;
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
        
        // First pass: calculate natural sizes and identify spacers/flex items
        float totalNaturalWidth = 0;
        float maxHeight = 0;
        var spacerIndices = new List<int>();
        var childInfos = new List<(ViewInstance instance, float naturalWidth, float flexGrow, float flexShrink, bool isSpacer)>();
        
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            
            if (child is SpacerInstance spacer)
            {
                spacerIndices.Add(i);
                // Give spacer minimum size for now
                var minLength = spacer.MinLength?.ToPixels(constraints.MaxWidth) ?? 0;
                childInfos.Add((child, minLength, 1f, 0, true)); // Spacers have implicit flexGrow=1
                totalNaturalWidth += minLength;
            }
            else
            {
                // Get flex properties if this is a Box
                var flexGrow = 0f;
                var flexShrink = 1f;
                var flexBasis = Length.Auto;
                float naturalWidth;
                
                if (child is BoxInstance boxChild && boxChild.GetBox() != null)
                {
                    var box = boxChild.GetBox()!;
                    flexGrow = box.FlexGrow;
                    flexShrink = box.FlexShrink;
                    flexBasis = box.FlexBasis;
                }
                
                // Calculate natural width based on flex-basis or content
                if (!flexBasis.IsAuto)
                {
                    naturalWidth = flexBasis.ToPixels(constraints.MaxWidth);
                }
                else
                {
                    // For HStack, non-spacer children get unconstrained width but constrained height
                    var childConstraints = new LayoutConstraints(
                        0, float.PositiveInfinity,
                        constraints.MinHeight, constraints.MaxHeight
                    );
                    
                    child.CalculateLayout(childConstraints);
                    naturalWidth = child.Layout.Width;
                }
                
                childInfos.Add((child, naturalWidth, flexGrow, flexShrink, false));
                
                totalNaturalWidth += naturalWidth;
                
                // Measure height separately if needed
                if (flexBasis.IsAuto)
                {
                    maxHeight = Math.Max(maxHeight, child.Layout.Height);
                }
                else
                {
                    // Need to measure for height
                    var childConstraints = new LayoutConstraints(
                        naturalWidth, naturalWidth,
                        constraints.MinHeight, constraints.MaxHeight
                    );
                    child.CalculateLayout(childConstraints);
                    maxHeight = Math.Max(maxHeight, child.Layout.Height);
                }
            }
        }
        
        // Add spacing
        float totalSpacing = 0;
        if (_childInstances.Count > 1)
        {
            totalSpacing = _spacing * (_childInstances.Count - 1);
        }
        
        // Calculate if we need to shrink or grow
        float totalRequiredWidth = totalNaturalWidth + totalSpacing;
        float availableWidth = constraints.MaxWidth;
        float remainingSpace = availableWidth - totalRequiredWidth;
        
        // Calculate final widths
        var finalWidths = new List<float>();
        
        if (remainingSpace < 0 && totalNaturalWidth > 0)
        {
            // Need to shrink - calculate total weighted shrink
            float totalWeightedShrink = 0;
            foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                if (!isSpacer)
                {
                    totalWeightedShrink += naturalWidth * flexShrink;
                }
            }
            
            // Shrink non-spacer items proportionally
            var shrinkAmount = Math.Abs(remainingSpace);
            foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                if (isSpacer)
                {
                    finalWidths.Add(naturalWidth); // Spacers keep minimum size when shrinking
                }
                else if (totalWeightedShrink > 0)
                {
                    var shrinkProportion = (naturalWidth * flexShrink) / totalWeightedShrink;
                    var shrinkValue = shrinkAmount * shrinkProportion;
                    var finalWidth = Math.Max(0, naturalWidth - shrinkValue);
                    finalWidths.Add(finalWidth);
                }
                else
                {
                    finalWidths.Add(naturalWidth);
                }
            }
        }
        else if (remainingSpace > 0)
        {
            // Distribute remaining space among items with flexGrow > 0
            float totalFlexGrow = 0;
            foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                totalFlexGrow += flexGrow;
            }
            
            if (totalFlexGrow > 0)
            {
                // Distribute based on flex grow
                foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
                {
                    var growAmount = (remainingSpace * flexGrow) / totalFlexGrow;
                    finalWidths.Add(naturalWidth + growAmount);
                }
            }
            else
            {
                // No flex items, just use natural widths
                foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
                {
                    finalWidths.Add(naturalWidth);
                }
            }
        }
        else
        {
            // No adjustment needed
            foreach (var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                finalWidths.Add(naturalWidth);
            }
        }
        
        // Calculate final dimensions
        float finalTotalWidth = finalWidths.Sum() + totalSpacing;
        layout.Width = constraints.ConstrainWidth(finalTotalWidth);
        layout.Height = constraints.ConstrainHeight(maxHeight);
        
        // Position children with final sizes
        float currentX = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var finalWidth = finalWidths[i];
            var (instance, naturalWidth, flexGrow, flexShrink, isSpacer) = childInfos[i];
            
            // Recalculate child with final width constraint
            var childConstraints = new LayoutConstraints(
                finalWidth, finalWidth,
                constraints.MinHeight, constraints.MaxHeight
            );
            child.CalculateLayout(childConstraints);
            
            // Center children vertically if they're smaller than the container
            child.Layout.X = currentX;
            child.Layout.Y = (layout.Height - child.Layout.Height) / 2;
            
            currentX += finalWidth;
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