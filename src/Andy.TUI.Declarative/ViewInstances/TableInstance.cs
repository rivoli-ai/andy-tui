using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Table view with sorting and selection.
/// </summary>
public class TableInstance<T> : ViewInstance, IFocusable
{
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private List<T> _sortedItems = new();
    private IReadOnlyList<TableColumn<T>> _columns = Array.Empty<TableColumn<T>>();
    private Binding<Optional<T>>? _selectedBinding;
    private int _visibleRows = 10;
    private bool _showHeader = true;
    private bool _showBorder = true;
    private bool _allowSelection = true;
    private bool _isFocused;
    private int _highlightedIndex = 0;
    private int _scrollOffset = 0;
    private int _sortColumnIndex = -1;
    private bool _sortAscending = true;
    private IDisposable? _bindingSubscription;
    private List<int> _columnWidths = new();
    
    public TableInstance(string id) : base(id)
    {
    }
    
    // IFocusable implementation
    public bool CanFocus => _allowSelection;
    public bool IsFocused => _isFocused;
    
    public void OnGotFocus()
    {
        _isFocused = true;
        
        // Set highlighted index to current selection if any
        if (_selectedBinding?.Value.TryGetValue(out var selectedValue) == true && _sortedItems.Count > 0)
        {
            var currentIndex = _sortedItems.IndexOf(selectedValue);
            if (currentIndex >= 0)
            {
                _highlightedIndex = currentIndex;
                UpdateScroll();
            }
        }
        
        InvalidateView();
    }
    
    public void OnLostFocus()
    {
        _isFocused = false;
        InvalidateView();
    }
    
    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (!_allowSelection || _sortedItems.Count == 0) return false;
        
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                if (_highlightedIndex > 0)
                {
                    _highlightedIndex--;
                    UpdateScroll();
                    InvalidateView();
                }
                return true;
                
            case ConsoleKey.DownArrow:
                if (_highlightedIndex < _sortedItems.Count - 1)
                {
                    _highlightedIndex++;
                    UpdateScroll();
                    InvalidateView();
                }
                return true;
                
            case ConsoleKey.Home:
                _highlightedIndex = 0;
                UpdateScroll();
                InvalidateView();
                return true;
                
            case ConsoleKey.End:
                _highlightedIndex = _sortedItems.Count - 1;
                UpdateScroll();
                InvalidateView();
                return true;
                
            case ConsoleKey.PageUp:
                _highlightedIndex = Math.Max(0, _highlightedIndex - _visibleRows);
                UpdateScroll();
                InvalidateView();
                return true;
                
            case ConsoleKey.PageDown:
                _highlightedIndex = Math.Min(_sortedItems.Count - 1, _highlightedIndex + _visibleRows);
                UpdateScroll();
                InvalidateView();
                return true;
                
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                // Select the highlighted item
                if (_selectedBinding != null && _highlightedIndex < _sortedItems.Count)
                {
                    _selectedBinding.Value = Optional<T>.Some(_sortedItems[_highlightedIndex]);
                }
                return true;
                
            // Sort by column using number keys
            case ConsoleKey.D1:
            case ConsoleKey.D2:
            case ConsoleKey.D3:
            case ConsoleKey.D4:
            case ConsoleKey.D5:
            case ConsoleKey.D6:
            case ConsoleKey.D7:
            case ConsoleKey.D8:
            case ConsoleKey.D9:
                var colIndex = (int)keyInfo.Key - (int)ConsoleKey.D1;
                if (colIndex < _columns.Count && _columns[colIndex].Sortable)
                {
                    SortByColumn(colIndex);
                    return true;
                }
                break;
        }
        
        return false;
    }
    
    private void SortByColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count) return;
        
        var column = _columns[columnIndex];
        if (!column.Sortable || column.Comparer == null) return;
        
        // Toggle sort direction if same column
        if (_sortColumnIndex == columnIndex)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumnIndex = columnIndex;
            _sortAscending = true;
        }
        
        // Sort items
        _sortedItems = _sortAscending
            ? _sortedItems.OrderBy(x => x, Comparer<T>.Create(column.Comparer)).ToList()
            : _sortedItems.OrderByDescending(x => x, Comparer<T>.Create(column.Comparer)).ToList();
        
        // Update highlighted index to track selected item
        if (_selectedBinding?.Value.TryGetValue(out var selectedValue) == true)
        {
            var newIndex = _sortedItems.IndexOf(selectedValue);
            if (newIndex >= 0)
            {
                _highlightedIndex = newIndex;
                UpdateScroll();
            }
        }
        
        InvalidateView();
    }
    
    private void UpdateScroll()
    {
        // Ensure highlighted item is visible
        if (_highlightedIndex < _scrollOffset)
        {
            _scrollOffset = _highlightedIndex;
        }
        else if (_highlightedIndex >= _scrollOffset + _visibleRows)
        {
            _scrollOffset = _highlightedIndex - _visibleRows + 1;
        }
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Table<T> table)
            throw new ArgumentException($"Expected Table<{typeof(T).Name}> declaration");
        
        // Update properties from declaration
        _items = table.GetItems();
        _columns = table.GetColumns();
        _visibleRows = table.GetVisibleRows();
        _showHeader = table.GetShowHeader();
        _showBorder = table.GetShowBorder();
        _allowSelection = table.GetAllowSelection();
        
        // Reset sorted items
        _sortedItems = _items.ToList();
        
        // Reapply sort if active
        if (_sortColumnIndex >= 0 && _sortColumnIndex < _columns.Count)
        {
            var column = _columns[_sortColumnIndex];
            if (column.Sortable && column.Comparer != null)
            {
                _sortedItems = _sortAscending
                    ? _sortedItems.OrderBy(x => x, Comparer<T>.Create(column.Comparer)).ToList()
                    : _sortedItems.OrderByDescending(x => x, Comparer<T>.Create(column.Comparer)).ToList();
            }
        }
        
        // Handle binding changes
        var newBinding = table.GetSelectedBinding();
        if (newBinding != _selectedBinding)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();
            
            // Subscribe to new binding
            _selectedBinding = newBinding;
            if (_selectedBinding != null)
            {
                _bindingSubscription = new BindingSubscription(_selectedBinding, () => InvalidateView());
            }
        }
        
        // Ensure highlighted index is valid
        if (_highlightedIndex >= _sortedItems.Count)
        {
            _highlightedIndex = Math.Max(0, _sortedItems.Count - 1);
        }
        
        // Calculate column widths
        CalculateColumnWidths();
    }
    
    private void CalculateColumnWidths()
    {
        _columnWidths.Clear();
        
        foreach (var column in _columns)
        {
            if (column.Width.HasValue)
            {
                _columnWidths.Add(column.Width.Value);
            }
            else
            {
                // Calculate based on content
                var maxWidth = column.Header.Length;
                foreach (var item in _items)
                {
                    var content = column.Renderer(item);
                    maxWidth = Math.Max(maxWidth, content.Length);
                }
                _columnWidths.Add(Math.Min(maxWidth + 2, 30)); // Cap at 30 chars
            }
        }
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();
        
        // Calculate total width
        var totalWidth = _columnWidths.Sum() + (_columns.Count - 1) * 3; // 3 chars for " | " separator
        if (_showBorder)
        {
            totalWidth += 4; // 2 for left/right borders + 2 padding
        }
        
        // Calculate height
        var height = _visibleRows;
        if (_showHeader)
        {
            height += 2; // Header + separator line
        }
        if (_showBorder)
        {
            height += 2; // Top/bottom borders
        }
        
        layout.Width = constraints.ConstrainWidth(totalWidth);
        layout.Height = constraints.ConstrainHeight(height);
        
        return layout;
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var elements = new List<VirtualNode>();
        var currentY = (int)layout.AbsoluteY;
        
        T? selectedItem = default;
        var hasSelection = _selectedBinding?.Value.TryGetValue(out selectedItem) == true;
        
        // Border style
        var borderStyle = _isFocused
            ? Style.Default.WithForegroundColor(Color.White)
            : Style.Default.WithForegroundColor(Color.DarkGray);
        
        // Top border
        if (_showBorder)
        {
            var topBorder = "┌" + new string('─', (int)layout.Width - 2) + "┐";
            elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, topBorder, borderStyle));
        }
        
        // Header
        if (_showHeader)
        {
            var headerLine = BuildTableRow(_columns.Select((col, i) => 
            {
                var header = col.Header;
                if (col.Sortable)
                {
                    if (_sortColumnIndex == i)
                    {
                        header += _sortAscending ? " ▲" : " ▼";
                    }
                    header = $"[{i + 1}] {header}";
                }
                return header;
            }).ToList());
            
            if (_showBorder)
            {
                headerLine = "│ " + headerLine + " │";
            }
            
            elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, headerLine, 
                Style.Default.WithBold(true).WithForegroundColor(Color.Cyan)));
            
            // Header separator
            var separator = BuildTableRow(_columnWidths.Select(w => new string('─', w)).ToList());
            if (_showBorder)
            {
                separator = "├─" + separator.Replace(" | ", "─┼─") + "─┤";
            }
            else
            {
                separator = separator.Replace(" | ", "─┼─");
            }
            
            elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, separator, borderStyle));
        }
        
        // Data rows
        for (int i = 0; i < _visibleRows; i++)
        {
            var itemIndex = _scrollOffset + i;
            if (itemIndex < _sortedItems.Count)
            {
                var item = _sortedItems[itemIndex];
                var rowData = _columns.Select(col => col.Renderer(item)).ToList();
                var rowLine = BuildTableRow(rowData);
                
                if (_showBorder)
                {
                    rowLine = "│ " + rowLine + " │";
                }
                
                // Determine row style
                var rowStyle = Style.Default;
                if (_allowSelection && _isFocused && itemIndex == _highlightedIndex)
                {
                    rowStyle = Style.Default
                        .WithForegroundColor(Color.Black)
                        .WithBackgroundColor(Color.White);
                }
                else if (hasSelection && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    rowStyle = Style.Default.WithForegroundColor(Color.Green);
                }
                
                elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, rowLine, rowStyle));
            }
            else
            {
                // Empty row
                var emptyRow = BuildTableRow(_columns.Select(_ => "").ToList());
                if (_showBorder)
                {
                    emptyRow = "│ " + emptyRow + " │";
                }
                
                elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, emptyRow, Style.Default));
            }
        }
        
        // Bottom border
        if (_showBorder)
        {
            var bottomBorder = "└" + new string('─', (int)layout.Width - 2) + "┘";
            elements.Add(CreateTextElement((int)layout.AbsoluteX, currentY++, bottomBorder, borderStyle));
        }
        
        // Scroll indicator
        if (_sortedItems.Count > _visibleRows)
        {
            var scrollBarHeight = Math.Max(1, (_visibleRows * _visibleRows) / _sortedItems.Count);
            var scrollBarPos = (_scrollOffset * (_visibleRows - scrollBarHeight)) / (_sortedItems.Count - _visibleRows);
            
            var scrollX = (int)(layout.AbsoluteX + layout.Width) + 1;
            var scrollStartY = (int)layout.AbsoluteY + (_showHeader ? 2 : 0) + (_showBorder ? 1 : 0);
            
            for (int i = 0; i < _visibleRows; i++)
            {
                var isScrollBar = i >= scrollBarPos && i < scrollBarPos + scrollBarHeight;
                var scrollChar = isScrollBar ? "█" : "░";
                
                elements.Add(CreateTextElement(scrollX, scrollStartY + i, scrollChar, 
                    Style.Default.WithForegroundColor(Color.DarkGray)));
            }
        }
        
        return Fragment(elements.ToArray());
    }
    
    private string BuildTableRow(List<string> values)
    {
        var parts = new List<string>();
        for (int i = 0; i < Math.Min(values.Count, _columnWidths.Count); i++)
        {
            var value = values[i];
            var width = _columnWidths[i];
            
            if (value.Length > width)
            {
                value = value.Substring(0, width - 3) + "...";
            }
            else
            {
                value = value.PadRight(width);
            }
            
            parts.Add(value);
        }
        
        return string.Join(" | ", parts);
    }
    
    private VirtualNode CreateTextElement(int x, int y, string text, Style style)
    {
        return Element("text")
            .WithProp("x", x)
            .WithProp("y", y)
            .WithProp("style", style)
            .WithChild(new TextNode(text))
            .Build();
    }
    
    public override void Dispose()
    {
        _bindingSubscription?.Dispose();
        base.Dispose();
    }
    
    // Helper class for binding subscriptions
    private class BindingSubscription : IDisposable
    {
        private readonly Binding<Optional<T>> _binding;
        private readonly Action _callback;
        
        public BindingSubscription(Binding<Optional<T>> binding, Action callback)
        {
            _binding = binding;
            _callback = callback;
            _binding.PropertyChanged += OnPropertyChanged;
        }
        
        private void OnPropertyChanged(object? sender, EventArgs e)
        {
            _callback();
        }
        
        public void Dispose()
        {
            _binding.PropertyChanged -= OnPropertyChanged;
        }
    }
}