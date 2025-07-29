namespace Andy.TUI.Terminal;

/// <summary>
/// Represents a single cell in the terminal buffer.
/// </summary>
public readonly struct Cell : IEquatable<Cell>
{
    /// <summary>
    /// Gets the character in this cell.
    /// </summary>
    public char Character { get; init; }
    
    /// <summary>
    /// Gets the style applied to this cell.
    /// </summary>
    public Style Style { get; init; }
    
    /// <summary>
    /// Gets whether this cell has been modified.
    /// </summary>
    public bool IsDirty { get; init; }
    
    /// <summary>
    /// An empty cell with space character and default style.
    /// </summary>
    public static Cell Empty { get; } = new Cell { Character = ' ', Style = Style.Default };
    
    /// <summary>
    /// Creates a new cell with the specified character and style.
    /// </summary>
    public Cell(char character, Style style = default)
    {
        Character = character;
        Style = style;
        IsDirty = true;
    }
    
    /// <summary>
    /// Creates a new cell marked as clean.
    /// </summary>
    public Cell AsClean() => this with { IsDirty = false };
    
    /// <summary>
    /// Creates a new cell marked as dirty.
    /// </summary>
    public Cell AsDirty() => this with { IsDirty = true };
    
    public bool Equals(Cell other)
    {
        return Character == other.Character && Style.Equals(other.Style);
    }
    
    public override bool Equals(object? obj) => obj is Cell other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(Character, Style);
    
    public static bool operator ==(Cell left, Cell right) => left.Equals(right);
    public static bool operator !=(Cell left, Cell right) => !left.Equals(right);
    
    public override string ToString() => $"'{Character}' {Style}";
}