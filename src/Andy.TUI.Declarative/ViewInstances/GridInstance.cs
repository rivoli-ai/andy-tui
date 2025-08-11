using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance of a Grid component with CSS Grid-like layout.
/// </summary>
public class GridInstance : ViewInstance
{
    private Grid? _grid;
    private readonly List<(ViewInstance instance, GridItem? item)> _childInstances = new();
    
    public GridInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Grid grid)
            throw new ArgumentException("Expected Grid declaration");
        
        _grid = grid;
        
        // Update child instances
        var children = grid.GetChildren();
        var manager = Context?.ViewInstanceManager;
        
        if (manager != null)
        {
            _childInstances.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                var childPath = $"{Id}/{i}";
                
                if (child is GridItem gridItem)
                {
                    // Handle GridItem wrapper
                    var actualChild = gridItem.Child;
                    var childInstance = manager.GetOrCreateInstance(actualChild, childPath);
                    _childInstances.Add((childInstance, gridItem));
                }
                else
                {
                    // Regular child without explicit placement
                    var childInstance = manager.GetOrCreateInstance(child, childPath);
                    _childInstances.Add((childInstance, null));
                }
            }
        }
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (_grid == null) return base.PerformLayout(constraints);
        
        var layout = new LayoutBox();
        
        if (_childInstances.Count == 0)
        {
            layout.Width = 0;
            layout.Height = 0;
            return layout;
        }
        
        // Calculate gaps
        var rowGap = _grid.Gap > 0 ? _grid.Gap : _grid.RowGap;
        var columnGap = _grid.Gap > 0 ? _grid.Gap : _grid.ColumnGap;
        
        // Determine grid structure
        var (columnSizes, rowSizes, gridWidth, gridHeight) = CalculateGridStructure(constraints, rowGap, columnGap);
        
        layout.Width = constraints.ConstrainWidth(gridWidth);
        layout.Height = constraints.ConstrainHeight(gridHeight);
        
        // Position children in grid cells
        PositionChildren(columnSizes, rowSizes, rowGap, columnGap, constraints);
        
        return layout;
    }
    
    private (float[] columnSizes, float[] rowSizes, float totalWidth, float totalHeight) 
        CalculateGridStructure(LayoutConstraints constraints, float rowGap, float columnGap)
    {
        if (_grid == null) return (Array.Empty<float>(), Array.Empty<float>(), 0, 0);
        
        // Determine number of columns and rows
        var columnCount = _grid.Columns.Count;
        var rowCount = _grid.Rows.Count;
        
        // Auto-generate columns/rows if not specified
        if (columnCount == 0)
        {
            // Default to single column if no columns specified
            columnCount = 1;
            _grid.Columns.Clear();
            _grid.Columns.Add(GridTrackSize.Auto);
        }
        
        if (rowCount == 0)
        {
            // Calculate rows needed based on children and columns, considering spans
            int maxRow = 0;
            
            // First check explicitly placed items
            foreach (var (instance, item) in _childInstances)
            {
                if (item != null && item.Row.HasValue)
                {
                    maxRow = Math.Max(maxRow, item.Row.Value + item.RowSpan - 1);
                }
            }
            
            // Simulate auto-placement to determine actual rows needed
            var occupiedCells = new bool[10, columnCount]; // Start with 10 rows max
            var simulatedCursor = 0;
            
            // Mark explicitly placed items
            foreach (var (instance, item) in _childInstances)
            {
                if (item != null && (item.Row.HasValue || item.Column.HasValue))
                {
                    var row = item.Row ?? 0;
                    var col = item.Column ?? 0;
                    for (int r = row; r < row + item.RowSpan && r < 10; r++)
                    {
                        for (int c = col; c < col + item.ColumnSpan && c < columnCount; c++)
                        {
                            occupiedCells[r, c] = true;
                        }
                    }
                    maxRow = Math.Max(maxRow, row + item.RowSpan - 1);
                }
            }
            
            // Simulate placement of auto-placed items
            foreach (var (instance, item) in _childInstances)
            {
                if (item == null || (!item.Row.HasValue && !item.Column.HasValue))
                {
                    var rowSpan = item?.RowSpan ?? 1;
                    var colSpan = item?.ColumnSpan ?? 1;
                    bool placed = false;
                    
                    while (simulatedCursor < 10 * columnCount && !placed)
                    {
                        int row = simulatedCursor / columnCount;
                        int col = simulatedCursor % columnCount;
                        
                        // Check if item fits
                        bool canFit = col + colSpan <= columnCount && row + rowSpan <= 10;
                        if (canFit)
                        {
                            for (int r = row; r < row + rowSpan && canFit; r++)
                            {
                                for (int c = col; c < col + colSpan && canFit; c++)
                                {
                                    if (occupiedCells[r, c]) canFit = false;
                                }
                            }
                        }
                        
                        if (canFit)
                        {
                            // Mark as occupied
                            for (int r = row; r < row + rowSpan; r++)
                            {
                                for (int c = col; c < col + colSpan; c++)
                                {
                                    occupiedCells[r, c] = true;
                                }
                            }
                            maxRow = Math.Max(maxRow, row + rowSpan - 1);
                            placed = true;
                            simulatedCursor = row * columnCount + col + 1;
                        }
                        else
                        {
                            simulatedCursor++;
                        }
                    }
                }
            }
            
            rowCount = maxRow + 1;
            _grid.Rows.Clear();
            
            // If parent has tight constraints AND columns are all Fr, use Fr for rows too
            // This ensures even distribution when the grid itself is constrained
            bool columnsAreFr = _grid.Columns.Count > 0 && _grid.Columns.All(c => c.Type == GridTrackSizeType.Fr);
            bool shouldDistributeEvenly = columnsAreFr && 
                                        constraints.MinHeight == constraints.MaxHeight && 
                                        constraints.MaxHeight > 0;
            
            for (int i = 0; i < rowCount; i++)
            {
                _grid.Rows.Add(shouldDistributeEvenly ? GridTrackSize.Fr(1) : GridTrackSize.Auto);
            }
        }
        
        // Calculate column sizes
        var columnSizes = CalculateTrackSizes(
            _grid.Columns, 
            constraints.MaxWidth,
            columnGap,
            true,
            constraints
        );
        
        // Calculate row sizes
        var rowSizes = CalculateTrackSizes(
            _grid.Rows,
            constraints.MaxHeight,
            rowGap,
            false,
            constraints
        );
        
        // Calculate total size based on actual content, not full available space
        var totalWidth = columnSizes.Sum() + Math.Max(0, columnCount - 1) * columnGap;
        var totalHeight = rowSizes.Sum() + Math.Max(0, rowCount - 1) * rowGap;
        
        // For grids with all columns of same type, size to content if appropriate
        bool allPixels = _grid.Columns.All(c => c.Type == GridTrackSizeType.Pixels);
        bool allFr = _grid.Columns.All(c => c.Type == GridTrackSizeType.Fr);
        
        if ((allPixels || allFr) && constraints.MinWidth != constraints.MaxWidth)
        {
            // Calculate actual content width based on occupied columns
            totalWidth = GetContentBasedWidth(columnSizes, columnGap);
        }
        
        return (columnSizes, rowSizes, totalWidth, totalHeight);
    }
    
    private float GetContentBasedWidth(float[] columnSizes, float columnGap)
    {
        // Calculate the actual content width by finding the rightmost occupied column
        int maxColumn = -1;
        
        // Track occupied cells to determine actual grid bounds
        var columnCount = columnSizes.Length;
        var rowCount = _grid?.Rows.Count ?? 1;
        var occupiedCells = new bool[rowCount, columnCount];
        
        // Mark all occupied cells
        foreach (var (instance, item) in _childInstances)
        {
            var (row, col) = GetChildPlacement(item, _childInstances.IndexOf((instance, item)));
            var rowSpan = item?.RowSpan ?? 1;
            var colSpan = item?.ColumnSpan ?? 1;
            
            for (int r = row; r < Math.Min(row + rowSpan, rowCount); r++)
            {
                for (int c = col; c < Math.Min(col + colSpan, columnCount); c++)
                {
                    if (r >= 0 && r < rowCount && c >= 0 && c < columnCount)
                    {
                        occupiedCells[r, c] = true;
                        maxColumn = Math.Max(maxColumn, c);
                    }
                }
            }
        }
        
        if (maxColumn < 0) return 0;
        
        float width = 0;
        for (int i = 0; i <= maxColumn && i < columnSizes.Length; i++)
        {
            width += columnSizes[i];
            if (i > 0) width += columnGap;
        }
        
        return width;
    }
    
    private float[] CalculateTrackSizes(
        List<GridTrackSize> tracks,
        float availableSize,
        float gap,
        bool isColumn,
        LayoutConstraints constraints)
    {
        var trackCount = tracks.Count;
        var sizes = new float[trackCount];
        var totalGap = Math.Max(0, trackCount - 1) * gap;
        var remainingSpace = availableSize - totalGap;
        
        // First pass: Calculate ideal sizes
        var totalFixed = 0f;
        var totalFr = 0f;
        var autoIndices = new List<int>();
        var frIndices = new List<int>();
        var fixedIndices = new List<int>();
        
        for (int i = 0; i < trackCount; i++)
        {
            var track = tracks[i];
            switch (track.Type)
            {
                case GridTrackSizeType.Pixels:
                    sizes[i] = track.Value;
                    totalFixed += track.Value;
                    fixedIndices.Add(i);
                    break;
                    
                case GridTrackSizeType.Percentage:
                    sizes[i] = availableSize * track.Value / 100f;
                    totalFixed += sizes[i];
                    fixedIndices.Add(i);
                    break;
                    
                case GridTrackSizeType.Auto:
                    autoIndices.Add(i);
                    // Will be calculated based on content
                    break;
                    
                case GridTrackSizeType.Fr:
                    frIndices.Add(i);
                    totalFr += track.Value;
                    break;
            }
        }
        
        // Calculate auto sizes based on content
        foreach (var autoIndex in autoIndices)
        {
            var maxSize = 0f;
            
            // Find all children in this track (considering spans)
            foreach (var (instance, item) in _childInstances)
            {
                var (row, col) = GetChildPlacement(item, _childInstances.IndexOf((instance, item)));
                var columnSpan = item?.ColumnSpan ?? 1;
                var rowSpan = item?.RowSpan ?? 1;
                
                // Check if child occupies this track
                bool occupiesTrack = false;
                if (isColumn)
                {
                    occupiesTrack = col <= autoIndex && autoIndex < col + columnSpan;
                }
                else
                {
                    occupiesTrack = row <= autoIndex && autoIndex < row + rowSpan;
                }
                
                if (occupiesTrack)
                {
                    // Measure child
                    var childConstraints = LayoutConstraints.Loose(
                        isColumn ? float.PositiveInfinity : constraints.MaxWidth,
                        isColumn ? constraints.MaxHeight : float.PositiveInfinity
                    );
                    
                    instance.CalculateLayout(childConstraints);
                    var childSize = isColumn ? instance.Layout.Width : instance.Layout.Height;
                    
                    // For spanning items, divide size by span count
                    var span = isColumn ? columnSpan : rowSpan;
                    if (span > 1)
                    {
                        childSize = childSize / span;
                    }
                    
                    maxSize = Math.Max(maxSize, childSize);
                }
            }
            
            sizes[autoIndex] = maxSize;
            totalFixed += maxSize;
        }
        
        // Check if we need to shrink fixed-size tracks
        if (totalFixed > remainingSpace && fixedIndices.Count > 0)
        {
            // Shrink fixed tracks proportionally
            var shrinkFactor = remainingSpace / totalFixed;
            foreach (var fixedIndex in fixedIndices)
            {
                sizes[fixedIndex] *= shrinkFactor;
            }
            totalFixed = remainingSpace;
        }
        
        // Second pass: Calculate fr sizes
        if (totalFr > 0)
        {
            // If all tracks are fr and we have loose constraints, use content sizing
            // But if parent has tight constraints, always distribute space evenly
            bool parentHasTightConstraints = constraints.MinWidth == constraints.MaxWidth && 
                                            constraints.MinHeight == constraints.MaxHeight;
            
            if (tracks.All(t => t.Type == GridTrackSizeType.Fr) && 
                !float.IsInfinity(availableSize) && 
                !parentHasTightConstraints)
            {
                // Calculate content-based minimum for fr tracks
                foreach (var frIndex in frIndices)
                {
                    var maxSize = 0f;
                    
                    foreach (var (instance, item) in _childInstances)
                    {
                        var (row, col) = GetChildPlacement(item, _childInstances.IndexOf((instance, item)));
                        var columnSpan = item?.ColumnSpan ?? 1;
                        var rowSpan = item?.RowSpan ?? 1;
                        
                        bool occupiesTrack = false;
                        if (isColumn)
                        {
                            occupiesTrack = col <= frIndex && frIndex < col + columnSpan;
                        }
                        else
                        {
                            occupiesTrack = row <= frIndex && frIndex < row + rowSpan;
                        }
                        
                        if (occupiesTrack)
                        {
                            var childConstraints = LayoutConstraints.Loose(
                                isColumn ? float.PositiveInfinity : constraints.MaxWidth,
                                isColumn ? constraints.MaxHeight : float.PositiveInfinity
                            );
                            
                            instance.CalculateLayout(childConstraints);
                            var childSize = isColumn ? instance.Layout.Width : instance.Layout.Height;
                            
                            var span = isColumn ? columnSpan : rowSpan;
                            if (span > 1)
                            {
                                childSize = childSize / span;
                            }
                            
                            maxSize = Math.Max(maxSize, childSize);
                        }
                    }
                    
                    sizes[frIndex] = maxSize;
                }
                
                // Check if content fits in available space
                var contentTotal = sizes.Sum();
                if (contentTotal <= remainingSpace)
                {
                    // Use content sizes
                    return sizes;
                }
            }
            
            // Standard fr distribution
            var frSpace = Math.Max(0, remainingSpace - totalFixed);
            var frUnit = frSpace / totalFr;
            
            for (int i = 0; i < trackCount; i++)
            {
                if (tracks[i].Type == GridTrackSizeType.Fr)
                {
                    sizes[i] = frUnit * tracks[i].Value;
                }
            }
        }
        
        return sizes;
    }
    
    private void PositionChildren(float[] columnSizes, float[] rowSizes, float rowGap, float columnGap, LayoutConstraints parentConstraints)
    {
        if (_grid == null) return;
        
        var columnCount = columnSizes.Length;
        var rowCount = rowSizes.Length;
        
        // Track occupied cells for auto-placement
        var occupiedCells = new bool[rowCount, columnCount];
        var autoPlacementCursor = 0;
        
        // First pass: mark explicitly placed items
        foreach (var (instance, item) in _childInstances)
        {
            if (item != null && (item.Row.HasValue || item.Column.HasValue))
            {
                var row = item.Row ?? 0;
                var col = item.Column ?? 0;
                var rowSpan = item.RowSpan;
                var colSpan = item.ColumnSpan;
                
                // Mark cells as occupied
                for (int r = row; r < Math.Min(row + rowSpan, rowCount); r++)
                {
                    for (int c = col; c < Math.Min(col + colSpan, columnCount); c++)
                    {
                        occupiedCells[r, c] = true;
                    }
                }
            }
        }
        
        // Second pass: position all children
        foreach (var (instance, item) in _childInstances)
        {
            int row = 0, col = 0;
            
            if (item != null && (item.Row.HasValue || item.Column.HasValue))
            {
                // Explicit placement
                row = item.Row ?? 0;
                col = item.Column ?? 0;
            }
            else
            {
                // Auto-placement: find next available cell
                var placed = false;
                var rowSpan = item?.RowSpan ?? 1;
                var colSpan = item?.ColumnSpan ?? 1;
                
                while (autoPlacementCursor < rowCount * columnCount && !placed)
                {
                    row = autoPlacementCursor / columnCount;
                    col = autoPlacementCursor % columnCount;
                    
                    // Check if this position can fit the item
                    bool canFit = true;
                    if (col + colSpan <= columnCount && row + rowSpan <= rowCount)
                    {
                        for (int r = row; r < row + rowSpan && canFit; r++)
                        {
                            for (int c = col; c < col + colSpan && canFit; c++)
                            {
                                if (occupiedCells[r, c])
                                {
                                    canFit = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        canFit = false;
                    }
                    
                    if (canFit)
                    {
                        // Mark cells as occupied
                        for (int r = row; r < row + rowSpan; r++)
                        {
                            for (int c = col; c < col + colSpan; c++)
                            {
                                occupiedCells[r, c] = true;
                            }
                        }
                        placed = true;
                        // Move cursor to next position after this placement
                        autoPlacementCursor = row * columnCount + col + 1;
                    }
                    else
                    {
                        autoPlacementCursor++;
                    }
                }
                
                if (!placed)
                {
                    // Fallback: place at the end
                    row = rowCount - 1;
                    col = columnCount - 1;
                }
            }
            
            // Ensure within bounds
            row = Math.Min(row, rowCount - 1);
            col = Math.Min(col, columnCount - 1);
            
            // Calculate position
            var x = 0f;
            for (int i = 0; i < col; i++)
            {
                x += columnSizes[i] + columnGap;
            }
            
            var y = 0f;
            for (int i = 0; i < row; i++)
            {
                y += rowSizes[i] + rowGap;
            }
            
            // Calculate cell size (accounting for spans)
            var cellWidth = columnSizes[col];
            var cellHeight = rowSizes[row];
            
            if (item != null)
            {
                // Add sizes for spanned cells
                for (int i = 1; i < item.ColumnSpan && col + i < columnCount; i++)
                {
                    cellWidth += columnGap + columnSizes[col + i];
                }
                
                for (int i = 1; i < item.RowSpan && row + i < rowCount; i++)
                {
                    cellHeight += rowGap + rowSizes[row + i];
                }
            }
            
            // Apply alignment within cell
            // Use appropriate constraints based on parent constraints and overflow settings
            LayoutConstraints childConstraints;
            
            // If parent has tight constraints, constrain children to cell size
            bool shouldConstrain = parentConstraints.MinWidth == parentConstraints.MaxWidth && 
                                   parentConstraints.MinHeight == parentConstraints.MaxHeight;
            
            if (shouldConstrain)
            {
                // Use tight constraints to force children to fit exactly in their cells
                childConstraints = LayoutConstraints.Tight(cellWidth, cellHeight);
            }
            else
            {
                // Use loose constraints to allow children to size naturally up to cell size
                childConstraints = LayoutConstraints.Loose(cellWidth, cellHeight);
            }
            
            instance.CalculateLayout(childConstraints);
            
            // For spanning items, ensure they use the full allocated space
            if (item != null && (item.ColumnSpan > 1 || item.RowSpan > 1))
            {
                // Spanning items should fill their allocated space
                instance.Layout.Width = cellWidth;
                instance.Layout.Height = cellHeight;
            }
            
            // Apply justify/align
            var childX = x;
            var childY = y;
            
            // Only apply justification/alignment if not stretching
            if (_grid.AlignItems != AlignItems.Stretch)
            {
                switch (_grid.JustifyItems)
                {
                    case JustifyContent.Center:
                        childX += (cellWidth - instance.Layout.Width) / 2;
                        break;
                    case JustifyContent.FlexEnd:
                        childX += cellWidth - instance.Layout.Width;
                        break;
                }
                
                switch (_grid.AlignItems)
                {
                    case AlignItems.Center:
                        childY += (cellHeight - instance.Layout.Height) / 2;
                        break;
                    case AlignItems.FlexEnd:
                        childY += cellHeight - instance.Layout.Height;
                        break;
                }
            }
            else
            {
                // Stretch: recalculate with tight constraints
                childConstraints = LayoutConstraints.Tight(cellWidth, cellHeight);
                instance.CalculateLayout(childConstraints);
                instance.Layout.Width = cellWidth;
                instance.Layout.Height = cellHeight;
            }
            
            instance.Layout.X = childX;
            instance.Layout.Y = childY;
        }
    }
    
    private (int row, int col) GetChildPlacement(GridItem? item, int autoIndex)
    {
        if (item != null && (item.Row.HasValue || item.Column.HasValue))
        {
            // Explicit placement - already 0-based from extension methods
            var row = item.Row ?? 0;
            var col = item.Column ?? 0;
            return (Math.Max(0, row), Math.Max(0, col));
        }
        else
        {
            // Auto placement
            var columnCount = _grid?.Columns.Count ?? 1;
            var row = autoIndex / columnCount;
            var col = autoIndex % columnCount;
            return (row, col);
        }
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_childInstances.Count == 0)
            return Fragment();
        
        var elements = new List<VirtualNode>();
        
        foreach (var (instance, _) in _childInstances)
        {
            // Update absolute position
            instance.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(instance.Layout.X);
            instance.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(instance.Layout.Y);
            
            // Render child
            var childNode = instance.Render();
            elements.Add(childNode);
        }
        
        return Fragment(elements.ToArray());
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances.Select(c => c.instance).ToList();
}