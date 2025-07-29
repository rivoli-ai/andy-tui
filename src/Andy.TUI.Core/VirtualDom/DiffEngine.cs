namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Compares virtual DOM trees and generates patches to transform one tree into another.
/// </summary>
public class DiffEngine
{
    /// <summary>
    /// Compares two virtual DOM trees and generates patches.
    /// </summary>
    /// <param name="oldTree">The old virtual DOM tree.</param>
    /// <param name="newTree">The new virtual DOM tree.</param>
    /// <returns>A list of patches to apply.</returns>
    public IReadOnlyList<Patch> Diff(VirtualNode? oldTree, VirtualNode? newTree)
    {
        var patches = new List<Patch>();
        DiffNodes(oldTree, newTree, Array.Empty<int>(), patches);
        return patches;
    }
    
    private void DiffNodes(VirtualNode? oldNode, VirtualNode? newNode, int[] path, List<Patch> patches)
    {
        // Both null - nothing to do
        if (oldNode == null && newNode == null)
            return;
            
        // Old exists but new doesn't - remove
        if (oldNode != null && newNode == null)
        {
            patches.Add(new RemovePatch(path, 0));
            return;
        }
        
        // New exists but old doesn't - insert
        if (oldNode == null && newNode != null)
        {
            patches.Add(new InsertPatch(path, newNode, 0));
            return;
        }
        
        // Both exist - check if they're the same type
        if (oldNode!.Type != newNode!.Type || !IsSameElement(oldNode, newNode))
        {
            patches.Add(new ReplacePatch(path, newNode));
            return;
        }
        
        // Same type - diff based on node type
        switch (oldNode.Type)
        {
            case VirtualNodeType.Text:
                DiffTextNodes((TextNode)oldNode, (TextNode)newNode, path, patches);
                break;
                
            case VirtualNodeType.Element:
                DiffElementNodes((ElementNode)oldNode, (ElementNode)newNode, path, patches);
                break;
                
            case VirtualNodeType.Fragment:
                DiffFragmentNodes((FragmentNode)oldNode, (FragmentNode)newNode, path, patches);
                break;
                
            case VirtualNodeType.Component:
                DiffComponentNodes((ComponentNode)oldNode, (ComponentNode)newNode, path, patches);
                break;
        }
    }
    
    private bool IsSameElement(VirtualNode oldNode, VirtualNode newNode)
    {
        // Check keys first for efficient comparison
        if (oldNode.Key != null && newNode.Key != null)
            return oldNode.Key == newNode.Key;
            
        // For elements, check tag name
        if (oldNode is ElementNode oldElement && newNode is ElementNode newElement)
            return oldElement.TagName == newElement.TagName;
            
        // For components, check type
        if (oldNode is ComponentNode oldComponent && newNode is ComponentNode newComponent)
            return oldComponent.ComponentType == newComponent.ComponentType;
            
        return true;
    }
    
    private void DiffTextNodes(TextNode oldNode, TextNode newNode, int[] path, List<Patch> patches)
    {
        if (oldNode.Content != newNode.Content)
        {
            patches.Add(new UpdateTextPatch(path, newNode.Content));
        }
    }
    
    private void DiffElementNodes(ElementNode oldNode, ElementNode newNode, int[] path, List<Patch> patches)
    {
        // Diff properties
        DiffProps(oldNode.Props, newNode.Props, path, patches);
        
        // Diff children
        DiffChildren(oldNode.Children, newNode.Children, path, patches);
    }
    
    private void DiffFragmentNodes(FragmentNode oldNode, FragmentNode newNode, int[] path, List<Patch> patches)
    {
        // Fragments only have children to diff
        DiffChildren(oldNode.Children, newNode.Children, path, patches);
    }
    
    private void DiffComponentNodes(ComponentNode oldNode, ComponentNode newNode, int[] path, List<Patch> patches)
    {
        // Diff component props
        DiffProps(oldNode.Props, newNode.Props, path, patches);
        
        // Component children are handled by the component itself during rendering
    }
    
    private void DiffProps(Dictionary<string, object?> oldProps, Dictionary<string, object?> newProps, int[] path, List<Patch> patches)
    {
        var propsToSet = new Dictionary<string, object?>();
        var propsToRemove = new HashSet<string>();
        
        // Find props to update or add
        foreach (var kvp in newProps)
        {
            if (!oldProps.TryGetValue(kvp.Key, out var oldValue) || !Equals(oldValue, kvp.Value))
            {
                propsToSet[kvp.Key] = kvp.Value;
            }
        }
        
        // Find props to remove
        foreach (var key in oldProps.Keys)
        {
            if (!newProps.ContainsKey(key))
            {
                propsToRemove.Add(key);
            }
        }
        
        if (propsToSet.Count > 0 || propsToRemove.Count > 0)
        {
            patches.Add(new UpdatePropsPatch(path, propsToSet, propsToRemove));
        }
    }
    
    private void DiffChildren(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        // Use key-based reconciliation if keys are present
        var hasKeys = oldChildren.Any(c => c.Key != null) || newChildren.Any(c => c.Key != null);
        
        if (hasKeys)
        {
            DiffChildrenWithKeys(oldChildren, newChildren, path, patches);
        }
        else
        {
            DiffChildrenWithoutKeys(oldChildren, newChildren, path, patches);
        }
    }
    
    private void DiffChildrenWithoutKeys(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        var maxLength = Math.Max(oldChildren.Count, newChildren.Count);
        
        for (int i = 0; i < maxLength; i++)
        {
            var oldChild = i < oldChildren.Count ? oldChildren[i] : null;
            var newChild = i < newChildren.Count ? newChildren[i] : null;
            
            if (oldChild == null && newChild != null)
            {
                // Insert new child
                patches.Add(new InsertPatch(path, newChild, i));
            }
            else if (oldChild != null && newChild == null)
            {
                // Remove old child
                patches.Add(new RemovePatch(path, oldChildren.Count - 1));
            }
            else if (oldChild != null && newChild != null)
            {
                // Diff existing children
                var childPath = AppendPath(path, i);
                DiffNodes(oldChild, newChild, childPath, patches);
            }
        }
    }
    
    private void DiffChildrenWithKeys(IReadOnlyList<VirtualNode> oldChildren, IReadOnlyList<VirtualNode> newChildren, int[] path, List<Patch> patches)
    {
        // Create maps for efficient lookup
        var oldKeyMap = new Dictionary<string, (VirtualNode node, int index)>();
        var newKeyMap = new Dictionary<string, (VirtualNode node, int index)>();
        
        for (int i = 0; i < oldChildren.Count; i++)
        {
            var child = oldChildren[i];
            if (child.Key != null)
                oldKeyMap[child.Key] = (child, i);
        }
        
        for (int i = 0; i < newChildren.Count; i++)
        {
            var child = newChildren[i];
            if (child.Key != null)
                newKeyMap[child.Key] = (child, i);
        }
        
        // Track moves
        var moves = new List<(int from, int to)>();
        var processedOldIndices = new HashSet<int>();
        
        // Process new children
        for (int newIndex = 0; newIndex < newChildren.Count; newIndex++)
        {
            var newChild = newChildren[newIndex];
            
            if (newChild.Key != null && oldKeyMap.TryGetValue(newChild.Key, out var oldEntry))
            {
                // Child exists in old tree - check if moved
                var oldIndex = oldEntry.index;
                processedOldIndices.Add(oldIndex);
                
                if (oldIndex != newIndex)
                {
                    moves.Add((oldIndex, newIndex));
                }
                
                // Diff the child itself
                var childPath = AppendPath(path, newIndex);
                DiffNodes(oldEntry.node, newChild, childPath, patches);
            }
            else
            {
                // New child - insert
                patches.Add(new InsertPatch(path, newChild, newIndex));
            }
        }
        
        // Remove children that no longer exist
        for (int oldIndex = oldChildren.Count - 1; oldIndex >= 0; oldIndex--)
        {
            if (!processedOldIndices.Contains(oldIndex))
            {
                patches.Add(new RemovePatch(path, oldIndex));
            }
        }
        
        // Add reorder patch if there were moves
        if (moves.Count > 0)
        {
            patches.Add(new ReorderPatch(path, moves));
        }
    }
    
    private int[] AppendPath(int[] path, int index)
    {
        var newPath = new int[path.Length + 1];
        Array.Copy(path, newPath, path.Length);
        newPath[path.Length] = index;
        return newPath;
    }
}