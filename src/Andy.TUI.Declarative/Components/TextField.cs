using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A declarative text field component with SwiftUI-like syntax.
/// </summary>
public class TextField : ISimpleComponent
{
    private readonly string _placeholder;
    private readonly Binding<string> _text;
    private readonly bool _isSecure;

    public TextField(string placeholder, Binding<string> text, bool isSecure = false)
    {
        _placeholder = placeholder ?? string.Empty;
        _text = text ?? throw new ArgumentNullException(nameof(text));
        _isSecure = isSecure;
    }

    public TextField Secure()
    {
        return new TextField(_placeholder, _text, true);
    }
    
    // Internal accessors for view instance
    internal string GetPlaceholder() => _placeholder;
    internal Binding<string> GetBinding() => _text;
    internal bool GetIsSecure() => _isSecure;

    public VirtualNode Render()
    {
        // This should not be called directly anymore
        // The ViewInstance will handle rendering
        throw new InvalidOperationException("TextField declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}