using System;

namespace Andy.TUI.Layout;

/// <summary>
/// Represents spacing values for margin or padding with individual side control.
/// </summary>
public readonly struct Spacing : IEquatable<Spacing>
{
    /// <summary>
    /// Gets the top spacing.
    /// </summary>
    public Length Top { get; }

    /// <summary>
    /// Gets the right spacing.
    /// </summary>
    public Length Right { get; }

    /// <summary>
    /// Gets the bottom spacing.
    /// </summary>
    public Length Bottom { get; }

    /// <summary>
    /// Gets the left spacing.
    /// </summary>
    public Length Left { get; }

    /// <summary>
    /// Zero spacing on all sides.
    /// </summary>
    public static Spacing Zero => new(0);

    /// <summary>
    /// Creates spacing with the same value on all sides.
    /// </summary>
    public Spacing(Length all) : this(all, all, all, all) { }

    /// <summary>
    /// Creates spacing with vertical and horizontal values.
    /// </summary>
    public Spacing(Length vertical, Length horizontal) : this(vertical, horizontal, vertical, horizontal) { }

    /// <summary>
    /// Creates spacing with individual values for each side.
    /// </summary>
    public Spacing(Length top, Length right, Length bottom, Length left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    /// <summary>
    /// Creates spacing from an integer value for all sides.
    /// </summary>
    public static implicit operator Spacing(int all) => new(all);

    /// <summary>
    /// Creates spacing from a float value for all sides.
    /// </summary>
    public static implicit operator Spacing(float all) => new(all);

    /// <summary>
    /// Creates spacing with only top value.
    /// </summary>
    public static Spacing OnlyTop(Length value) => new(value, 0, 0, 0);

    /// <summary>
    /// Creates spacing with only right value.
    /// </summary>
    public static Spacing OnlyRight(Length value) => new(0, value, 0, 0);

    /// <summary>
    /// Creates spacing with only bottom value.
    /// </summary>
    public static Spacing OnlyBottom(Length value) => new(0, 0, value, 0);

    /// <summary>
    /// Creates spacing with only left value.
    /// </summary>
    public static Spacing OnlyLeft(Length value) => new(0, 0, 0, value);

    /// <summary>
    /// Creates spacing with only horizontal values (left and right).
    /// </summary>
    public static Spacing Horizontal(Length value) => new(0, value, 0, value);

    /// <summary>
    /// Creates spacing with only vertical values (top and bottom).
    /// </summary>
    public static Spacing Vertical(Length value) => new(value, 0, value, 0);

    /// <summary>
    /// Returns a new spacing with the specified top value.
    /// </summary>
    public Spacing WithTop(Length top) => new(top, Right, Bottom, Left);

    /// <summary>
    /// Returns a new spacing with the specified right value.
    /// </summary>
    public Spacing WithRight(Length right) => new(Top, right, Bottom, Left);

    /// <summary>
    /// Returns a new spacing with the specified bottom value.
    /// </summary>
    public Spacing WithBottom(Length bottom) => new(Top, Right, bottom, Left);

    /// <summary>
    /// Returns a new spacing with the specified left value.
    /// </summary>
    public Spacing WithLeft(Length left) => new(Top, Right, Bottom, left);

    /// <summary>
    /// Gets the total horizontal spacing (left + right).
    /// </summary>
    public float GetHorizontalTotal(float containerWidth)
    {
        return Left.ToPixels(containerWidth) + Right.ToPixels(containerWidth);
    }

    /// <summary>
    /// Gets the total vertical spacing (top + bottom).
    /// </summary>
    public float GetVerticalTotal(float containerHeight)
    {
        return Top.ToPixels(containerHeight) + Bottom.ToPixels(containerHeight);
    }

    public override string ToString()
    {
        if (Top == Right && Right == Bottom && Bottom == Left)
            return Top.ToString();

        if (Top == Bottom && Left == Right)
            return $"{Top} {Right}";

        return $"{Top} {Right} {Bottom} {Left}";
    }

    public bool Equals(Spacing other) =>
        Top.Equals(other.Top) &&
        Right.Equals(other.Right) &&
        Bottom.Equals(other.Bottom) &&
        Left.Equals(other.Left);

    public override bool Equals(object? obj) => obj is Spacing other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Top, Right, Bottom, Left);

    public static bool operator ==(Spacing left, Spacing right) => left.Equals(right);
    public static bool operator !=(Spacing left, Spacing right) => !left.Equals(right);
}