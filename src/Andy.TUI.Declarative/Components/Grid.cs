using System.Collections;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Layout;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A container that arranges its children in a two-dimensional grid.
/// Similar to CSS Grid layout.
/// </summary>
public class Grid : ISimpleComponent, IEnumerable<ISimpleComponent>
{
    private readonly List<ISimpleComponent> _children = new();

    // Grid template properties
    public List<GridTrackSize> Columns { get; set; } = new();
    public List<GridTrackSize> Rows { get; set; } = new();

    // Grid gap properties
    public float RowGap { get; set; } = 0;
    public float ColumnGap { get; set; } = 0;
    public float Gap { get; set; } = 0; // Sets both row and column gap

    // Alignment properties
    public JustifyContent JustifyItems { get; set; } = JustifyContent.FlexStart;
    public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    public AlignItems AlignContent { get; set; } = AlignItems.FlexStart;

    /// <summary>
    /// Creates a new Grid component.
    /// </summary>
    public Grid()
    {
    }

    /// <summary>
    /// Creates a new Grid with specified columns.
    /// </summary>
    public Grid(params GridTrackSize[] columns)
    {
        Columns.AddRange(columns);
    }

    // Fluent API
    public Grid WithColumns(params GridTrackSize[] columns)
    {
        Columns.Clear();
        Columns.AddRange(columns);
        return this;
    }

    public Grid WithRows(params GridTrackSize[] rows)
    {
        Rows.Clear();
        Rows.AddRange(rows);
        return this;
    }

    public Grid WithGap(float gap)
    {
        Gap = gap;
        return this;
    }

    public Grid WithRowGap(float rowGap)
    {
        RowGap = rowGap;
        return this;
    }

    public Grid WithColumnGap(float columnGap)
    {
        ColumnGap = columnGap;
        return this;
    }

    public Grid WithJustifyItems(JustifyContent justify)
    {
        JustifyItems = justify;
        return this;
    }

    public Grid WithAlignItems(AlignItems align)
    {
        AlignItems = align;
        return this;
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

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Grid declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // IEnumerable implementation
    public IEnumerator<ISimpleComponent> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Internal methods for view instance access
    internal IReadOnlyList<ISimpleComponent> GetChildren() => _children;
}

/// <summary>
/// Represents a grid track (column or row) size.
/// </summary>
public struct GridTrackSize
{
    public GridTrackSizeType Type { get; }
    public float Value { get; }

    private GridTrackSize(GridTrackSizeType type, float value)
    {
        Type = type;
        Value = value;
    }

    /// <summary>
    /// Creates a fixed pixel size track.
    /// </summary>
    public static GridTrackSize Pixels(float pixels) => new(GridTrackSizeType.Pixels, pixels);

    /// <summary>
    /// Creates a fractional unit (fr) track that shares available space.
    /// </summary>
    public static GridTrackSize Fr(float fraction = 1) => new(GridTrackSizeType.Fr, fraction);

    /// <summary>
    /// Creates an auto-sized track that fits its content.
    /// </summary>
    public static GridTrackSize Auto => new(GridTrackSizeType.Auto, 0);

    /// <summary>
    /// Creates a percentage-based track.
    /// </summary>
    public static GridTrackSize Percentage(float percentage) => new(GridTrackSizeType.Percentage, percentage);

    // Implicit conversions
    public static implicit operator GridTrackSize(int pixels) => Pixels(pixels);
    public static implicit operator GridTrackSize(float pixels) => Pixels(pixels);

    public override string ToString() => Type switch
    {
        GridTrackSizeType.Pixels => $"{Value}px",
        GridTrackSizeType.Fr => $"{Value}fr",
        GridTrackSizeType.Auto => "auto",
        GridTrackSizeType.Percentage => $"{Value}%",
        _ => "unknown"
    };
}

/// <summary>
/// Types of grid track sizes.
/// </summary>
public enum GridTrackSizeType
{
    /// <summary>
    /// Fixed pixel size.
    /// </summary>
    Pixels,

    /// <summary>
    /// Fractional unit - shares available space proportionally.
    /// </summary>
    Fr,

    /// <summary>
    /// Auto size - fits content.
    /// </summary>
    Auto,

    /// <summary>
    /// Percentage of container size.
    /// </summary>
    Percentage
}

/// <summary>
/// Extension class for creating grid child items with placement properties.
/// </summary>
public static class GridItemExtensions
{
    /// <summary>
    /// Wraps a component in a GridItem with placement properties.
    /// </summary>
    public static GridItem GridColumn(this ISimpleComponent component, int column, int span = 1)
    {
        return new GridItem(component)
        {
            Column = column,
            ColumnSpan = span
        };
    }

    /// <summary>
    /// Wraps a component in a GridItem with placement properties.
    /// </summary>
    public static GridItem GridRow(this ISimpleComponent component, int row, int span = 1)
    {
        return new GridItem(component)
        {
            Row = row,
            RowSpan = span
        };
    }

    /// <summary>
    /// Wraps a component in a GridItem with placement properties.
    /// </summary>
    public static GridItem GridArea(this ISimpleComponent component, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        return new GridItem(component)
        {
            Row = row,
            Column = column,
            RowSpan = rowSpan,
            ColumnSpan = columnSpan
        };
    }
}

/// <summary>
/// Wrapper for grid children that specifies placement.
/// </summary>
public class GridItem : ISimpleComponent
{
    public ISimpleComponent Child { get; }

    // Placement properties (1-based like CSS Grid)
    public int? Row { get; set; }
    public int? Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;

    public GridItem(ISimpleComponent child)
    {
        Child = child ?? throw new ArgumentNullException(nameof(child));
    }

    public VirtualNode Render()
    {
        throw new InvalidOperationException("GridItem declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}