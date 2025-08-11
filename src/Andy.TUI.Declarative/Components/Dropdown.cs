using System;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A dropdown/select component with SwiftUI-like declarative syntax.
/// </summary>
public class Dropdown<T> : ISimpleComponent where T : class
{
    private readonly string _placeholder;
    private readonly IEnumerable<T> _items;
    private readonly Binding<T> _selection;
    private readonly Func<T, string>? _displayText;
    private Terminal.Color _textColor = Terminal.Color.White;
    private Terminal.Color _placeholderColor = Terminal.Color.Gray;
    
    /// <summary>
    /// Creates a new dropdown with a placeholder and selection binding.
    /// </summary>
    public Dropdown(string placeholder, IEnumerable<T> items, Binding<T> selection, Func<T, string>? displayText = null)
    {
        _placeholder = placeholder;
        _items = items;
        _selection = selection;
        _displayText = displayText;
    }
    
    /// <summary>
    /// Sets the text color.
    /// </summary>
    public Dropdown<T> Color(Terminal.Color color)
    {
        _textColor = color;
        return this;
    }
    
    /// <summary>
    /// Sets the placeholder color.
    /// </summary>
    public Dropdown<T> PlaceholderColor(Terminal.Color color)
    {
        _placeholderColor = color;
        return this;
    }
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Dropdown declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    // Internal accessors for ViewInstance
    internal string GetPlaceholder() => _placeholder;
    internal IEnumerable<T> GetItems() => _items;
    internal Binding<T> GetSelection() => _selection;
    internal Func<T, string>? GetDisplayText() => _displayText;
    internal Terminal.Color GetTextColor() => _textColor;
    internal Terminal.Color GetPlaceholderColor() => _placeholderColor;
}