using System;
using System.Collections.Generic;

namespace Andy.TUI.VirtualDom;

public sealed class ComponentNode : VirtualNode
{
    public Type ComponentType { get; }
    public object? ComponentInstance { get; internal set; }
    public VirtualNode? RenderedContent { get; internal set; }
    public override VirtualNodeType Type => VirtualNodeType.Component;
    public override IReadOnlyList<VirtualNode> Children => RenderedContent != null ? new[] { RenderedContent } : Array.Empty<VirtualNode>();

    public ComponentNode(Type componentType, Dictionary<string, object?>? props = null)
    {
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        if (props != null) { Props = new Dictionary<string, object?>(props); }
    }

    public static ComponentNode Create<TComponent>(Dictionary<string, object?>? props = null) => new(typeof(TComponent), props);

    public override bool Equals(VirtualNode? other)
    {
        if (other is not ComponentNode component) return false;
        if (ComponentType != component.ComponentType || Key != component.Key) return false;
        if (Props.Count != component.Props.Count) return false;
        foreach (var kvp in Props)
        {
            if (!component.Props.TryGetValue(kvp.Key, out var otherValue) || !Equals(kvp.Value, otherValue)) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type); hash.Add(ComponentType); hash.Add(Key); hash.Add(Props.Count);
        return hash.ToHashCode();
    }

    public override VirtualNode Clone()
    {
        var clonedProps = new Dictionary<string, object?>(Props);
        return new ComponentNode(ComponentType, clonedProps) { Key = Key, ComponentInstance = ComponentInstance, RenderedContent = RenderedContent?.Clone() };
    }

    public override void Accept(IVirtualNodeVisitor visitor) => visitor.VisitComponent(this);

    public override string ToString()
    {
        return $"Component: {ComponentType.Name}";
    }
}


