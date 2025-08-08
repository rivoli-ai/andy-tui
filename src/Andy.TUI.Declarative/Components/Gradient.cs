using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Gradient direction for color transitions.
/// </summary>
public enum GradientDirection
{
    Horizontal,
    Vertical,
    Diagonal,
    Radial
}

/// <summary>
/// A component that displays text with a color gradient effect.
/// </summary>
public class Gradient : ISimpleComponent
{
    private readonly string _text;
    private readonly Color _startColor;
    private readonly Color _endColor;
    private readonly GradientDirection _direction;
    private readonly bool _bold;
    private readonly bool _italic;
    private readonly bool _underline;
    
    public Gradient(
        string text,
        Color startColor,
        Color endColor,
        GradientDirection direction = GradientDirection.Horizontal,
        bool bold = false,
        bool italic = false,
        bool underline = false)
    {
        _text = text ?? "";
        _startColor = startColor;
        _endColor = endColor;
        _direction = direction;
        _bold = bold;
        _italic = italic;
        _underline = underline;
    }
    
    // Internal accessors for view instance
    internal string GetText() => _text;
    internal Color GetStartColor() => _startColor;
    internal Color GetEndColor() => _endColor;
    internal GradientDirection GetDirection() => _direction;
    internal bool GetBold() => _bold;
    internal bool GetItalic() => _italic;
    internal bool GetUnderline() => _underline;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Gradient declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    // Helper method to interpolate between colors
    public static Color InterpolateColor(Color start, Color end, float t)
    {
        t = Math.Max(0f, Math.Min(1f, t));
        
        // Simple RGB interpolation (not perfect for terminal colors but good enough)
        // Map terminal colors to approximate RGB values
        var startRgb = GetApproximateRgb(start);
        var endRgb = GetApproximateRgb(end);
        
        var r = (byte)(startRgb.r + (endRgb.r - startRgb.r) * t);
        var g = (byte)(startRgb.g + (endRgb.g - startRgb.g) * t);
        var b = (byte)(startRgb.b + (endRgb.b - startRgb.b) * t);
        
        // Map back to nearest terminal color
        return GetNearestTerminalColor(r, g, b);
    }
    
    private static (byte r, byte g, byte b) GetApproximateRgb(Color color)
    {
        // Use ConsoleColor if available
        if (color.ConsoleColor.HasValue)
        {
            return color.ConsoleColor.Value switch
            {
                System.ConsoleColor.Black => (0, 0, 0),
                System.ConsoleColor.DarkBlue => (0, 0, 128),
                System.ConsoleColor.DarkGreen => (0, 128, 0),
                System.ConsoleColor.DarkCyan => (0, 128, 128),
                System.ConsoleColor.DarkRed => (128, 0, 0),
                System.ConsoleColor.DarkMagenta => (128, 0, 128),
                System.ConsoleColor.DarkYellow => (128, 128, 0),
                System.ConsoleColor.Gray => (192, 192, 192),
                System.ConsoleColor.DarkGray => (128, 128, 128),
                System.ConsoleColor.Blue => (0, 0, 255),
                System.ConsoleColor.Green => (0, 255, 0),
                System.ConsoleColor.Cyan => (0, 255, 255),
                System.ConsoleColor.Red => (255, 0, 0),
                System.ConsoleColor.Magenta => (255, 0, 255),
                System.ConsoleColor.Yellow => (255, 255, 0),
                System.ConsoleColor.White => (255, 255, 255),
                _ => (128, 128, 128)
            };
        }
        
        // Use RGB if available
        if (color.Rgb.HasValue)
        {
            return color.Rgb.Value;
        }
        
        // Default to gray
        return (128, 128, 128);
    }
    
    private static Color GetNearestTerminalColor(byte r, byte g, byte b)
    {
        // Simple color distance calculation
        Color nearestColor = Color.Gray;
        int minDistance = int.MaxValue;
        
        // Check all standard console colors
        var colors = new[]
        {
            Color.Black, Color.DarkBlue, Color.DarkGreen, Color.DarkCyan,
            Color.DarkRed, Color.DarkMagenta, Color.DarkYellow, Color.Gray,
            Color.DarkGray, Color.Blue, Color.Green, Color.Cyan,
            Color.Red, Color.Magenta, Color.Yellow, Color.White
        };
        
        foreach (var color in colors)
        {
            var rgb = GetApproximateRgb(color);
            var distance = Math.Abs(r - rgb.r) + Math.Abs(g - rgb.g) + Math.Abs(b - rgb.b);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestColor = color;
            }
        }
        
        return nearestColor;
    }
}