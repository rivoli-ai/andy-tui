using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Column definition for a table.
/// </summary>
public class TableColumn<T>
{
    public string Header { get; }
    public Func<T, string> Renderer { get; }
    public int? Width { get; }
    public bool Sortable { get; }
    public Comparison<T>? Comparer { get; }
    
    public TableColumn(
        string header, 
        Func<T, string> renderer, 
        int? width = null,
        bool sortable = false,
        Comparison<T>? comparer = null)
    {
        Header = header;
        Renderer = renderer;
        Width = width;
        Sortable = sortable;
        Comparer = comparer;
    }
}

/// <summary>
/// A declarative table component with sorting and selection support.
/// </summary>
public class Table<T> : ISimpleComponent
{
    private readonly IReadOnlyList<T> _items;
    private readonly IReadOnlyList<TableColumn<T>> _columns;
    private readonly Binding<Optional<T>>? _selectedItem;
    private readonly int _visibleRows;
    private readonly bool _showHeader;
    private readonly bool _showBorder;
    private readonly bool _allowSelection;
    
    public Table(
        IEnumerable<T> items,
        IEnumerable<TableColumn<T>> columns,
        Binding<Optional<T>>? selectedItem = null,
        int visibleRows = 10,
        bool showHeader = true,
        bool showBorder = true,
        bool allowSelection = true)
    {
        _items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        _columns = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
        _selectedItem = selectedItem;
        _visibleRows = Math.Max(1, visibleRows);
        _showHeader = showHeader;
        _showBorder = showBorder;
        _allowSelection = allowSelection && selectedItem != null;
    }
    
    public Table<T> VisibleRows(int rows)
    {
        return new Table<T>(_items, _columns, _selectedItem, rows, _showHeader, _showBorder, _allowSelection);
    }
    
    public Table<T> HideHeader()
    {
        return new Table<T>(_items, _columns, _selectedItem, _visibleRows, false, _showBorder, _allowSelection);
    }
    
    public Table<T> HideBorder()
    {
        return new Table<T>(_items, _columns, _selectedItem, _visibleRows, _showHeader, false, _allowSelection);
    }
    
    public Table<T> DisableSelection()
    {
        return new Table<T>(_items, _columns, _selectedItem, _visibleRows, _showHeader, _showBorder, false);
    }
    
    // Internal accessors for view instance
    internal IReadOnlyList<T> GetItems() => _items;
    internal IReadOnlyList<TableColumn<T>> GetColumns() => _columns;
    internal Binding<Optional<T>>? GetSelectedBinding() => _selectedItem;
    internal int GetVisibleRows() => _visibleRows;
    internal bool GetShowHeader() => _showHeader;
    internal bool GetShowBorder() => _showBorder;
    internal bool GetAllowSelection() => _allowSelection;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Table declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}