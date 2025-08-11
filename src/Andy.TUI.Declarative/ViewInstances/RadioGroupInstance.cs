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
/// Runtime instance for RadioGroup component.
/// </summary>
public class RadioGroupInstance<T> : ViewInstance, IFocusable
{
    private string _label = "";
    private IReadOnlyList<T> _options = Array.Empty<T>();
    private Binding<Optional<T>>? _selectedOptionBinding;
    private Func<T, string> _optionRenderer = _ => "";
    private string _selectedMark = "(•)";
    private string _unselectedMark = "( )";
    private bool _vertical = true;
    private int _currentIndex = 0;
    private IDisposable? _bindingSubscription;
    private Optional<T> _lastBindingValue = Optional<T>.None;

    public RadioGroupInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not RadioGroup<T> radioGroup)
            throw new InvalidOperationException($"Expected RadioGroup<{typeof(T).Name}>, got {viewDeclaration.GetType()}");

        _label = radioGroup.GetLabel();
        _options = radioGroup.GetOptions();
        _selectedOptionBinding = radioGroup.GetSelectedOptionBinding();
        _optionRenderer = radioGroup.GetOptionRenderer();
        _selectedMark = radioGroup.GetSelectedMark();
        _unselectedMark = radioGroup.GetUnselectedMark();
        _vertical = radioGroup.GetVertical();

        // Subscribe to binding changes
        if (_selectedOptionBinding != null)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();

            // Subscribe to new binding
            _bindingSubscription = new BindingSubscription<Optional<T>>(_selectedOptionBinding, () => InvalidateView());
        }

        // Find current selection index - only update if binding value changed
        var currentBindingValue = _selectedOptionBinding?.Value ?? Optional<T>.None;
        if (!EqualityComparer<Optional<T>>.Default.Equals(_lastBindingValue, currentBindingValue))
        {
            _lastBindingValue = currentBindingValue;

            if (currentBindingValue.HasValue)
            {
                var selectedValue = currentBindingValue.Value;
                for (int i = 0; i < _options.Count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(_options[i], selectedValue))
                    {
                        _currentIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // Reset to first option if no selection
                _currentIndex = 0;
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (_options.Count == 0)
        {
            return new LayoutBox { Width = constraints.MaxWidth, Height = 1 };
        }

        var labelHeight = string.IsNullOrEmpty(_label) ? 0 : 1;
        var markLength = Math.Max(_selectedMark.Length, _unselectedMark.Length);

        if (_vertical)
        {
            // Vertical layout
            var maxOptionWidth = _options.Max(opt =>
                _optionRenderer(opt).Length + markLength + 1);
            var labelWidth = _label.Length;

            var width = Math.Min(Math.Max(maxOptionWidth, labelWidth), constraints.MaxWidth);
            var height = Math.Min(labelHeight + _options.Count, constraints.MaxHeight);

            return new LayoutBox { Width = width, Height = height };
        }
        else
        {
            // Horizontal layout
            var totalWidth = _options.Sum(opt =>
                _optionRenderer(opt).Length + markLength + 2); // +2 for spacing

            var width = Math.Min(totalWidth, constraints.MaxWidth);
            var height = Math.Min(labelHeight + 1, constraints.MaxHeight);

            return new LayoutBox { Width = width, Height = height };
        }
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var children = new List<VirtualNode>();
        var currentY = 0;

        // Render label if present
        if (!string.IsNullOrEmpty(_label))
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithBold(true))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode(_label))
                .Build());
            currentY++;
        }

        if (_options.Count == 0)
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode("(No options)"))
                .Build());
        }
        else
        {
            var selectedOption = _selectedOptionBinding?.Value;
            var currentX = 0;

            for (int i = 0; i < _options.Count; i++)
            {
                var option = _options[i];
                var isSelected = false;
                if (selectedOption != null && selectedOption.Value.HasValue)
                {
                    T selectedValue = selectedOption.Value.Value;
                    var eq = EqualityComparer<T>.Default;
                    isSelected = eq.Equals(option, selectedValue);
                }
                var isFocused = i == _currentIndex && IsFocused;

                var mark = isSelected ? _selectedMark : _unselectedMark;
                var text = $"{mark} {_optionRenderer(option)}";

                var style = Style.Default;
                if (isFocused)
                {
                    style = style.WithForegroundColor(Color.Black).WithBackgroundColor(Color.Cyan);
                }
                else if (isSelected)
                {
                    style = style.WithForegroundColor(Color.Green);
                }

                if (_vertical)
                {
                    children.Add(Element("text")
                        .WithProp("style", style)
                        .WithProp("x", (int)layout.AbsoluteX)
                        .WithProp("y", (int)(layout.AbsoluteY + currentY + i))
                        .WithChild(new TextNode(text.PadRight((int)layout.Width)))
                        .Build());
                }
                else
                {
                    children.Add(Element("text")
                        .WithProp("style", style)
                        .WithProp("x", (int)(layout.AbsoluteX + currentX))
                        .WithProp("y", (int)(layout.AbsoluteY + currentY))
                        .WithChild(new TextNode(text))
                        .Build());
                    currentX += text.Length + 2; // Add spacing
                }
            }
        }

        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }

    public bool IsFocused { get; private set; }
    public bool CanFocus => _options.Count > 0;

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
        if (_options.Count == 0) return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_vertical && _currentIndex > 0)
                {
                    _currentIndex--;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.DownArrow:
                if (_vertical && _currentIndex < _options.Count - 1)
                {
                    _currentIndex++;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.LeftArrow:
                if (!_vertical && _currentIndex > 0)
                {
                    _currentIndex--;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.RightArrow:
                if (!_vertical && _currentIndex < _options.Count - 1)
                {
                    _currentIndex++;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.Spacebar:
            case ConsoleKey.Enter:
                if (_selectedOptionBinding != null && _currentIndex < _options.Count)
                {
                    _selectedOptionBinding.Value = Optional<T>.Some(_options[_currentIndex]);
                    InvalidateView();
                }
                return true;

            case ConsoleKey.Home:
                _currentIndex = 0;
                InvalidateView();
                return true;

            case ConsoleKey.End:
                _currentIndex = _options.Count - 1;
                InvalidateView();
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