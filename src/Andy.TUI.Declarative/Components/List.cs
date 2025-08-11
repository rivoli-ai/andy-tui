using System;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// List marker styles.
/// </summary>
public enum ListMarkerStyle
{
    Bullet,    // •
    Dash,      // -
    Arrow,     // →
    Star,      // *
    Number,    // 1. 2. 3.
    Letter,    // a. b. c.
    Roman,     // i. ii. iii.
    Square,    // ■
    Circle,    // ○
    Diamond,   // ◆
    Custom
}

/// <summary>
/// A list component with customizable markers.
/// </summary>
public class List : ISimpleComponent
{
    private readonly IReadOnlyList<ISimpleComponent> _items;
    private readonly ListMarkerStyle _markerStyle;
    private readonly string _customMarker;
    private readonly Color _markerColor;
    private readonly int _indent;
    private readonly int _spacing;
    
    public List(
        IReadOnlyList<ISimpleComponent> items,
        ListMarkerStyle markerStyle = ListMarkerStyle.Bullet,
        string customMarker = "",
        Color? markerColor = null,
        int indent = 2,
        int spacing = 0)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _markerStyle = markerStyle;
        _customMarker = customMarker;
        _markerColor = markerColor ?? Color.Gray;
        _indent = Math.Max(0, indent);
        _spacing = Math.Max(0, spacing);
    }
    
    // Constructor with collection initializer support
    public List(ListMarkerStyle markerStyle = ListMarkerStyle.Bullet) : this(Array.Empty<ISimpleComponent>(), markerStyle)
    {
    }
    
    // Collection initializer support
    public void Add(ISimpleComponent item) => throw new NotSupportedException("Use constructor with items parameter");
    
    // Internal accessors for view instance
    internal IReadOnlyList<ISimpleComponent> GetItems() => _items;
    internal ListMarkerStyle GetMarkerStyle() => _markerStyle;
    internal string GetCustomMarker() => _customMarker;
    internal Color GetMarkerColor() => _markerColor;
    internal int GetIndent() => _indent;
    internal int GetSpacing() => _spacing;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("List declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    // Helper method to get marker for a given index
    public static string GetMarker(ListMarkerStyle style, int index, string customMarker = "")
    {
        return style switch
        {
            ListMarkerStyle.Bullet => "•",
            ListMarkerStyle.Dash => "-",
            ListMarkerStyle.Arrow => "→",
            ListMarkerStyle.Star => "*",
            ListMarkerStyle.Number => $"{index + 1}.",
            ListMarkerStyle.Letter => $"{(char)('a' + (index % 26))}.",
            ListMarkerStyle.Roman => ToRoman(index + 1).ToLower() + ".",
            ListMarkerStyle.Square => "■",
            ListMarkerStyle.Circle => "○",
            ListMarkerStyle.Diamond => "◆",
            ListMarkerStyle.Custom => customMarker,
            _ => "•"
        };
    }
    
    private static string ToRoman(int number)
    {
        if (number < 1) return "";
        if (number >= 4000) return number.ToString();
        
        var values = new[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        var numerals = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        
        var result = "";
        for (int i = 0; i < values.Length; i++)
        {
            while (number >= values[i])
            {
                number -= values[i];
                result += numerals[i];
            }
        }
        return result;
    }
}