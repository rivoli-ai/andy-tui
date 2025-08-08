# Andy.TUI.Spatial Project Documentation

## Overview

Andy.TUI.Spatial implements advanced spatial indexing and occlusion culling algorithms for terminal user interfaces. This module provides efficient 3D spatial queries, overlap detection, and visibility calculations, enabling optimal rendering performance for complex, layered terminal UIs with overlapping components.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Namespace Configuration
```xml
<RootNamespace>Andy.TUI.Core.Spatial</RootNamespace>
<AssemblyName>Andy.TUI.Spatial</AssemblyName>
```

### Dependencies
```xml
<ProjectReference Include="..\Andy.TUI.VirtualDom\Andy.TUI.VirtualDom.csproj" />
```
Integrates with Virtual DOM for spatial element management.

## Core Architecture

```
           Enhanced3DRTree
                 │
    ┌────────────┼────────────┐
    │            │            │
    ▼            ▼            ▼
Insertion    Query      Occlusion
Operations   Operations  Calculator
    │            │            │
    └────────────┴────────────┘
                 │
                 ▼
          SpatialElement
           Management
```

## Key Components

### 1. Enhanced3DRTree
Advanced R-Tree implementation for 3D spatial indexing:
- Optimized for terminal UI coordinates (X, Y, Z-order)
- Dynamic tree balancing
- Bulk loading support
- Efficient range queries

### 2. OcclusionCalculator
Determines visible regions considering overlapping elements:
- Z-order based visibility
- Partial occlusion detection
- Dirty region calculation
- Viewport clipping

### 3. SpatialElement
Represents UI elements in 3D space:
- Bounding box representation
- Z-index layering
- Metadata association
- Fast intersection tests

### 4. RTreeNode
Internal tree structure for spatial index:
- Minimum bounding rectangles (MBR)
- Node splitting algorithms
- Sibling redistribution
- Height-balanced operations

## Spatial Indexing Concepts

### 3D Coordinate System
```
     Y-axis
        ↑
        │
        │  Z-axis (layers)
        │ ↗
        │╱
    ────┼────► X-axis
        │
        │
        ↓

Terminal Grid: X (columns), Y (rows), Z (depth)
```

### R-Tree Structure
```
                Root
                 │
        ┌────────┼────────┐
        │                 │
    Internal           Internal
    Node A             Node B
        │                 │
    ┌───┼───┐        ┌───┼───┐
    │   │   │        │   │   │
  Leaf Leaf Leaf   Leaf Leaf Leaf
   └─Elements─┘     └─Elements─┘
```

### Occlusion Layers
```
Layer 3 (Top)    ┌─────────┐
                 │ Dialog  │
Layer 2         ┌┴────┬────┴┐
                │ Menu │     │
Layer 1      ┌──┴─────┴──┬──┴──┐
             │  Main UI  │     │
Layer 0      └───────────┴─────┘
             (Background)

Visible = Union(Layers) - Occluded Regions
```

## Query Operations

### 1. Range Query
Find all elements within a bounding box:
```
Query Box ──► R-Tree ──► Matching Elements
   │                           │
   └── (x1,y1,z1)             └── Results
       (x2,y2,z2)
```

### 2. Point Query
Find elements containing a specific point:
```
Point (x,y,z) ──► Tree Traversal ──► Containing Elements
```

### 3. Intersection Query
Find overlapping elements:
```
Element A ──► Spatial Index ──► Overlapping with A
```

## Performance Characteristics

### Time Complexity
```
┌─────────────────────────────────┐
│   Operation     │   Complexity  │
├─────────────────────────────────┤
│   Insert        │   O(log n)    │
│   Delete        │   O(log n)    │
│   Search        │   O(log n)    │
│   Range Query   │   O(log n + k)│
│   Update        │   O(log n)    │
└─────────────────────────────────┘
n = number of elements
k = number of results
```

### Space Optimization
```
Node Capacity: 4-8 children (configurable)
      │
      ├──► Minimize tree height
      ├──► Reduce memory overhead
      └──► Balance query performance
```

## Occlusion Culling Algorithm

### Visibility Determination
```
For each pixel (x, y):
    1. Query spatial index for elements at (x, y)
    2. Sort by Z-order (highest first)
    3. Return topmost opaque element
    4. Track dirty regions for updates
```

### Dirty Region Tracking
```
Element Change ──► Calculate Bounds ──► Mark Dirty
                          │                  │
                          ▼                  ▼
                   Old Position       New Position
                          │                  │
                          └──────┬───────────┘
                                 │
                                 ▼
                          Minimal Redraw
```

## Usage Examples

### Example 1: Window Management
```
Windows in spatial index:
┌──────────────────────────┐
│ Window 1 (z=0)          │
│   ┌──────────────────┐  │
│   │ Window 2 (z=1)   │  │
│   │   ┌──────────┐   │  │
│   │   │Dialog(z=2│   │  │
│   │   └──────────┘   │  │
│   └──────────────────┘  │
└──────────────────────────┘

Query: What's visible at (10, 5)?
Result: Dialog (highest Z-order)
```

### Example 2: Efficient Rendering
```
Viewport (0,0,80,25) ──► Spatial Query ──► Visible Elements
                              │                   │
                              ▼                   ▼
                        Only elements        Render only
                         in viewport          these
```

### Example 3: Hit Testing
```
Mouse Click (x, y) ──► Point Query ──► Top Element ──► Handle Event
```

## Integration Patterns

### With Virtual DOM
```
Virtual Node ──► Spatial Element ──► R-Tree Index
     │                │                   │
     └── Bounds      └── Z-Order         └── Fast Query
```

### With Rendering System
```
Render Request ──► Occlusion Check ──► Visible Elements ──► Draw
                         │                    │
                    Spatial Index        Skip Hidden
```

## Optimization Strategies

### 1. Bulk Loading
```
Elements[] ──► Sort by X ──► Build Tree ──► Balanced Index
                  │
                  └── Optimal packing
```

### 2. Incremental Updates
```
Single Change ──► Local Tree Update ──► No Full Rebuild
```

### 3. Caching
```
Frequent Queries ──► Cache Results ──► Invalidate on Change
```

## Testing Support

### Internal Visibility
```xml
<InternalsVisibleTo Include="Andy.TUI.Core.Tests" />
<InternalsVisibleTo Include="Andy.TUI.Terminal.Tests" />
```

### Test Scenarios
1. **Tree Operations**: Insert, delete, rebalance
2. **Query Accuracy**: Range, point, nearest neighbor
3. **Occlusion**: Visibility calculations
4. **Performance**: Large dataset handling
5. **Edge Cases**: Overlapping elements, empty regions

## Advanced Features

### Dynamic Balancing
- Automatic tree restructuring
- Node splitting strategies
- Redistribution algorithms

### Spatial Hashing
- Grid-based acceleration
- Hybrid index structures
- Cache-friendly layouts

### Parallel Queries
- Thread-safe read operations
- Concurrent tree traversal
- Query result streaming

## Best Practices

### For UI Layout
1. Use appropriate Z-ordering
2. Minimize overlapping regions
3. Group related elements spatially
4. Consider viewport boundaries

### For Performance
1. Batch spatial updates
2. Use dirty region tracking
3. Implement view frustum culling
4. Cache query results when possible

### For Memory Usage
1. Tune node capacity
2. Implement element pooling
3. Clear unused spatial data
4. Use weak references where appropriate