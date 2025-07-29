using System;
using System.Collections.Generic;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Display;

/// <summary>
/// A progress bar component that displays progress visually.
/// </summary>
public class ProgressBar : LayoutComponent
{
    private double _value = 0;
    private double _minimum = 0;
    private double _maximum = 100;
    private ProgressBarStyle _style = ProgressBarStyle.Blocks;
    private bool _showPercentage = true;
    private string _label = string.Empty;
    private Color _fillColor = Color.Green;
    private Color _backgroundColor = Color.DarkGray;
    
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    public double Value
    {
        get => _value;
        set
        {
            _value = Math.Max(_minimum, Math.Min(_maximum, value));
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            _value = Math.Max(_minimum, _value);
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            _value = Math.Min(_maximum, _value);
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the progress bar style.
    /// </summary>
    public ProgressBarStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets whether to show the percentage.
    /// </summary>
    public bool ShowPercentage
    {
        get => _showPercentage;
        set
        {
            _showPercentage = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the label text.
    /// </summary>
    public string Label
    {
        get => _label;
        set
        {
            _label = value ?? string.Empty;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the fill color.
    /// </summary>
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets the current percentage (0-100).
    /// </summary>
    public double Percentage
    {
        get
        {
            var range = _maximum - _minimum;
            if (range <= 0) return 0;
            return ((_value - _minimum) / range) * 100;
        }
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        var height = 1;
        if (!string.IsNullOrEmpty(_label))
            height++;
            
        var width = Math.Min(40, availableSize.Width); // Default width
        return new Size(width, height);
    }
    
    protected override void ArrangeCore(Rectangle finalRect)
    {
        // Base arranges the bounds
    }
    
    protected override VirtualNode OnRender()
    {
        var nodes = new List<VirtualNode>();
        var y = 0;
        
        // Render label if present
        if (!string.IsNullOrEmpty(_label))
        {
            nodes.Add(new ElementNode("text", new Dictionary<string, object?>
            {
                ["x"] = 0,
                ["y"] = y,
                ["style"] = Terminal.Style.Default
            }, new TextNode(_label)));
            y++;
        }
        
        // Calculate progress
        var percentage = Percentage / 100.0;
        var barWidth = Bounds.Width;
        
        // Reserve space for percentage if shown
        if (_showPercentage)
            barWidth = Math.Max(10, barWidth - 6); // " 100%" takes 5 chars + 1 space
            
        var filledWidth = (int)Math.Round(barWidth * percentage);
        
        // Build progress bar
        var bar = new System.Text.StringBuilder();
        
        switch (_style)
        {
            case ProgressBarStyle.Blocks:
                RenderBlockStyle(bar, barWidth, filledWidth);
                break;
                
            case ProgressBarStyle.Line:
                RenderLineStyle(bar, barWidth, filledWidth);
                break;
                
            case ProgressBarStyle.Dots:
                RenderDotStyle(bar, barWidth, filledWidth);
                break;
                
            case ProgressBarStyle.Gradient:
                RenderGradientStyle(bar, barWidth, filledWidth);
                break;
        }
        
        // Add percentage if shown
        if (_showPercentage)
        {
            bar.Append($" {Percentage,3:F0}%");
        }
        
        nodes.Add(new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = y,
            ["style"] = Terminal.Style.Default
        }, new TextNode(bar.ToString())));
        
        return new ElementNode("progressbar", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height
        }, nodes.ToArray());
    }
    
    private void RenderBlockStyle(System.Text.StringBuilder bar, int barWidth, int filledWidth)
    {
        for (int i = 0; i < barWidth; i++)
        {
            if (i < filledWidth)
            {
                bar.Append('█'); // Full block
            }
            else if (i == filledWidth && filledWidth < barWidth)
            {
                // Partial block based on fraction
                var fraction = (Percentage / 100.0 * barWidth) - filledWidth;
                if (fraction >= 0.875) bar.Append('█');
                else if (fraction >= 0.75) bar.Append('▇');
                else if (fraction >= 0.625) bar.Append('▆');
                else if (fraction >= 0.5) bar.Append('▅');
                else if (fraction >= 0.375) bar.Append('▄');
                else if (fraction >= 0.25) bar.Append('▃');
                else if (fraction >= 0.125) bar.Append('▂');
                else bar.Append('▁');
            }
            else
            {
                bar.Append('░'); // Light shade
            }
        }
    }
    
    private void RenderLineStyle(System.Text.StringBuilder bar, int barWidth, int filledWidth)
    {
        bar.Append('[');
        for (int i = 0; i < barWidth - 2; i++)
        {
            if (i < filledWidth - 1)
                bar.Append('=');
            else if (i == filledWidth - 1 && filledWidth > 0)
                bar.Append('>');
            else
                bar.Append(' ');
        }
        bar.Append(']');
    }
    
    private void RenderDotStyle(System.Text.StringBuilder bar, int barWidth, int filledWidth)
    {
        for (int i = 0; i < barWidth; i++)
        {
            if (i < filledWidth)
                bar.Append('●'); // Black circle
            else
                bar.Append('○'); // White circle
        }
    }
    
    private void RenderGradientStyle(System.Text.StringBuilder bar, int barWidth, int filledWidth)
    {
        bar.Append('▏'); // Left one eighth block
        
        for (int i = 1; i < barWidth - 1; i++)
        {
            if (i < filledWidth)
            {
                // Use different shades based on position
                if (i < barWidth * 0.33)
                    bar.Append('▓'); // Dark shade
                else if (i < barWidth * 0.66)
                    bar.Append('▒'); // Medium shade
                else
                    bar.Append('░'); // Light shade
            }
            else
            {
                bar.Append(' ');
            }
        }
        
        bar.Append('▕'); // Right one eighth block
    }
    
    /// <summary>
    /// Sets the progress as a percentage (0-100).
    /// </summary>
    public void SetPercentage(double percentage)
    {
        var range = _maximum - _minimum;
        Value = _minimum + (range * percentage / 100.0);
    }
    
    /// <summary>
    /// Increments the value by the specified amount.
    /// </summary>
    public void Increment(double amount = 1)
    {
        Value = _value + amount;
    }
    
    /// <summary>
    /// Gets whether the progress is complete.
    /// </summary>
    public bool IsComplete => Math.Abs(_value - _maximum) < 0.0001;
}

/// <summary>
/// Defines progress bar visual styles.
/// </summary>
public enum ProgressBarStyle
{
    /// <summary>
    /// Block characters with partial blocks.
    /// </summary>
    Blocks,
    
    /// <summary>
    /// Classic ASCII style with brackets and equals.
    /// </summary>
    Line,
    
    /// <summary>
    /// Dots/circles style.
    /// </summary>
    Dots,
    
    /// <summary>
    /// Gradient shading style.
    /// </summary>
    Gradient
}