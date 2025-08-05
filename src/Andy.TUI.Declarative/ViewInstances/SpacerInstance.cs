using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance of a Spacer component that expands to fill available space.
/// </summary>
public class SpacerInstance : ViewInstance
{
    private Spacer _spacer;
    private Length? _minLength;
    
    public SpacerInstance(Spacer spacer, string key) : base(key)
    {
        _spacer = spacer;
        _minLength = spacer.GetMinLength();
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Spacer spacer)
            throw new ArgumentException("Expected Spacer declaration");
        
        _spacer = spacer;
        _minLength = spacer.GetMinLength();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Spacer takes up available space in the parent's main axis
        // If we have tight constraints, that means the parent has calculated our size
        
        float width = 0;
        float height = 0;
        
        // Check for tight constraints - this means parent has calculated our final size
        if (constraints.MinWidth == constraints.MaxWidth)
        {
            width = constraints.MinWidth;
        }
        else if (_minLength.HasValue)
        {
            // Use minimum width if specified
            width = _minLength.Value.ToPixels(constraints.MaxWidth);
        }
        
        if (constraints.MinHeight == constraints.MaxHeight)
        {
            height = constraints.MinHeight;
        }
        else if (_minLength.HasValue)
        {
            // Use minimum height if specified
            height = _minLength.Value.ToPixels(constraints.MaxHeight);
        }
        
        // Spacer is flexible - it wants to grow as much as possible
        return new LayoutBox { X = 0, Y = 0, Width = width, Height = height };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        // Spacer doesn't render anything visible
        // It just takes up space in the layout
        return new EmptyNode();
    }
    
    /// <summary>
    /// Gets whether this spacer should expand to fill available space.
    /// </summary>
    public bool ShouldExpand => true;
    
    /// <summary>
    /// Gets the minimum length for this spacer in the parent's main axis.
    /// </summary>
    public Length? MinLength => _minLength;
}