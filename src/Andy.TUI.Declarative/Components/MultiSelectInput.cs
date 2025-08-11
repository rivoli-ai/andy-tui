using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A component that allows selecting multiple items from a list with checkboxes.
/// </summary>
public class MultiSelectInput<T> : ISimpleComponent
{
    private readonly IReadOnlyList<T> _items;
    private readonly Binding<ISet<T>> _selectedItems;
    private readonly Func<T, string> _itemRenderer;
    private readonly string _checkedMark;
    private readonly string _uncheckedMark;
    
    public MultiSelectInput(
        IReadOnlyList<T> items,
        Binding<ISet<T>> selectedItems,
        Func<T, string>? itemRenderer = null,
        string checkedMark = "[×]",
        string uncheckedMark = "[ ]")
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _selectedItems = selectedItems ?? throw new ArgumentNullException(nameof(selectedItems));
        _itemRenderer = itemRenderer ?? (item => item?.ToString() ?? "");
        _checkedMark = checkedMark;
        _uncheckedMark = uncheckedMark;
    }
    
    // Internal accessors for view instance
    internal IReadOnlyList<T> GetItems() => _items;
    internal Binding<ISet<T>> GetSelectedItemsBinding() => _selectedItems;
    internal Func<T, string> GetItemRenderer() => _itemRenderer;
    internal string GetCheckedMark() => _checkedMark;
    internal string GetUncheckedMark() => _uncheckedMark;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("MultiSelectInput declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}