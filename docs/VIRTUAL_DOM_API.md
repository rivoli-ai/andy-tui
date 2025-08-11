# Virtual DOM API Reference

The Virtual DOM system provides an efficient way to represent and update terminal UI components through a declarative API and intelligent diffing algorithm.

## Core Concepts

### Virtual Nodes

Virtual nodes are lightweight representations of UI elements that can be efficiently compared and updated.

#### Node Types

1. **TextNode** - Represents text content
2. **ElementNode** - Represents UI elements with properties and children
3. **FragmentNode** - Groups multiple nodes without a wrapper
4. **ComponentNode** - Represents reusable components

### Builder API

The `VirtualDomBuilder` provides a fluent API for constructing virtual DOM trees:

```csharp
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

var ui = VBox()
    .WithClass("container")
    .WithChildren(
        Label().WithText("Hello World"),
        Button()
            .WithText("Click Me")
            .OnClick(() => Console.WriteLine("Clicked!"))
    )
    .Build();
```

### Diff Engine

The `DiffEngine` compares virtual DOM trees and generates minimal patches:

```csharp
var diffEngine = new DiffEngine();
var patches = diffEngine.Diff(oldTree, newTree);
```

## API Reference

### VirtualNode Base Class

```csharp
public abstract class VirtualNode : IEquatable<VirtualNode>
{
    public abstract VirtualNodeType Type { get; }
    public string? Key { get; init; }
    public Dictionary<string, object?> Props { get; init; }
    public abstract IReadOnlyList<VirtualNode> Children { get; }
    public abstract VirtualNode Clone();
    public abstract void Accept(IVirtualNodeVisitor visitor);
}
```

### TextNode

```csharp
public class TextNode : VirtualNode
{
    public string Content { get; }
    public TextNode(string content);
}
```

### ElementNode

```csharp
public class ElementNode : VirtualNode
{
    public string TagName { get; }
    public ElementNode(string tagName, Dictionary<string, object?>? props = null, 
                      params VirtualNode[] children);
    
    public void AddChild(VirtualNode child);
    public bool RemoveChild(VirtualNode child);
    public void ReplaceChild(int index, VirtualNode newChild);
}
```

### FragmentNode

```csharp
public class FragmentNode : VirtualNode
{
    public FragmentNode(params VirtualNode[] children);
    public FragmentNode(IEnumerable<VirtualNode> children);
    
    public void AddChild(VirtualNode child);
    public bool RemoveChild(VirtualNode child);
    public void ReplaceChild(int index, VirtualNode newChild);
}
```

### ComponentNode

```csharp
public class ComponentNode : VirtualNode
{
    public Type ComponentType { get; }
    public object? ComponentInstance { get; set; }
    public VirtualNode? RenderedContent { get; set; }
    
    public ComponentNode(Type componentType, Dictionary<string, object?>? props = null);
    public static ComponentNode Create<TComponent>(Dictionary<string, object?>? props = null);
}
```

### VirtualDomBuilder

```csharp
public static class VirtualDomBuilder
{
    // Node creation
    public static TextNode Text(string content);
    public static ElementBuilder Element(string tagName);
    public static FragmentNode Fragment(params VirtualNode[] children);
    public static ComponentNode Component<TComponent>(Dictionary<string, object?>? props = null);
    
    // Common elements
    public static ElementBuilder Div();
    public static ElementBuilder Span();
    public static ElementBuilder Box();
    public static ElementBuilder VBox();
    public static ElementBuilder HBox();
    public static ElementBuilder Button();
    public static ElementBuilder Input();
    public static ElementBuilder Label();
    public static ElementBuilder List();
    public static ElementBuilder ListItem();
}
```

### ElementBuilder

```csharp
public class ElementBuilder
{
    // Core methods
    public ElementBuilder WithKey(string key);
    public ElementBuilder WithProp(string name, object? value);
    public ElementBuilder WithProps(Dictionary<string, object?> props);
    public ElementBuilder WithChild(VirtualNode child);
    public ElementBuilder WithChildren(params VirtualNode[] children);
    public ElementBuilder WithText(string text);
    public ElementNode Build();
    
    // Property shortcuts
    public ElementBuilder WithClass(string className);
    public ElementBuilder WithId(string id);
    public ElementBuilder WithStyle(string style);
    public ElementBuilder WithWidth(int width);
    public ElementBuilder WithHeight(int height);
    public ElementBuilder WithFlex(int flex);
    public ElementBuilder WithPadding(int padding);
    public ElementBuilder WithMargin(int margin);
    public ElementBuilder WithBorder(string border);
    public ElementBuilder WithBackground(string color);
    public ElementBuilder WithForeground(string color);
    public ElementBuilder WithAlign(string alignment);
    public ElementBuilder WithJustify(string justification);
    
    // Event handlers
    public ElementBuilder OnClick(Action handler);
    public ElementBuilder OnEnter(Action handler);
    public ElementBuilder OnFocus(Action handler);
    public ElementBuilder OnBlur(Action handler);
}
```

### DiffEngine

```csharp
public class DiffEngine
{
    public IReadOnlyList<Patch> Diff(VirtualNode? oldTree, VirtualNode? newTree);
}
```

### Patch Types

All patches implement the base `Patch` class:

```csharp
public abstract class Patch
{
    public abstract PatchType Type { get; }
    public int[] Path { get; }
    public abstract void Accept(IPatchVisitor visitor);
}
```

#### Available Patch Types

1. **ReplacePatch** - Replace entire node
2. **UpdatePropsPatch** - Update node properties
3. **UpdateTextPatch** - Update text content
4. **InsertPatch** - Insert new node
5. **RemovePatch** - Remove existing node
6. **MovePatch** - Move node to different position
7. **ReorderPatch** - Reorder multiple nodes

## Usage Examples

### Basic UI Construction

```csharp
var ui = VBox()
    .WithClass("app")
    .WithChildren(
        // Header
        HBox()
            .WithClass("header")
            .WithHeight(3)
            .WithChildren(
                Label().WithText("My App"),
                Span().WithFlex(1), // Spacer
                Button().WithText("Menu")
            ),
        
        // Content
        Box()
            .WithClass("content")
            .WithFlex(1)
            .WithChildren(
                Text("Welcome to my application!")
            ),
        
        // Footer
        HBox()
            .WithClass("footer")
            .WithHeight(1)
            .WithText("Ready")
    )
    .Build();
```

### Keyed Reconciliation

```csharp
// Use keys for efficient list updates
var list = List()
    .WithChildren(
        items.Select(item =>
            ListItem()
                .WithKey(item.Id)
                .WithText(item.Name)
                .Build()
        ).ToArray()
    )
    .Build();
```

### Reactive Updates with Observables

```csharp
var counter = new ObservableProperty<int>(0);
var diffEngine = new DiffEngine();
VirtualNode? previousTree = null;

// Build UI based on state
VirtualNode BuildUI() => 
    Box()
        .WithChildren(
            Label().WithText($"Count: {counter.Value}"),
            Button()
                .WithText("Increment")
                .OnClick(() => counter.Value++)
        )
        .Build();

// Update on changes
counter.Subscribe(_ => {
    var newTree = BuildUI();
    if (previousTree != null) {
        var patches = diffEngine.Diff(previousTree, newTree);
        // Apply patches to actual UI
    }
    previousTree = newTree;
});
```

### Component Pattern

```csharp
// Define a component
public class UserCard : IComponent
{
    public string Name { get; set; }
    public string Role { get; set; }
    
    public VirtualNode Render() =>
        Box()
            .WithClass("user-card")
            .WithChildren(
                Label().WithText(Name).WithClass("name"),
                Label().WithText(Role).WithClass("role")
            )
            .Build();
}

// Use the component
var userCard = Component<UserCard>(new Dictionary<string, object?> {
    ["Name"] = "John Doe",
    ["Role"] = "Developer"
});
```

## Best Practices

1. **Use Keys for Dynamic Lists**: Always provide stable keys when rendering lists that can change
2. **Minimize Property Changes**: Group related updates together to reduce patches
3. **Leverage Fragments**: Use fragments to avoid unnecessary wrapper elements
4. **Use builders/AddChild in VDOM; collection initializer syntax in Declarative components**: Keep APIs consistent with their layer
5. **Batch Updates**: Collect multiple state changes before diffing for better performance

## Performance Considerations

- The diff algorithm is optimized for common UI update patterns
- Keyed reconciliation provides O(n) performance for list reordering
- Patches are generated depth-first for efficient application
- Virtual nodes are immutable - use `Clone()` for modifications
- The builder pattern minimizes object allocations

## Integration with Observable System

The Virtual DOM integrates seamlessly with the Observable system:

```csharp
var state = new AppState();
var ui = new ReactiveUI(state);

// UI automatically updates when state changes
state.Items.Add(new Item());
state.SelectedItem.Value = state.Items[0];
```

See the [Reactive Virtual DOM Example](../examples/VirtualDom/ReactiveVirtualDomExample.cs) for comprehensive integration patterns.