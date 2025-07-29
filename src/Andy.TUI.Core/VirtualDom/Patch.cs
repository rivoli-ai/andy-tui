namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents a patch operation to apply to the virtual DOM.
/// </summary>
public abstract class Patch
{
    /// <summary>
    /// Gets the type of patch operation.
    /// </summary>
    public abstract PatchType Type { get; }
    
    /// <summary>
    /// Gets the path to the node being patched.
    /// </summary>
    public int[] Path { get; }
    
    /// <summary>
    /// Initializes a new instance of the Patch class.
    /// </summary>
    /// <param name="path">The path to the node being patched.</param>
    protected Patch(int[] path)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }
    
    /// <summary>
    /// Accepts a visitor for processing patches.
    /// </summary>
    public abstract void Accept(IPatchVisitor visitor);
}

/// <summary>
/// Defines the types of patch operations.
/// </summary>
public enum PatchType
{
    /// <summary>
    /// Replace a node with a new node.
    /// </summary>
    Replace,
    
    /// <summary>
    /// Update node properties.
    /// </summary>
    UpdateProps,
    
    /// <summary>
    /// Update text content.
    /// </summary>
    UpdateText,
    
    /// <summary>
    /// Insert a new node.
    /// </summary>
    Insert,
    
    /// <summary>
    /// Remove a node.
    /// </summary>
    Remove,
    
    /// <summary>
    /// Move a node to a new position.
    /// </summary>
    Move,
    
    /// <summary>
    /// Reorder child nodes.
    /// </summary>
    Reorder
}

/// <summary>
/// Patch to replace a node with a new node.
/// </summary>
public sealed class ReplacePatch : Patch
{
    public override PatchType Type => PatchType.Replace;
    
    /// <summary>
    /// Gets the new node to replace with.
    /// </summary>
    public VirtualNode NewNode { get; }
    
    public ReplacePatch(int[] path, VirtualNode newNode) : base(path)
    {
        NewNode = newNode ?? throw new ArgumentNullException(nameof(newNode));
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitReplace(this);
}

/// <summary>
/// Patch to update node properties.
/// </summary>
public sealed class UpdatePropsPatch : Patch
{
    public override PatchType Type => PatchType.UpdateProps;
    
    /// <summary>
    /// Gets the properties to add or update.
    /// </summary>
    public Dictionary<string, object?> PropsToSet { get; }
    
    /// <summary>
    /// Gets the property keys to remove.
    /// </summary>
    public HashSet<string> PropsToRemove { get; }
    
    public UpdatePropsPatch(int[] path, Dictionary<string, object?> propsToSet, HashSet<string> propsToRemove) : base(path)
    {
        PropsToSet = propsToSet ?? new Dictionary<string, object?>();
        PropsToRemove = propsToRemove ?? new HashSet<string>();
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitUpdateProps(this);
}

/// <summary>
/// Patch to update text content.
/// </summary>
public sealed class UpdateTextPatch : Patch
{
    public override PatchType Type => PatchType.UpdateText;
    
    /// <summary>
    /// Gets the new text content.
    /// </summary>
    public string NewText { get; }
    
    public UpdateTextPatch(int[] path, string newText) : base(path)
    {
        NewText = newText ?? string.Empty;
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitUpdateText(this);
}

/// <summary>
/// Patch to insert a new node.
/// </summary>
public sealed class InsertPatch : Patch
{
    public override PatchType Type => PatchType.Insert;
    
    /// <summary>
    /// Gets the node to insert.
    /// </summary>
    public VirtualNode Node { get; }
    
    /// <summary>
    /// Gets the index at which to insert the node.
    /// </summary>
    public int Index { get; }
    
    public InsertPatch(int[] path, VirtualNode node, int index) : base(path)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Index = index;
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitInsert(this);
}

/// <summary>
/// Patch to remove a node.
/// </summary>
public sealed class RemovePatch : Patch
{
    public override PatchType Type => PatchType.Remove;
    
    /// <summary>
    /// Gets the index of the node to remove.
    /// </summary>
    public int Index { get; }
    
    public RemovePatch(int[] path, int index) : base(path)
    {
        Index = index;
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitRemove(this);
}

/// <summary>
/// Patch to move a node to a new position.
/// </summary>
public sealed class MovePatch : Patch
{
    public override PatchType Type => PatchType.Move;
    
    /// <summary>
    /// Gets the current index of the node.
    /// </summary>
    public int FromIndex { get; }
    
    /// <summary>
    /// Gets the new index for the node.
    /// </summary>
    public int ToIndex { get; }
    
    public MovePatch(int[] path, int fromIndex, int toIndex) : base(path)
    {
        FromIndex = fromIndex;
        ToIndex = toIndex;
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitMove(this);
}

/// <summary>
/// Patch to reorder child nodes.
/// </summary>
public sealed class ReorderPatch : Patch
{
    public override PatchType Type => PatchType.Reorder;
    
    /// <summary>
    /// Gets the moves to perform for reordering.
    /// </summary>
    public IReadOnlyList<(int from, int to)> Moves { get; }
    
    public ReorderPatch(int[] path, IReadOnlyList<(int from, int to)> moves) : base(path)
    {
        Moves = moves ?? throw new ArgumentNullException(nameof(moves));
    }
    
    public override void Accept(IPatchVisitor visitor) => visitor.VisitReorder(this);
}

/// <summary>
/// Visitor interface for processing patches.
/// </summary>
public interface IPatchVisitor
{
    void VisitReplace(ReplacePatch patch);
    void VisitUpdateProps(UpdatePropsPatch patch);
    void VisitUpdateText(UpdateTextPatch patch);
    void VisitInsert(InsertPatch patch);
    void VisitRemove(RemovePatch patch);
    void VisitMove(MovePatch patch);
    void VisitReorder(ReorderPatch patch);
}