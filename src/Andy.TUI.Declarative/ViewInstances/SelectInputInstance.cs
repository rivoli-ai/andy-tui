using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a SelectInput view with keyboard navigation.
/// </summary>
public class SelectInputInstance<T> : ViewInstance, IFocusable
{
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private Binding<Optional<T>>? _selectedBinding;
    private Func<T, string> _itemRenderer = item => item?.ToString() ?? "";
    private int _visibleItems = 5;
    private bool _showIndicator = true;
    private string _placeholder = "Select an item...";
    private bool _isFocused;
    private int _highlightedIndex = 0;
    private int _scrollOffset = 0;
    private IDisposable? _bindingSubscription;
    private int? _cachedMaxItemWidth;
    private int _lastMeasuredFromIndex = -1;
    private int _lastMeasuredCount = 0;

    public SelectInputInstance(string id) : base(id)
    {
    }

    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;

    public void OnGotFocus()
    {
        _isFocused = true;

        // Set highlighted index to current selection if any
        if (_selectedBinding?.Value.TryGetValue(out var selectedValue) == true && _items.Count > 0)
        {
            var currentIndex = _items.ToList().IndexOf(selectedValue);
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
        if (_items.Count == 0) return false;

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
                if (_highlightedIndex < _items.Count - 1)
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
                _highlightedIndex = _items.Count - 1;
                UpdateScroll();
                InvalidateView();
                return true;

            case ConsoleKey.PageUp:
                _highlightedIndex = Math.Max(0, _highlightedIndex - _visibleItems);
                UpdateScroll();
                InvalidateView();
                return true;

            case ConsoleKey.PageDown:
                _highlightedIndex = Math.Min(_items.Count - 1, _highlightedIndex + _visibleItems);
                UpdateScroll();
                InvalidateView();
                return true;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                // Select the highlighted item
                if (_selectedBinding != null && _highlightedIndex < _items.Count)
                {
                    _selectedBinding.Value = Optional<T>.Some(_items[_highlightedIndex]);
                }
                return true;
        }

        return false;
    }

    private void UpdateScroll()
    {
        // Ensure highlighted item is visible
        if (_highlightedIndex < _scrollOffset)
        {
            _scrollOffset = _highlightedIndex;
        }
        else if (_highlightedIndex >= _scrollOffset + _visibleItems)
        {
            _scrollOffset = _highlightedIndex - _visibleItems + 1;
        }
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not SelectInput<T> selectInput)
            throw new ArgumentException($"Expected SelectInput<{typeof(T).Name}> declaration");

        // Update properties from declaration
        var prevCount = _items.Count;
        _items = selectInput.GetItems();
        _itemRenderer = selectInput.GetItemRenderer();
        _visibleItems = selectInput.GetVisibleItems();
        _showIndicator = selectInput.GetShowIndicator();
        _placeholder = selectInput.GetPlaceholder();
        if (_items.Count != prevCount)
        {
            _cachedMaxItemWidth = null;
            _lastMeasuredFromIndex = -1;
            _lastMeasuredCount = 0;
        }

        // Handle binding changes
        var newBinding = selectInput.GetBinding();
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
        if (_highlightedIndex >= _items.Count)
        {
            _highlightedIndex = Math.Max(0, _items.Count - 1);
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();

        // If not focused, only need single line height
        if (!_isFocused)
        {
            // Simple single-line layout
            var displayTextLength = _placeholder.Length;
            if (_selectedBinding?.Value.TryGetValue(out var selected) == true)
            {
                displayTextLength = _itemRenderer(selected).Length;
            }
            
            // Add space for brackets and padding: "[ text ]"
            var singleLineWidth = displayTextLength + 4;
            layout.Width = constraints.ConstrainWidth(singleLineWidth);
            layout.Height = constraints.ConstrainHeight(1);
            return layout;
        }

        // When focused, calculate full dropdown size
        // Calculate width based on longest item (virtualized sampling to avoid O(N) on large lists)
        var maxItemWidth = _placeholder.Length;
        if (_items.Count > 0)
        {
            if (_cachedMaxItemWidth.HasValue && _lastMeasuredFromIndex == _scrollOffset && _lastMeasuredCount == _visibleItems)
            {
                maxItemWidth = Math.Max(maxItemWidth, _cachedMaxItemWidth.Value);
            }
            else
            {
                var from = Math.Max(0, _scrollOffset - 2);
                var to = Math.Min(_items.Count, _scrollOffset + _visibleItems + 2);
                var windowWidth = 0;
                for (int i = from; i < to; i++)
                {
                    var len = _itemRenderer(_items[i]).Length;
                    if (len > windowWidth) windowWidth = len;
                }
                var headSample = Math.Min(_items.Count, 50);
                for (int i = 0; i < headSample; i++)
                {
                    var len = _itemRenderer(_items[i]).Length;
                    if (len > windowWidth) windowWidth = len;
                }
                _cachedMaxItemWidth = windowWidth;
                _lastMeasuredFromIndex = _scrollOffset;
                _lastMeasuredCount = _visibleItems;
                maxItemWidth = Math.Max(maxItemWidth, windowWidth);
            }
        }

        // Add space for selection indicator and border
        var width = maxItemWidth + (_showIndicator ? 4 : 2); // 2 for border, 2 for indicator

        layout.Width = constraints.ConstrainWidth(width);
        layout.Height = constraints.ConstrainHeight(_visibleItems + 2); // +2 for borders

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var elements = new List<VirtualNode>();

        // Determine style based on focus
        var borderStyle = _isFocused
            ? Style.Default.WithForegroundColor(Color.White)
            : Style.Default.WithForegroundColor(Color.DarkGray);

        T? selectedItem = default;
        var hasSelection = _selectedBinding?.Value.TryGetValue(out selectedItem) == true;
        var displayText = hasSelection ? _itemRenderer(selectedItem!) : _placeholder;

        // If not focused, only render a simple single-line element
        if (!_isFocused)
        {
            // Render as a simple bordered text field showing current selection or placeholder
            var text = displayText;
            var maxWidth = (int)layout.Width - 4; // Leave room for brackets and spacing
            if (text.Length > maxWidth)
            {
                text = text.Substring(0, maxWidth - 3) + "...";
            }
            
            var style = hasSelection
                ? Style.Default.WithForegroundColor(Color.White)
                : Style.Default.WithForegroundColor(Color.DarkGray);
            
            // Simple single-line rendering with brackets
            elements.Add(
                Element("text")
                    .WithProp("x", layout.AbsoluteX)
                    .WithProp("y", layout.AbsoluteY)
                    .WithProp("style", style)
                    .WithChild(new TextNode($"[ {text} ]"))
                    .Build()
            );
            
            return Fragment(elements.ToArray());
        }

        // When focused, render the full dropdown
        // Calculate inner width
        var innerWidth = (int)layout.Width - 2; // -2 for borders

        // Top border with current selection embedded
        var selectionText = displayText;
        var maxSelectionLength = Math.Max(0, innerWidth - 4); // Leave room for "─ " and " ─"
        if (selectionText.Length > maxSelectionLength && maxSelectionLength > 3)
        {
            selectionText = selectionText.Substring(0, maxSelectionLength - 3) + "...";
        }
        else if (selectionText.Length > maxSelectionLength)
        {
            selectionText = selectionText.Substring(0, maxSelectionLength);
        }

        // Build the top border with embedded selection text
        var remainingWidth = innerWidth - selectionText.Length - 2; // -2 for the spaces around text
        var leftPadding = remainingWidth / 2;
        var rightPadding = remainingWidth - leftPadding;
        
        // Render the complete top border as a single line
        var topBorderLine = "┌" + new string('─', leftPadding) + " " + selectionText + " " + new string('─', rightPadding) + "┐";
        
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                .WithProp("style", borderStyle)
                .WithChild(new TextNode(topBorderLine))
                .Build()
        );

        // Render visible items
        for (int i = 0; i < _visibleItems; i++)
        {
            var itemIndex = _scrollOffset + i;
            var lineContent = "";
            var isHighlighted = false;
            var isSelected = false;
            T? currentItem = default;

            if (itemIndex < _items.Count)
            {
                currentItem = _items[itemIndex];
                var itemText = _itemRenderer(currentItem);

                // Add selection indicator
                if (_showIndicator)
                {
                    var indicator = itemIndex == _highlightedIndex ? "▶ " : "  ";
                    itemText = indicator + itemText;
                }

                isHighlighted = _isFocused && itemIndex == _highlightedIndex;
                isSelected = hasSelection && EqualityComparer<T>.Default.Equals(currentItem, selectedItem);
                lineContent = itemText;
            }

            // Truncate to fit (leave room for borders)
            if (lineContent.Length > innerWidth)
            {
                lineContent = lineContent.Substring(0, innerWidth - 3) + "...";
            }
            
            // Pad content to exactly innerWidth for consistent rendering
            lineContent = lineContent.PadRight(innerWidth);

            // Render entire line as a single element to ensure consistent background
            // This prevents partial background rendering issues
            var fullLine = "│" + lineContent + "│";
            
            // Determine the style for this line
            if (isHighlighted)
            {
                // Render with highlight background - entire line including borders
                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", Style.Default
                            .WithForegroundColor(Color.Black)
                            .WithBackgroundColor(Color.White))
                        .WithChild(new TextNode(fullLine))
                        .Build()
                );
            }
            else if (isSelected)
            {
                // Selected but not highlighted - show content in green, borders normal
                elements.Add(
                    Fragment(
                        Element("text")
                            .WithProp("x", layout.AbsoluteX)
                            .WithProp("y", layout.AbsoluteY + i + 1)
                            .WithProp("style", borderStyle)
                            .WithChild(new TextNode("│"))
                            .Build(),
                        Element("text")
                            .WithProp("x", layout.AbsoluteX + 1)
                            .WithProp("y", layout.AbsoluteY + i + 1)
                            .WithProp("style", Style.Default.WithForegroundColor(Color.Green))
                            .WithChild(new TextNode(lineContent))
                            .Build(),
                        Element("text")
                            .WithProp("x", layout.AbsoluteX + innerWidth + 1)
                            .WithProp("y", layout.AbsoluteY + i + 1)
                            .WithProp("style", borderStyle)
                            .WithChild(new TextNode("│"))
                            .Build()
                    )
                );
            }
            else
            {
                // Normal line - render as single element for efficiency
                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", borderStyle)
                        .WithChild(new TextNode(fullLine))
                        .Build()
                );
            }
        }

        // Bottom border
        var bottomBorder = "└" + new string('─', innerWidth) + "┘";
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY + _visibleItems + 1)
                .WithProp("style", borderStyle)
                .WithChild(new TextNode(bottomBorder))
                .Build()
        );

        // Show scroll indicator if needed
        if (_items.Count > _visibleItems)
        {
            var scrollBarHeight = Math.Max(1, (_visibleItems * _visibleItems) / _items.Count);
            var scrollBarPos = (_scrollOffset * (_visibleItems - scrollBarHeight)) / (_items.Count - _visibleItems);

            for (int i = 0; i < _visibleItems; i++)
            {
                var isScrollBar = i >= scrollBarPos && i < scrollBarPos + scrollBarHeight;
                var scrollChar = isScrollBar ? "█" : "░";

                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + innerWidth + 2)
                        .WithProp("y", layout.AbsoluteY + i + 1)
                        .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                        .WithChild(new TextNode(scrollChar))
                        .Build()
                );
            }
        }

        return Fragment(elements.ToArray());
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