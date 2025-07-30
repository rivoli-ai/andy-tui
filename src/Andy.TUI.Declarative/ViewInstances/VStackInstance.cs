using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a VStack view with child management.
/// </summary>
public class VStackInstance : ViewInstance
{
    private int _spacing;
    private readonly List<ViewInstance> _childInstances = new();
    
    public VStackInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not VStack vstack)
            throw new ArgumentException("Expected VStack declaration");
        
        _spacing = vstack.GetSpacing();
        
        // Update child instances
        var children = vstack.GetChildren();
        var manager = Context?.ViewInstanceManager;
        
        if (manager != null)
        {
            _childInstances.Clear();
            for (int i = 0; i < children.Count; i++)
            {
                var childPath = $"{Id}/{i}";
                var childInstance = manager.GetOrCreateInstance(children[i], childPath);
                _childInstances.Add(childInstance);
            }
        }
    }
    
    public override VirtualNode Render()
    {
        return RenderWithOffset(0, 0);
    }
    
    internal VirtualNode RenderWithOffset(int offsetX, int offsetY)
    {
        if (_childInstances.Count == 0)
            return Fragment();

        var childElements = new List<VirtualNode>();
        int currentY = offsetY;
        
        foreach (var childInstance in _childInstances)
        {
            VirtualNode childNode;
            
            // If child is an HStack, render it with the current offset
            if (childInstance is HStackInstance hstack)
            {
                childNode = hstack.RenderWithOffset(offsetX, currentY);
            }
            else
            {
                childNode = childInstance.Render();
                
                // Position the child
                if (childNode is ElementNode element)
                {
                    var props = new Dictionary<string, object?>(element.Props)
                    {
                        ["x"] = offsetX,
                        ["y"] = currentY
                    };
                    childNode = new ElementNode(element.TagName, props, element.Children.ToArray());
                }
            }
            
            childElements.Add(childNode);
            currentY += 1 + _spacing;
        }
        
        return Fragment(childElements.ToArray());
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}