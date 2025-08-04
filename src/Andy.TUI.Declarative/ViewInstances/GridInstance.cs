using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

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
        PositionChildren(columnSizes, rowSizes, rowGap, columnGap);
        
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
            // Default to auto-sized columns based on number of children
            columnCount = Math.Min(_childInstances.Count, 3); // Max 3 columns by default
            _grid.Columns.Clear();
            for (int i = 0; i < columnCount; i++)
            {
                _grid.Columns.Add(GridTrackSize.Fr(1));
            }
        }
        
        if (rowCount == 0)
        {
            // Calculate rows needed based on children and columns
            rowCount = (int)Math.Ceiling((double)_childInstances.Count / columnCount);
            _grid.Rows.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                _grid.Rows.Add(GridTrackSize.Auto);
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
        
        // Calculate total size
        var totalWidth = columnSizes.Sum() + Math.Max(0, columnCount - 1) * columnGap;
        var totalHeight = rowSizes.Sum() + Math.Max(0, rowCount - 1) * rowGap;
        
        return (columnSizes, rowSizes, totalWidth, totalHeight);
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
        
        // First pass: Calculate fixed sizes and auto sizes
        var totalFixed = 0f;
        var totalFr = 0f;
        var autoIndices = new List<int>();
        
        for (int i = 0; i < trackCount; i++)
        {
            var track = tracks[i];
            switch (track.Type)
            {
                case GridTrackSizeType.Pixels:
                    sizes[i] = track.Value;
                    totalFixed += track.Value;
                    break;
                    
                case GridTrackSizeType.Percentage:
                    sizes[i] = availableSize * track.Value / 100f;
                    totalFixed += sizes[i];
                    break;
                    
                case GridTrackSizeType.Auto:
                    autoIndices.Add(i);
                    // Will be calculated based on content
                    break;
                    
                case GridTrackSizeType.Fr:
                    totalFr += track.Value;
                    break;
            }
        }
        
        // Calculate auto sizes based on content
        foreach (var autoIndex in autoIndices)
        {
            var maxSize = 0f;
            
            // Find all children in this track
            foreach (var (instance, item) in _childInstances)
            {
                var (row, col) = GetChildPlacement(item, _childInstances.IndexOf((instance, item)));
                
                if ((isColumn && col == autoIndex) || (!isColumn && row == autoIndex))
                {
                    // Measure child
                    var childConstraints = LayoutConstraints.Loose(
                        isColumn ? float.PositiveInfinity : constraints.MaxWidth,
                        isColumn ? constraints.MaxHeight : float.PositiveInfinity
                    );
                    
                    instance.CalculateLayout(childConstraints);
                    var childSize = isColumn ? instance.Layout.Width : instance.Layout.Height;
                    maxSize = Math.Max(maxSize, childSize);
                }
            }
            
            sizes[autoIndex] = maxSize;
            totalFixed += maxSize;
        }
        
        // Second pass: Distribute remaining space to fr units
        if (totalFr > 0)
        {
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
    
    private void PositionChildren(float[] columnSizes, float[] rowSizes, float rowGap, float columnGap)
    {
        if (_grid == null) return;
        
        var columnCount = columnSizes.Length;
        var rowCount = rowSizes.Length;
        var autoPlacementIndex = 0;
        
        foreach (var (instance, item) in _childInstances)
        {
            // Determine grid placement
            var (row, col) = GetChildPlacement(item, autoPlacementIndex);
            
            if (item == null)
            {
                autoPlacementIndex++;
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
            var childConstraints = LayoutConstraints.Loose(cellWidth, cellHeight);
            instance.CalculateLayout(childConstraints);
            
            // Apply justify/align
            var childX = x;
            var childY = y;
            
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
                case AlignItems.Stretch:
                    // Recalculate with tight constraints
                    childConstraints = LayoutConstraints.Tight(cellWidth, cellHeight);
                    instance.CalculateLayout(childConstraints);
                    break;
            }
            
            instance.Layout.X = childX;
            instance.Layout.Y = childY;
        }
    }
    
    private (int row, int col) GetChildPlacement(GridItem? item, int autoIndex)
    {
        if (item != null && (item.Row.HasValue || item.Column.HasValue))
        {
            // Explicit placement (convert from 1-based to 0-based)
            var row = (item.Row ?? 1) - 1;
            var col = (item.Column ?? 1) - 1;
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