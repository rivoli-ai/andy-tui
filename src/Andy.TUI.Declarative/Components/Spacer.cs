using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A flexible space component that expands to fill available space.
/// Similar to SwiftUI's Spacer or CSS flex-grow: 1.
/// </summary>
public class Spacer : ISimpleComponent
{
    private readonly Length? _minLength;
    
    /// <summary>
    /// Creates a spacer that expands to fill available space.
    /// </summary>
    public Spacer()
    {
        _minLength = null;
    }
    
    /// <summary>
    /// Creates a spacer with a minimum length.
    /// </summary>
    /// <param name="minLength">The minimum length the spacer should occupy.</param>
    public Spacer(Length minLength)
    {
        _minLength = minLength;
    }
    
    /// <summary>
    /// Creates a spacer with a minimum length in pixels.
    /// </summary>
    /// <param name="minLength">The minimum length in pixels.</param>
    public Spacer(int minLength) : this(Length.Pixels(minLength))
    {
    }
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Spacer declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    internal Length? GetMinLength() => _minLength;
}