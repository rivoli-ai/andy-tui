using System;

namespace Andy.TUI.Layout;

/// <summary>
/// Represents a length value that can be absolute (pixels) or relative (percentage).
/// </summary>
public readonly struct Length : IEquatable<Length>
{
    private readonly float _value;
    private readonly LengthUnit _unit;
    
    /// <summary>
    /// Gets the numeric value of the length.
    /// </summary>
    public float Value => _value;
    
    /// <summary>
    /// Gets the unit of the length.
    /// </summary>
    public LengthUnit Unit => _unit;
    
    /// <summary>
    /// Gets whether this length is auto (no specific value).
    /// </summary>
    public bool IsAuto => _unit == LengthUnit.Auto;
    
    /// <summary>
    /// Gets whether this length is a percentage.
    /// </summary>
    public bool IsPercentage => _unit == LengthUnit.Percentage;
    
    /// <summary>
    /// Gets whether this length is in pixels.
    /// </summary>
    public bool IsPixels => _unit == LengthUnit.Pixels;
    
    /// <summary>
    /// Represents an automatic length.
    /// </summary>
    public static Length Auto => new(0, LengthUnit.Auto);
    
    private Length(float value, LengthUnit unit)
    {
        _value = value;
        _unit = unit;
    }
    
    /// <summary>
    /// Creates a length in pixels.
    /// </summary>
    public static Length Pixels(float value) => new(value, LengthUnit.Pixels);
    
    /// <summary>
    /// Creates a length as a percentage.
    /// </summary>
    public static Length Percentage(float value) => new(value, LengthUnit.Percentage);
    
    /// <summary>
    /// Implicit conversion from int to pixel length.
    /// </summary>
    public static implicit operator Length(int pixels) => Pixels(pixels);
    
    /// <summary>
    /// Implicit conversion from float to pixel length.
    /// </summary>
    public static implicit operator Length(float pixels) => Pixels(pixels);
    
    /// <summary>
    /// Creates a percentage length from a string like "50%".
    /// </summary>
    public static Length Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("auto", StringComparison.OrdinalIgnoreCase))
            return Auto;
        
        if (value.EndsWith("%"))
        {
            var numericPart = value.Substring(0, value.Length - 1);
            if (float.TryParse(numericPart, out var percentage))
                return Percentage(percentage);
        }
        
        if (float.TryParse(value, out var pixels))
            return Pixels(pixels);
        
        throw new FormatException($"Invalid length value: {value}");
    }
    
    /// <summary>
    /// Calculates the actual pixel value given a container size.
    /// </summary>
    public float ToPixels(float containerSize)
    {
        return _unit switch
        {
            LengthUnit.Pixels => _value,
            LengthUnit.Percentage => containerSize * (_value / 100f),
            LengthUnit.Auto => 0, // Auto should be handled by layout algorithm
            _ => throw new InvalidOperationException($"Unknown length unit: {_unit}")
        };
    }
    
    public override string ToString()
    {
        return _unit switch
        {
            LengthUnit.Auto => "auto",
            LengthUnit.Pixels => $"{_value}px",
            LengthUnit.Percentage => $"{_value}%",
            _ => _value.ToString()
        };
    }
    
    public bool Equals(Length other) => _value.Equals(other._value) && _unit == other._unit;
    public override bool Equals(object? obj) => obj is Length other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(_value, _unit);
    
    public static bool operator ==(Length left, Length right) => left.Equals(right);
    public static bool operator !=(Length left, Length right) => !left.Equals(right);
}

/// <summary>
/// Units for length values.
/// </summary>
public enum LengthUnit
{
    /// <summary>
    /// Automatic sizing based on content.
    /// </summary>
    Auto,
    
    /// <summary>
    /// Absolute pixels.
    /// </summary>
    Pixels,
    
    /// <summary>
    /// Percentage of parent container.
    /// </summary>
    Percentage
}