using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Theming;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Dropdown view with state management.
/// </summary>
public class DropdownInstance<T> : ViewInstance, IFocusable where T : class
{
    private string _placeholder = "";
    private List<T> _items = new();
    private Binding<T>? _selection;
    private Func<T, string>? _displayText;
    private Color _textColor;
    private Color _placeholderColor;

    private bool _isFocused;
    private bool _isOpen;
    private int _highlightedIndex = -1;
    private int _lastMenuHeight = 0;
    private int _lastMenuWidth = 0;
    private int _lastTriggerWidth = 0;
    private int _renderVersion = 0;
    private IDisposable? _bindingSubscription;

    public DropdownInstance(string id) : base(id)
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        _textColor = theme.Default.Foreground;
        _placeholderColor = theme.Disabled.Foreground;
    }

    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;

    public void OnGotFocus()
    {
        _isFocused = true;
        InvalidateView();
    }

    public void OnLostFocus()
    {
        _isFocused = false;
        _isOpen = false;
        _highlightedIndex = -1;
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (!_isFocused) return false;

        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                if (!_isOpen)
                {
                    // Open the dropdown and set highlight to selected item if any, otherwise first
                    _isOpen = true;
                    if (_selection?.Value != null)
                    {
                        var selIndex = _items.FindIndex(i => EqualityComparer<T>.Default.Equals(i, _selection.Value));
                        _highlightedIndex = selIndex >= 0 ? selIndex : (_items.Count > 0 ? 0 : -1);
                    }
                    else
                    {
                        _highlightedIndex = _items.Count > 0 ? 0 : -1;
                    }
                    _renderVersion++;
                }
                else if (_highlightedIndex >= 0 && _highlightedIndex < _items.Count)
                {
                    // Select the highlighted item and close
                    if (_selection != null)
                    {
                        _selection.Value = _items[_highlightedIndex];
                    }
                    _isOpen = false;
                    _highlightedIndex = -1;
                    _renderVersion++;
                }
                InvalidateView();
                return true;

            case ConsoleKey.Escape:
                if (_isOpen)
                {
                    _isOpen = false;
                    _highlightedIndex = -1;
                    _renderVersion++;
                    InvalidateView();
                    return true;
                }
                break;

            case ConsoleKey.DownArrow:
                if (_isOpen && _items.Count > 0)
                {
                    _highlightedIndex = Math.Min(_highlightedIndex + 1, _items.Count - 1);
                    _renderVersion++;
                    InvalidateView();
                    return true;
                }
                else if (!_isOpen)
                {
                    _isOpen = true;
                    _highlightedIndex = 0;
                    _renderVersion++;
                    InvalidateView();
                    return true;
                }
                break;

            case ConsoleKey.UpArrow:
                if (_isOpen && _items.Count > 0)
                {
                    _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
                    _renderVersion++;
                    InvalidateView();
                    return true;
                }
                break;
        }

        return false;
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Dropdown<T> dropdown)
            throw new ArgumentException("Expected Dropdown<T> declaration");

        // Update properties from declaration
        _placeholder = dropdown.GetPlaceholder();
        _items = dropdown.GetItems().ToList();
        _displayText = dropdown.GetDisplayText();
        _textColor = dropdown.GetTextColor();
        _placeholderColor = dropdown.GetPlaceholderColor();

        // Handle binding changes
        var newBinding = dropdown.GetSelection();
        if (newBinding != _selection)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();

            // Subscribe to new binding
            _selection = newBinding;
            if (_selection != null)
            {
                _bindingSubscription = new BindingSubscription(_selection, () => InvalidateView());
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();

        // Calculate dropdown width based on content
        var currentValue = _selection?.Value;
        bool hasValue = currentValue != null && (currentValue is not string str || !string.IsNullOrEmpty(str));
        var displayText = hasValue
            ? (_displayText?.Invoke(currentValue!) ?? currentValue!.ToString() ?? "")
            : _placeholder;

        var dropdownText = $"▶ {displayText}"; // Use closed state for width calculation
        layout.Width = constraints.ConstrainWidth(dropdownText.Length);

        // Keep intrinsic height to 1; the open menu is rendered as an overlay, not affecting layout flow
        layout.Height = constraints.ConstrainHeight(1);

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var currentValue = _selection?.Value;
        string displayText;

        // Check if we have a value - for strings, also check if not empty
        bool hasValue = currentValue != null && (currentValue is not string str || !string.IsNullOrEmpty(str));

        // When open, show the highlighted item as the current display to reflect navigation
        if (_isOpen && _highlightedIndex >= 0 && _highlightedIndex < _items.Count)
        {
            var highlighted = _items[_highlightedIndex];
            displayText = _displayText?.Invoke(highlighted) ?? highlighted?.ToString() ?? _placeholder;
        }
        else if (hasValue)
        {
            displayText = _displayText?.Invoke(currentValue!) ?? currentValue!.ToString() ?? "";
        }
        else
        {
            displayText = _placeholder;
        }

        var elements = new List<VirtualNode>();

        // Main dropdown element
        var theme = ThemeManager.Instance.CurrentTheme;
        var dropdownStyle = _isFocused
            ? Style.Default.WithForegroundColor(theme.Primary.Foreground).WithBackgroundColor(theme.Primary.Background)
            : Style.Default.WithForegroundColor(hasValue ? _textColor : _placeholderColor).WithBackgroundColor(theme.Default.Background);

        var dropdownText = _isOpen ? $"▼ {displayText}" : $"▶ {displayText}";

        // Clear previous trigger area to avoid stale characters when text shrinks
        var triggerClearWidth = Math.Max(_lastTriggerWidth, dropdownText.Length);
        if (triggerClearWidth > 0)
        {
            elements.Add(
                Element("rect")
                    .WithProp("x", layout.AbsoluteX)
                    .WithProp("y", layout.AbsoluteY)
                    .WithProp("width", triggerClearWidth)
                    .WithProp("height", 1)
                    .WithProp("z-index", 5) // ensure it renders before the trigger text
                    .WithProp("version", _renderVersion)
                    .WithProp("fill", theme.Default.Background)
                    .Build()
            );
        }

        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                // Ensure the trigger renders above surrounding text
                .WithProp("z-index", _isOpen ? 90 : 10)
                // attach a version so diff sees a change and repaints the whole trigger node when navigating
                .WithProp("version", _renderVersion)
                .WithProp("style", dropdownStyle)
                .WithChild(new TextNode(dropdownText))
                .Build()
        );
        _lastTriggerWidth = dropdownText.Length;

        // Dropdown items (when open)
        if (_isOpen && _items.Count > 0)
        {
            // Compute a consistent menu width wide enough to cover previous content
            int maxItemTextLen = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemTextForMeasure = _displayText?.Invoke(item) ?? item?.ToString() ?? "";
                if (itemTextForMeasure.Length > maxItemTextLen)
                    maxItemTextLen = itemTextForMeasure.Length;
            }
            var menuWidth = (int)Math.Max(Math.Round(layout.Width), maxItemTextLen + 4); // two-space left padding + some slack
            _lastMenuHeight = _items.Count;
            _lastMenuWidth = menuWidth;

            // Clear the entire dropdown menu area before drawing items (overlay area independent of layout height)
            // Clear area for menu behind items
            elements.Add(
                Element("rect")
                    .WithProp("x", layout.AbsoluteX)
                    .WithProp("y", layout.AbsoluteY + 1)
                    .WithProp("width", menuWidth)
                    .WithProp("height", _items.Count)
                    .WithProp("z-index", 90)
                    .WithProp("version", _renderVersion)
                    .WithProp("fill", theme.Default.Background)
                    .Build()
            );

            int y = 1;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemText = _displayText?.Invoke(item) ?? item?.ToString() ?? "";
                var isHighlighted = i == _highlightedIndex;
                var isSelected = currentValue != null && EqualityComparer<T>.Default.Equals(item, currentValue);

                var itemStyle = isHighlighted
                    ? Style.Default.WithForegroundColor(theme.Primary.Foreground).WithBackgroundColor(theme.Primary.Background)
                    : isSelected
                        ? Style.Default.WithForegroundColor(theme.Primary.AccentColor ?? theme.Primary.Foreground)
                        : Style.Default.WithForegroundColor(theme.Default.Foreground).WithBackgroundColor(theme.Default.Background);

                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + 2)
                        .WithProp("y", layout.AbsoluteY + y)
                        // Render items above most content to avoid occlusion
                        .WithProp("z-index", 95)
                        .WithProp("style", itemStyle)
                        .WithChild(new TextNode($"  {itemText}"))
                        .Build()
                );
                y++;
            }
        }
        else
        {
            // If we just closed the menu, clear the area where it was rendered last time
            if (_lastMenuHeight > 0)
            {
                var clearWidth = Math.Max(_lastMenuWidth, layout.Width);
                elements.Add(
                    Element("rect")
                        .WithProp("x", layout.AbsoluteX)
                        .WithProp("y", layout.AbsoluteY + 1)
                        .WithProp("width", clearWidth)
                        .WithProp("height", _lastMenuHeight)
                        .WithProp("z-index", -100) // very low, ensure it renders first
                        .WithProp("version", _renderVersion)
                        .WithProp("fill", theme.Default.Background)
                        .Build()
                );
                // Clear a generous action row area beneath the menu to erase overwritten UI
                elements.Add(
                    Element("rect")
                        .WithProp("x", layout.AbsoluteX - 20)
                        .WithProp("y", layout.AbsoluteY + _lastMenuHeight + 1)
                        .WithProp("width", clearWidth + 40)
                        .WithProp("height", 1)
                        .WithProp("z-index", -100)
                        .WithProp("version", _renderVersion)
                        .WithProp("fill", theme.Default.Background)
                        .Build()
                );
                // Reset after emitting clear rect
                _lastMenuHeight = 0;
                _lastMenuWidth = 0;
                _renderVersion++;
            }
        }

        // Re-emit trigger last to guarantee it paints above any clears
        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                .WithProp("z-index", _isOpen ? 96 : 10)
                .WithProp("version", _renderVersion)
                .WithProp("style", dropdownStyle)
                .WithChild(new TextNode(dropdownText))
                .Build()
        );
        _lastTriggerWidth = Math.Max(_lastTriggerWidth, dropdownText.Length);

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
        private readonly INotifyPropertyChanged _binding;
        private readonly Action _callback;

        public BindingSubscription(INotifyPropertyChanged binding, Action callback)
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