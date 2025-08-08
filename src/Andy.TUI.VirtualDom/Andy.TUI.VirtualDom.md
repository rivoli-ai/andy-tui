# Andy.TUI.VirtualDom Project Documentation

## Overview

Andy.TUI.VirtualDom implements a high-performance virtual DOM system optimized for terminal user interfaces. This module provides efficient diffing algorithms, patch generation, and tree reconciliation to minimize terminal redraws and optimize rendering performance through intelligent change detection and incremental updates.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Namespace Configuration
```xml
<AssemblyName>Andy.TUI.VirtualDom</AssemblyName>
<RootNamespace>Andy.TUI.Core.VirtualDom</RootNamespace>
```
Maintains backward compatibility with original namespace structure.

## Core Architecture

```
        VirtualDomBuilder
               │
               ▼
         VirtualNode Tree
               │
        ┌──────┴──────┐
        │             │
        ▼             ▼
    Old Tree     New Tree
        │             │
        └──────┬──────┘
               │
               ▼
          DiffEngine
               │
               ▼
         Patch Operations
               │
               ▼
        Terminal Update
```

## Node Types Hierarchy

```
           VirtualNode (Base)
                 │
    ┌────────────┼────────────────┐
    │            │                │
    ▼            ▼                ▼
ElementNode  ComponentNode    FragmentNode
    │            │                │
    ├── TextNode └── Children     └── Children[]
    ├── ClippingNode
    └── EmptyNode
```

## Key Components

### 1. VirtualNode (Abstract Base)
Core abstraction for all virtual DOM nodes:
- Unique key management
- Property storage
- Parent-child relationships
- Equality comparison

### 2. ElementNode
Represents terminal UI elements:
- Style properties
- Position and dimensions
- Event handlers
- Child node management

### 3. TextNode
Optimized text content nodes:
- String content
- Text formatting
- Efficient updates
- No child nodes

### 4. ComponentNode
Encapsulates reusable components:
- Component state
- Props management
- Lifecycle hooks
- Render function

### 5. FragmentNode
Groups multiple nodes without wrapper:
- Array of children
- No visual representation
- Flattening optimization

### 6. ClippingNode
Manages viewport clipping:
- Boundary enforcement
- Overflow handling
- Scissor rectangles

### 7. EmptyNode
Placeholder for conditional rendering:
- Zero-cost abstraction
- Simplifies diffing
- No terminal output

## Diffing Algorithm

### Tree Traversal Strategy
```
     Old Tree              New Tree
         A                     A'
        ╱│╲                   ╱│╲
       B C D                 B' E D'
       │   │                 │   │
       F   G                 F'  G'

Diff Process:
1. Compare A with A' (Update)
2. Compare B with B' (Update)
3. C missing in new (Remove)
4. E new node (Insert)
5. Compare D with D' (Update)
6. Recurse into children
```

### Patch Types
```
┌─────────────────────────────────┐
│   Patch Type    │  Description  │
├─────────────────────────────────┤
│   Create        │  New node     │
│   Update        │  Props change │
│   Move          │  Position     │
│   Remove        │  Delete node  │
│   Replace       │  Type change  │
│   UpdateText    │  Text content │
└─────────────────────────────────┘
```

## Virtual DOM Builder

### Fluent API Pattern
```
VirtualDomBuilder
    .Element("div")
        .WithStyle(...)
        .WithChildren(
            Text("Hello"),
            Element("span").WithText("World")
        )
    .Build()
```

### Tree Construction
```
Builder ──► Node Factory ──► Tree Assembly ──► Validation
                │                  │              │
                └── Type          └── Parent     └── Keys
                    Check             Links          Check
```

## Optimization Strategies

### 1. Key-based Reconciliation
```
Old: [A(key=1), B(key=2), C(key=3)]
New: [B(key=2), A(key=1), D(key=4)]

Result: Move A, Move B, Remove C, Add D
(Without keys: Replace all)
```

### 2. Memoization
```
Component ──► ShouldUpdate? ──► No ──► Skip Subtree
                  │                        │
                  Yes                     Reuse
                  │
                  └──► Render ──► Diff
```

### 3. Batch Updates
```
Multiple Changes ──► Queue ──► Batch ──► Single Diff ──► Apply
                       │         │           │
                      Async    Merge      Optimize
```

## Usage Examples

### Example 1: Simple Update
```
Old DOM:                New DOM:
<div>                   <div>
  <span>Hello</span>      <span>Hi</span>
</div>                  </div>

Patches:
1. UpdateText(span, "Hi")
```

### Example 2: List Reordering
```
Old: [Item1, Item2, Item3]
New: [Item3, Item1, Item2]

With Keys:
- Move Item3 to position 0
- No recreations needed

Without Keys:
- Replace all items
- Full re-render
```

### Example 3: Component Updates
```
<UserCard user={oldUser} />
    ↓ (user prop changes)
<UserCard user={newUser} />
    ↓
Component.ShouldUpdate(oldProps, newProps)
    ↓
Selective re-render
```

## Performance Characteristics

### Time Complexity
```
┌──────────────────────────────────┐
│  Operation      │   Complexity   │
├──────────────────────────────────┤
│  Diff           │   O(n)         │
│  Patch Apply    │   O(m)         │
│  Tree Build     │   O(n)         │
│  Key Lookup     │   O(1)         │
└──────────────────────────────────┘
n = nodes in tree, m = patches
```

### Memory Usage
```
Tree Storage: O(n) nodes
Patch Buffer: O(m) patches
Key Map: O(k) unique keys
```

## Integration Points

### With Rendering System
```
Virtual DOM ──► Patches ──► Render Queue ──► Terminal
                              │
                              └── Optimized draws
```

### With Component System
```
Component State ──► Virtual DOM ──► Diff ──► Update
        ↑                               │
        └───────────────────────────────┘
                  Re-render cycle
```

### With Event System
```
User Input ──► Event Handler ──► State Change ──► VDOM Update
```

## Testing Support

### Internal Visibility
```xml
<InternalsVisibleTo Include="Andy.TUI.Core.Tests" />
<InternalsVisibleTo Include="Andy.TUI.VirtualDom.Tests" />
```

### Test Scenarios
1. **Node Creation**: All node types
2. **Diffing**: Various tree structures
3. **Patching**: All patch types
4. **Keys**: Reconciliation with/without
5. **Performance**: Large tree operations

## Advanced Features

### Lazy Loading
```
ComponentNode ──► Render on demand ──► Cache result
                        │
                        └── Defer until visible
```

### Incremental Rendering
```
Large Tree ──► Chunk ──► Render ──► Yield ──► Continue
                │          │         │
                └── 16ms   └── Draw  └── Next frame
```

### Virtual Scrolling
```
1000 items ──► Render viewport only ──► Update on scroll
                      │
                      └── ~25 visible items
```

## Best Practices

### For Performance
1. Always use keys for dynamic lists
2. Implement shouldComponentUpdate
3. Minimize tree depth
4. Use fragments to avoid wrappers
5. Batch related updates

### For Memory
1. Reuse node instances when possible
2. Clean up event handlers
3. Avoid capturing large closures
4. Use object pooling for nodes

### For Maintainability
1. Keep components pure
2. Avoid direct DOM manipulation
3. Use immutable update patterns
4. Profile before optimizing