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
        
        // Calculate own size based on width/height properties
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
        
        layout.Width = width;
        layout.Height = height;
        
        // Layout children using flexbox algorithm
        if (_childInstances.Count > 0)
        {
            LayoutChildren(layout, constraints);
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
        
        // Calculate flex items
        var totalFlexGrow = 0f;
        var totalFlexShrink = 0f;
        var totalFixedSize = 0f;
        
        foreach (var child in _childInstances)
        {
            if (child is BoxInstance boxChild && boxChild._box != null)
            {
                totalFlexGrow += boxChild._box.FlexGrow;
                totalFlexShrink += boxChild._box.FlexShrink;
                
                // For now, assume fixed size children take their preferred size
                if (boxChild._box.FlexGrow == 0)
                {
                    var childConstraints = isRow
                        ? LayoutConstraints.Loose(availableMainSize, crossAxisSize)
                        : LayoutConstraints.Loose(crossAxisSize, availableMainSize);
                    
                    child.CalculateLayout(childConstraints);
                    totalFixedSize += isRow ? child.Layout.Width : child.Layout.Height;
                }
            }
        }
        
        // Distribute remaining space to flex items
        var remainingSpace = availableMainSize - totalFixedSize;
        var flexUnit = totalFlexGrow > 0 ? remainingSpace / totalFlexGrow : 0;
        
        // Position children
        var mainPos = 0f;
        var crossPos = 0f;
        
        foreach (var child in _childInstances)
        {
            var childBox = child as BoxInstance;
            var flexGrow = childBox?._box?.FlexGrow ?? 0;
            
            // Calculate child size
            float childMainSize;
            if (flexGrow > 0)
            {
                childMainSize = flexUnit * flexGrow;
            }
            else
            {
                childMainSize = isRow ? child.Layout.Width : child.Layout.Height;
            }
            
            // Apply child constraints
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
            
            mainPos += childMainSize + gap;
        }
        
        // Apply justification and alignment
        ApplyJustification(parentLayout, isRow, mainPos - gap);
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
        
        return Fragment(elements.ToArray());
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}