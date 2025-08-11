using System.Collections;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Layout;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A flexible box container component with full flexbox layout support.
/// This is the foundation component for building layouts, similar to Ink's Box.
/// </summary>
public class Box : ISimpleComponent, IEnumerable<ISimpleComponent>
{
    private readonly List<ISimpleComponent> _children = new();
    
    // Layout properties
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    public FlexWrap FlexWrap { get; set; } = FlexWrap.NoWrap;
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    public AlignItems AlignItems { get; set; } = AlignItems.Stretch;
    public AlignSelf AlignSelf { get; set; } = AlignSelf.Auto;
    
    // Sizing properties
    public Length Width { get; set; } = Length.Auto;
    public Length Height { get; set; } = Length.Auto;
    public Length MinWidth { get; set; } = 0;
    public Length MinHeight { get; set; } = 0;
    public Length MaxWidth { get; set; } = Length.Auto;
    public Length MaxHeight { get; set; } = Length.Auto;
    
    // Flex properties
    public float FlexGrow { get; set; } = 0;
    public float FlexShrink { get; set; } = 1;
    public Length FlexBasis { get; set; } = Length.Auto;
    
    // Spacing properties
    public Spacing Margin { get; set; } = Spacing.Zero;
    public Spacing Padding { get; set; } = Spacing.Zero;
    public float Gap { get; set; } = 0;
    public float RowGap { get; set; } = 0;
    public float ColumnGap { get; set; } = 0;
    
    // Display properties
    public bool Display { get; set; } = true;
    public Overflow Overflow { get; set; } = Overflow.Visible;
    
    /// <summary>
    /// Creates a new Box component.
    /// </summary>
    public Box()
    {
    }
    
    // Collection initializer support
    public void Add(ISimpleComponent component)
    {
        if (component != null)
        {
            _children.Add(component);
        }
    }
    
    public void Add(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _children.Add(new Text(text));
        }
    }
    
    // Fluent API for common properties
    public Box Direction(FlexDirection direction) { FlexDirection = direction; return this; }
    public Box Wrap(FlexWrap wrap = FlexWrap.Wrap) { FlexWrap = wrap; return this; }
    public Box Justify(JustifyContent justify) { JustifyContent = justify; return this; }
    public Box Align(AlignItems align) { AlignItems = align; return this; }
    public Box AlignSelfTo(AlignSelf align) { AlignSelf = align; return this; }
    
    public Box WithWidth(Length width) { Width = width; return this; }
    public Box WithHeight(Length height) { Height = height; return this; }
    public Box WithMinWidth(Length minWidth) { MinWidth = minWidth; return this; }
    public Box WithMinHeight(Length minHeight) { MinHeight = minHeight; return this; }
    public Box WithMaxWidth(Length maxWidth) { MaxWidth = maxWidth; return this; }
    public Box WithMaxHeight(Length maxHeight) { MaxHeight = maxHeight; return this; }
    
    public Box Grow(float grow) { FlexGrow = grow; return this; }
    public Box Shrink(float shrink) { FlexShrink = shrink; return this; }
    public Box Basis(Length basis) { FlexBasis = basis; return this; }
    
    public Box WithMargin(Spacing margin) { Margin = margin; return this; }
    public Box WithPadding(Spacing padding) { Padding = padding; return this; }
    public Box WithGap(float gap) { Gap = gap; return this; }
    public Box WithRowGap(float rowGap) { RowGap = rowGap; return this; }
    public Box WithColumnGap(float columnGap) { ColumnGap = columnGap; return this; }
    
    public Box Hide() { Display = false; return this; }
    public Box Show() { Display = true; return this; }
    public Box WithOverflow(Overflow overflow) { Overflow = overflow; return this; }
    
    // ISimpleComponent implementation
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Box declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    // IEnumerable implementation
    public IEnumerator<ISimpleComponent> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    // Internal methods for view instance access
    internal IReadOnlyList<ISimpleComponent> GetChildren() => _children;
}

/// <summary>
/// Defines overflow behavior for box content.
/// </summary>
public enum Overflow
{
    /// <summary>
    /// Content is not clipped and may overflow the box (default).
    /// </summary>
    Visible,
    
    /// <summary>
    /// Content is clipped to the box boundaries.
    /// </summary>
    Hidden,
    
    /// <summary>
    /// Content is clipped and scrollbars are shown if needed.
    /// </summary>
    Scroll
}