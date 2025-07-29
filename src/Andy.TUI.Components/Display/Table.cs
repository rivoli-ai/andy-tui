using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Display;

/// <summary>
/// A table component that displays data in rows and columns.
/// </summary>
public class Table<T> : LayoutComponent
{
    private List<TableColumn<T>> _columns = new();
    private List<T> _items = new();
    private int _selectedIndex = -1;
    private int _scrollOffset = 0;
    private bool _showHeader = true;
    private bool _showBorder = true;
    private bool _allowSelection = false;
    private int _visibleRows;
    
    /// <summary>
    /// Gets or sets the columns for the table.
    /// </summary>
    public List<TableColumn<T>> Columns
    {
        get => _columns;
        set
        {
            _columns = value ?? new List<TableColumn<T>>();
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the items to display in the table.
    /// </summary>
    public List<T> Items
    {
        get => _items;
        set
        {
            _items = value ?? new List<T>();
            _selectedIndex = _items.Count > 0 && _allowSelection ? 0 : -1;
            _scrollOffset = 0;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets whether to show the header row.
    /// </summary>
    public bool ShowHeader
    {
        get => _showHeader;
        set
        {
            _showHeader = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets whether to show borders.
    /// </summary>
    public bool ShowBorder
    {
        get => _showBorder;
        set
        {
            _showBorder = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets whether to allow row selection.
    /// </summary>
    public bool AllowSelection
    {
        get => _allowSelection;
        set
        {
            _allowSelection = value;
            if (!value)
                _selectedIndex = -1;
            else if (_items.Count > 0 && _selectedIndex == -1)
                _selectedIndex = 0;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the selected index.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < -1 || value >= _items.Count)
                return;
                
            _selectedIndex = value;
            EnsureSelectedVisible();
            RequestRender();
            OnSelectionChanged();
        }
    }
    
    /// <summary>
    /// Gets the selected item.
    /// </summary>
    public T? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : default;
    
    /// <summary>
    /// Event raised when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;
    
    /// <summary>
    /// Event raised when a row is activated (Enter key).
    /// </summary>
    public event EventHandler<TableRowEventArgs<T>>? RowActivated;
    
    protected override Size MeasureCore(Size availableSize)
    {
        if (_columns.Count == 0)
            return new Size(0, 0);
            
        var totalWidth = _showBorder ? 1 : 0; // Left border
        
        foreach (var column in _columns)
        {
            totalWidth += column.Width;
            if (_showBorder)
                totalWidth += 1; // Right border
        }
        
        var totalHeight = 0;
        if (_showBorder)
            totalHeight += 1; // Top border
        if (_showHeader)
            totalHeight += _showBorder ? 2 : 1; // Header + optional separator
        totalHeight += Math.Min(_items.Count, 10); // Default to show up to 10 rows
        if (_showBorder)
            totalHeight += 1; // Bottom border
            
        return new Size(totalWidth, totalHeight);
    }
    
    protected override void ArrangeCore(Rectangle finalRect)
    {
        
        // Calculate visible rows
        _visibleRows = finalRect.Height;
        if (_showBorder)
            _visibleRows -= 2; // Top and bottom borders
        if (_showHeader)
            _visibleRows -= _showBorder ? 2 : 1; // Header + optional separator
            
        _visibleRows = Math.Max(0, _visibleRows);
        EnsureSelectedVisible();
    }
    
    protected override VirtualNode OnRender()
    {
        if (_columns.Count == 0 || Bounds.Width < 3 || Bounds.Height < 3)
            return new ElementNode("table", new Dictionary<string, object?>
            {
                ["x"] = Bounds.X,
                ["y"] = Bounds.Y,
                ["width"] = Bounds.Width,
                ["height"] = Bounds.Height
            });
            
        var nodes = new List<VirtualNode>();
        var currentY = 0;
        
        // Top border
        if (_showBorder)
        {
            nodes.Add(RenderBorder(currentY, BorderPosition.Top));
            currentY++;
        }
        
        // Header
        if (_showHeader)
        {
            nodes.Add(RenderHeaderRow(currentY));
            currentY++;
            
            if (_showBorder)
            {
                nodes.Add(RenderBorder(currentY, BorderPosition.Middle));
                currentY++;
            }
        }
        
        // Data rows
        var visibleItems = _items.Skip(_scrollOffset).Take(_visibleRows).ToList();
        for (int i = 0; i < visibleItems.Count; i++)
        {
            var item = visibleItems[i];
            var isSelected = _allowSelection && (_scrollOffset + i) == _selectedIndex;
            nodes.Add(RenderDataRow(currentY, item, isSelected));
            currentY++;
        }
        
        // Don't fill empty space - let the table size to its content
        
        // Bottom border
        if (_showBorder)
        {
            nodes.Add(RenderBorder(currentY, BorderPosition.Bottom));
        }
        
        // Combine all nodes into a container
        return new ElementNode("table", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height
        }, nodes.ToArray());
    }
    
    private VirtualNode RenderBorder(int y, BorderPosition position)
    {
        var chars = GetBorderCharacters(position);
        var content = new System.Text.StringBuilder();
        
        content.Append(chars.left);
        
        for (int i = 0; i < _columns.Count; i++)
        {
            content.Append(new string(chars.horizontal, _columns[i].Width));
            if (i < _columns.Count - 1)
                content.Append(chars.cross);
        }
        
        content.Append(chars.right);
        
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = y,
            ["style"] = Terminal.Style.Default
        }, new TextNode(content.ToString()));
    }
    
    private VirtualNode RenderHeaderRow(int y)
    {
        var content = new System.Text.StringBuilder();
        var x = 0;
        
        if (_showBorder)
        {
            content.Append('│');
            x++;
        }
        
        for (int i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            var header = column.Header.PadRight(column.Width);
            if (header.Length > column.Width)
                header = header.Substring(0, column.Width);
            content.Append(header);
            
            if (_showBorder && i < _columns.Count - 1)
                content.Append('│');
        }
        
        if (_showBorder)
            content.Append('│');
            
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = y,
            ["style"] = Terminal.Style.Default.WithBold()
        }, new TextNode(content.ToString()));
    }
    
    private VirtualNode RenderDataRow(int y, T item, bool isSelected)
    {
        var content = new System.Text.StringBuilder();
        var style = isSelected ? 
            Terminal.Style.Default.WithBackgroundColor(Color.DarkBlue).WithForegroundColor(Color.White) :
            Terminal.Style.Default;
        
        if (_showBorder)
            content.Append('│');
            
        for (int i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            var value = column.GetValue(item) ?? string.Empty;
            
            // Format according to alignment
            string formatted;
            if (value.Length > column.Width)
            {
                formatted = value.Substring(0, column.Width);
            }
            else
            {
                formatted = column.Alignment switch
                {
                    TableAlignment.Left => value.PadRight(column.Width),
                    TableAlignment.Right => value.PadLeft(column.Width),
                    TableAlignment.Center => value.PadLeft((column.Width + value.Length) / 2).PadRight(column.Width),
                    _ => value.PadRight(column.Width)
                };
            }
            
            content.Append(formatted);
            
            if (_showBorder && i < _columns.Count - 1)
                content.Append('│');
        }
        
        if (_showBorder)
            content.Append('│');
            
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = y,
            ["style"] = style
        }, new TextNode(content.ToString()));
    }
    
    private VirtualNode RenderEmptyRow(int y)
    {
        var content = new System.Text.StringBuilder();
        
        if (_showBorder)
            content.Append('│');
            
        for (int i = 0; i < _columns.Count; i++)
        {
            content.Append(new string(' ', _columns[i].Width));
            if (_showBorder && i < _columns.Count - 1)
                content.Append('│');
        }
        
        if (_showBorder)
            content.Append('│');
            
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = y,
            ["style"] = Terminal.Style.Default
        }, new TextNode(content.ToString()));
    }
    
    private (char left, char horizontal, char cross, char right) GetBorderCharacters(BorderPosition position)
    {
        return position switch
        {
            BorderPosition.Top => ('┌', '─', '┬', '┐'),
            BorderPosition.Middle => ('├', '─', '┼', '┤'),
            BorderPosition.Bottom => ('└', '─', '┴', '┘'),
            _ => ('+', '-', '+', '+')
        };
    }
    
    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (!_allowSelection || _items.Count == 0)
            return false;
            
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                if (_selectedIndex > 0)
                {
                    SelectedIndex = _selectedIndex - 1;
                    return true;
                }
                break;
                
            case ConsoleKey.DownArrow:
                if (_selectedIndex < _items.Count - 1)
                {
                    SelectedIndex = _selectedIndex + 1;
                    return true;
                }
                break;
                
            case ConsoleKey.PageUp:
                SelectedIndex = Math.Max(0, _selectedIndex - _visibleRows);
                return true;
                
            case ConsoleKey.PageDown:
                SelectedIndex = Math.Min(_items.Count - 1, _selectedIndex + _visibleRows);
                return true;
                
            case ConsoleKey.Home:
                SelectedIndex = 0;
                return true;
                
            case ConsoleKey.End:
                SelectedIndex = _items.Count - 1;
                return true;
                
            case ConsoleKey.Enter:
                if (_selectedIndex >= 0)
                {
                    OnRowActivated();
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    private void EnsureSelectedVisible()
    {
        if (_selectedIndex < 0 || _visibleRows <= 0)
            return;
            
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + _visibleRows)
        {
            _scrollOffset = _selectedIndex - _visibleRows + 1;
        }
    }
    
    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnRowActivated()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
        {
            RowActivated?.Invoke(this, new TableRowEventArgs<T>(_items[_selectedIndex], _selectedIndex));
        }
    }
    
    private enum BorderPosition
    {
        Top,
        Middle,
        Bottom
    }
}

/// <summary>
/// Represents a column in a table.
/// </summary>
public class TableColumn<T>
{
    /// <summary>
    /// Gets or sets the column header text.
    /// </summary>
    public string Header { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the column width.
    /// </summary>
    public int Width { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    public TableAlignment Alignment { get; set; } = TableAlignment.Left;
    
    /// <summary>
    /// Gets or sets the function to extract the value from an item.
    /// </summary>
    public Func<T, string?>? ValueGetter { get; set; }
    
    /// <summary>
    /// Gets the display value for an item.
    /// </summary>
    public string GetValue(T item)
    {
        return ValueGetter?.Invoke(item) ?? item?.ToString() ?? string.Empty;
    }
}

/// <summary>
/// Defines table text alignment options.
/// </summary>
public enum TableAlignment
{
    /// <summary>
    /// Left-aligned text.
    /// </summary>
    Left,
    
    /// <summary>
    /// Right-aligned text.
    /// </summary>
    Right,
    
    /// <summary>
    /// Center-aligned text.
    /// </summary>
    Center
}

/// <summary>
/// Event arguments for table row events.
/// </summary>
public class TableRowEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the item associated with the row.
    /// </summary>
    public T Item { get; }
    
    /// <summary>
    /// Gets the index of the row.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Creates a new instance of TableRowEventArgs.
    /// </summary>
    public TableRowEventArgs(T item, int index)
    {
        Item = item;
        Index = index;
    }
}