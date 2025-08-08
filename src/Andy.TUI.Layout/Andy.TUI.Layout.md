# Andy.TUI.Layout Project Documentation

## Overview

Andy.TUI.Layout provides a comprehensive flexbox-based layout system for terminal user interfaces. This module implements a constraint-based layout engine inspired by CSS Flexbox and Yoga, enabling responsive terminal UI layouts with precise control over positioning, spacing, and alignment.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Namespace Configuration
```xml
<RootNamespace>Andy.TUI.Declarative.Layout</RootNamespace>
<AssemblyName>Andy.TUI.Layout</AssemblyName>
```
Note: Maintains namespace compatibility while being a separate assembly.

### Dependencies
```xml
<ProjectReference Include="..\Andy.TUI.VirtualDom\Andy.TUI.VirtualDom.csproj" />
```
Integrates with Virtual DOM for layout calculations on virtual nodes.

## Core Architecture

```
        Layout System
             │
    ┌────────┼────────┐
    │        │        │
Constraints Box     Flexbox
    │      Model    Engine
    │        │        │
    └────────┼────────┘
             │
    ┌────────┼────────┐
    │        │        │
  HStack  VStack  Custom
         Layouts
```

## Key Components

### 1. LayoutBox
The fundamental layout primitive:
- Position (X, Y)
- Size (Width, Height)
- Padding, Margin, Border
- Content vs. Border box model
- Absolute and relative positioning

### 2. LayoutConstraints
Constraint-based sizing system:
- Minimum and maximum dimensions
- Preferred size
- Flex grow/shrink factors
- Aspect ratio constraints
- Priority levels

### 3. Length
Flexible dimension specification:
```
┌─────────────────────────────┐
│      Length Types           │
├─────────────────────────────┤
│ • Auto - Automatic sizing   │
│ • Pixels - Fixed size       │
│ • Percentage - Relative     │
│ • Flex - Flexible units     │
└─────────────────────────────┘
```

### 4. Spacing
Comprehensive spacing management:
- Uniform spacing (all sides)
- Individual side control (top, right, bottom, left)
- Horizontal/Vertical shortcuts
- Negative spacing support

## Flexbox Properties

### FlexDirection
Controls the main axis direction:
```
Row (→)          RowReverse (←)
┌─┬─┬─┐          ┌─┬─┬─┐
│1│2│3│          │3│2│1│
└─┴─┴─┘          └─┴─┴─┘

Column (↓)       ColumnReverse (↑)
┌─┐              ┌─┐
│1│              │3│
├─┤              ├─┤
│2│              │2│
├─┤              ├─┤
│3│              │1│
└─┘              └─┘
```

### JustifyContent
Alignment along the main axis:
```
FlexStart        Center          FlexEnd
┌────────┐       ┌────────┐      ┌────────┐
│■■□□□□□□│       │□□□■■□□□│      │□□□□□□■■│
└────────┘       └────────┘      └────────┘

SpaceBetween     SpaceAround     SpaceEvenly
┌────────┐       ┌────────┐      ┌────────┐
│■□□□□□□■│       │□■□□□□■□│      │□□■□□■□□│
└────────┘       └────────┘      └────────┘
```

### AlignItems
Alignment along the cross axis:
```
FlexStart (Top)     Center           FlexEnd (Bottom)
┌──────────┐        ┌──────────┐     ┌──────────┐
│■■■       │        │          │     │          │
│          │        │   ■■■    │     │          │
│          │        │          │     │       ■■■│
└──────────┘        └──────────┘     └──────────┘

Stretch             Baseline
┌──────────┐        ┌──────────┐
│■■■■■■■■■■│        │■■■  ■■■  │
│■■■■■■■■■■│        │     ───  │ (text baseline)
│■■■■■■■■■■│        │          │
└──────────┘        └──────────┘
```

### AlignSelf
Individual item cross-axis alignment:
- Overrides parent's AlignItems
- Same values as AlignItems
- Auto (default to parent)

### FlexWrap
Controls line wrapping:
```
NoWrap (default):    Wrap:           WrapReverse:
┌──────────┐         ┌──────────┐    ┌──────────┐
│■■■■■■■■■→│         │■■■■      │    │■■■■      │
└──────────┘         │■■■■      │    │■■■■      │
(overflow)           └──────────┘    └──────────┘
                     (new lines)     (reverse order)
```

## Layout Containers

### HStack (Horizontal Stack)
Arranges children horizontally:
```csharp
new HStack(spacing: 2) {
    new Text("Item 1"),
    new Spacer(),  // Flexible space
    new Text("Item 2"),
    new Text("Item 3")
}

Result: Item 1          Item 2  Item 3
```

### VStack (Vertical Stack)
Arranges children vertically:
```csharp
new VStack(spacing: 1) {
    new Text("Header"),
    new Divider(),
    new Text("Content"),
    new Spacer(),
    new Text("Footer")
}

Result:
Header
────────
Content

Footer
```

## Layout Algorithm

### Layout Pipeline
```
1. Constraint Resolution
        │
        ▼
2. Measure Pass (bottom-up)
        │
        ▼
3. Arrange Pass (top-down)
        │
        ▼
4. Final Positioning
```

### Constraint Resolution Process
```
Parent Constraints
        │
        ▼
Apply Flex Properties
        │
        ▼
Calculate Available Space
        │
        ▼
Distribute to Children
        │
        ▼
Resolve Child Sizes
```

### Flex Distribution
```
Total Space = Container Size - Fixed Items - Spacing

Flex Item Size = Base Size + (Flex Grow × Available Space)
                           - (Flex Shrink × Overflow)
```

## Usage Examples

### Example 1: Responsive Layout
```
Container (Width: 80)
├── Fixed (20 chars)
├── Flexible (flex: 1) → Gets 30 chars
└── Flexible (flex: 2) → Gets 30 chars

Distribution: 20 + 30 + 30 = 80
```

### Example 2: Centered Content
```
┌────────────────────────┐
│                        │
│    ┌──────────┐       │
│    │ Centered │       │
│    └──────────┘       │
│                        │
└────────────────────────┘

JustifyContent: Center
AlignItems: Center
```

### Example 3: Complex Form Layout
```
┌─────────────────────────────┐
│ Name:     [___________]     │ ← HStack
│ Email:    [___________]     │ ← HStack
│                             │ ← Spacer
│ [Cancel]         [Submit]   │ ← HStack with SpaceBetween
└─────────────────────────────┘
   ↑
VStack Container
```

## Performance Optimizations

### Layout Caching
```
Component ──► Hash Constraints ──► Cache Hit? ──► Use Cached
                                        │
                                        No
                                        │
                                        ▼
                                  Calculate New
```

### Incremental Layout
- Only recalculate changed subtrees
- Propagate changes upward only when necessary
- Batch layout updates

### Constraint Propagation
```
Parent Change ──► Affected Children ──► Selective Update
       │                                      │
       └── Early termination if              │
           constraints unchanged ────────────┘
```

## Integration Points

### With Virtual DOM
```
Virtual Node ──► Layout Properties ──► Layout Engine ──► Position
      │               │                      │            │
   Children      Constraints            Calculate      Apply
```

### With Rendering
```
Layout Result ──► Absolute Positions ──► Render Commands
      │                │                      │
   Box Model      Transformed           Terminal Output
```

### With Components
```
Component Props ──► Layout Config ──► Layout System
        │                │                │
    User Input      Constraints      Final Layout
```

## Terminal Constraints

### Character Grid Alignment
```
Requested: 10.7 chars → Rounded: 11 chars
Position: (3.2, 5.8) → Snapped: (3, 6)
```

### Aspect Ratio Considerations
- Terminal characters are typically 2:1 (height:width)
- Account for this in circular/square layouts
- Adjust spacing accordingly

## Best Practices

### For Layout Design
1. Use semantic containers (HStack, VStack)
2. Leverage Spacer for flexible spacing
3. Prefer relative over absolute positioning
4. Consider terminal size variability

### For Performance
1. Minimize layout tree depth
2. Use fixed dimensions when possible
3. Batch layout changes
4. Cache computed layouts

### For Responsiveness
1. Use flex properties for adaptation
2. Set minimum/maximum constraints
3. Test with various terminal sizes
4. Provide fallback layouts

### For Maintainability
1. Separate layout from styling
2. Create reusable layout components
3. Document constraint dependencies
4. Use consistent spacing patterns

## Debugging Layout Issues

### Common Problems
1. **Overflow**: Content exceeds container
   - Solution: Use FlexWrap or ScrollView
   
2. **Incorrect Alignment**: Items not positioned correctly
   - Solution: Check JustifyContent and AlignItems
   
3. **Unexpected Sizing**: Elements too large/small
   - Solution: Verify flex properties and constraints
   
4. **Layout Thrashing**: Constant recalculation
   - Solution: Stabilize constraint dependencies

### Debug Helpers
- Layout bounds visualization
- Constraint conflict detection
- Performance profiling
- Layout tree inspection