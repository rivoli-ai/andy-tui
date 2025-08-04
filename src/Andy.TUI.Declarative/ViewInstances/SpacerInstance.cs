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
        // The actual size will be determined by the parent container
        // For now, we just report our minimum size requirements
        
        float minWidth = 0;
        float minHeight = 0;
        
        if (_minLength.HasValue)
        {
            var minValue = _minLength.Value.ToPixels(0); // No parent size context yet
            
            // We don't know the parent's direction here, so we set both dimensions
            // The parent will use the appropriate one based on its flex direction
            minWidth = minValue;
            minHeight = minValue;
        }
        
        // Spacer is flexible - it wants to grow as much as possible
        // Return minimum size; parent will expand us as needed
        return new LayoutBox { X = 0, Y = 0, Width = minWidth, Height = minHeight };
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