using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for Slider component.
/// </summary>
public class SliderInstance : ViewInstance, IFocusable
{
    private Binding<float>? _valueBinding;
    private float _minValue = 0f;
    private float _maxValue = 100f;
    private float _step = 1f;
    private int _width = 20;
    private SliderOrientation _orientation = SliderOrientation.Horizontal;
    private string _label = "";
    private bool _showValue = true;
    private string _valueFormat = "F0";
    private char _trackChar = '─';
    private char _thumbChar = '█';
    private Color _trackColor = Color.DarkGray;
    private Color _thumbColor = Color.Cyan;
    
    public SliderInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Slider slider)
            throw new InvalidOperationException($"Expected Slider, got {viewDeclaration.GetType()}");
        
        _valueBinding = slider.GetValueBinding();
        _minValue = slider.GetMinValue();
        _maxValue = slider.GetMaxValue();
        _step = slider.GetStep();
        _width = slider.GetWidth();
        _orientation = slider.GetOrientation();
        _label = slider.GetLabel();
        _showValue = slider.GetShowValue();
        _valueFormat = slider.GetValueFormat();
        _trackChar = slider.GetTrackChar();
        _thumbChar = slider.GetThumbChar();
        _trackColor = slider.GetTrackColor();
        _thumbColor = slider.GetThumbColor();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var labelHeight = string.IsNullOrEmpty(_label) ? 0 : 1;
        
        if (_orientation == SliderOrientation.Horizontal)
        {
            var totalWidth = _width;
            if (_showValue)
            {
                // Add space for value display
                var maxValueLength = Math.Max(
                    _minValue.ToString(_valueFormat).Length,
                    _maxValue.ToString(_valueFormat).Length
                );
                totalWidth += maxValueLength + 2; // +2 for spacing
            }
            
            return new LayoutBox 
            { 
                Width = Math.Min(totalWidth, constraints.MaxWidth),
                Height = Math.Min(labelHeight + 1, constraints.MaxHeight)
            };
        }
        else
        {
            // Vertical slider
            var width = _showValue ? 10 : 3; // Wider if showing value
            return new LayoutBox 
            { 
                Width = Math.Min(width, constraints.MaxWidth),
                Height = Math.Min(labelHeight + _width, constraints.MaxHeight)
            };
        }
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var children = new System.Collections.Generic.List<VirtualNode>();
        var currentY = 0;
        
        // Render label if present
        if (!string.IsNullOrEmpty(_label))
        {
            children.Add(Element("text")
                .WithProp("style", Style.Default)
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode(_label))
                .Build());
            currentY++;
        }
        
        // Get current value
        var value = _valueBinding?.Value ?? _minValue;
        value = Math.Max(_minValue, Math.Min(_maxValue, value));
        
        // Calculate thumb position
        var range = _maxValue - _minValue;
        var normalizedValue = range > 0 ? (value - _minValue) / range : 0f;
        
        if (_orientation == SliderOrientation.Horizontal)
        {
            RenderHorizontalSlider(children, layout, currentY, normalizedValue, value);
        }
        else
        {
            RenderVerticalSlider(children, layout, currentY, normalizedValue, value);
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
    
    private void RenderHorizontalSlider(
        System.Collections.Generic.List<VirtualNode> children, 
        LayoutBox layout, 
        int currentY, 
        float normalizedValue,
        float value)
    {
        var thumbPosition = (int)Math.Round((_width - 1) * normalizedValue);
        
        // Render track
        for (int i = 0; i < _width; i++)
        {
            var isThumb = i == thumbPosition;
            var ch = isThumb ? _thumbChar : _trackChar;
            var color = isThumb ? 
                (IsFocused ? Color.White : _thumbColor) : 
                _trackColor;
            
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(color))
                .WithProp("x", (int)(layout.AbsoluteX + i))
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(ch.ToString()))
                .Build());
        }
        
        // Render value if enabled
        if (_showValue)
        {
            var valueText = value.ToString(_valueFormat);
            children.Add(Element("text")
                .WithProp("style", Style.Default)
                .WithProp("x", (int)(layout.AbsoluteX + _width + 1))
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(valueText))
                .Build());
        }
    }
    
    private void RenderVerticalSlider(
        System.Collections.Generic.List<VirtualNode> children, 
        LayoutBox layout, 
        int currentY, 
        float normalizedValue,
        float value)
    {
        var thumbPosition = (int)Math.Round((_width - 1) * (1f - normalizedValue)); // Inverted for vertical
        
        // Render track
        for (int i = 0; i < _width; i++)
        {
            var isThumb = i == thumbPosition;
            var ch = isThumb ? _thumbChar : '│';
            var color = isThumb ? 
                (IsFocused ? Color.White : _thumbColor) : 
                _trackColor;
            
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(color))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)(layout.AbsoluteY + currentY + i))
                .WithChild(new TextNode(ch.ToString()))
                .Build());
        }
        
        // Render value if enabled
        if (_showValue)
        {
            var valueText = value.ToString(_valueFormat);
            children.Add(Element("text")
                .WithProp("style", Style.Default)
                .WithProp("x", (int)(layout.AbsoluteX + 2))
                .WithProp("y", (int)(layout.AbsoluteY + currentY + thumbPosition))
                .WithChild(new TextNode(valueText))
                .Build());
        }
    }
    
    public bool IsFocused { get; private set; }
    public bool CanFocus => true;
    
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
        if (_valueBinding == null) return false;
        
        var value = _valueBinding.Value;
        var handled = false;
        
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
                if (_orientation == SliderOrientation.Horizontal)
                {
                    value = Math.Max(_minValue, value - _step);
                    handled = true;
                }
                break;
                
            case ConsoleKey.RightArrow:
                if (_orientation == SliderOrientation.Horizontal)
                {
                    value = Math.Min(_maxValue, value + _step);
                    handled = true;
                }
                break;
                
            case ConsoleKey.UpArrow:
                if (_orientation == SliderOrientation.Vertical)
                {
                    value = Math.Min(_maxValue, value + _step);
                    handled = true;
                }
                break;
                
            case ConsoleKey.DownArrow:
                if (_orientation == SliderOrientation.Vertical)
                {
                    value = Math.Max(_minValue, value - _step);
                    handled = true;
                }
                break;
                
            case ConsoleKey.Home:
                value = _minValue;
                handled = true;
                break;
                
            case ConsoleKey.End:
                value = _maxValue;
                handled = true;
                break;
                
            case ConsoleKey.PageUp:
                value = Math.Min(_maxValue, value + _step * 10);
                handled = true;
                break;
                
            case ConsoleKey.PageDown:
                value = Math.Max(_minValue, value - _step * 10);
                handled = true;
                break;
        }
        
        if (handled && value != _valueBinding.Value)
        {
            _valueBinding.Value = value;
            InvalidateView();
        }
        
        return handled;
    }
}