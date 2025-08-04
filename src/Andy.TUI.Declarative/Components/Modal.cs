using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Modal size presets.
/// </summary>
public enum ModalSize
{
    Small,   // 40% width, 30% height
    Medium,  // 60% width, 50% height
    Large,   // 80% width, 70% height
    FullScreen // 95% width, 90% height
}

/// <summary>
/// A declarative modal/dialog component that overlays content.
/// </summary>
public class Modal : ISimpleComponent
{
    private readonly string _title;
    private readonly ISimpleComponent _content;
    private readonly Binding<bool> _isOpen;
    private readonly ModalSize _size;
    private readonly bool _showCloseButton;
    private readonly bool _closeOnEscape;
    private readonly bool _closeOnBackdropClick;
    private readonly Color _backdropColor;
    private readonly float _backdropOpacity;
    
    public Modal(
        string title,
        ISimpleComponent content,
        Binding<bool> isOpen,
        ModalSize size = ModalSize.Medium,
        bool showCloseButton = true,
        bool closeOnEscape = true,
        bool closeOnBackdropClick = true,
        Color? backdropColor = null,
        float backdropOpacity = 0.5f)
    {
        _title = title ?? "";
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _isOpen = isOpen ?? throw new ArgumentNullException(nameof(isOpen));
        _size = size;
        _showCloseButton = showCloseButton;
        _closeOnEscape = closeOnEscape;
        _closeOnBackdropClick = closeOnBackdropClick;
        _backdropColor = backdropColor ?? Color.Black;
        _backdropOpacity = Math.Clamp(backdropOpacity, 0f, 1f);
    }
    
    public Modal Size(ModalSize size)
    {
        return new Modal(_title, _content, _isOpen, size, _showCloseButton, 
            _closeOnEscape, _closeOnBackdropClick, _backdropColor, _backdropOpacity);
    }
    
    public Modal HideCloseButton()
    {
        return new Modal(_title, _content, _isOpen, _size, false, 
            _closeOnEscape, _closeOnBackdropClick, _backdropColor, _backdropOpacity);
    }
    
    public Modal DisableEscapeClose()
    {
        return new Modal(_title, _content, _isOpen, _size, _showCloseButton, 
            false, _closeOnBackdropClick, _backdropColor, _backdropOpacity);
    }
    
    public Modal DisableBackdropClick()
    {
        return new Modal(_title, _content, _isOpen, _size, _showCloseButton, 
            _closeOnEscape, false, _backdropColor, _backdropOpacity);
    }
    
    public Modal BackdropColor(Color color, float opacity = 0.5f)
    {
        return new Modal(_title, _content, _isOpen, _size, _showCloseButton, 
            _closeOnEscape, _closeOnBackdropClick, color, opacity);
    }
    
    // Internal accessors for view instance
    internal string GetTitle() => _title;
    internal ISimpleComponent GetContent() => _content;
    internal Binding<bool> GetIsOpenBinding() => _isOpen;
    internal ModalSize GetSize() => _size;
    internal bool GetShowCloseButton() => _showCloseButton;
    internal bool GetCloseOnEscape() => _closeOnEscape;
    internal bool GetCloseOnBackdropClick() => _closeOnBackdropClick;
    internal Color GetBackdropColor() => _backdropColor;
    internal float GetBackdropOpacity() => _backdropOpacity;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Modal declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}

/// <summary>
/// Helper component for creating common dialog patterns.
/// </summary>
public static class Dialog
{
    public static Modal Alert(
        string title, 
        string message, 
        Binding<bool> isOpen,
        string buttonText = "OK")
    {
        var content = new VStack(spacing: 2) 
        {
            new Text(message),
            new HStack 
            { 
                new Spacer(),
                new Button(buttonText, () => isOpen.Value = false).Primary()
            }
        };
        
        return new Modal(title, content, isOpen, ModalSize.Small)
            .DisableBackdropClick();
    }
    
    public static Modal Confirm(
        string title,
        string message,
        Binding<bool> isOpen,
        Action onConfirm,
        string confirmText = "Yes",
        string cancelText = "No")
    {
        var content = new VStack(spacing: 2) 
        {
            new Text(message),
            new HStack(spacing: 2) 
            { 
                new Spacer(),
                new Button(cancelText, () => isOpen.Value = false).Secondary(),
                new Button(confirmText, () => 
                {
                    onConfirm();
                    isOpen.Value = false;
                }).Primary()
            }
        };
        
        return new Modal(title, content, isOpen, ModalSize.Small)
            .DisableBackdropClick();
    }
    
    public static Modal Prompt(
        string title,
        string message,
        Binding<bool> isOpen,
        Binding<string> inputValue,
        Action<string> onSubmit,
        string placeholder = "",
        string submitText = "Submit",
        string cancelText = "Cancel")
    {
        var content = new VStack(spacing: 2) 
        {
            new Text(message),
            new TextField(placeholder, inputValue),
            new HStack(spacing: 2) 
            { 
                new Spacer(),
                new Button(cancelText, () => isOpen.Value = false).Secondary(),
                new Button(submitText, () => 
                {
                    onSubmit(inputValue.Value);
                    isOpen.Value = false;
                }).Primary()
            }
        };
        
        return new Modal(title, content, isOpen, ModalSize.Small)
            .DisableBackdropClick();
    }
}