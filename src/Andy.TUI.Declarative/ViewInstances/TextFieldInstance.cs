using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Diagnostics;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a TextField view with preserved state.
/// </summary>
public class TextFieldInstance : ViewInstance, IFocusable
{
    private string _placeholder = "";
    private Binding<string>? _textBinding;
    private bool _isSecure;
    private bool _isFocused;
    private int _cursorPosition;
    private IDisposable? _bindingSubscription;
    private readonly ILogger _logger;

    public TextFieldInstance(string id) : base(id)
    {
        _logger = LogManager.GetLogger($"TextField[{id}]");
        _logger.Debug($"TextFieldInstance created: ID={id}");
    }

    // IFocusable implementation
    public bool CanFocus => true;
    public bool IsFocused => _isFocused;

    public void OnGotFocus()
    {
        _isFocused = true;
        _cursorPosition = _textBinding?.Value?.Length ?? 0;
        _logger.Debug($"Got focus. CursorPosition={_cursorPosition}, Text='{_textBinding?.Value}'");
        InvalidateView();
    }

    public void OnLostFocus()
    {
        _isFocused = false;
        _logger.Debug($"Lost focus. Text='{_textBinding?.Value}'");
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (_textBinding == null)
        {
            _logger.Warning("HandleKeyPress called but no text binding");
            return false;
        }

        var currentText = _textBinding.Value ?? string.Empty;
        _logger.LogKeyInput(keyInfo, $"TextField[{Id}]");
        _logger.Debug($"Before: Text='{currentText}', CursorPos={_cursorPosition}");

        switch (keyInfo.Key)
        {
            case ConsoleKey.Backspace:
                if (_cursorPosition > 0)
                {
                    var newText = currentText.Remove(_cursorPosition - 1, 1);
                    _textBinding.Value = newText;
                    _cursorPosition--;
                    if (_cursorPosition < 0) _cursorPosition = 0;
                    _logger.Debug($"Backspace: Removed char at pos {_cursorPosition + 1}, NewText='{newText}'");
                }
                else
                {
                    _logger.Debug("Backspace: At beginning, no action");
                }
                return true;

            case ConsoleKey.Delete:
                if (_cursorPosition < currentText.Length)
                {
                    var newText = currentText.Remove(_cursorPosition, 1);
                    _textBinding.Value = newText;
                    _logger.Debug($"Delete: Removed char at pos {_cursorPosition}, NewText='{newText}'");
                }
                else
                {
                    _logger.Debug("Delete: At end, no action");
                }
                return true;

            case ConsoleKey.LeftArrow:
                if (_cursorPosition > 0)
                {
                    _cursorPosition--;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.RightArrow:
                if (_cursorPosition < currentText.Length)
                {
                    _cursorPosition++;
                    InvalidateView();
                }
                return true;

            case ConsoleKey.Home:
                _cursorPosition = 0;
                InvalidateView();
                return true;

            case ConsoleKey.End:
                _cursorPosition = currentText.Length;
                InvalidateView();
                return true;

            default:
                if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Clamp cursor within current text bounds to avoid exceptions
                    if (_cursorPosition < 0) _cursorPosition = 0;
                    _logger.Debug($"Inserting character '{keyInfo.KeyChar}' at position {_cursorPosition}");
                    if (_cursorPosition > currentText.Length) _cursorPosition = currentText.Length;
                    var newText = currentText.Insert(_cursorPosition, keyInfo.KeyChar.ToString());
                    // Debug logging (uncomment to debug text updates)
                    // Console.Error.WriteLine($"[TextFieldInstance] Setting text from '{currentText}' to '{newText}'");
                    _textBinding.Value = newText;
                    _cursorPosition++;
                    // Console.Error.WriteLine($"[TextFieldInstance] After setting, binding value is: '{_textBinding.Value}'");
                    InvalidateView();
                    return true;
                }
                break;
        }

        return false;
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not TextField textField)
            throw new ArgumentException("Expected TextField declaration");

        // Update properties from declaration
        _placeholder = textField.GetPlaceholder();
        _isSecure = textField.GetIsSecure();

        // Handle binding changes
        var newBinding = textField.GetBinding();
        if (newBinding != _textBinding)
        {
            // Unsubscribe from old binding
            _bindingSubscription?.Dispose();

            // Subscribe to new binding
            _textBinding = newBinding;
            if (_textBinding != null)
            {
                _bindingSubscription = new BindingSubscription(_textBinding, () => InvalidateView());
            }
        }
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();

        // TextField has fixed width with brackets
        const int fieldWidth = 20;
        layout.Width = constraints.ConstrainWidth(fieldWidth + 2); // +2 for brackets
        layout.Height = constraints.ConstrainHeight(1);

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var currentText = _textBinding?.Value ?? string.Empty;
        var displayText = string.IsNullOrEmpty(currentText) ? _placeholder :
                         _isSecure ? new string('•', currentText.Length) : currentText;

        const int fieldWidth = 20;

        // Show cursor when focused
        string fieldContent;
        if (_isFocused && _cursorPosition <= displayText.Length)
        {
            // Insert cursor at position
            if (_cursorPosition < displayText.Length)
                fieldContent = displayText.Insert(_cursorPosition, "│");
            else
                fieldContent = displayText + "│";

            // Ensure it fits in field
            if (fieldContent.Length > fieldWidth)
            {
                // Scroll to keep cursor visible
                var start = Math.Max(0, _cursorPosition - fieldWidth + 5);
                fieldContent = fieldContent.Substring(start, Math.Min(fieldWidth, fieldContent.Length - start));
            }
            else
            {
                fieldContent = fieldContent.PadRight(fieldWidth);
            }
        }
        else
        {
            // No cursor, just display text
            if (displayText.Length > fieldWidth)
                fieldContent = displayText.Substring(0, fieldWidth);
            else
                fieldContent = displayText.PadRight(fieldWidth);
        }

        // Style based on focus
        var style = _isFocused
            ? Style.Default.WithForegroundColor(Color.Red).WithBackgroundColor(Color.DarkBlue)
            : Style.Default.WithForegroundColor(Color.Gray);

        return Element("text")
            .WithProp("x", layout.AbsoluteX)
            .WithProp("y", layout.AbsoluteY)
            .WithProp("style", style)
            .WithChild(new TextNode($"[{fieldContent}]"))
            .Build();
    }

    public override void Dispose()
    {
        _bindingSubscription?.Dispose();
        base.Dispose();
    }

    // Helper class for binding subscriptions
    private class BindingSubscription : IDisposable
    {
        private readonly Binding<string> _binding;
        private readonly Action _callback;

        public BindingSubscription(Binding<string> binding, Action callback)
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