using Andy.TUI.VirtualDom;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class PatchTests
{
    [Fact]
    public void ReplacePatch_StoresCorrectData()
    {
        var path = new[] { 0, 1, 2 };
        var newNode = new TextNode("New");
        var patch = new ReplacePatch(path, newNode);
        
        Assert.Equal(PatchType.Replace, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal(newNode, patch.NewNode);
    }
    
    [Fact]
    public void UpdatePropsPatch_StoresCorrectData()
    {
        var path = new[] { 0 };
        var propsToSet = new Dictionary<string, object?> { ["id"] = "new" };
        var propsToRemove = new HashSet<string> { "class" };
        var patch = new UpdatePropsPatch(path, propsToSet, propsToRemove);
        
        Assert.Equal(PatchType.UpdateProps, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal("new", patch.PropsToSet["id"]);
        Assert.Contains("class", patch.PropsToRemove);
    }
    
    [Fact]
    public void UpdateTextPatch_StoresCorrectData()
    {
        var path = new[] { 1, 0 };
        var patch = new UpdateTextPatch(path, "Updated Text");
        
        Assert.Equal(PatchType.UpdateText, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal("Updated Text", patch.NewText);
    }
    
    [Fact]
    public void InsertPatch_StoresCorrectData()
    {
        var path = new[] { 0 };
        var node = new ElementNode("div");
        var patch = new InsertPatch(path, node, 2);
        
        Assert.Equal(PatchType.Insert, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal(node, patch.Node);
        Assert.Equal(2, patch.Index);
    }
    
    [Fact]
    public void RemovePatch_StoresCorrectData()
    {
        var path = new[] { 0, 1 };
        var patch = new RemovePatch(path, 3);
        
        Assert.Equal(PatchType.Remove, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal(3, patch.Index);
    }
    
    [Fact]
    public void MovePatch_StoresCorrectData()
    {
        var path = new[] { 0 };
        var patch = new MovePatch(path, 2, 5);
        
        Assert.Equal(PatchType.Move, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal(2, patch.FromIndex);
        Assert.Equal(5, patch.ToIndex);
    }
    
    [Fact]
    public void ReorderPatch_StoresCorrectData()
    {
        var path = new[] { 0 };
        var moves = new List<(int, int)> { (0, 2), (1, 0), (2, 1) };
        var patch = new ReorderPatch(path, moves);
        
        Assert.Equal(PatchType.Reorder, patch.Type);
        Assert.Equal(path, patch.Path);
        Assert.Equal(moves, patch.Moves);
    }
    
    [Fact]
    public void Patch_Visitor_VisitsCorrectMethods()
    {
        var visitor = new TestPatchVisitor();
        var path = Array.Empty<int>();
        
        new ReplacePatch(path, new TextNode("")).Accept(visitor);
        new UpdatePropsPatch(path, new Dictionary<string, object?>(), new HashSet<string>()).Accept(visitor);
        new UpdateTextPatch(path, "").Accept(visitor);
        new InsertPatch(path, new TextNode(""), 0).Accept(visitor);
        new RemovePatch(path, 0).Accept(visitor);
        new MovePatch(path, 0, 1).Accept(visitor);
        new ReorderPatch(path, new List<(int, int)>()).Accept(visitor);
        
        Assert.True(visitor.VisitedReplace);
        Assert.True(visitor.VisitedUpdateProps);
        Assert.True(visitor.VisitedUpdateText);
        Assert.True(visitor.VisitedInsert);
        Assert.True(visitor.VisitedRemove);
        Assert.True(visitor.VisitedMove);
        Assert.True(visitor.VisitedReorder);
    }
    
    [Fact]
    public void Patch_NullPath_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplacePatch(null!, new TextNode("")));
        Assert.Throws<ArgumentNullException>(() => new UpdatePropsPatch(null!, new Dictionary<string, object?>(), new HashSet<string>()));
        Assert.Throws<ArgumentNullException>(() => new UpdateTextPatch(null!, ""));
        Assert.Throws<ArgumentNullException>(() => new InsertPatch(null!, new TextNode(""), 0));
        Assert.Throws<ArgumentNullException>(() => new RemovePatch(null!, 0));
        Assert.Throws<ArgumentNullException>(() => new MovePatch(null!, 0, 1));
        Assert.Throws<ArgumentNullException>(() => new ReorderPatch(null!, new List<(int, int)>()));
    }
    
    [Fact]
    public void ReplacePatch_NullNode_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReplacePatch(Array.Empty<int>(), null!));
    }
    
    [Fact]
    public void InsertPatch_NullNode_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new InsertPatch(Array.Empty<int>(), null!, 0));
    }
    
    [Fact]
    public void ReorderPatch_NullMoves_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new ReorderPatch(Array.Empty<int>(), null!));
    }
    
    private class TestPatchVisitor : IPatchVisitor
    {
        public bool VisitedReplace { get; private set; }
        public bool VisitedUpdateProps { get; private set; }
        public bool VisitedUpdateText { get; private set; }
        public bool VisitedInsert { get; private set; }
        public bool VisitedRemove { get; private set; }
        public bool VisitedMove { get; private set; }
        public bool VisitedReorder { get; private set; }
        
        public void VisitReplace(ReplacePatch patch) => VisitedReplace = true;
        public void VisitUpdateProps(UpdatePropsPatch patch) => VisitedUpdateProps = true;
        public void VisitUpdateText(UpdateTextPatch patch) => VisitedUpdateText = true;
        public void VisitInsert(InsertPatch patch) => VisitedInsert = true;
        public void VisitRemove(RemovePatch patch) => VisitedRemove = true;
        public void VisitMove(MovePatch patch) => VisitedMove = true;
        public void VisitReorder(ReorderPatch patch) => VisitedReorder = true;
    }
}