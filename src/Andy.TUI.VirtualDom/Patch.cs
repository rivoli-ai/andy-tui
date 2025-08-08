using System;
using System.Collections.Generic;

namespace Andy.TUI.Core.VirtualDom;

public abstract class Patch
{
    public abstract PatchType Type { get; }
    public int[] Path { get; }
    protected Patch(int[] path) { Path = path ?? throw new ArgumentNullException(nameof(path)); }
    public abstract void Accept(IPatchVisitor visitor);
}

public enum PatchType { Replace, UpdateProps, UpdateText, Insert, Remove, Move, Reorder }

public sealed class ReplacePatch : Patch
{
    public override PatchType Type => PatchType.Replace;
    public VirtualNode NewNode { get; }
    public ReplacePatch(int[] path, VirtualNode newNode) : base(path) { NewNode = newNode ?? throw new ArgumentNullException(nameof(newNode)); }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitReplace(this);
}

public sealed class UpdatePropsPatch : Patch
{
    public override PatchType Type => PatchType.UpdateProps;
    public Dictionary<string, object?> PropsToSet { get; }
    public HashSet<string> PropsToRemove { get; }
    public UpdatePropsPatch(int[] path, Dictionary<string, object?> propsToSet, HashSet<string> propsToRemove) : base(path)
    { PropsToSet = propsToSet ?? new Dictionary<string, object?>(); PropsToRemove = propsToRemove ?? new HashSet<string>(); }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitUpdateProps(this);
}

public sealed class UpdateTextPatch : Patch
{
    public override PatchType Type => PatchType.UpdateText;
    public string NewText { get; }
    public UpdateTextPatch(int[] path, string newText) : base(path) { NewText = newText ?? string.Empty; }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitUpdateText(this);
}

public sealed class InsertPatch : Patch
{
    public override PatchType Type => PatchType.Insert;
    public VirtualNode Node { get; }
    public int Index { get; }
    public InsertPatch(int[] path, VirtualNode node, int index) : base(path) { Node = node ?? throw new ArgumentNullException(nameof(node)); Index = index; }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitInsert(this);
}

public sealed class RemovePatch : Patch
{
    public override PatchType Type => PatchType.Remove;
    public int Index { get; }
    public RemovePatch(int[] path, int index) : base(path) { Index = index; }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitRemove(this);
}

public sealed class MovePatch : Patch
{
    public override PatchType Type => PatchType.Move;
    public int FromIndex { get; }
    public int ToIndex { get; }
    public MovePatch(int[] path, int fromIndex, int toIndex) : base(path) { FromIndex = fromIndex; ToIndex = toIndex; }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitMove(this);
}

public sealed class ReorderPatch : Patch
{
    public override PatchType Type => PatchType.Reorder;
    public IReadOnlyList<(int from, int to)> Moves { get; }
    public ReorderPatch(int[] path, IReadOnlyList<(int from, int to)> moves) : base(path) { Moves = moves ?? throw new ArgumentNullException(nameof(moves)); }
    public override void Accept(IPatchVisitor visitor) => visitor.VisitReorder(this);
}

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


