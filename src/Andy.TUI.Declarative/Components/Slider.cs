using System;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Slider orientation.
/// </summary>
public enum SliderOrientation
{
    Horizontal,
    Vertical
}

/// <summary>
/// A slider component for numeric input.
/// </summary>
public class Slider : ISimpleComponent
{
    private readonly Binding<float> _value;
    private readonly float _minValue;
    private readonly float _maxValue;
    private readonly float _step;
    private readonly int _width;
    private readonly SliderOrientation _orientation;
    private readonly string _label;
    private readonly bool _showValue;
    private readonly string _valueFormat;
    private readonly char _trackChar;
    private readonly char _thumbChar;
    private readonly Color _trackColor;
    private readonly Color _thumbColor;
    
    public Slider(
        Binding<float> value,
        float minValue = 0f,
        float maxValue = 100f,
        float step = 1f,
        int width = 20,
        SliderOrientation orientation = SliderOrientation.Horizontal,
        string label = "",
        bool showValue = true,
        string valueFormat = "F0",
        char trackChar = '─',
        char thumbChar = '█',
        Color? trackColor = null,
        Color? thumbColor = null)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _minValue = minValue;
        _maxValue = maxValue;
        _step = Math.Max(0.001f, step);
        _width = Math.Max(5, width);
        _orientation = orientation;
        _label = label ?? "";
        _showValue = showValue;
        _valueFormat = valueFormat ?? "F0";
        _trackChar = trackChar;
        _thumbChar = thumbChar;
        _trackColor = trackColor ?? Color.DarkGray;
        _thumbColor = thumbColor ?? Color.Cyan;
    }
    
    // Internal accessors for view instance
    internal Binding<float> GetValueBinding() => _value;
    internal float GetMinValue() => _minValue;
    internal float GetMaxValue() => _maxValue;
    internal float GetStep() => _step;
    internal int GetWidth() => _width;
    internal SliderOrientation GetOrientation() => _orientation;
    internal string GetLabel() => _label;
    internal bool GetShowValue() => _showValue;
    internal string GetValueFormat() => _valueFormat;
    internal char GetTrackChar() => _trackChar;
    internal char GetThumbChar() => _thumbChar;
    internal Color GetTrackColor() => _trackColor;
    internal Color GetThumbColor() => _thumbColor;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Slider declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}