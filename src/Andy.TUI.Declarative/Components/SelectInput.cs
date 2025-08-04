using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative select input component for choosing from a list of items with keyboard navigation.
/// </summary>
public class SelectInput<T> : ISimpleComponent
{
    private readonly IReadOnlyList<T> _items;
    private readonly Binding<Optional<T>> _selectedItem;
    private readonly Func<T, string> _itemRenderer;
    private readonly int _visibleItems;
    private readonly bool _showIndicator;
    private readonly string _placeholder;

    public SelectInput(
        IEnumerable<T> items, 
        Binding<Optional<T>> selectedItem,
        Func<T, string>? itemRenderer = null,
        int visibleItems = 5,
        string placeholder = "Select an item...",
        bool showIndicator = true)
    {
        _items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        _selectedItem = selectedItem ?? throw new ArgumentNullException(nameof(selectedItem));
        _itemRenderer = itemRenderer ?? (item => item?.ToString() ?? "");
        _visibleItems = Math.Max(1, visibleItems);
        _showIndicator = showIndicator;
        _placeholder = placeholder;
    }

    public SelectInput<T> VisibleItems(int count)
    {
        return new SelectInput<T>(_items, _selectedItem, _itemRenderer, count, _placeholder, _showIndicator);
    }

    public SelectInput<T> HideIndicator()
    {
        return new SelectInput<T>(_items, _selectedItem, _itemRenderer, _visibleItems, _placeholder, false);
    }

    public SelectInput<T> Placeholder(string placeholder)
    {
        return new SelectInput<T>(_items, _selectedItem, _itemRenderer, _visibleItems, placeholder, _showIndicator);
    }

    // Internal accessors for view instance
    internal IReadOnlyList<T> GetItems() => _items;
    internal Binding<Optional<T>> GetBinding() => _selectedItem;
    internal Func<T, string> GetItemRenderer() => _itemRenderer;
    internal int GetVisibleItems() => _visibleItems;
    internal bool GetShowIndicator() => _showIndicator;
    internal string GetPlaceholder() => _placeholder;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("SelectInput declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}