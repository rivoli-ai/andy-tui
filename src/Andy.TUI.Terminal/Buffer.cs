namespace Andy.TUI.Terminal;

/// <summary>
/// Represents a buffer of cells for terminal rendering.
/// </summary>
public class Buffer
{
    private readonly Cell[,] _cells;
    
    /// <summary>
    /// Gets the width of the buffer.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Gets the height of the buffer.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// Creates a new buffer with the specified dimensions.
    /// </summary>
    public Buffer(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
            
        Width = width;
        Height = height;
        _cells = new Cell[height, width];
        Clear();
    }
    
    /// <summary>
    /// Gets or sets a cell at the specified position.
    /// </summary>
    public Cell this[int x, int y]
    {
        get
        {
            if (!IsInBounds(x, y))
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds.");
            return _cells[y, x];
        }
        set
        {
            if (!IsInBounds(x, y))
                throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds.");
            _cells[y, x] = value.AsDirty();
        }
    }
    
    /// <summary>
    /// Checks if the specified position is within bounds.
    /// </summary>
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
    
    /// <summary>
    /// Sets a cell at the specified position if it's within bounds.
    /// </summary>
    public bool TrySetCell(int x, int y, Cell cell)
    {
        if (!IsInBounds(x, y))
            return false;
            
        _cells[y, x] = cell.AsDirty();
        return true;
    }
    
    /// <summary>
    /// Gets a cell at the specified position if it's within bounds.
    /// </summary>
    public bool TryGetCell(int x, int y, out Cell cell)
    {
        if (!IsInBounds(x, y))
        {
            cell = Cell.Empty;
            return false;
        }
        
        cell = _cells[y, x];
        return true;
    }
    
    /// <summary>
    /// Clears the entire buffer.
    /// </summary>
    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[y, x] = Cell.Empty.AsDirty();
            }
        }
    }
    
    /// <summary>
    /// Clears a rectangular region in the buffer.
    /// </summary>
    public void ClearRect(int x, int y, int width, int height)
    {
        var x2 = Math.Min(x + width, Width);
        var y2 = Math.Min(y + height, Height);
        var x1 = Math.Max(x, 0);
        var y1 = Math.Max(y, 0);
        
        for (int row = y1; row < y2; row++)
        {
            for (int col = x1; col < x2; col++)
            {
                _cells[row, col] = Cell.Empty.AsDirty();
            }
        }
    }
    
    /// <summary>
    /// Fills a rectangular region with the specified cell.
    /// </summary>
    public void FillRect(int x, int y, int width, int height, Cell cell)
    {
        var x2 = Math.Min(x + width, Width);
        var y2 = Math.Min(y + height, Height);
        var x1 = Math.Max(x, 0);
        var y1 = Math.Max(y, 0);
        
        for (int row = y1; row < y2; row++)
        {
            for (int col = x1; col < x2; col++)
            {
                _cells[row, col] = cell.AsDirty();
            }
        }
    }
    
    /// <summary>
    /// Marks all cells as clean (not dirty).
    /// </summary>
    public void MarkAllClean()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[y, x] = _cells[y, x].AsClean();
            }
        }
    }
    
    /// <summary>
    /// Marks all cells as dirty.
    /// </summary>
    public void MarkAllDirty()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[y, x] = _cells[y, x].AsDirty();
            }
        }
    }
    
    /// <summary>
    /// Copies the contents of another buffer into this one.
    /// </summary>
    public void CopyFrom(Buffer other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
            
        var minWidth = Math.Min(Width, other.Width);
        var minHeight = Math.Min(Height, other.Height);
        
        for (int y = 0; y < minHeight; y++)
        {
            for (int x = 0; x < minWidth; x++)
            {
                _cells[y, x] = other._cells[y, x];
            }
        }
    }
    
    /// <summary>
    /// Creates a copy of this buffer.
    /// </summary>
    public Buffer Clone()
    {
        var clone = new Buffer(Width, Height);
        clone.CopyFrom(this);
        return clone;
    }
}