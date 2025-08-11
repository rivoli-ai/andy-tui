namespace Andy.TUI.Layout;

/// <summary>
/// Defines the direction of the main axis in a flex container.
/// </summary>
public enum FlexDirection
{
    /// <summary>
    /// Items are placed horizontally from left to right (default).
    /// </summary>
    Row,
    
    /// <summary>
    /// Items are placed horizontally from right to left.
    /// </summary>
    RowReverse,
    
    /// <summary>
    /// Items are placed vertically from top to bottom.
    /// </summary>
    Column,
    
    /// <summary>
    /// Items are placed vertically from bottom to top.
    /// </summary>
    ColumnReverse
}