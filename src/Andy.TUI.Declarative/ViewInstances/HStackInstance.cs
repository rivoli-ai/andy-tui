using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of an HStack view with child management.
/// </summary>
public class HStackInstance : ViewInstance
{
    private int _spacing;
    private readonly List<ViewInstance> _childInstances = new();
    
    public HStackInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not HStack hstack)
            throw new ArgumentException("Expected HStack declaration");
        
        _spacing = hstack.GetSpacing();
        
        // Update child instances
        var children = hstack.GetChildren();
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
        int currentX = offsetX;
        
        foreach (var childInstance in _childInstances)
        {
            var childNode = childInstance.Render();
            
            // Position the child
            if (childNode is ElementNode element)
            {
                var props = new Dictionary<string, object?>(element.Props)
                {
                    ["x"] = currentX,
                    ["y"] = offsetY
                };
                childNode = new ElementNode(element.TagName, props, element.Children.ToArray());
            }
            else if (childNode is FragmentNode fragment)
            {
                // For fragments, position each child element
                var fragmentChildren = new List<VirtualNode>();
                foreach (var fragChild in fragment.Children)
                {
                    if (fragChild is ElementNode fragElement)
                    {
                        var props = new Dictionary<string, object?>(fragElement.Props)
                        {
                            ["x"] = currentX + (int)(fragElement.Props.GetValueOrDefault("x") ?? 0),
                            ["y"] = offsetY + (int)(fragElement.Props.GetValueOrDefault("y") ?? 0)
                        };
                        fragmentChildren.Add(new ElementNode(fragElement.TagName, props, fragElement.Children.ToArray()));
                    }
                    else
                    {
                        fragmentChildren.Add(fragChild);
                    }
                }
                childNode = Fragment(fragmentChildren.ToArray());
            }
            
            childElements.Add(childNode);
            
            // Estimate width
            int width = EstimateWidth(childNode);
            currentX += width + _spacing;
        }
        
        return Fragment(childElements.ToArray());
    }
    
    private int EstimateWidth(VirtualNode node)
    {
        if (node is TextNode textNode)
            return textNode.Content.Length;
        if (node is ElementNode element && element.Children.FirstOrDefault() is TextNode text)
            return text.Content.Length;
        if (node is FragmentNode fragment)
        {
            // For fragments, use the width of the first element
            var firstChild = fragment.Children.FirstOrDefault();
            if (firstChild != null)
                return EstimateWidth(firstChild);
        }
        return 10; // Default width
    }
    
    public IReadOnlyList<ViewInstance> GetChildInstances() => _childInstances;
}