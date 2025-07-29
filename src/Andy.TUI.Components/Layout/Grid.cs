using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Components.Layout;

/// <summary>
/// Represents a grid column definition.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the width of the column.
    /// </summary>
    public GridLength Width { get; set; } = GridLength.Star(1);
    
    /// <summary>
    /// Gets or sets the minimum width of the column.
    /// </summary>
    public int MinWidth { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the maximum width of the column.
    /// </summary>
    public int? MaxWidth { get; set; }
}

/// <summary>
/// Represents a grid row definition.
/// </summary>
public class RowDefinition
{
    /// <summary>
    /// Gets or sets the height of the row.
    /// </summary>
    public GridLength Height { get; set; } = GridLength.Star(1);
    
    /// <summary>
    /// Gets or sets the minimum height of the row.
    /// </summary>
    public int MinHeight { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the maximum height of the row.
    /// </summary>
    public int? MaxHeight { get; set; }
}

/// <summary>
/// Represents a grid length value that can be absolute, auto, or star.
/// </summary>
public struct GridLength
{
    public enum GridUnitType
    {
        Absolute,
        Auto,
        Star
    }
    
    public double Value { get; }
    public GridUnitType Type { get; }
    
    private GridLength(double value, GridUnitType type)
    {
        Value = value;
        Type = type;
    }
    
    /// <summary>
    /// Creates an absolute grid length.
    /// </summary>
    public static GridLength Absolute(int value) => new(value, GridUnitType.Absolute);
    
    /// <summary>
    /// Creates an auto-sized grid length.
    /// </summary>
    public static GridLength Auto => new(0, GridUnitType.Auto);
    
    /// <summary>
    /// Creates a star-sized grid length.
    /// </summary>
    public static GridLength Star(double value = 1) => new(value, GridUnitType.Star);
}

/// <summary>
/// Represents a child in the grid with its position.
/// </summary>
public class GridChild
{
    public VirtualNode Node { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public int RowSpan { get; set; } = 1;
    public int ColumnSpan { get; set; } = 1;
    
    public GridChild(VirtualNode node, int row = 0, int column = 0)
    {
        Node = node;
        Row = row;
        Column = column;
    }
}

/// <summary>
/// A layout component that arranges children in a grid.
/// </summary>
public class Grid : LayoutComponent
{
    private readonly List<ColumnDefinition> _columns = new();
    private readonly List<RowDefinition> _rows = new();
    private readonly List<GridChild> _children = new();
    private int[]? _columnWidths;
    private int[]? _rowHeights;
    
    /// <summary>
    /// Gets or sets the gap between rows.
    /// </summary>
    public int RowGap { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the gap between columns.
    /// </summary>
    public int ColumnGap { get; set; } = 0;
    
    /// <summary>
    /// Gets the column definitions.
    /// </summary>
    public IReadOnlyList<ColumnDefinition> Columns => _columns;
    
    /// <summary>
    /// Gets the row definitions.
    /// </summary>
    public IReadOnlyList<RowDefinition> Rows => _rows;
    
    /// <summary>
    /// Gets the children in the grid.
    /// </summary>
    public IReadOnlyList<GridChild> Children => _children;
    
    /// <summary>
    /// Adds a column definition.
    /// </summary>
    public void AddColumn(ColumnDefinition column)
    {
        _columns.Add(column);
        RequestRender();
    }
    
    /// <summary>
    /// Adds a row definition.
    /// </summary>
    public void AddRow(RowDefinition row)
    {
        _rows.Add(row);
        RequestRender();
    }
    
    /// <summary>
    /// Adds a child to the grid.
    /// </summary>
    public void AddChild(VirtualNode node, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        _children.Add(new GridChild(node, row, column) 
        { 
            RowSpan = rowSpan, 
            ColumnSpan = columnSpan 
        });
        RequestRender();
    }
    
    /// <summary>
    /// Removes all children from the grid.
    /// </summary>
    public void ClearChildren()
    {
        _children.Clear();
        RequestRender();
    }
    
    /// <summary>
    /// Sets the columns from widths.
    /// </summary>
    public void SetColumns(params GridLength[] widths)
    {
        _columns.Clear();
        foreach (var width in widths)
        {
            _columns.Add(new ColumnDefinition { Width = width });
        }
        RequestRender();
    }
    
    /// <summary>
    /// Sets the rows from heights.
    /// </summary>
    public void SetRows(params GridLength[] heights)
    {
        _rows.Clear();
        foreach (var height in heights)
        {
            _rows.Add(new RowDefinition { Height = height });
        }
        RequestRender();
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        if (_columns.Count == 0 || _rows.Count == 0)
            return new Size(Padding.Horizontal, Padding.Vertical);
        
        var contentWidth = availableSize.Width - Padding.Horizontal;
        var contentHeight = availableSize.Height - Padding.Vertical;
        
        // Calculate column widths
        _columnWidths = CalculateSizes(_columns.Select(c => c.Width).ToList(), 
            contentWidth - ColumnGap * (_columns.Count - 1),
            _columns.Select(c => c.MinWidth).ToList(),
            _columns.Select(c => c.MaxWidth).ToList(),
            true);
        
        // Calculate row heights
        _rowHeights = CalculateSizes(_rows.Select(r => r.Height).ToList(),
            contentHeight - RowGap * (_rows.Count - 1),
            _rows.Select(r => r.MinHeight).ToList(),
            _rows.Select(r => r.MaxHeight).ToList(),
            false);
        
        // Measure children for auto-sized cells
        foreach (var child in _children)
        {
            if (child.Row >= _rows.Count || child.Column >= _columns.Count)
                continue;
            
            var childWidth = 0;
            var childHeight = 0;
            
            for (int i = 0; i < child.ColumnSpan && child.Column + i < _columns.Count; i++)
            {
                childWidth += _columnWidths[child.Column + i];
                if (i > 0) childWidth += ColumnGap;
            }
            
            for (int i = 0; i < child.RowSpan && child.Row + i < _rows.Count; i++)
            {
                childHeight += _rowHeights[child.Row + i];
                if (i > 0) childHeight += RowGap;
            }
            
            MeasureChild(child.Node, new Size(childWidth, childHeight));
        }
        
        var totalWidth = _columnWidths.Sum() + ColumnGap * (_columns.Count - 1);
        var totalHeight = _rowHeights.Sum() + RowGap * (_rows.Count - 1);
        
        return new Size(
            totalWidth + Padding.Horizontal,
            totalHeight + Padding.Vertical);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        if (_columnWidths == null || _rowHeights == null)
            return;
        
        var contentBounds = bounds.Inset(Padding);
        
        foreach (var child in _children)
        {
            if (child.Row >= _rows.Count || child.Column >= _columns.Count)
                continue;
            
            // Calculate child position
            var x = contentBounds.X;
            var y = contentBounds.Y;
            
            for (int i = 0; i < child.Column && i < _columns.Count; i++)
            {
                x += _columnWidths[i] + ColumnGap;
            }
            
            for (int i = 0; i < child.Row && i < _rows.Count; i++)
            {
                y += _rowHeights[i] + RowGap;
            }
            
            // Calculate child size
            var width = 0;
            var height = 0;
            
            for (int i = 0; i < child.ColumnSpan && child.Column + i < _columns.Count; i++)
            {
                width += _columnWidths[child.Column + i];
                if (i > 0) width += ColumnGap;
            }
            
            for (int i = 0; i < child.RowSpan && child.Row + i < _rows.Count; i++)
            {
                height += _rowHeights[child.Row + i];
                if (i > 0) height += RowGap;
            }
            
            var childBounds = new Rectangle(x, y, width, height);
            ArrangeChild(child.Node, childBounds);
        }
    }
    
    protected override VirtualNode OnRender()
    {
        var attributes = new Dictionary<string, object?>
        {
            ["rows"] = _rows.Count,
            ["columns"] = _columns.Count,
            ["row-gap"] = RowGap,
            ["column-gap"] = ColumnGap
        };
        
        var children = _children.Select(child =>
        {
            var childAttrs = new Dictionary<string, object?>
            {
                ["grid-row"] = child.Row,
                ["grid-column"] = child.Column,
                ["grid-row-span"] = child.RowSpan,
                ["grid-column-span"] = child.ColumnSpan
            };
            
            return new ElementNode("grid-item", childAttrs, child.Node);
        }).ToArray();
        
        return new ElementNode("grid", attributes, children);
    }
    
    private int[] CalculateSizes(List<GridLength> definitions, int availableSize, 
        List<int> minSizes, List<int?> maxSizes, bool isColumn)
    {
        var sizes = new int[definitions.Count];
        var totalFixed = 0;
        var totalStars = 0.0;
        var autoIndices = new List<int>();
        
        // First pass: assign fixed sizes and count stars
        for (int i = 0; i < definitions.Count; i++)
        {
            var def = definitions[i];
            
            switch (def.Type)
            {
                case GridLength.GridUnitType.Absolute:
                    sizes[i] = (int)def.Value;
                    totalFixed += sizes[i];
                    break;
                
                case GridLength.GridUnitType.Auto:
                    autoIndices.Add(i);
                    // Will be calculated after measuring children
                    sizes[i] = minSizes[i];
                    totalFixed += sizes[i];
                    break;
                
                case GridLength.GridUnitType.Star:
                    totalStars += def.Value;
                    break;
            }
        }
        
        // Second pass: distribute remaining space to star columns/rows
        var remainingSpace = Math.Max(0, availableSize - totalFixed);
        
        if (totalStars > 0)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Type == GridLength.GridUnitType.Star)
                {
                    sizes[i] = (int)(remainingSpace * definitions[i].Value / totalStars);
                }
            }
        }
        
        // Apply min/max constraints
        for (int i = 0; i < sizes.Length; i++)
        {
            sizes[i] = Math.Max(sizes[i], minSizes[i]);
            if (maxSizes[i].HasValue)
            {
                sizes[i] = Math.Min(sizes[i], maxSizes[i]!.Value);
            }
        }
        
        return sizes;
    }
    
    private Size MeasureChild(VirtualNode child, Size availableSize)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            return layoutComponent.Measure(availableSize);
        }
        
        // For non-layout nodes, estimate size
        if (child is TextNode textNode)
        {
            var lines = textNode.Content.Split('\n');
            return new Size(lines.Length > 0 ? lines.Max(l => l.Length) : 0, lines.Length);
        }
        
        return new Size(1, 1);
    }
    
    private void ArrangeChild(VirtualNode child, Rectangle bounds)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            layoutComponent.Arrange(bounds);
        }
    }
}