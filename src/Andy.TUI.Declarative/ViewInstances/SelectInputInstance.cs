using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Core.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

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
        _items = selectInput.GetItems();
        _itemRenderer = selectInput.GetItemRenderer();
        _visibleItems = selectInput.GetVisibleItems();
        _showIndicator = selectInput.GetShowIndicator();
        _placeholder = selectInput.GetPlaceholder();
        
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
        
        // Calculate width based on longest item
        var maxItemWidth = _placeholder.Length;
        if (_items.Count > 0)
        {
            maxItemWidth = Math.Max(maxItemWidth, 
                _items.Max(item => _itemRenderer(item).Length));
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
        
        // Calculate inner width
        var innerWidth = (int)layout.Width - 2; // -2 for borders
        
        // Top border with current selection
        var topBorder = "┌" + new string('─', innerWidth) + "┐";
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                .WithProp("style", borderStyle)
                .WithChild(new TextNode(topBorder))
                .Build()
        );
        
        // Show current selection on top border
        var selectionText = displayText;
        if (selectionText.Length > innerWidth - 4)
        {
            selectionText = selectionText.Substring(0, innerWidth - 7) + "...";
        }
        
        var selectionStyle = hasSelection
            ? Style.Default.WithForegroundColor(Color.White)
            : Style.Default.WithForegroundColor(Color.DarkGray);
            
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX + 2)
                .WithProp("y", layout.AbsoluteY)
                .WithProp("style", selectionStyle)
                .WithChild(new TextNode($" {selectionText} "))
                .Build()
        );
        
        // Render visible items
        for (int i = 0; i < _visibleItems; i++)
        {
            var itemIndex = _scrollOffset + i;
            var lineContent = "";
            var lineStyle = Style.Default;
            
            if (itemIndex < _items.Count)
            {
                var item = _items[itemIndex];
                var itemText = _itemRenderer(item);
                
                // Add selection indicator
                if (_showIndicator)
                {
                    var indicator = itemIndex == _highlightedIndex ? "▶ " : "  ";
                    itemText = indicator + itemText;
                }
                
                // Highlight current item
                if (_isFocused && itemIndex == _highlightedIndex)
                {
                    lineStyle = Style.Default
                        .WithForegroundColor(Color.Black)
                        .WithBackgroundColor(Color.White);
                }
                else if (hasSelection && EqualityComparer<T>.Default.Equals(item, selectedItem))
                {
                    lineStyle = Style.Default.WithForegroundColor(Color.Green);
                }
                
                lineContent = itemText;
            }
            
            // Truncate or pad to fit
            if (lineContent.Length > innerWidth)
            {
                lineContent = lineContent.Substring(0, innerWidth - 3) + "...";
            }
            else
            {
                lineContent = lineContent.PadRight(innerWidth);
            }
            
            // Render line with borders
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
                        .WithProp("style", lineStyle)
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