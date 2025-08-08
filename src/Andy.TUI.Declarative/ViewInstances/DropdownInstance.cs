using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

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
    private Color _textColor = Color.White;
    private Color _placeholderColor = Color.Gray;

    private bool _isFocused;
    private bool _isOpen;
    private int _highlightedIndex = -1;
    private IDisposable? _bindingSubscription;

    public DropdownInstance(string id) : base(id)
    {
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
                    _isOpen = true;
                    _highlightedIndex = _items.Count > 0 ? 0 : -1;
                }
                else if (_highlightedIndex >= 0 && _highlightedIndex < _items.Count)
                {
                    // Select the highlighted item
                    if (_selection != null)
                    {
                        _selection.Value = _items[_highlightedIndex];
                    }
                    _isOpen = false;
                    _highlightedIndex = -1;
                }
                InvalidateView();
                return true;

            case ConsoleKey.Escape:
                if (_isOpen)
                {
                    _isOpen = false;
                    _highlightedIndex = -1;
                    InvalidateView();
                    return true;
                }
                break;

            case ConsoleKey.DownArrow:
                if (_isOpen && _items.Count > 0)
                {
                    _highlightedIndex = Math.Min(_highlightedIndex + 1, _items.Count - 1);
                    InvalidateView();
                    return true;
                }
                else if (!_isOpen)
                {
                    _isOpen = true;
                    _highlightedIndex = 0;
                    InvalidateView();
                    return true;
                }
                break;

            case ConsoleKey.UpArrow:
                if (_isOpen && _items.Count > 0)
                {
                    _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
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

        // Height depends on whether dropdown is open
        if (_isOpen && _items.Count > 0)
        {
            layout.Height = constraints.ConstrainHeight(1 + _items.Count);
        }
        else
        {
            layout.Height = constraints.ConstrainHeight(1);
        }

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var currentValue = _selection?.Value;
        string displayText;

        // Check if we have a value - for strings, also check if not empty
        bool hasValue = currentValue != null && (currentValue is not string str || !string.IsNullOrEmpty(str));

        if (hasValue)
        {
            displayText = _displayText?.Invoke(currentValue!) ?? currentValue!.ToString() ?? "";
        }
        else
        {
            displayText = _placeholder;
        }

        var elements = new List<VirtualNode>();

        // Main dropdown element
        var dropdownStyle = _isFocused
            ? Style.Default.WithForegroundColor(Color.White).WithBackgroundColor(Color.DarkBlue)
            : Style.Default.WithForegroundColor(hasValue ? _textColor : _placeholderColor);

        var dropdownText = _isOpen ? $"▼ {displayText}" : $"▶ {displayText}";

        elements.Add(
            Element("text")
                .WithProp("x", layout.AbsoluteX)
                .WithProp("y", layout.AbsoluteY)
                // Ensure the trigger renders above surrounding text
                .WithProp("z-index", _isOpen ? 90 : 10)
                .WithProp("style", dropdownStyle)
                .WithChild(new TextNode(dropdownText))
                .Build()
        );

        // Dropdown items (when open)
        if (_isOpen && _items.Count > 0)
        {
            int y = 1;
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemText = _displayText?.Invoke(item) ?? item?.ToString() ?? "";
                var isHighlighted = i == _highlightedIndex;
                var isSelected = currentValue != null && EqualityComparer<T>.Default.Equals(item, currentValue);

                var itemStyle = isHighlighted
                    ? Style.Default.WithForegroundColor(Color.Black).WithBackgroundColor(Color.White)
                    : isSelected
                        ? Style.Default.WithForegroundColor(Color.Green)
                        : Style.Default.WithForegroundColor(Color.Gray);

                elements.Add(
                    Element("text")
                        .WithProp("x", layout.AbsoluteX + 2)
                        .WithProp("y", layout.AbsoluteY + y)
                        // Render items above most content to avoid occlusion
                        .WithProp("z-index", 100)
                        .WithProp("style", itemStyle)
                        .WithChild(new TextNode($"  {itemText}"))
                        .Build()
                );
                y++;
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