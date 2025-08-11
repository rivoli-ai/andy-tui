using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for MultiSelectInput component.
/// </summary>
public class MultiSelectInputInstance<T> : ViewInstance, IFocusable
{
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private Binding<ISet<T>>? _selectedItemsBinding;
    private Func<T, string> _itemRenderer = _ => "";
    private string _checkedMark = "[×]";
    private string _uncheckedMark = "[ ]";
    private int _currentIndex = 0;
    private IDisposable? _bindingSubscription;

    public MultiSelectInputInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not MultiSelectInput<T> multiSelect)
            throw new InvalidOperationException($"Expected MultiSelectInput<{typeof(T).Name}>, got {viewDeclaration.GetType()}");

        // Check if items have changed
        var itemsChanged = !ReferenceEquals(_items, multiSelect.GetItems());

        _items = multiSelect.GetItems();
        _selectedItemsBinding = multiSelect.GetSelectedItemsBinding();
        _itemRenderer = multiSelect.GetItemRenderer();
        _checkedMark = multiSelect.GetCheckedMark();
        _uncheckedMark = multiSelect.GetUncheckedMark();

        // Subscribe to binding changes
        if (_selectedItemsBinding != null)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();

            // Subscribe to new binding
            _bindingSubscription = new BindingSubscription<ISet<T>>(_selectedItemsBinding, () => InvalidateView());
        }

        // If items changed, validate current index
        if (itemsChanged && _currentIndex >= _items.Count)
        {
            _currentIndex = Math.Max(0, _items.Count - 1);
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (_items.Count == 0)
        {
            return new LayoutBox { Width = constraints.MaxWidth, Height = 1 };
        }

        // Compute content width based on items and marks
        var markLength = Math.Max(_checkedMark.Length, _uncheckedMark.Length);
        var maxItemWidth = _items.Max(item => _itemRenderer(item).Length + markLength + 1);

        // Prefer constraint when finite/positive; otherwise fallback to content width
        var width = (!float.IsFinite(constraints.MaxWidth) || constraints.MaxWidth <= 0)
            ? maxItemWidth
            : Math.Min(maxItemWidth, constraints.MaxWidth);

        // Height constrained by items and constraints
        var height = (!float.IsFinite(constraints.MaxHeight) || constraints.MaxHeight <= 0)
            ? _items.Count
            : Math.Min(_items.Count, constraints.MaxHeight);

        return new LayoutBox { Width = width, Height = height };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_items.Count == 0)
        {
            return Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode("(No items)"))
                .Build();
        }

        var selectedItems = _selectedItemsBinding?.Value ?? new HashSet<T>();
        var children = new List<VirtualNode>();
        // Clear background region to avoid bleed
        var bgWidth = Math.Max(1, layout.ContentWidth);
        var bgHeight = Math.Max(1, layout.ContentHeight);
        children.Add(
            Element("rect")
                .WithProp("x", layout.ContentX)
                .WithProp("y", layout.ContentY)
                .WithProp("width", bgWidth)
                .WithProp("height", bgHeight)
                .Build()
        );
        var visibleCount = Math.Min(_items.Count, (int)layout.Height);

        // Calculate scroll offset to keep current item visible
        int scrollOffset = 0;
        if (_currentIndex >= visibleCount)
        {
            scrollOffset = _currentIndex - visibleCount + 1;
        }

        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = i + scrollOffset;
            if (itemIndex >= _items.Count) break;

            var item = _items[itemIndex];
            var isSelected = selectedItems.Contains(item);
            var isFocused = itemIndex == _currentIndex && IsFocused;

            var mark = isSelected ? _checkedMark : _uncheckedMark;
            var text = $"{mark} {_itemRenderer(item)}";
            // Pad or trim to exact width to prevent bleed/overlap (defensive for non-finite widths)
            var rawWidth = layout.ContentWidth;
            if (!float.IsFinite(rawWidth) || rawWidth <= 0)
            {
                rawWidth = _itemRenderer(item).Length + Math.Max(_checkedMark.Length, _uncheckedMark.Length) + 1;
            }
            var lineWidth = Math.Max(1, (int)rawWidth);
            var paddedText = text.Length >= lineWidth ? text.Substring(0, lineWidth) : text.PadRight(lineWidth);

            var textColor = isFocused ? Color.Black : Color.Gray;
            var bgColor = isFocused ? Color.Cyan : Color.Black;

            var style = Style.Default
                .WithForegroundColor(textColor)
                .WithBackgroundColor(bgColor);

            var textNode = Element("text")
                .WithProp("style", style)
                .WithProp("x", layout.ContentX)
                .WithProp("y", layout.ContentY + i)
                .WithChild(new TextNode(paddedText))
                .Build();

            children.Add(textNode);
        }

        // Add scroll indicators if needed (render after list so they overlay)
        if (scrollOffset > 0 && layout.ContentWidth > 0)
        {
            var upIndicator = Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", layout.ContentX + layout.ContentWidth - 1)
                .WithProp("y", layout.ContentY)
                .WithChild(new TextNode("↑"))
                .Build();
            children.Add(upIndicator);
        }

        if (scrollOffset + visibleCount < _items.Count && layout.ContentWidth > 0)
        {
            var downIndicator = Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", layout.ContentX + layout.ContentWidth - 1)
                .WithProp("y", layout.ContentY + visibleCount - 1)
                .WithChild(new TextNode("↓"))
                .Build();
            children.Add(downIndicator);
        }

        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }

    public bool IsFocused { get; private set; }
    public bool CanFocus => true;

    public void OnGotFocus()
    {
        IsFocused = true;
        InvalidateView();
    }

    public void OnLostFocus()
    {
        IsFocused = false;
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo key)
    {
        if (_items.Count == 0) return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.DownArrow:
                if (_currentIndex < _items.Count - 1)
                {
                    _currentIndex++;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.Home:
                _currentIndex = 0;
                InvalidateView();
                return true;

            case ConsoleKey.End:
                _currentIndex = _items.Count - 1;
                InvalidateView();
                return true;

            case ConsoleKey.PageUp:
                _currentIndex = Math.Max(0, _currentIndex - 5);
                InvalidateView();
                return true;

            case ConsoleKey.PageDown:
                _currentIndex = Math.Min(_items.Count - 1, _currentIndex + 5);
                InvalidateView();
                return true;

            case ConsoleKey.Spacebar:
            case ConsoleKey.Enter:
                if (_selectedItemsBinding != null && _currentIndex < _items.Count)
                {
                    var currentItem = _items[_currentIndex];
                    var selectedItems = new HashSet<T>(_selectedItemsBinding.Value ?? new HashSet<T>());

                    if (selectedItems.Contains(currentItem))
                    {
                        selectedItems.Remove(currentItem);
                    }
                    else
                    {
                        selectedItems.Add(currentItem);
                    }

                    _selectedItemsBinding.Value = selectedItems;
                    InvalidateView();
                }
                return true;

            default:
                return false;
        }
    }

    public override void Dispose()
    {
        _bindingSubscription?.Dispose();
        base.Dispose();
    }

    // Helper class for binding subscriptions
    private class BindingSubscription<TValue> : IDisposable
    {
        private readonly Binding<TValue> _binding;
        private readonly Action _callback;

        public BindingSubscription(Binding<TValue> binding, Action callback)
        {
            _binding = binding;
            _callback = callback;
            _binding.PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _callback();
        }

        public void Dispose()
        {
            _binding.PropertyChanged -= OnPropertyChanged;
        }
    }
}