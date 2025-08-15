using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.VirtualDom;

public class DiffEngine
{
    private static void Guard(bool condition, string message)
    {
        if (!condition)
        {
            throw new DiffInvariantViolationException(message);
        }
    }

    public IReadOnlyList<Patch> Diff(VirtualNode? oldTree, VirtualNode? newTree)
    {
        var patches = new List<Patch>();
        DiffNodes(oldTree, newTree, Array.Empty<int>(), patches);
        // Invariants: patch paths must be unique and non-negative; no overlapping conflicting ops
        var pathSets = new HashSet<string>();
        foreach (var p in patches)
        {
            // Path is guaranteed non-null by Patch ctor; still be defensive for analyzers
            var path = p.Path ?? Array.Empty<int>();
            Guard(path.All(i => i >= 0), $"Patch contains negative index at [{string.Join(",", path)}]");

            // Build a uniqueness key that distinguishes operations of the same type at the same path
            // by including their operation-specific identifiers (e.g., indices).
            string key;
            switch (p)
            {
                case InsertPatch ins:
                    key = $"Insert:{string.Join(",", path)}:{ins.Index}";
                    break;
                case RemovePatch rem:
                    key = $"Remove:{string.Join(",", path)}:{rem.Index}";
                    break;
                case MovePatch mv:
                    key = $"Move:{string.Join(",", path)}:{mv.FromIndex}->{mv.ToIndex}";
                    break;
                case ReorderPatch ro:
                    var movesSig = string.Join("|", ro.Moves.Select(m => $"{m.from}->{m.to}"));
                    key = $"Reorder:{string.Join(",", path)}:{movesSig}";
                    break;
                default:
                    key = $"{p.Type}:{string.Join(",", path)}";
                    break;
            }

            Guard(pathSets.Add(key), $"Duplicate patch for same path and type: {key}");
        }
        return patches;
    }

    private void DiffNodes(VirtualNode? oldNode, VirtualNode? newNode, int[] path, List<Patch> patches)
    {
        if (oldNode == null && newNode == null) return;
        if (oldNode != null && newNode == null) { patches.Add(new RemovePatch(path, 0)); return; }
        if (oldNode == null && newNode != null) { patches.Add(new InsertPatch(path, newNode, 0)); return; }
        if (oldNode!.Type != newNode!.Type || !IsSameElement(oldNode, newNode)) { patches.Add(new ReplacePatch(path, newNode)); return; }
        switch (oldNode.Type)
        {
            case VirtualNodeType.Text: DiffTextNodes((TextNode)oldNode, (TextNode)newNode, path, patches); break;
            case VirtualNodeType.Element: DiffElementNodes((ElementNode)oldNode, (ElementNode)newNode, path, patches); break;
            case VirtualNodeType.Fragment: DiffFragmentNodes((FragmentNode)oldNode, (FragmentNode)newNode, path, patches); break;
            case VirtualNodeType.Component: DiffComponentNodes((ComponentNode)oldNode, (ComponentNode)newNode, path, patches); break;
            case VirtualNodeType.Empty: break;
        }
    }

    private bool IsSameElement(VirtualNode oldNode, VirtualNode newNode)
    {
        if (oldNode.Key != null && newNode.Key != null) return oldNode.Key == newNode.Key;
        if (oldNode is ElementNode oe && newNode is ElementNode ne) return oe.TagName == ne.TagName;
        if (oldNode is ComponentNode oc && newNode is ComponentNode nc) return oc.ComponentType == nc.ComponentType;
        return true;
    }

    private void DiffTextNodes(TextNode oldNode, TextNode newNode, int[] path, List<Patch> patches)
    { if (oldNode.Content != newNode.Content) patches.Add(new UpdateTextPatch(path, newNode.Content)); }

    private void DiffElementNodes(ElementNode oldNode, ElementNode newNode, int[] path, List<Patch> patches)
    { DiffProps(oldNode.Props, newNode.Props, path, patches); DiffChildren(oldNode.Children, newNode.Children, path, patches); }

    private void DiffFragmentNodes(FragmentNode oldNode, FragmentNode newNode, int[] path, List<Patch> patches)
    { DiffChildren(oldNode.Children, newNode.Children, path, patches); }

    private void DiffComponentNodes(ComponentNode oldNode, ComponentNode newNode, int[] path, List<Patch> patches)
    { DiffProps(oldNode.Props, newNode.Props, path, patches); }

    private void DiffProps(Dictionary<string, object?> oldProps, Dictionary<string, object?> newProps, int[] path, List<Patch> patches)
    {
        var propsToSet = new Dictionary<string, object?>();
        var propsToRemove = new HashSet<string>();
        foreach (var kvp in newProps) { if (!oldProps.TryGetValue(kvp.Key, out var oldValue) || !Equals(oldValue, kvp.Value)) propsToSet[kvp.Key] = kvp.Value; }
        foreach (var key in oldProps.Keys) { if (!newProps.ContainsKey(key)) propsToRemove.Add(key); }
        if (propsToSet.Count > 0 || propsToRemove.Count > 0) patches.Add(new UpdatePropsPatch(path, propsToSet, propsToRemove));
    }

    private void DiffChildren(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        var hasKeys = oldChildren.Any(c => c.Key != null) || newChildren.Any(c => c.Key != null);
        if (hasKeys) DiffChildrenWithKeys(oldChildren, newChildren, path, patches); else DiffChildrenWithoutKeys(oldChildren, newChildren, path, patches);
    }

    private void DiffChildrenWithoutKeys(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        var maxLength = Math.Max(oldChildren.Count, newChildren.Count);
        // Invariant: children counts must be reasonable (defensive)
        Guard(maxLength >= 0 && maxLength < 1000000, $"Unreasonable child count at [{string.Join(",", path)}]: {maxLength}");
        for (int i = 0; i < maxLength; i++)
        {
            var oldChild = i < oldChildren.Count ? oldChildren[i] : null;
            var newChild = i < newChildren.Count ? newChildren[i] : null;
            if (oldChild == null && newChild != null) patches.Add(new InsertPatch(path, newChild, i));
            else if (oldChild != null && newChild == null) patches.Add(new RemovePatch(path, Math.Min(i, oldChildren.Count - 1)));
            else if (oldChild != null && newChild != null) { var childPath = AppendPath(path, i); DiffNodes(oldChild, newChild, childPath, patches); }
        }
    }

    private void DiffChildrenWithKeys(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        var oldKeyMap = new Dictionary<string, (VirtualNode node, int index)>();
        var newKeyMap = new Dictionary<string, (VirtualNode node, int index)>();
        for (int i = 0; i < oldChildren.Count; i++) { var c = oldChildren[i]; if (c.Key != null) oldKeyMap[c.Key] = (c, i); }
        for (int i = 0; i < newChildren.Count; i++) { var c = newChildren[i]; if (c.Key != null) newKeyMap[c.Key] = (c, i); }
        // Invariant: keys must be unique within siblings
        Guard(oldKeyMap.Count == oldChildren.Where(c => c.Key != null).Count(), $"Duplicate keys detected in old children at [{string.Join(",", path)}]");
        Guard(newKeyMap.Count == newChildren.Where(c => c.Key != null).Count(), $"Duplicate keys detected in new children at [{string.Join(",", path)}]");
        var moves = new List<(int from, int to)>(); var processedOld = new HashSet<int>();
        for (int newIndex = 0; newIndex < newChildren.Count; newIndex++)
        {
            var newChild = newChildren[newIndex];
            if (newChild.Key != null && oldKeyMap.TryGetValue(newChild.Key, out var oldEntry))
            {
                var oldIndex = oldEntry.index; processedOld.Add(oldIndex);
                if (oldIndex != newIndex) moves.Add((oldIndex, newIndex));
                var childPath = AppendPath(path, newIndex); DiffNodes(oldEntry.node, newChild, childPath, patches);
            }
            else { patches.Add(new InsertPatch(path, newChild, newIndex)); }
        }
        for (int oldIndex = oldChildren.Count - 1; oldIndex >= 0; oldIndex--) { if (!processedOld.Contains(oldIndex)) patches.Add(new RemovePatch(path, oldIndex)); }
        if (moves.Count > 0) patches.Add(new ReorderPatch(path, moves));
    }

    private int[] AppendPath(int[] path, int index)
    { var newPath = new int[path.Length + 1]; Array.Copy(path, newPath, path.Length); newPath[path.Length] = index; return newPath; }
}


public sealed class DiffInvariantViolationException : Exception
{
    public DiffInvariantViolationException(string message) : base(message) { }
}


