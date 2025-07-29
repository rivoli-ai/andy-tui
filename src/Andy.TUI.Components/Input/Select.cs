using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Input;

/// <summary>
/// A dropdown select component for choosing from a list of options.
/// </summary>
/// <typeparam name="T">The type of items in the select list.</typeparam>
public class Select<T> : InputComponent where T : notnull
{
    private readonly List<SelectItem<T>> _items = new();
    private int _selectedIndex = -1;
    private int _highlightedIndex = -1;
    private bool _isOpen;
    private string _searchText = string.Empty;
    private DateTime _lastSearchTime = DateTime.MinValue;
    
    /// <summary>
    /// Gets or sets the items in the select list.
    /// </summary>
    public IEnumerable<SelectItem<T>> Items
    {
        get => _items;
        set
        {
            _items.Clear();
            _items.AddRange(value);
            
            // Reset selection if current selection is invalid
            if (_selectedIndex >= _items.Count)
            {
                _selectedIndex = -1;
                OnSelectionChanged();
            }
            
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    public T? SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count 
            ? _items[_selectedIndex].Value 
            : default;
        set
        {
            var index = _items.FindIndex(i => EqualityComparer<T>.Default.Equals(i.Value, value));
            if (index != _selectedIndex)
            {
                _selectedIndex = index;
                OnSelectionChanged();
                RequestRender();
            }
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
                throw new ArgumentOutOfRangeException(nameof(value));
                
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnSelectionChanged();
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the placeholder text shown when no item is selected.
    /// </summary>
    public string Placeholder { get; set; } = "Select an option...";
    
    /// <summary>
    /// Gets or sets whether the dropdown is currently open.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                if (_isOpen)
                {
                    _highlightedIndex = _selectedIndex >= 0 ? _selectedIndex : 0;
                }
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets the currently highlighted index when the dropdown is open.
    /// </summary>
    public int HighlightedIndex => _highlightedIndex;
    
    /// <summary>
    /// Gets or sets the maximum number of items to display when open.
    /// </summary>
    public int MaxDisplayItems { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets whether to allow filtering items by typing.
    /// </summary>
    public bool AllowFiltering { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the display function for items.
    /// </summary>
    public Func<T, string>? DisplayFunc { get; set; }
    
    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs<T>>? SelectionChanged;
    
    protected override Size MeasureCore(Size availableSize)
    {
        var width = Math.Min(availableSize.Width - Padding.Horizontal, 30); // Default width
        var height = 1 + Padding.Vertical + 2; // Single line + borders
        
        if (_isOpen)
        {
            var dropdownHeight = Math.Min(_items.Count, MaxDisplayItems) + 2; // +2 for borders
            height += dropdownHeight;
        }
        
        return new Size(width + Padding.Horizontal, height);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        // Base handles bounds
    }
    
    protected override VirtualNode OnRender()
    {
        var children = new List<VirtualNode>();
        var contentBounds = ContentBounds;
        
        // Main select box
        RenderSelectBox(children, contentBounds);
        
        // Dropdown list when open
        if (_isOpen)
        {
            RenderDropdown(children, contentBounds);
        }
        
        return CreateLayoutNode("select", children.ToArray());
    }
    
    private void RenderSelectBox(List<VirtualNode> children, Rectangle contentBounds)
    {
        // Background
        var bgColor = !IsEnabled ? Color.DarkGray : 
                      IsFocused ? Color.DarkBlue : 
                      Color.None;
                      
        if (bgColor != Color.None)
        {
            children.Add(CreateBackground(Bounds, bgColor));
        }
        
        // Display text
        var displayText = GetDisplayText();
        var textColor = !IsEnabled ? Color.Gray :
                        _selectedIndex < 0 ? Color.Gray :
                        IsFocused ? Color.White : Color.Gray;
                        
        children.Add(CreateText(contentBounds.X, contentBounds.Y, displayText, textColor));
        
        // Dropdown arrow
        var arrow = _isOpen ? "▲" : "▼";
        children.Add(CreateText(contentBounds.Right - 2, contentBounds.Y, arrow, textColor));
        
        // Border
        var borderColor = !IsEnabled ? Color.DarkGray :
                          IsFocused ? Color.Cyan :
                          Color.Gray;
        children.Add(CreateBorder(new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 3), BorderStyle.Single, borderColor));
    }
    
    private void RenderDropdown(List<VirtualNode> children, Rectangle contentBounds)
    {
        var dropdownY = Bounds.Y + 3;
        var dropdownHeight = Math.Min(_items.Count, MaxDisplayItems) + 2;
        var dropdownBounds = new Rectangle(Bounds.X, dropdownY, Bounds.Width, dropdownHeight);
        
        // Background
        children.Add(CreateBackground(dropdownBounds, Color.Black));
        
        // Items
        var visibleItems = GetVisibleItems();
        var itemY = dropdownY + 1;
        
        for (int i = 0; i < visibleItems.Count && i < MaxDisplayItems; i++)
        {
            var item = visibleItems[i];
            var itemIndex = _items.IndexOf(item);
            var isHighlighted = itemIndex == _highlightedIndex;
            var isSelected = itemIndex == _selectedIndex;
            
            // Item background
            if (isHighlighted)
            {
                children.Add(CreateBackground(
                    new Rectangle(dropdownBounds.X + 1, itemY, dropdownBounds.Width - 2, 1),
                    Color.DarkCyan));
            }
            
            // Item text
            var itemText = GetItemDisplayText(item);
            var maxTextWidth = Math.Max(1, dropdownBounds.Width - 4);
            if (itemText.Length > maxTextWidth && maxTextWidth > 3)
            {
                itemText = itemText.Substring(0, maxTextWidth - 3) + "...";
            }
            else if (itemText.Length > maxTextWidth)
            {
                itemText = itemText.Substring(0, maxTextWidth);
            }
            
            var itemColor = isHighlighted ? Color.White : 
                            isSelected ? Color.Cyan : 
                            Color.Gray;
                            
            children.Add(CreateText(dropdownBounds.X + 2, itemY, itemText, itemColor));
            
            // Selection indicator
            if (isSelected)
            {
                children.Add(CreateText(dropdownBounds.X + 1, itemY, "●", Color.Cyan));
            }
            
            itemY++;
        }
        
        // Border
        children.Add(CreateBorder(dropdownBounds, BorderStyle.Single, Color.Gray));
        
        // Scrollbar if needed
        if (_items.Count > MaxDisplayItems)
        {
            RenderScrollbar(children, dropdownBounds);
        }
    }
    
    private void RenderScrollbar(List<VirtualNode> children, Rectangle bounds)
    {
        var scrollbarX = bounds.Right - 1;
        var scrollbarHeight = bounds.Height - 2;
        var thumbHeight = Math.Max(1, (scrollbarHeight * MaxDisplayItems) / _items.Count);
        var scrollPosition = _highlightedIndex * scrollbarHeight / _items.Count;
        
        // Track
        for (int y = bounds.Y + 1; y < bounds.Bottom - 1; y++)
        {
            children.Add(CreateText(scrollbarX, y, "│", Color.DarkGray));
        }
        
        // Thumb
        for (int i = 0; i < thumbHeight && scrollPosition + i < scrollbarHeight; i++)
        {
            children.Add(CreateText(scrollbarX, bounds.Y + 1 + scrollPosition + i, "█", Color.Gray));
        }
    }
    
    protected override bool OnKeyPress(KeyEventArgs args)
    {
        if (!_isOpen)
        {
            switch (args.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                case ConsoleKey.DownArrow:
                    IsOpen = true;
                    return true;
                    
                default:
                    if (AllowFiltering && args.KeyChar != '\0' && !char.IsControl(args.KeyChar))
                    {
                        IsOpen = true;
                        SearchForItem(args.KeyChar);
                        return true;
                    }
                    break;
            }
        }
        else
        {
            switch (args.Key)
            {
                case ConsoleKey.Escape:
                    IsOpen = false;
                    return true;
                    
                case ConsoleKey.Enter:
                    if (_highlightedIndex >= 0 && _highlightedIndex < _items.Count)
                    {
                        SelectedIndex = _highlightedIndex;
                        IsOpen = false;
                    }
                    return true;
                    
                case ConsoleKey.UpArrow:
                    MoveHighlight(-1);
                    return true;
                    
                case ConsoleKey.DownArrow:
                    MoveHighlight(1);
                    return true;
                    
                case ConsoleKey.PageUp:
                    MoveHighlight(-MaxDisplayItems);
                    return true;
                    
                case ConsoleKey.PageDown:
                    MoveHighlight(MaxDisplayItems);
                    return true;
                    
                case ConsoleKey.Home:
                    _highlightedIndex = 0;
                    RequestRender();
                    return true;
                    
                case ConsoleKey.End:
                    _highlightedIndex = _items.Count - 1;
                    RequestRender();
                    return true;
                    
                default:
                    if (AllowFiltering && args.KeyChar != '\0' && !char.IsControl(args.KeyChar))
                    {
                        SearchForItem(args.KeyChar);
                        return true;
                    }
                    break;
            }
        }
        
        return false;
    }
    
    protected override bool OnMouseEvent(MouseEventArgs args)
    {
        if (args.Button == MouseButton.Left)
        {
            // Check if click is on main select box
            if (args.Y >= Bounds.Y && args.Y < Bounds.Y + 3)
            {
                Focus();
                IsOpen = !IsOpen;
                return true;
            }
            
            // Check if click is on dropdown item
            if (_isOpen)
            {
                var dropdownY = Bounds.Y + 3;
                if (args.Y > dropdownY && args.Y < dropdownY + Math.Min(_items.Count, MaxDisplayItems) + 1)
                {
                    var itemIndex = args.Y - dropdownY - 1;
                    if (itemIndex >= 0 && itemIndex < _items.Count)
                    {
                        SelectedIndex = itemIndex;
                        IsOpen = false;
                        return true;
                    }
                }
            }
        }
        
        return base.OnMouseEvent(args);
    }
    
    private void MoveHighlight(int delta)
    {
        var newIndex = _highlightedIndex + delta;
        _highlightedIndex = Math.Max(0, Math.Min(_items.Count - 1, newIndex));
        RequestRender();
    }
    
    private void SearchForItem(char keyChar)
    {
        var now = DateTime.Now;
        
        // Reset search if more than 1 second has passed
        if ((now - _lastSearchTime).TotalSeconds > 1)
        {
            _searchText = string.Empty;
        }
        
        _searchText += keyChar;
        _lastSearchTime = now;
        
        // Find first item that starts with search text
        var searchIndex = _items.FindIndex(i => 
            GetItemDisplayText(i).StartsWith(_searchText, StringComparison.OrdinalIgnoreCase));
            
        if (searchIndex >= 0)
        {
            _highlightedIndex = searchIndex;
            RequestRender();
        }
    }
    
    private string GetDisplayText()
    {
        if (_selectedIndex < 0)
            return Placeholder;
            
        return GetItemDisplayText(_items[_selectedIndex]);
    }
    
    private string GetItemDisplayText(SelectItem<T> item)
    {
        if (!string.IsNullOrEmpty(item.Display))
            return item.Display;
            
        if (DisplayFunc != null)
            return DisplayFunc(item.Value);
            
        return item.Value.ToString() ?? string.Empty;
    }
    
    private List<SelectItem<T>> GetVisibleItems()
    {
        // TODO: Implement filtering
        return _items;
    }
    
    private void OnSelectionChanged()
    {
        var args = new SelectionChangedEventArgs<T>(
            _selectedIndex >= 0 ? _items[_selectedIndex].Value : default,
            _selectedIndex);
            
        SelectionChanged?.Invoke(this, args);
    }
    
    private VirtualNode CreateBackground(Rectangle bounds, Color color)
    {
        return new ElementNode("rect", new Dictionary<string, object?>
        {
            ["x"] = bounds.X,
            ["y"] = bounds.Y,
            ["width"] = bounds.Width,
            ["height"] = bounds.Height,
            ["fill"] = color
        });
    }
    
    private VirtualNode CreateText(int x, int y, string text, Color color)
    {
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = x,
            ["y"] = y,
            ["color"] = color
        }, new TextNode(text));
    }
    
    private VirtualNode CreateBorder(Rectangle bounds, BorderStyle style, Color color)
    {
        var chars = GetBorderChars(style);
        var nodes = new List<VirtualNode>();
        
        // Top border
        nodes.Add(CreateText(bounds.X, bounds.Y, chars.TopLeft.ToString(), color));
        for (int x = bounds.X + 1; x < bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, bounds.Y, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(bounds.Right - 1, bounds.Y, chars.TopRight.ToString(), color));
        
        // Side borders
        for (int y = bounds.Y + 1; y < bounds.Bottom - 1; y++)
        {
            nodes.Add(CreateText(bounds.X, y, chars.Vertical.ToString(), color));
            nodes.Add(CreateText(bounds.Right - 1, y, chars.Vertical.ToString(), color));
        }
        
        // Bottom border
        nodes.Add(CreateText(bounds.X, bounds.Bottom - 1, chars.BottomLeft.ToString(), color));
        for (int x = bounds.X + 1; x < bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, bounds.Bottom - 1, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(bounds.Right - 1, bounds.Bottom - 1, chars.BottomRight.ToString(), color));
        
        return new FragmentNode(nodes);
    }
    
    private (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => ('╭', '╮', '╰', '╯', '─', '│'),
            _ => ('+', '+', '+', '+', '-', '|')
        };
    }
}

/// <summary>
/// Represents an item in a select list.
/// </summary>
public class SelectItem<T> where T : notnull
{
    /// <summary>
    /// Gets or sets the value of the item.
    /// </summary>
    public T Value { get; set; }
    
    /// <summary>
    /// Gets or sets the display text for the item.
    /// </summary>
    public string? Display { get; set; }
    
    /// <summary>
    /// Gets or sets whether the item is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the SelectItem class.
    /// </summary>
    public SelectItem(T value, string? display = null)
    {
        Value = value;
        Display = display;
    }
}

/// <summary>
/// Event arguments for selection change events.
/// </summary>
public class SelectionChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the selected value.
    /// </summary>
    public T? SelectedValue { get; }
    
    /// <summary>
    /// Gets the selected index.
    /// </summary>
    public int SelectedIndex { get; }
    
    public SelectionChangedEventArgs(T? selectedValue, int selectedIndex)
    {
        SelectedValue = selectedValue;
        SelectedIndex = selectedIndex;
    }
}