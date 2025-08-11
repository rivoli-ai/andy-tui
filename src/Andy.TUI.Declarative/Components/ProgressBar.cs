using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Progress bar styles.
/// </summary>
public enum ProgressBarStyle
{
    Solid,      // ████████░░
    Line,       // ━━━━━━━━──
    Dots,       // ●●●●●●●●○○
    Blocks,     // ▓▓▓▓▓▓▓▓░░
    Arrows,     // →→→→→→→→──
    Custom
}

/// <summary>
/// A progress bar component with customizable styles.
/// </summary>
public class ProgressBar : ISimpleComponent
{
    private readonly float _value;
    private readonly float _minValue;
    private readonly float _maxValue;
    private readonly int _width;
    private readonly ProgressBarStyle _style;
    private readonly char _filledChar;
    private readonly char _emptyChar;
    private readonly Color _filledColor;
    private readonly Color _emptyColor;
    private readonly bool _showPercentage;
    private readonly string _label;

    public ProgressBar(
        float value,
        float minValue = 0f,
        float maxValue = 100f,
        int width = 20,
        ProgressBarStyle style = ProgressBarStyle.Solid,
        char filledChar = '█',
        char emptyChar = '░',
        Color? filledColor = null,
        Color? emptyColor = null,
        bool showPercentage = true,
        string label = "")
    {
        _value = value;
        _minValue = minValue;
        _maxValue = maxValue;
        _width = Math.Max(3, width);
        _style = style;
        _filledChar = filledChar;
        _emptyChar = emptyChar;
        _filledColor = filledColor ?? Color.Green;
        _emptyColor = emptyColor ?? Color.DarkGray;
        _showPercentage = showPercentage;
        _label = label ?? "";
    }

    // Internal accessors for view instance
    internal float GetValue() => _value;
    internal float GetMinValue() => _minValue;
    internal float GetMaxValue() => _maxValue;
    internal int GetWidth() => _width;
    internal ProgressBarStyle GetStyle() => _style;
    internal char GetFilledChar() => _filledChar;
    internal char GetEmptyChar() => _emptyChar;
    internal Color GetFilledColor() => _filledColor;
    internal Color GetEmptyColor() => _emptyColor;
    internal bool GetShowPercentage() => _showPercentage;
    internal string GetLabel() => _label;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("ProgressBar declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Helper method to get style characters
    public static (char filled, char empty) GetStyleChars(ProgressBarStyle style)
    {
        return style switch
        {
            ProgressBarStyle.Solid => ('█', '░'),
            ProgressBarStyle.Line => ('━', '─'),
            ProgressBarStyle.Dots => ('●', '○'),
            ProgressBarStyle.Blocks => ('▓', '░'),
            ProgressBarStyle.Arrows => ('→', '─'),
            ProgressBarStyle.Custom => (' ', ' '), // Use provided chars
            _ => ('█', '░')
        };
    }
}