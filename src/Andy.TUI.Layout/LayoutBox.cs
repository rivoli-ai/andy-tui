using System;

namespace Andy.TUI.Layout;

/// <summary>
/// Represents the calculated layout box for a component after layout computation.
/// </summary>
public class LayoutBox
{
    /// <summary>
    /// Gets or sets the X position relative to parent.
    /// </summary>
    public float X { get; set; }
    
    /// <summary>
    /// Gets or sets the Y position relative to parent.
    /// </summary>
    public float Y { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the content area (excluding padding).
    /// </summary>
    public float Width { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the content area (excluding padding).
    /// </summary>
    public float Height { get; set; }
    
    /// <summary>
    /// Gets or sets the padding space inside the border.
    /// </summary>
    public Spacing Padding { get; set; }
    
    /// <summary>
    /// Gets or sets the margin space outside the border.
    /// </summary>
    public Spacing Margin { get; set; }
    
    /// <summary>
    /// Gets the total width including padding but excluding margin.
    /// </summary>
    public float OuterWidth => Width + Padding.Left.ToPixels(Width) + Padding.Right.ToPixels(Width);
    
    /// <summary>
    /// Gets the total height including padding but excluding margin.
    /// </summary>
    public float OuterHeight => Height + Padding.Top.ToPixels(Height) + Padding.Bottom.ToPixels(Height);
    
    /// <summary>
    /// Gets the absolute X position in terminal coordinates.
    /// </summary>
    public int AbsoluteX { get; set; }
    
    /// <summary>
    /// Gets the absolute Y position in terminal coordinates.
    /// </summary>
    public int AbsoluteY { get; set; }
    
    /// <summary>
    /// Gets the content X position (absolute position plus padding).
    /// </summary>
    public int ContentX => AbsoluteX + (int)Math.Round(Padding.Left.ToPixels(Width));
    
    /// <summary>
    /// Gets the content Y position (absolute position plus padding).
    /// </summary>
    public int ContentY => AbsoluteY + (int)Math.Round(Padding.Top.ToPixels(Height));
    
    /// <summary>
    /// Gets the content width in integer terminal cells.
    /// </summary>
    public int ContentWidth => (int)Math.Round(Width);
    
    /// <summary>
    /// Gets the content height in integer terminal cells.
    /// </summary>
    public int ContentHeight => (int)Math.Round(Height);
    
    /// <summary>
    /// Creates a new layout box with default values.
    /// </summary>
    public LayoutBox()
    {
        Padding = Spacing.Zero;
        Margin = Spacing.Zero;
    }
    
    /// <summary>
    /// Creates a copy of this layout box.
    /// </summary>
    public LayoutBox Clone()
    {
        return new LayoutBox
        {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Padding = Padding,
            Margin = Margin,
            AbsoluteX = AbsoluteX,
            AbsoluteY = AbsoluteY
        };
    }
    
    /// <summary>
    /// Checks if a point is within this layout box.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x >= AbsoluteX && 
               x < AbsoluteX + OuterWidth &&
               y >= AbsoluteY && 
               y < AbsoluteY + OuterHeight;
    }
    
    public override string ToString()
    {
        return $"LayoutBox(X:{X}, Y:{Y}, W:{Width}, H:{Height}, " +
               $"AbsX:{AbsoluteX}, AbsY:{AbsoluteY}, " +
               $"Padding:{Padding}, Margin:{Margin})";
    }
}