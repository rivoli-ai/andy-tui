using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Box component with flexbox layout calculation.
/// </summary>
public class BoxInstance : ViewInstance
{
    private Box? _box;
    private readonly List<ViewInstance> _childInstances = new();
    
    public BoxInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Box box)
            throw new ArgumentException("Expected Box declaration");
        
        _box = box;
        
        // Update child instances
        var children = box.GetChildren();
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
        if (_box == null) return base.PerformLayout(constraints);
        
        var layout = new LayoutBox
        {
            Padding = _box.Padding,
            Margin = _box.Margin
        };
        
        // Calculate tentative size based on width/height properties
        var width = CalculateSize(_box.Width, constraints.MinWidth, constraints.MaxWidth, constraints.MaxWidth);
        var height = CalculateSize(_box.Height, constraints.MinHeight, constraints.MaxHeight, constraints.MaxHeight);
        
        // Apply min/max constraints
        if (!_box.MinWidth.IsAuto)
            width = Math.Max(width, _box.MinWidth.ToPixels(constraints.MaxWidth));
        if (!_box.MaxWidth.IsAuto)
            width = Math.Min(width, _box.MaxWidth.ToPixels(constraints.MaxWidth));
        if (!_box.MinHeight.IsAuto)
            height = Math.Max(height, _box.MinHeight.ToPixels(constraints.MaxHeight));
        if (!_box.MaxHeight.IsAuto)
            height = Math.Min(height, _box.MaxHeight.ToPixels(constraints.MaxHeight));
        
        // Set tentative dimensions
        layout.Width = width;
        layout.Height = height;
        
        // Layout children with current dimensions
        if (_childInstances.Count > 0)
        {
            LayoutChildren(layout, constraints);
            
            // If width/height is auto, adjust to fit content
            if (_box.Width.IsAuto)
            {
                var contentWidth = _childInstances.Count > 0 
                    ? _childInstances.Max(c => c.Layout.X + c.Layout.Width) 
                    : 0;
                layout.Width = contentWidth + _box.Padding.Left.Value + _box.Padding.Right.Value;
            }
            if (_box.Height.IsAuto)
            {
                var contentHeight = _childInstances.Count > 0 
                    ? _childInstances.Max(c => c.Layout.Y + c.Layout.Height)
                    : 0;
                layout.Height = contentHeight + _box.Padding.Top.Value + _box.Padding.Bottom.Value;
            }
        }
        
        return layout;
    }
    
    private float CalculateSize(Length size, float min, float max, float parentSize)
    {
        if (size.IsAuto)
        {
            // Auto size - will be determined by content
            return min;
        }
        else if (size.IsPercentage)
        {
            return Math.Max(min, Math.Min(max, size.ToPixels(parentSize)));
        }
        else
        {
            return Math.Max(min, Math.Min(max, size.Value));
        }
    }
    
    private void LayoutChildren(LayoutBox parentLayout, LayoutConstraints constraints)
    {
        if (_box == null || _childInstances.Count == 0) return;
        
        // Calculate content area after padding
        var contentConstraints = constraints.Deflate(_box.Padding, parentLayout.Width, parentLayout.Height);
        
        var isRow = _box.FlexDirection == FlexDirection.Row || _box.FlexDirection == FlexDirection.RowReverse;
        var mainAxisSize = isRow ? contentConstraints.MaxWidth : contentConstraints.MaxHeight;
        var crossAxisSize = isRow ? contentConstraints.MaxHeight : contentConstraints.MaxWidth;
        
        // Simple flex layout implementation (can be enhanced with full Yoga algorithm)
        var gap = _box.Gap > 0 ? _box.Gap : (isRow ? _box.ColumnGap : _box.RowGap);
        var totalGap = gap * Math.Max(0, _childInstances.Count - 1);
        var availableMainSize = mainAxisSize - totalGap;
        
        // First pass: Calculate natural sizes and collect flex properties
        var childInfos = new List<(ViewInstance instance, float naturalSize, float flexGrow, float flexShrink, float flexBasis)>();
        var totalNaturalSize = 0f;
        var totalFlexGrow = 0f;
        var totalWeightedFlexShrink = 0f;
        
        foreach (var child in _childInstances)
        {
            var flexGrow = 0f;
            var flexShrink = 1f;
            var flexBasis = Length.Auto;
            
            if (child is BoxInstance boxChild && boxChild._box != null)
            {
                flexGrow = boxChild._box.FlexGrow;
                flexShrink = boxChild._box.FlexShrink;
                flexBasis = boxChild._box.FlexBasis;
            }
            
            // Calculate natural size (flex-basis or content size)
            float naturalSize;
            if (!flexBasis.IsAuto)
            {
                naturalSize = flexBasis.ToPixels(availableMainSize);
            }
            else
            {
                // Measure content
                // For text wrapping to work, we need to pass the actual available width
                // when the parent has a fixed width, not infinity
                var mainConstraint = isRow ? contentConstraints.MaxWidth : contentConstraints.MaxHeight;
                var childConstraints = isRow
                    ? LayoutConstraints.Loose(mainConstraint, crossAxisSize)
                    : LayoutConstraints.Loose(crossAxisSize, mainConstraint);
                
                child.CalculateLayout(childConstraints);
                naturalSize = isRow ? child.Layout.Width : child.Layout.Height;
            }
            
            childInfos.Add((child, naturalSize, flexGrow, flexShrink, naturalSize));
            totalNaturalSize += naturalSize;
            totalFlexGrow += flexGrow;
            totalWeightedFlexShrink += naturalSize * flexShrink; // Weighted by natural size
        }
        
        // Determine if we need to grow or shrink
        var totalSize = totalNaturalSize + totalGap;
        var availableSpace = availableMainSize;
        var needsToShrink = totalSize > availableSpace;
        var remainingSpace = availableSpace - totalSize;
        
        // Calculate final sizes
        var finalSizes = new List<float>();
        
        if (needsToShrink && totalWeightedFlexShrink > 0)
        {
            // Shrink items proportionally
            var shrinkAmount = Math.Abs(remainingSpace);
            
            foreach (var (instance, naturalSize, flexGrow, flexShrink, flexBasis) in childInfos)
            {
                var shrinkProportion = (naturalSize * flexShrink) / totalWeightedFlexShrink;
                var shrinkValue = shrinkAmount * shrinkProportion;
                var finalSize = Math.Max(0, naturalSize - shrinkValue);
                finalSizes.Add(finalSize);
            }
        }
        else if (!needsToShrink && totalFlexGrow > 0 && remainingSpace > 0)
        {
            // Grow items proportionally
            foreach (var (instance, naturalSize, flexGrow, flexShrink, flexBasis) in childInfos)
            {
                var growValue = flexGrow > 0 ? (remainingSpace * flexGrow / totalFlexGrow) : 0;
                var finalSize = naturalSize + growValue;
                finalSizes.Add(finalSize);
            }
        }
        else
        {
            // No grow or shrink needed
            foreach (var (instance, naturalSize, flexGrow, flexShrink, flexBasis) in childInfos)
            {
                finalSizes.Add(naturalSize);
            }
        }
        
        // Position children with final sizes
        var mainPos = 0f;
        var crossPos = 0f;
        
        for (int i = 0; i < _childInstances.Count; i++)
        {
            var child = _childInstances[i];
            var childMainSize = finalSizes[i];
            
            // Apply child constraints with final size
            var childConstraints = isRow
                ? LayoutConstraints.Tight(childMainSize, crossAxisSize)
                : LayoutConstraints.Tight(crossAxisSize, childMainSize);
            
            child.CalculateLayout(childConstraints);
            
            // Position child
            if (isRow)
            {
                child.Layout.X = mainPos;
                child.Layout.Y = crossPos;
            }
            else
            {
                child.Layout.X = crossPos;
                child.Layout.Y = mainPos;
            }
            
            mainPos += childMainSize;
            if (i < _childInstances.Count - 1)
            {
                mainPos += gap;
            }
        }
        
        // Apply justification and alignment
        ApplyJustification(parentLayout, isRow, mainPos);
        ApplyAlignment(parentLayout, isRow, crossAxisSize);
    }
    
    private void ApplyJustification(LayoutBox parentLayout, bool isRow, float totalMainSize)
    {
        if (_box == null || _childInstances.Count == 0) return;
        
        var availableSize = isRow ? parentLayout.Width : parentLayout.Height;
        var remainingSpace = availableSize - totalMainSize;
        
        if (remainingSpace <= 0) return;
        
        float offset = 0;
        float spacing = 0;
        
        switch (_box.JustifyContent)
        {
            case JustifyContent.FlexEnd:
                offset = remainingSpace;
                break;
            case JustifyContent.Center:
                offset = remainingSpace / 2;
                break;
            case JustifyContent.SpaceBetween:
                if (_childInstances.Count > 1)
                    spacing = remainingSpace / (_childInstances.Count - 1);
                break;
            case JustifyContent.SpaceAround:
                spacing = remainingSpace / _childInstances.Count;
                offset = spacing / 2;
                break;
            case JustifyContent.SpaceEvenly:
                spacing = remainingSpace / (_childInstances.Count + 1);
                offset = spacing;
                break;
        }
        
        // Apply offset and spacing
        foreach (var child in _childInstances)
        {
            if (isRow)
            {
                child.Layout.X += offset;
                offset += spacing;
            }
            else
            {
                child.Layout.Y += offset;
                offset += spacing;
            }
        }
    }
    
    private void ApplyAlignment(LayoutBox parentLayout, bool isRow, float crossAxisSize)
    {
        if (_box == null) return;
        
        foreach (var child in _childInstances)
        {
            var childCrossSize = isRow ? child.Layout.Height : child.Layout.Width;
            var alignSelf = AlignSelf.Auto;
            
            if (child is BoxInstance boxChild && boxChild._box != null)
            {
                alignSelf = boxChild._box.AlignSelf;
            }
            
            var alignment = alignSelf == AlignSelf.Auto ? _box.AlignItems : ConvertAlignSelf(alignSelf);
            
            float crossOffset = 0;
            switch (alignment)
            {
                case AlignItems.FlexEnd:
                    crossOffset = crossAxisSize - childCrossSize;
                    break;
                case AlignItems.Center:
                    crossOffset = (crossAxisSize - childCrossSize) / 2;
                    break;
                case AlignItems.Stretch:
                    if (isRow)
                        child.Layout.Height = crossAxisSize;
                    else
                        child.Layout.Width = crossAxisSize;
                    break;
            }
            
            if (isRow)
                child.Layout.Y += crossOffset;
            else
                child.Layout.X += crossOffset;
        }
    }
    
    private AlignItems ConvertAlignSelf(AlignSelf alignSelf)
    {
        return alignSelf switch
        {
            AlignSelf.FlexStart => AlignItems.FlexStart,
            AlignSelf.FlexEnd => AlignItems.FlexEnd,
            AlignSelf.Center => AlignItems.Center,
            AlignSelf.Stretch => AlignItems.Stretch,
            AlignSelf.Baseline => AlignItems.Baseline,
            _ => AlignItems.FlexStart
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_box == null || !_box.Display)
            return Fragment();
        
        var elements = new List<VirtualNode>();
        
        // Render children with their calculated positions
        foreach (var child in _childInstances)
        {
            var childNode = child.Render();
            
            // Apply absolute positioning based on layout
            var absoluteX = layout.ContentX + (int)Math.Round(child.Layout.X);
            var absoluteY = layout.ContentY + (int)Math.Round(child.Layout.Y);
            
            child.Layout.AbsoluteX = absoluteX;
            child.Layout.AbsoluteY = absoluteY;
            
            // Position the child node
            if (childNode is ElementNode element)
            {
                var props = new Dictionary<string, object?>(element.Props)
                {
                    ["x"] = absoluteX,
                    ["y"] = absoluteY
                };
                childNode = new ElementNode(element.TagName, props, element.Children.ToArray());
            }
            else if (childNode is FragmentNode fragment)
            {
                // For fragments, position each child element
                var fragmentChildren = new List<VirtualNode>();
                foreach (var fragChild in fragment.Children)
                {
                    if (fragChild is ElementNode fragElement)
                    {
                        var props = new Dictionary<string, object?>(fragElement.Props)
                        {
                            ["x"] = absoluteX + (int)(fragElement.Props.GetValueOrDefault("x") ?? 0),
                            ["y"] = absoluteY + (int)(fragElement.Props.GetValueOrDefault("y") ?? 0)
                        };
                        fragmentChildren.Add(new ElementNode(fragElement.TagName, props, fragElement.Children.ToArray()));
                    }
                    else
                    {
                        fragmentChildren.Add(fragChild);
                    }
                }
                childNode = Fragment(fragmentChildren.ToArray());
            }
            
            elements.Add(childNode);
        }
        
        // Apply overflow clipping if needed
        if (_box.Overflow == Overflow.Hidden && elements.Count > 0)
        {
            // Create a clipping node that constrains children to the box's content area
            var clipX = layout.ContentX;
            var clipY = layout.ContentY;
            var clipWidth = (int)Math.Round((double)layout.ContentWidth);
            var clipHeight = (int)Math.Round((double)layout.ContentHeight);
            
            return Clip(clipX, clipY, clipWidth, clipHeight, elements.ToArray());
        }
        
        return Fragment(elements.ToArray());
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
    
    internal Box? GetBox() => _box;
}