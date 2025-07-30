using System.Collections.Generic;

namespace Andy.TUI.Declarative;

/// <summary>
/// Tracks component positions for hit testing and layout.
/// </summary>
public class ComponentBounds
{
    private readonly Dictionary<ISimpleComponent, Rectangle> _bounds = new();
    
    /// <summary>
    /// Registers the bounds of a component.
    /// </summary>
    public void RegisterBounds(ISimpleComponent component, int x, int y, int width, int height)
    {
        _bounds[component] = new Rectangle(x, y, width, height);
    }
    
    /// <summary>
    /// Gets the bounds of a component.
    /// </summary>
    public Rectangle? GetBounds(ISimpleComponent component)
    {
        return _bounds.TryGetValue(component, out var bounds) ? bounds : null;
    }
    
    /// <summary>
    /// Performs hit testing to find the component at the specified position.
    /// </summary>
    public ISimpleComponent? HitTest(int x, int y)
    {
        foreach (var (component, bounds) in _bounds)
        {
            if (bounds.Contains(x, y))
            {
                return component;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Clears all registered bounds.
    /// </summary>
    public void Clear()
    {
        _bounds.Clear();
    }
}

/// <summary>
/// Represents a rectangle.
/// </summary>
public struct Rectangle
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public bool Contains(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }
}