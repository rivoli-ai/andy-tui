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
        
        // First pass: calculate natural sizes and identify spacers/flex items
        float totalNaturalHeight = 0;
        float maxWidth = 0;
        var spacerIndices = new List<int>();
        var childInfos = new List<(ViewInstance instance, float naturalHeight, float flexGrow, float flexShrink, bool isSpacer)>();
        
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            
            if (child is SpacerInstance spacer)
            {
                spacerIndices.Add(i);
                // Give spacer minimum size for now
                var minLength = spacer.MinLength?.ToPixels(constraints.MaxHeight) ?? 0;
                childInfos.Add((child, minLength, 1f, 0, true)); // Spacers have implicit flexGrow=1
                totalNaturalHeight += minLength;
            }
            else
            {
                // Get flex properties if this is a Box
                var flexGrow = 0f;
                var flexShrink = 1f;
                var flexBasis = Length.Auto;
                float naturalHeight;
                
                if (child is BoxInstance boxChild && boxChild.GetBox() != null)
                {
                    var box = boxChild.GetBox()!;
                    flexGrow = box.FlexGrow;
                    flexShrink = box.FlexShrink;
                    flexBasis = box.FlexBasis;
                }
                
                // Calculate natural height based on flex-basis or content
                if (!flexBasis.IsAuto)
                {
                    naturalHeight = flexBasis.ToPixels(constraints.MaxHeight);
                }
                else
                {
                    // For VStack, non-spacer children get constrained width but unconstrained height
                    var childConstraints = new LayoutConstraints(
                        constraints.MinWidth, constraints.MaxWidth,
                        0, float.PositiveInfinity
                    );
                    
                    child.CalculateLayout(childConstraints);
                    naturalHeight = child.Layout.Height;
                }
                
                childInfos.Add((child, naturalHeight, flexGrow, flexShrink, false));
                
                totalNaturalHeight += naturalHeight;
                
                // Measure width separately if needed
                if (flexBasis.IsAuto)
                {
                    maxWidth = Math.Max(maxWidth, child.Layout.Width);
                }
                else
                {
                    // Need to measure for width
                    var childConstraints = new LayoutConstraints(
                        constraints.MinWidth, constraints.MaxWidth,
                        naturalHeight, naturalHeight
                    );
                    child.CalculateLayout(childConstraints);
                    maxWidth = Math.Max(maxWidth, child.Layout.Width);
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
        float totalRequiredHeight = totalNaturalHeight + totalSpacing;
        float availableHeight = constraints.MaxHeight;
        float remainingSpace = availableHeight - totalRequiredHeight;
        
        // Calculate final heights
        var finalHeights = new List<float>();
        
        if (remainingSpace < 0 && totalNaturalHeight > 0)
        {
            // Need to shrink - calculate total weighted shrink
            float totalWeightedShrink = 0;
            foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                if (!isSpacer)
                {
                    totalWeightedShrink += naturalHeight * flexShrink;
                }
            }
            
            // Shrink non-spacer items proportionally
            var shrinkAmount = Math.Abs(remainingSpace);
            foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                if (isSpacer)
                {
                    finalHeights.Add(naturalHeight); // Spacers keep minimum size when shrinking
                }
                else if (totalWeightedShrink > 0 && flexShrink > 0)
                {
                    // Only shrink items that have flexShrink > 0
                    var shrinkProportion = (naturalHeight * flexShrink) / totalWeightedShrink;
                    var shrinkValue = shrinkAmount * shrinkProportion;
                    var finalHeight = Math.Max(0, naturalHeight - shrinkValue);
                    finalHeights.Add(finalHeight);
                }
                else
                {
                    // Keep natural height for items with flexShrink = 0 (fixed size)
                    finalHeights.Add(naturalHeight);
                }
            }
        }
        else if (remainingSpace > 0)
        {
            // Distribute remaining space among items with flexGrow > 0
            float totalFlexGrow = 0;
            foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                totalFlexGrow += flexGrow;
            }
            
            if (totalFlexGrow > 0)
            {
                // Distribute based on flex grow
                foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
                {
                    var growAmount = (remainingSpace * flexGrow) / totalFlexGrow;
                    finalHeights.Add(naturalHeight + growAmount);
                }
            }
            else
            {
                // No flex items, just use natural heights
                foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
                {
                    finalHeights.Add(naturalHeight);
                }
            }
        }
        else
        {
            // No adjustment needed
            foreach (var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) in childInfos)
            {
                finalHeights.Add(naturalHeight);
            }
        }
        
        // Calculate final dimensions
        float finalTotalHeight = finalHeights.Sum() + totalSpacing;
        layout.Width = constraints.ConstrainWidth(maxWidth);
        layout.Height = constraints.ConstrainHeight(finalTotalHeight);
        
        // Position children with final sizes
        float currentY = 0;
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var finalHeight = finalHeights[i];
            var (instance, naturalHeight, flexGrow, flexShrink, isSpacer) = childInfos[i];
            
            // Recalculate child with final height constraint
            var childConstraints = new LayoutConstraints(
                constraints.MinWidth, constraints.MaxWidth,
                finalHeight, finalHeight
            );
            child.CalculateLayout(childConstraints);
            
            // Left-align children by default
            child.Layout.X = 0;
            child.Layout.Y = currentY;
            
            currentY += finalHeight;
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