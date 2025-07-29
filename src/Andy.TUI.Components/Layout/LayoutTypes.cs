namespace Andy.TUI.Components.Layout;

/// <summary>
/// Represents spacing values for layout components.
/// </summary>
public record Spacing(int Top, int Right, int Bottom, int Left)
{
    /// <summary>
    /// Creates uniform spacing on all sides.
    /// </summary>
    public Spacing(int all) : this(all, all, all, all) { }
    
    /// <summary>
    /// Creates vertical and horizontal spacing.
    /// </summary>
    public Spacing(int vertical, int horizontal) : this(vertical, horizontal, vertical, horizontal) { }
    
    /// <summary>
    /// No spacing.
    /// </summary>
    public static Spacing None => new(0);
    
    /// <summary>
    /// Gets the total horizontal spacing.
    /// </summary>
    public int Horizontal => Left + Right;
    
    /// <summary>
    /// Gets the total vertical spacing.
    /// </summary>
    public int Vertical => Top + Bottom;
}

/// <summary>
/// Represents border styles for components.
/// </summary>
public enum BorderStyle
{
    None,
    Single,
    Double,
    Rounded,
    Heavy,
    Dashed,
    Custom
}

/// <summary>
/// Represents border configuration.
/// </summary>
public record Border(
    BorderStyle Style = BorderStyle.Single,
    bool Top = true,
    bool Right = true,
    bool Bottom = true,
    bool Left = true)
{
    /// <summary>
    /// No border.
    /// </summary>
    public static Border None => new(BorderStyle.None, false, false, false, false);
    
    /// <summary>
    /// Single border on all sides.
    /// </summary>
    public static Border Single => new(BorderStyle.Single);
    
    /// <summary>
    /// Double border on all sides.
    /// </summary>
    public static Border Double => new(BorderStyle.Double);
    
    /// <summary>
    /// Rounded border on all sides.
    /// </summary>
    public static Border Rounded => new(BorderStyle.Rounded);
}

/// <summary>
/// Represents alignment options for content.
/// </summary>
public enum Alignment
{
    Start,
    Center,
    End,
    Stretch
}

/// <summary>
/// Represents orientation for layout components.
/// </summary>
public enum Orientation
{
    Horizontal,
    Vertical
}

/// <summary>
/// Represents size constraints for layout.
/// </summary>
public record Size(int Width, int Height)
{
    /// <summary>
    /// Unlimited size.
    /// </summary>
    public static Size Unlimited => new(int.MaxValue, int.MaxValue);
    
    /// <summary>
    /// Zero size.
    /// </summary>
    public static Size Zero => new(0, 0);
}

/// <summary>
/// Represents a rectangle for layout calculations.
/// </summary>
public record Rectangle(int X, int Y, int Width, int Height)
{
    /// <summary>
    /// Gets the right edge position.
    /// </summary>
    public int Right => X + Width;
    
    /// <summary>
    /// Gets the bottom edge position.
    /// </summary>
    public int Bottom => Y + Height;
    
    /// <summary>
    /// Empty rectangle.
    /// </summary>
    public static Rectangle Empty => new(0, 0, 0, 0);
    
    /// <summary>
    /// Creates a rectangle with the specified insets.
    /// </summary>
    public Rectangle Inset(Spacing spacing) => new(
        X + spacing.Left,
        Y + spacing.Top,
        Width - spacing.Horizontal,
        Height - spacing.Vertical);
}