using System.Collections.Concurrent;

namespace Andy.TUI.Terminal;

/// <summary>
/// Manages double buffering and dirty region tracking for efficient terminal rendering.
/// </summary>
public class TerminalBuffer
{
    private Buffer _frontBuffer;
    private Buffer _backBuffer;
    private readonly object _swapLock = new();
    private readonly HashSet<(int x, int y)> _dirtyRegions = new();
    private readonly object _dirtyLock = new();
    
    /// <summary>
    /// Gets the width of the buffer.
    /// </summary>
    public int Width => _backBuffer.Width;
    
    /// <summary>
    /// Gets the height of the buffer.
    /// </summary>
    public int Height => _backBuffer.Height;
    
    /// <summary>
    /// Gets whether there are any dirty regions that need rendering.
    /// </summary>
    public bool IsDirty
    {
        get
        {
            lock (_dirtyLock)
            {
                return _dirtyRegions.Count > 0;
            }
        }
    }
    
    /// <summary>
    /// Creates a new terminal buffer with the specified dimensions.
    /// </summary>
    public TerminalBuffer(int width, int height)
    {
        _frontBuffer = new Buffer(width, height);
        _backBuffer = new Buffer(width, height);
    }
    
    /// <summary>
    /// Sets a cell in the back buffer and marks the region as dirty.
    /// </summary>
    public void SetCell(int x, int y, Cell cell)
    {
        if (_backBuffer.TrySetCell(x, y, cell))
        {
            MarkDirty(x, y);
        }
    }
    
    /// <summary>
    /// Sets a cell in the back buffer and marks the region as dirty.
    /// </summary>
    public void SetCell(int x, int y, char character, Style style = default)
    {
        SetCell(x, y, new Cell(character, style));
    }
    
    /// <summary>
    /// Writes text starting at the specified position.
    /// </summary>
    public void WriteText(int x, int y, string text, Style style = default)
    {
        if (string.IsNullOrEmpty(text))
            return;
            
        int currentX = x;
        foreach (char ch in text)
        {
            if (ch == '\n')
            {
                y++;
                currentX = x;
                continue;
            }
            
            if (ch == '\r')
            {
                currentX = x;
                continue;
            }
            
            SetCell(currentX, y, ch, style);
            currentX++;
            
            // Wrap to next line if needed
            if (currentX >= Width)
            {
                currentX = 0;
                y++;
            }
            
            // Stop if we've gone past the bottom
            if (y >= Height)
                break;
        }
    }
    
    /// <summary>
    /// Clears the entire buffer.
    /// </summary>
    public void Clear()
    {
        _backBuffer.Clear();
        lock (_dirtyLock)
        {
            _dirtyRegions.Clear();
            // Mark entire buffer as dirty
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _dirtyRegions.Add((x, y));
                }
            }
        }
    }
    
    /// <summary>
    /// Clears a rectangular region in the buffer.
    /// </summary>
    public void ClearRect(int x, int y, int width, int height)
    {
        _backBuffer.ClearRect(x, y, width, height);
        
        // Mark the cleared region as dirty
        for (int row = y; row < Math.Min(y + height, Height); row++)
        {
            for (int col = x; col < Math.Min(x + width, Width); col++)
            {
                if (col >= 0 && row >= 0)
                {
                    MarkDirty(col, row);
                }
            }
        }
    }
    
    /// <summary>
    /// Fills a rectangular region with the specified cell.
    /// </summary>
    public void FillRect(int x, int y, int width, int height, Cell cell)
    {
        _backBuffer.FillRect(x, y, width, height, cell);
        
        // Mark the filled region as dirty
        for (int row = y; row < Math.Min(y + height, Height); row++)
        {
            for (int col = x; col < Math.Min(x + width, Width); col++)
            {
                if (col >= 0 && row >= 0)
                {
                    MarkDirty(col, row);
                }
            }
        }
    }
    
    /// <summary>
    /// Swaps the front and back buffers and returns the dirty regions.
    /// </summary>
    public IEnumerable<DirtyRegion> SwapBuffers()
    {
        List<DirtyRegion> regions;
        
        lock (_swapLock)
        {
            // Get dirty regions with their old and new cells
            regions = new List<DirtyRegion>();
            
            lock (_dirtyLock)
            {
                foreach (var (x, y) in _dirtyRegions)
                {
                    if (_backBuffer.TryGetCell(x, y, out var newCell) &&
                        _frontBuffer.TryGetCell(x, y, out var oldCell))
                    {
                        // Only include if the cell actually changed
                        if (newCell != oldCell)
                        {
                            regions.Add(new DirtyRegion(x, y, oldCell, newCell));
                        }
                    }
                }
                
                _dirtyRegions.Clear();
            }
            
            // Swap buffers
            (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
            
            // Copy front buffer content to back buffer
            _backBuffer.CopyFrom(_frontBuffer);
        }
        
        return regions;
    }
    
    /// <summary>
    /// Resizes the buffers to the new dimensions.
    /// </summary>
    public void Resize(int width, int height)
    {
        lock (_swapLock)
        {
            var oldFront = _frontBuffer;
            
            // Create new buffers with new dimensions
            _frontBuffer = new Buffer(width, height);
            _backBuffer = new Buffer(width, height);
            
            // Copy old content to back buffer (what we want to display)
            _backBuffer.CopyFrom(oldFront);
            
            // Force front buffer to have different content to ensure all cells are detected as changed
            // We'll fill it with a special "invalid" cell that will never match normal content
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _frontBuffer.TrySetCell(x, y, new Cell('\uffff', new Style { Foreground = Color.FromRgb(255, 255, 255) }));
                }
            }
            
            // Mark everything as dirty after resize
            lock (_dirtyLock)
            {
                _dirtyRegions.Clear();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        _dirtyRegions.Add((x, y));
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the current front buffer for reading.
    /// </summary>
    public Buffer GetFrontBuffer()
    {
        lock (_swapLock)
        {
            return _frontBuffer.Clone();
        }
    }
    
    /// <summary>
    /// Marks a specific cell as dirty.
    /// </summary>
    private void MarkDirty(int x, int y)
    {
        lock (_dirtyLock)
        {
            _dirtyRegions.Add((x, y));
        }
    }
    
    /// <summary>
    /// Marks all cells as dirty, forcing a full redraw.
    /// </summary>
    public void MarkAllDirty()
    {
        lock (_dirtyLock)
        {
            _dirtyRegions.Clear();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _dirtyRegions.Add((x, y));
                }
            }
        }
    }
}

/// <summary>
/// Represents a dirty region that needs to be rendered.
/// </summary>
public readonly struct DirtyRegion
{
    /// <summary>
    /// Gets the X coordinate of the dirty cell.
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// Gets the Y coordinate of the dirty cell.
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// Gets the old cell value before the change.
    /// </summary>
    public Cell OldCell { get; }
    
    /// <summary>
    /// Gets the new cell value after the change.
    /// </summary>
    public Cell NewCell { get; }
    
    public DirtyRegion(int x, int y, Cell oldCell, Cell newCell)
    {
        X = x;
        Y = y;
        OldCell = oldCell;
        NewCell = newCell;
    }
}