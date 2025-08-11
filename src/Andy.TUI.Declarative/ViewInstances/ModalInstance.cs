using System;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Modal view.
/// </summary>
public class ModalInstance : ViewInstance, IFocusable
{
    private string _title = "";
    private ISimpleComponent? _content;
    private Binding<bool>? _isOpenBinding;
    private ModalSize _size = ModalSize.Medium;
    private bool _showCloseButton = true;
    private bool _closeOnEscape = true;
    private bool _closeOnBackdropClick = true;
    private Color _backdropColor = Color.Black;
    private float _backdropOpacity = 0.5f;
    private bool _isFocused;
    private IDisposable? _bindingSubscription;
    private ViewInstance? _contentInstance;
    private readonly ViewInstanceManager _viewInstanceManager;

    public ModalInstance(string id, ViewInstanceManager viewInstanceManager) : base(id)
    {
        _viewInstanceManager = viewInstanceManager ?? throw new ArgumentNullException(nameof(viewInstanceManager));
    }

    /// <summary>
    /// Gets the content instance for focus traversal.
    /// </summary>
    public ViewInstance? GetContentInstance() => _contentInstance;

    // IFocusable implementation
    public bool CanFocus => _isOpenBinding?.Value ?? false;
    public bool IsFocused => _isFocused;

    public void OnGotFocus()
    {
        _isFocused = true;
        // Focus the content if it's focusable
        if (_contentInstance is IFocusable focusable && focusable.CanFocus)
        {
            focusable.OnGotFocus();
        }
        InvalidateView();
    }

    public void OnLostFocus()
    {
        _isFocused = false;
        if (_contentInstance is IFocusable focusable)
        {
            focusable.OnLostFocus();
        }
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        // Handle escape key to close
        if (_closeOnEscape && keyInfo.Key == ConsoleKey.Escape)
        {
            if (_isOpenBinding != null)
            {
                _isOpenBinding.Value = false;
            }
            return true;
        }

        // Forward key presses to content if it's focusable
        if (_contentInstance is IFocusable focusable && focusable.IsFocused)
        {
            return focusable.HandleKeyPress(keyInfo);
        }

        // Handle close button shortcut (Ctrl+W)
        if (_showCloseButton && keyInfo.Key == ConsoleKey.W &&
            keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            if (_isOpenBinding != null)
            {
                _isOpenBinding.Value = false;
            }
            return true;
        }

        return false;
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Modal modal)
            throw new ArgumentException("Expected Modal declaration");

        // Update properties from declaration
        _title = modal.GetTitle();
        _content = modal.GetContent();
        _size = modal.GetSize();
        _showCloseButton = modal.GetShowCloseButton();
        _closeOnEscape = modal.GetCloseOnEscape();
        _closeOnBackdropClick = modal.GetCloseOnBackdropClick();
        _backdropColor = modal.GetBackdropColor();
        _backdropOpacity = modal.GetBackdropOpacity();

        // Update content instance
        if (_content != null)
        {
            var contentId = $"{Id}_content";
            _contentInstance = _viewInstanceManager.GetOrCreateInstance(_content, contentId);
            _contentInstance.Update(_content);
        }

        // Handle binding changes
        var newBinding = modal.GetIsOpenBinding();
        if (newBinding != _isOpenBinding)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();

            // Subscribe to new binding
            _isOpenBinding = newBinding;
            if (_isOpenBinding != null)
            {
                _bindingSubscription = new BindingSubscription(_isOpenBinding, () => InvalidateView());
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Modal takes full screen space
        var layout = new LayoutBox
        {
            Width = constraints.MaxWidth,
            Height = constraints.MaxHeight
        };

        // Calculate modal content size based on size preset
        if (_isOpenBinding?.Value == true && _contentInstance != null)
        {
            var (widthPercent, heightPercent) = GetSizePercentages();
            var modalWidth = (int)(constraints.MaxWidth * widthPercent);
            var modalHeight = (int)(constraints.MaxHeight * heightPercent);

            // Account for border and title
            var contentWidth = modalWidth - 2; // -2 for borders
            var contentHeight = modalHeight - 4; // -4 for borders and title

            // Layout content
            var contentConstraints = LayoutConstraints.Tight(contentWidth, contentHeight);
            _contentInstance.CalculateLayout(contentConstraints);
        }

        return layout;
    }

    private (float width, float height) GetSizePercentages()
    {
        return _size switch
        {
            ModalSize.Small => (0.4f, 0.3f),
            ModalSize.Medium => (0.6f, 0.5f),
            ModalSize.Large => (0.8f, 0.7f),
            ModalSize.FullScreen => (0.95f, 0.9f),
            _ => (0.6f, 0.5f)
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        // If modal is not open, render nothing
        if (_isOpenBinding?.Value != true)
        {
            return Fragment();
        }

        var elements = new List<VirtualNode>();

        // Calculate modal dimensions and position
        var (widthPercent, heightPercent) = GetSizePercentages();
        var modalWidth = (int)(layout.Width * widthPercent);
        var modalHeight = (int)(layout.Height * heightPercent);
        var modalX = (int)layout.AbsoluteX + ((int)layout.Width - modalWidth) / 2;
        var modalY = (int)layout.AbsoluteY + ((int)layout.Height - modalHeight) / 2;

        // Render backdrop
        RenderBackdrop(elements, layout);

        // Render modal border
        var borderStyle = _isFocused
            ? Style.Default.WithForegroundColor(Color.White).WithBackgroundColor(Color.Black)
            : Style.Default.WithForegroundColor(Color.Gray).WithBackgroundColor(Color.Black);

        // Top border with title
        var topBorder = "╔" + new string('═', modalWidth - 2) + "╗";
        elements.Add(CreateTextElement(modalX, modalY, topBorder, borderStyle));

        // Title
        if (!string.IsNullOrEmpty(_title))
        {
            var titleText = _title;
            if (titleText.Length > modalWidth - 6)
            {
                titleText = titleText.Substring(0, modalWidth - 9) + "...";
            }

            elements.Add(CreateTextElement(modalX + 2, modalY, $" {titleText} ",
                Style.Default.WithBold(true).WithForegroundColor(Color.White).WithBackgroundColor(Color.Black)));
        }

        // Close button
        if (_showCloseButton)
        {
            var closeButton = "[X]";
            elements.Add(CreateTextElement(modalX + modalWidth - 4, modalY, closeButton,
                Style.Default.WithForegroundColor(Color.Red).WithBackgroundColor(Color.Black)));
        }

        // Content area with borders
        for (int i = 1; i < modalHeight - 1; i++)
        {
            // Left border
            elements.Add(CreateTextElement(modalX, modalY + i, "║", borderStyle));

            // Clear content area background
            var clearLine = new string(' ', modalWidth - 2);
            elements.Add(CreateTextElement(modalX + 1, modalY + i, clearLine,
                Style.Default.WithBackgroundColor(Color.Black)));

            // Right border
            elements.Add(CreateTextElement(modalX + modalWidth - 1, modalY + i, "║", borderStyle));
        }

        // Bottom border
        var bottomBorder = "╚" + new string('═', modalWidth - 2) + "╝";
        elements.Add(CreateTextElement(modalX, modalY + modalHeight - 1, bottomBorder, borderStyle));

        // Render content
        if (_contentInstance != null)
        {
            // Position content inside modal
            _contentInstance.Layout.AbsoluteX = modalX + 1;
            _contentInstance.Layout.AbsoluteY = modalY + 2; // +2 for border and title

            var contentNode = _contentInstance.Render();
            if (contentNode != null)
            {
                elements.Add(contentNode);
            }
        }

        // Help text at bottom
        if (_closeOnEscape)
        {
            var helpText = "Press ESC to close";
            elements.Add(CreateTextElement(modalX + modalWidth - helpText.Length - 2,
                modalY + modalHeight - 1, helpText,
                Style.Default.WithForegroundColor(Color.DarkGray).WithBackgroundColor(Color.Black)));
        }

        return Fragment(elements.ToArray());
    }

    private void RenderBackdrop(List<VirtualNode> elements, LayoutBox layout)
    {
        // Create semi-transparent backdrop effect with ASCII characters
        var backdropChar = '░'; // Light shade character
        var backdropStyle = Style.Default.WithForegroundColor(_backdropColor);

        for (int y = 0; y < layout.Height; y++)
        {
            var line = new string(backdropChar, (int)layout.Width);
            elements.Add(CreateTextElement((int)layout.AbsoluteX, (int)layout.AbsoluteY + y,
                line, backdropStyle));
        }
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
        _contentInstance?.Dispose();
        base.Dispose();
    }

    // Helper class for binding subscriptions
    private class BindingSubscription : IDisposable
    {
        private readonly Binding<bool> _binding;
        private readonly Action _callback;

        public BindingSubscription(Binding<bool> binding, Action callback)
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