using System;
using System.Collections.Generic;

namespace Andy.TUI.VirtualDom;

public static class VirtualDomInvariants
{
    public static void AssertTreeIsRenderable(VirtualNode root)
    {
        if (root == null) throw new VirtualDomInvariantViolationException("Root VDOM is null");
        var visited = new HashSet<VirtualNode>(ReferenceEqualityComparer.Instance);
        Traverse(root, visited);
    }

    private static void Traverse(VirtualNode node, HashSet<VirtualNode> visited)
    {
        if (!visited.Add(node))
            throw new VirtualDomInvariantViolationException("Cycle detected in VDOM tree");

        if (node is ComponentNode)
            throw new VirtualDomInvariantViolationException("ComponentNode must be expanded before rendering");

        if (node.Children == null)
            throw new VirtualDomInvariantViolationException("Node.Children is null");

        foreach (var child in node.Children)
        {
            if (child == null)
                throw new VirtualDomInvariantViolationException("Null child in VDOM tree");
            Traverse(child, visited);
        }
    }
}

public sealed class VirtualDomInvariantViolationException : Exception
{
    public VirtualDomInvariantViolationException(string message) : base(message) { }
}

internal sealed class ReferenceEqualityComparer : IEqualityComparer<object?>
{
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
    private ReferenceEqualityComparer() { }
    public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
    public int GetHashCode(object? obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj!);
}
