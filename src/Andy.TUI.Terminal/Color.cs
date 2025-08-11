namespace Andy.TUI.Terminal;

/// <summary>
/// Represents a color in the terminal.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>
    /// Gets the color type.
    /// </summary>
    public ColorType Type { get; }

    /// <summary>
    /// Gets the console color (for ColorType.ConsoleColor).
    /// </summary>
    public System.ConsoleColor? ConsoleColor { get; }

    /// <summary>
    /// Gets the RGB components (for ColorType.Rgb).
    /// </summary>
    public (byte R, byte G, byte B)? Rgb { get; }

    /// <summary>
    /// Gets the 8-bit color index (for ColorType.EightBit).
    /// </summary>
    public byte? ColorIndex { get; }

    /// <summary>
    /// Represents no color (default).
    /// </summary>
    public static Color None { get; } = new Color();

    /// <summary>
    /// Standard console colors.
    /// </summary>
    public static Color Black { get; } = new Color(System.ConsoleColor.Black);
    public static Color DarkBlue { get; } = new Color(System.ConsoleColor.DarkBlue);
    public static Color DarkGreen { get; } = new Color(System.ConsoleColor.DarkGreen);
    public static Color DarkCyan { get; } = new Color(System.ConsoleColor.DarkCyan);
    public static Color DarkRed { get; } = new Color(System.ConsoleColor.DarkRed);
    public static Color DarkMagenta { get; } = new Color(System.ConsoleColor.DarkMagenta);
    public static Color DarkYellow { get; } = new Color(System.ConsoleColor.DarkYellow);
    public static Color Gray { get; } = new Color(System.ConsoleColor.Gray);
    public static Color DarkGray { get; } = new Color(System.ConsoleColor.DarkGray);
    public static Color Blue { get; } = new Color(System.ConsoleColor.Blue);
    public static Color Green { get; } = new Color(System.ConsoleColor.Green);
    public static Color Cyan { get; } = new Color(System.ConsoleColor.Cyan);
    public static Color Red { get; } = new Color(System.ConsoleColor.Red);
    public static Color Magenta { get; } = new Color(System.ConsoleColor.Magenta);
    public static Color Yellow { get; } = new Color(System.ConsoleColor.Yellow);
    public static Color White { get; } = new Color(System.ConsoleColor.White);

    /// <summary>
    /// Bright console colors (using 8-bit color indices).
    /// </summary>
    public static Color BrightBlack { get; } = new Color((byte)8);
    public static Color BrightRed { get; } = new Color((byte)9);
    public static Color BrightGreen { get; } = new Color((byte)10);
    public static Color BrightYellow { get; } = new Color((byte)11);
    public static Color BrightBlue { get; } = new Color((byte)12);
    public static Color BrightMagenta { get; } = new Color((byte)13);
    public static Color BrightCyan { get; } = new Color((byte)14);
    public static Color BrightWhite { get; } = new Color((byte)15);

    public Color()
    {
        Type = ColorType.None;
    }

    public Color(System.ConsoleColor consoleColor)
    {
        Type = ColorType.ConsoleColor;
        ConsoleColor = consoleColor;
    }

    public Color(byte r, byte g, byte b)
    {
        Type = ColorType.Rgb;
        Rgb = (r, g, b);
    }

    public Color(byte colorIndex)
    {
        Type = ColorType.EightBit;
        ColorIndex = colorIndex;
    }

    /// <summary>
    /// Creates a color from RGB values.
    /// </summary>
    public static Color FromRgb(byte r, byte g, byte b) => new Color(r, g, b);

    /// <summary>
    /// Creates a color from a hex string.
    /// </summary>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
            throw new ArgumentNullException(nameof(hex));

        hex = hex.TrimStart('#');

        if (hex.Length != 6)
            throw new ArgumentException("Hex color must be 6 characters long", nameof(hex));

        var r = Convert.ToByte(hex.Substring(0, 2), 16);
        var g = Convert.ToByte(hex.Substring(2, 2), 16);
        var b = Convert.ToByte(hex.Substring(4, 2), 16);

        return new Color(r, g, b);
    }

    /// <summary>
    /// Creates a color from an 8-bit color index.
    /// </summary>
    public static Color FromEightBit(byte index) => new Color(index);

    public bool Equals(Color other)
    {
        if (Type != other.Type)
            return false;

        return Type switch
        {
            ColorType.None => true,
            ColorType.ConsoleColor => ConsoleColor == other.ConsoleColor,
            ColorType.Rgb => Rgb == other.Rgb,
            ColorType.EightBit => ColorIndex == other.ColorIndex,
            _ => false
        };
    }

    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    public override int GetHashCode()
    {
        return Type switch
        {
            ColorType.None => 0,
            ColorType.ConsoleColor => HashCode.Combine(Type, ConsoleColor),
            ColorType.Rgb => HashCode.Combine(Type, Rgb),
            ColorType.EightBit => HashCode.Combine(Type, ColorIndex),
            _ => 0
        };
    }

    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public override string ToString()
    {
        return Type switch
        {
            ColorType.None => "None",
            ColorType.ConsoleColor => ConsoleColor.ToString()!,
            ColorType.Rgb => $"RGB({Rgb!.Value.R}, {Rgb.Value.G}, {Rgb.Value.B})",
            ColorType.EightBit => $"8-bit({ColorIndex})",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Defines the type of color.
/// </summary>
public enum ColorType
{
    /// <summary>
    /// No color specified.
    /// </summary>
    None,

    /// <summary>
    /// Standard 16-color console color.
    /// </summary>
    ConsoleColor,

    /// <summary>
    /// 24-bit RGB color.
    /// </summary>
    Rgb,

    /// <summary>
    /// 8-bit indexed color (256 colors).
    /// </summary>
    EightBit
}