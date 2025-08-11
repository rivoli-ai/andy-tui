using System;
using System.Collections.Generic;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Big text font styles.
/// </summary>
public enum BigTextFont
{
    Block,      // Simple block letters
    Slim,       // Slimmer variant
    Mini,       // Minimal 3x3 font
    Banner,     // Banner-style
    Custom      // User-provided font
}

/// <summary>
/// A component that displays text in large ASCII art style.
/// </summary>
public class BigText : ISimpleComponent
{
    private readonly string _text;
    private readonly BigTextFont _font;
    private readonly Dictionary<char, string[]>? _customFont;
    private readonly Color _color;
    private readonly char _fillChar;
    private readonly int _spacing;

    public BigText(
        string text,
        BigTextFont font = BigTextFont.Block,
        Dictionary<char, string[]>? customFont = null,
        Color? color = null,
        char fillChar = '█',
        int spacing = 1)
    {
        _text = text?.ToUpper() ?? "";
        _font = font;
        _customFont = customFont;
        _color = color ?? Color.White;
        _fillChar = fillChar;
        _spacing = Math.Max(0, spacing);
    }

    // Internal accessors for view instance
    internal string GetText() => _text;
    internal BigTextFont GetFont() => _font;
    internal Dictionary<char, string[]>? GetCustomFont() => _customFont;
    internal Color GetColor() => _color;
    internal char GetFillChar() => _fillChar;
    internal int GetSpacing() => _spacing;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("BigText declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Get font data for a character
    public static string[]? GetCharacterPattern(char c, BigTextFont font)
    {
        var fonts = GetFontData(font);
        return fonts.TryGetValue(char.ToUpper(c), out var pattern) ? pattern : null;
    }

    // Basic block font data
    private static Dictionary<char, string[]> GetFontData(BigTextFont font)
    {
        return font switch
        {
            BigTextFont.Block => BlockFont,
            BigTextFont.Slim => SlimFont,
            BigTextFont.Mini => MiniFont,
            _ => BlockFont
        };
    }

    private static readonly Dictionary<char, string[]> BlockFont = new()
    {
        ['A'] = new[] { " ███ ", "█   █", "█████", "█   █", "█   █" },
        ['B'] = new[] { "████ ", "█   █", "████ ", "█   █", "████ " },
        ['C'] = new[] { " ████", "█    ", "█    ", "█    ", " ████" },
        ['D'] = new[] { "████ ", "█   █", "█   █", "█   █", "████ " },
        ['E'] = new[] { "█████", "█    ", "████ ", "█    ", "█████" },
        ['F'] = new[] { "█████", "█    ", "████ ", "█    ", "█    " },
        ['G'] = new[] { " ████", "█    ", "█  ██", "█   █", " ████" },
        ['H'] = new[] { "█   █", "█   █", "█████", "█   █", "█   █" },
        ['I'] = new[] { "█████", "  █  ", "  █  ", "  █  ", "█████" },
        ['J'] = new[] { "█████", "    █", "    █", "█   █", " ███ " },
        ['K'] = new[] { "█   █", "█  █ ", "███  ", "█  █ ", "█   █" },
        ['L'] = new[] { "█    ", "█    ", "█    ", "█    ", "█████" },
        ['M'] = new[] { "█   █", "██ ██", "█ █ █", "█   █", "█   █" },
        ['N'] = new[] { "█   █", "██  █", "█ █ █", "█  ██", "█   █" },
        ['O'] = new[] { " ███ ", "█   █", "█   █", "█   █", " ███ " },
        ['P'] = new[] { "████ ", "█   █", "████ ", "█    ", "█    " },
        ['Q'] = new[] { " ███ ", "█   █", "█   █", "█  ██", " ████" },
        ['R'] = new[] { "████ ", "█   █", "████ ", "█  █ ", "█   █" },
        ['S'] = new[] { " ████", "█    ", " ███ ", "    █", "████ " },
        ['T'] = new[] { "█████", "  █  ", "  █  ", "  █  ", "  █  " },
        ['U'] = new[] { "█   █", "█   █", "█   █", "█   █", " ███ " },
        ['V'] = new[] { "█   █", "█   █", "█   █", " █ █ ", "  █  " },
        ['W'] = new[] { "█   █", "█   █", "█ █ █", "██ ██", "█   █" },
        ['X'] = new[] { "█   █", " █ █ ", "  █  ", " █ █ ", "█   █" },
        ['Y'] = new[] { "█   █", " █ █ ", "  █  ", "  █  ", "  █  " },
        ['Z'] = new[] { "█████", "   █ ", "  █  ", " █   ", "█████" },
        ['0'] = new[] { " ███ ", "█  ██", "█ █ █", "██  █", " ███ " },
        ['1'] = new[] { "  █  ", " ██  ", "  █  ", "  █  ", "█████" },
        ['2'] = new[] { " ███ ", "█   █", "   █ ", "  █  ", "█████" },
        ['3'] = new[] { " ███ ", "█   █", "  ██ ", "█   █", " ███ " },
        ['4'] = new[] { "█   █", "█   █", "█████", "    █", "    █" },
        ['5'] = new[] { "█████", "█    ", "████ ", "    █", "████ " },
        ['6'] = new[] { " ████", "█    ", "████ ", "█   █", " ███ " },
        ['7'] = new[] { "█████", "    █", "   █ ", "  █  ", " █   " },
        ['8'] = new[] { " ███ ", "█   █", " ███ ", "█   █", " ███ " },
        ['9'] = new[] { " ███ ", "█   █", " ████", "    █", "████ " },
        [' '] = new[] { "     ", "     ", "     ", "     ", "     " },
        ['!'] = new[] { "  █  ", "  █  ", "  █  ", "     ", "  █  " },
        ['?'] = new[] { " ███ ", "█   █", "   █ ", "     ", "  █  " },
        ['.'] = new[] { "     ", "     ", "     ", "     ", "  █  " },
        [','] = new[] { "     ", "     ", "     ", "  █  ", " █   " },
        ['-'] = new[] { "     ", "     ", "█████", "     ", "     " }
    };

    private static readonly Dictionary<char, string[]> SlimFont = new()
    {
        ['A'] = new[] { " ▄▄ ", "█  █", "████", "█  █" },
        ['B'] = new[] { "███ ", "█  █", "███ ", "████" },
        ['C'] = new[] { " ███", "█   ", "█   ", " ███" },
        ['D'] = new[] { "███ ", "█  █", "█  █", "███ " },
        ['E'] = new[] { "████", "█   ", "███ ", "████" },
        ['F'] = new[] { "████", "█   ", "███ ", "█   " },
        ['G'] = new[] { " ███", "█   ", "█ ██", " ███" },
        ['H'] = new[] { "█  █", "████", "█  █", "█  █" },
        ['I'] = new[] { "███", " █ ", " █ ", "███" },
        ['J'] = new[] { " ███", "   █", "█  █", " ██ " },
        ['K'] = new[] { "█  █", "█ █ ", "██  ", "█  █" },
        ['L'] = new[] { "█   ", "█   ", "█   ", "████" },
        ['M'] = new[] { "█  █", "████", "█  █", "█  █" },
        ['N'] = new[] { "█  █", "██ █", "█ ██", "█  █" },
        ['O'] = new[] { " ██ ", "█  █", "█  █", " ██ " },
        ['P'] = new[] { "███ ", "█  █", "███ ", "█   " },
        ['Q'] = new[] { " ██ ", "█  █", "█ ██", " ███" },
        ['R'] = new[] { "███ ", "█  █", "███ ", "█  █" },
        ['S'] = new[] { " ███", "█   ", " ██ ", "███ " },
        ['T'] = new[] { "███", " █ ", " █ ", " █ " },
        ['U'] = new[] { "█  █", "█  █", "█  █", " ██ " },
        ['V'] = new[] { "█  █", "█  █", " ██ ", " ▀▀ " },
        ['W'] = new[] { "█  █", "█  █", "████", "█  █" },
        ['X'] = new[] { "█  █", " ██ ", " ██ ", "█  █" },
        ['Y'] = new[] { "█  █", " ██ ", " █  ", " █  " },
        ['Z'] = new[] { "████", "  █ ", " █  ", "████" },
        ['0'] = new[] { " ██ ", "█ ██", "██ █", " ██ " },
        ['1'] = new[] { " █ ", "██ ", " █ ", "███" },
        ['2'] = new[] { "███ ", "   █", " ██ ", "████" },
        ['3'] = new[] { "███ ", "   █", " ██ ", "███ " },
        ['4'] = new[] { "█  █", "████", "   █", "   █" },
        ['5'] = new[] { "████", "███ ", "   █", "███ " },
        ['6'] = new[] { " ███", "█   ", "███ ", " ██ " },
        ['7'] = new[] { "████", "   █", "  █ ", " █  " },
        ['8'] = new[] { " ██ ", "█  █", " ██ ", " ██ " },
        ['9'] = new[] { " ██ ", "█  █", " ███", "   █" },
        [' '] = new[] { "    ", "    ", "    ", "    " }
    };

    private static readonly Dictionary<char, string[]> MiniFont = new()
    {
        ['A'] = new[] { "▄▄▄", "█▄█", "█ █" },
        ['B'] = new[] { "██▄", "█▄█", "██▀" },
        ['C'] = new[] { "▄██", "█  ", "▀██" },
        ['D'] = new[] { "██▄", "█ █", "██▀" },
        ['E'] = new[] { "███", "██ ", "███" },
        ['F'] = new[] { "███", "██ ", "█  " },
        ['G'] = new[] { "▄██", "█▄█", "▀██" },
        ['H'] = new[] { "█ █", "███", "█ █" },
        ['I'] = new[] { "███", " █ ", "███" },
        ['J'] = new[] { " ██", "  █", "██ " },
        ['K'] = new[] { "█▄█", "██ ", "█ █" },
        ['L'] = new[] { "█  ", "█  ", "███" },
        ['M'] = new[] { "█▄█", "███", "█ █" },
        ['N'] = new[] { "██▄", "█▄█", "█ █" },
        ['O'] = new[] { "▄█▄", "█ █", "▀█▀" },
        ['P'] = new[] { "██▄", "██▀", "█  " },
        ['Q'] = new[] { "▄█▄", "█ █", "▀██" },
        ['R'] = new[] { "██▄", "██ ", "█ █" },
        ['S'] = new[] { "▄██", "▀█▄", "██▀" },
        ['T'] = new[] { "███", " █ ", " █ " },
        ['U'] = new[] { "█ █", "█ █", "▀█▀" },
        ['V'] = new[] { "█ █", "█▄█", " █ " },
        ['W'] = new[] { "█ █", "███", "█▄█" },
        ['X'] = new[] { "█ █", " █ ", "█ █" },
        ['Y'] = new[] { "█ █", " █ ", " █ " },
        ['Z'] = new[] { "███", " █ ", "███" },
        ['0'] = new[] { "▄█▄", "█▄█", "▀█▀" },
        ['1'] = new[] { "▄█ ", " █ ", "███" },
        ['2'] = new[] { "██▄", " ▄█", "███" },
        ['3'] = new[] { "██▄", " ▄█", "██▀" },
        ['4'] = new[] { "█ █", "▀██", "  █" },
        ['5'] = new[] { "███", "██▄", "▀█▀" },
        ['6'] = new[] { "▄██", "██▄", "▀█▀" },
        ['7'] = new[] { "███", " ▄█", " █ " },
        ['8'] = new[] { "▄█▄", "▄█▄", "▀█▀" },
        ['9'] = new[] { "▄█▄", "▀██", " █ " },
        [' '] = new[] { "   ", "   ", "   " }
    };
}