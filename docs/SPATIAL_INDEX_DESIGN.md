# Spatial Index Design for UI Element Management

## Overview

This document outlines the design and implementation of a spatial index system to efficiently manage UI element positioning, overlap detection, and rendering optimization in the Andy.TUI framework.

## Problem Statement

The current linear approach for element management has several issues:
1. **O(n) overlap detection** - Must check every element against dirty regions
2. **No spatial awareness** - Cannot efficiently answer "what's at position (x,y)?"
3. **Inefficient invalidation** - When elements move, hard to determine what needs clearing
4. **Poor scalability** - Performance degrades with many UI elements

## Spatial Structure Choice: R-Tree

**Selected Approach**: R-Tree (Rectangle Tree)
- **Best for**: 2D rectangular regions (perfect for UI elements)
- **Complexity**: O(log n) insertion, deletion, and range queries
- **Memory**: Efficient spatial clustering reduces cache misses

### Why R-Tree over alternatives:
- **Quad-Tree**: Good for point data, less optimal for rectangles
- **Spatial Hash**: Fast but memory-intensive, poor for varying element sizes
- **Grid**: Simple but fixed resolution, bad for mixed element sizes

## Spatial Index Interface Design

```csharp
public interface ISpatialIndex<T>
{
    // Core operations
    void Insert(Rectangle bounds, T element);
    bool Remove(Rectangle bounds, T element);
    void Update(Rectangle oldBounds, Rectangle newBounds, T element);
    
    // Query operations
    IEnumerable<T> Query(Rectangle region);
    IEnumerable<T> QueryPoint(int x, int y);
    IEnumerable<T> QueryIntersecting(Rectangle region);
    
    // Bulk operations
    void Clear();
    void Rebuild();
}

public class UIElementSpatialIndex : ISpatialIndex<RenderedElement>
{
    private readonly RTree<RenderedElement> _rtree;
    
    // Implementation details...
}
```

## Detailed Spatial Index Testing Scenarios

### Phase 1: Core Spatial Operations ⏳

#### 1.1 Basic Element Management

**Test**: `RTree_Insert_SingleElement_DetailedScenario`

**Scenario**: Insert a single UI element into empty spatial index
```csharp
// Setup
var spatialIndex = new RTree<string>();
var element = "Button1";
var bounds = new Rectangle(10, 20, 50, 30); // x=10, y=20, w=50, h=30

// Action
spatialIndex.Insert(bounds, element);

// Verification - Element should be queryable
var pointQuery = spatialIndex.QueryPoint(25, 35); // Inside bounds
Assert.Contains(element, pointQuery);

var regionQuery = spatialIndex.Query(new Rectangle(0, 0, 100, 100)); // Overlapping region
Assert.Contains(element, regionQuery);

var outsideQuery = spatialIndex.QueryPoint(5, 5); // Outside bounds
Assert.DoesNotContain(element, outsideQuery);
```

**Expected R-Tree Internal State**:
- Root node contains single entry: (Rectangle(10,20,50,30), "Button1")
- Tree height: 1 (leaf level only)
- Node utilization: 1 element

---

**Test**: `RTree_Remove_ExistingElement_DetailedScenario`

**Initial State**: R-Tree with 3 elements
```
Element A: Rectangle(0, 0, 10, 10)   -> "ElementA"
Element B: Rectangle(15, 15, 10, 10) -> "ElementB"
Element C: Rectangle(30, 30, 10, 10) -> "ElementC"

R-Tree Structure:
Root Node: [Bounding: (0,0,40,40)]
  ├─ Entry A: Rectangle(0,0,10,10) -> "ElementA"
  ├─ Entry B: Rectangle(15,15,10,10) -> "ElementB"  
  └─ Entry C: Rectangle(30,30,10,10) -> "ElementC"
```

**Action**: Remove Element B
```csharp
bool removed = spatialIndex.Remove(new Rectangle(15, 15, 10, 10), "ElementB");
Assert.True(removed);
```

**Expected Result**:
- Element B no longer queryable at (15,15)
- Elements A and C still queryable
- Root bounding box recalculated: (0,0,40,40) (unchanged in this case)

**Verification Queries**:
```csharp
var queryB = spatialIndex.QueryPoint(20, 20); // Was Element B's center
Assert.DoesNotContain("ElementB", queryB);

var queryA = spatialIndex.QueryPoint(5, 5); // Element A still there
Assert.Contains("ElementA", queryA);

Assert.Equal(2, spatialIndex.Count); // Only 2 elements remain
```

#### 1.2 Element Position Updates

**Test**: `RTree_Update_ElementMovement_DetailedScenario`

**Initial State**: Single element at known position
```
Element: "MovableText" at Rectangle(5, 10, 15, 5)
  Occupies screen area: x=5-19, y=10-14
  
R-Tree: Root[Entry: (5,10,15,5) -> "MovableText"]
```

**Action**: Move element right by 10 pixels
```csharp
var oldBounds = new Rectangle(5, 10, 15, 5);
var newBounds = new Rectangle(15, 10, 15, 5); // Moved right
spatialIndex.Update(oldBounds, newBounds, "MovableText");
```

**Expected Process**:
1. **Remove Phase**: Find and remove old entry (5,10,15,5) -> "MovableText"
2. **Insert Phase**: Add new entry (15,10,15,5) -> "MovableText"
3. **Tree Rebalancing**: Update bounding boxes if necessary

**Verification**:
```csharp
// Old position should not find element
var oldQuery = spatialIndex.QueryPoint(10, 12); // Was inside old bounds
Assert.DoesNotContain("MovableText", oldQuery);

// New position should find element
var newQuery = spatialIndex.QueryPoint(20, 12); // Inside new bounds
Assert.Contains("MovableText", newQuery);

// Overlapping region query should find element
var overlapQuery = spatialIndex.Query(new Rectangle(10, 8, 20, 10));
Assert.Contains("MovableText", overlapQuery);
```

### Phase 2: Complex Movement and Overlap Scenarios ⏳

#### 2.1 Multi-Element Overlap Detection

**Test**: `RTree_OverlapDetection_ThreeElementScenario_Detailed`

**Initial State**: Three elements in specific layout
```
Terminal Grid Visualization (30x20):
012345678901234567890123456789
0 ······························
5 ·····AAAAA··········BBBBB·····
6 ·····AAAAA··········BBBBB·····
7 ·····AAAAA··········BBBBB·····
10 ··········CCC···················
11 ··········CCC···················
12 ··········CCC···················

Element Definitions:
- ElementA: Rectangle(5, 5, 5, 3) -> "ElementA"
- ElementB: Rectangle(20, 5, 5, 3) -> "ElementB"  
- ElementC: Rectangle(10, 10, 3, 3) -> "ElementC"
```

**R-Tree Structure**:
```
Root Node: [Bounding: (5,5,20,8)] // Union of all elements
  ├─ Entry A: (5,5,5,3) -> "ElementA"
  ├─ Entry B: (20,5,5,3) -> "ElementB"
  └─ Entry C: (10,10,3,3) -> "ElementC"
```

**Scenario**: Element A expands rightward, potentially overlapping Element C

**Action**: Update Element A to span columns 5-15 (width 5→11)
```csharp
var oldBounds = new Rectangle(5, 5, 5, 3);
var newBounds = new Rectangle(5, 5, 11, 3); // Expanded width
spatialIndex.Update(oldBounds, newBounds, "ElementA");
```

**Critical Overlap Analysis**:
- ElementA new bounds: (5,5,11,3) spans x=5-15, y=5-7
- ElementC bounds: (10,10,3,3) spans x=10-12, y=10-12
- **No overlap**: ElementA ends at y=7, ElementC starts at y=10

**Overlap Detection Queries**:
```csharp
// Query region that covers ElementA's expansion path
var expansionRegion = new Rectangle(5, 5, 11, 8); // Covers A's path + buffer
var affectedElements = spatialIndex.Query(expansionRegion);

// Should return: ElementA (obviously) and potentially ElementC if they intersect
Assert.Contains("ElementA", affectedElements);

// Verify ElementC intersection
var elementCBounds = new Rectangle(10, 10, 3, 3);
var elementANewBounds = new Rectangle(5, 5, 11, 3);
bool shouldIntersect = elementANewBounds.IntersectsWith(elementCBounds);
Assert.False(shouldIntersect); // No intersection in this case

if (shouldIntersect)
    Assert.Contains("ElementC", affectedElements);
else
    Assert.DoesNotContain("ElementC", affectedElements);
```

#### 2.2 Column Shift Detection (MultiSelectInput Scenario)

**Test**: `RTree_ColumnExpansion_MultiElementShift_DetailedScenario`

**Initial State**: Three-column layout (mimics MultiSelectInput)
```
Terminal Layout (60 chars):
012345678901234567890123456789012345678901234567890123456789
2 Programming···Favorite Colors···Lucky Numbers·····

Elements in Spatial Index:
- Column1: Rectangle(0, 2, 11, 1) -> "ProgrammingColumn"
- Column2: Rectangle(15, 2, 14, 1) -> "ColorsColumn"
- Column3: Rectangle(32, 2, 12, 1) -> "NumbersColumn"

R-Tree Structure:
Root [Bounding: (0,2,44,1)]
  ├─ (0,2,11,1) -> "ProgrammingColumn"
  ├─ (15,2,14,1) -> "ColorsColumn"
  └─ (32,2,12,1) -> "NumbersColumn"
```

**User Action**: Add item to Programming column, expanding its width

**Layout Change**: Column 1 width increases from 11 to 17 characters
```
Expected New Layout:
012345678901234567890123456789012345678901234567890123456789
2 Programming······Favorite Colors···Lucky Numbers
3 Machine Learning····················

New Element Positions:
- Column1: Rectangle(0, 2, 17, 2) -> "ProgrammingColumn" (expanded)
- Column2: Rectangle(20, 2, 14, 1) -> "ColorsColumn" (shifted right)
- Column3: Rectangle(37, 2, 12, 1) -> "NumbersColumn" (shifted right)
```

**Spatial Index Operations**:

1. **Update Column1** (width expansion):
```csharp
var col1Old = new Rectangle(0, 2, 11, 1);
var col1New = new Rectangle(0, 2, 17, 2); // Width and height increased
spatialIndex.Update(col1Old, col1New, "ProgrammingColumn");
```

2. **Update Column2** (rightward shift):
```csharp
var col2Old = new Rectangle(15, 2, 14, 1);
var col2New = new Rectangle(20, 2, 14, 1); // Shifted right by 5
spatialIndex.Update(col2Old, col2New, "ColorsColumn");
```

3. **Update Column3** (rightward shift):
```csharp
var col3Old = new Rectangle(32, 2, 12, 1);
var col3New = new Rectangle(37, 2, 12, 1); // Shifted right by 5
spatialIndex.Update(col3Old, col3New, "NumbersColumn");
```

**Critical Overlap Detection**:

After Column1 expansion, need to identify all affected elements:
```csharp
// Find all elements that intersect with the expansion influence area
var influenceArea = Rectangle.Union(
    col1Old,  // (0,2,11,1) - old position
    new Rectangle(0, 2, 45, 2) // Potential new layout area
);

var affectedElements = spatialIndex.Query(influenceArea);
// Should return: ["ProgrammingColumn", "ColorsColumn", "NumbersColumn"]

// For each affected element, determine if it needs to move
foreach (var element in affectedElements)
{
    if (element != "ProgrammingColumn") // Skip the expanding element
    {
        var elementBounds = GetElementBounds(element);
        if (elementBounds.IntersectsWith(col1New))
        {
            // This element needs to be repositioned
            MarkForRepositioning(element);
        }
    }
}
```

**Expected Query Results**:
- **Before updates**: Query(0,2,45,2) returns all 3 columns at old positions
- **After Column1 update**: Query finds Column1 at new position, others at old positions
- **After all updates**: Query finds all columns at new positions with no overlaps

### Phase 3: Performance Benchmark Scenarios ⏳

#### 3.1 Linear vs Spatial Comparison

**Test**: `Benchmark_OverlapDetection_100Elements_DetailedScenario`

**Setup**: Create 100 UI elements in 10x10 grid layout
```csharp
// Generate 100 elements in predictable grid
var elements = new List<(Rectangle bounds, string id)>();
for (int row = 0; row < 10; row++)
{
    for (int col = 0; col < 10; col++)
    {
        var bounds = new Rectangle(col * 12, row * 5, 10, 4);
        var id = $"Element_{row}_{col}";
        elements.Add((bounds, id));
    }
}

// Total layout: 120x50 terminal area with 100 elements
```

**Linear Approach Simulation**:
```csharp
// Simulate current VirtualDomRenderer approach
var linearSearchTime = MeasureTime(() => {
    var queryRegion = new Rectangle(25, 12, 30, 15); // Query overlapping region
    var results = new List<string>();
    
    // O(n) - check every element
    foreach (var (bounds, id) in elements)
    {
        if (bounds.IntersectsWith(queryRegion))
            results.Add(id);
    }
    return results;
});
```

**Spatial Index Approach**:
```csharp
// R-Tree spatial query
var spatialQueryTime = MeasureTime(() => {
    var queryRegion = new Rectangle(25, 12, 30, 15);
    return spatialIndex.Query(queryRegion).ToList(); // O(log n)
});
```

**Performance Expectations**:
- **Linear**: ~100 intersection checks (O(n))
- **Spatial**: ~7-10 node visits (O(log n)) for balanced R-Tree
- **Expected Speedup**: 5-10x improvement for 100 elements
- **Scalability**: Gap widens significantly with more elements (1000+)

**Verification**: Both approaches must return identical results
```csharp
Assert.Equal(linearResults.OrderBy(x => x), spatialResults.OrderBy(x => x));
Assert.True(spatialQueryTime < linearSearchTime); // Performance improvement
```

This detailed breakdown shows exactly how the spatial index will work in practice and what performance gains we can expect.

## Integration with VirtualDomRenderer

### Current Architecture
```
VirtualDomRenderer
├── _renderedElements: Dictionary<int[], RenderedElement>
├── _dirtyRegionTracker: DirtyRegionTracker  
└── RenderDirtyRegions(): void
```

### New Spatial Architecture
```
VirtualDomRenderer
├── _renderedElements: Dictionary<int[], RenderedElement>  [unchanged]
├── _spatialIndex: ISpatialIndex<RenderedElement>          [NEW]
├── _dirtyRegionTracker: DirtyRegionTracker                [enhanced]
└── RenderDirtyRegions(): void                             [optimized]
```

### Enhanced Patch Application

**Before (Linear)**:
```csharp
public void VisitUpdateProps(UpdatePropsPatch patch)
{
    // Mark old position dirty
    _dirtyRegionTracker.MarkDirty(oldBounds);
    
    // Update element properties
    UpdateElementProps(element, patch);
    
    // Mark new position dirty
    _dirtyRegionTracker.MarkDirty(newBounds);
}
```

**After (Spatial)**:
```csharp
public void VisitUpdateProps(UpdatePropsPatch patch)
{
    var oldBounds = GetElementBounds(element);
    
    // Update spatial index
    _spatialIndex.Remove(oldBounds, element);
    UpdateElementProps(element, patch);
    var newBounds = GetElementBounds(element);
    _spatialIndex.Insert(newBounds, element);
    
    // Mark affected regions for clearing/redrawing
    _dirtyRegionTracker.MarkDirty(oldBounds);  // Clear old
    _dirtyRegionTracker.MarkDirty(newBounds);  // Draw new
    
    // Find and mark overlapping elements that need redrawing
    var affectedElements = _spatialIndex.QueryIntersecting(
        Rectangle.Union(oldBounds, newBounds));
    foreach (var affected in affectedElements)
    {
        if (affected != element)
            _dirtyRegionTracker.MarkDirty(GetElementBounds(affected));
    }
}
```

### Enhanced Dirty Region Rendering

**Before (Linear Search)**:
```csharp
private void RenderDirtyRegions()
{
    foreach (var region in _dirtyRegionTracker.GetDirtyRegions())
    {
        // Clear dirty region
        _renderingSystem.FillRect(region.X, region.Y, region.Width, region.Height, ' ');
        
        // Linear search through ALL elements
        if (_rootElement != null)
            RenderElementsInRegion(_rootElement, region);  // O(n)
    }
}
```

**After (Spatial Query)**:
```csharp
private void RenderDirtyRegions()
{
    foreach (var region in _dirtyRegionTracker.GetDirtyRegions())
    {
        // Clear dirty region  
        _renderingSystem.FillRect(region.X, region.Y, region.Width, region.Height, ' ');
        
        // Spatial query for intersecting elements - O(log n)
        var elementsToRender = _spatialIndex.QueryIntersecting(region);
        foreach (var element in elementsToRender)
        {
            RenderElement(element, element.X, element.Y);
        }
    }
}
```

## Implementation Phases

### Phase 1: Z-Index Architecture Foundation 🆕 ⏳
**Priority**: HIGH - Required before spatial index integration
**Files to Create**:
- `/src/Andy.TUI.Core/Rendering/ZIndexContext.cs` - Stack-based z-index resolver
- `/src/Andy.TUI.Core/Rendering/IZIndexAware.cs` - Interface for z-aware components
- `/src/Andy.TUI.Declarative/Components/TabView.cs` - Tab container component
- `/src/Andy.TUI.Declarative/Components/Tab.cs` - Individual tab component
- `/src/Andy.TUI.Declarative/ViewInstances/TabViewInstance.cs` - Tab runtime management

**Files to Modify**:
- `/src/Andy.TUI.Declarative/ViewInstance.cs` - Add RelativeZIndex, AbsoluteZIndex
- `/src/Andy.TUI.Terminal/Rendering/VirtualDomRenderer.cs` - Use absolute z-index

### Phase 2: Enhanced 3D R-Tree Implementation ⏳
**Files to Create**:
- `/src/Andy.TUI.Core/Spatial/Rectangle.cs` ✅ (already exists)
- `/src/Andy.TUI.Core/Spatial/ISpatialIndex.cs` ✅ (already exists)
- `/src/Andy.TUI.Core/Spatial/I3DSpatialIndex.cs` - Enhanced interface with z-support
- `/src/Andy.TUI.Core/Spatial/Enhanced3DRTree.cs` - R-Tree with z-index operations
- `/src/Andy.TUI.Core/Spatial/SpatialElement.cs` - Element wrapper with z-metadata
- `/tests/Andy.TUI.Core.Tests/Spatial/Enhanced3DRTreeTests.cs`

### Phase 3: Z-Aware VirtualDomRenderer Integration ⏳
**Files to Modify**:
- `/src/Andy.TUI.Terminal/Rendering/VirtualDomRenderer.cs`
  - Integrate Enhanced3DRTree
  - Use absolute z-index for rendering order
  - Add UpdateZIndex operation support
  - Implement occlusion-aware rendering

### Phase 4: TabView Testing and Optimization ⏳
**Files to Create**:
- `/tests/Andy.TUI.Declarative.Tests/TabViewTests.cs`
- `/tests/Andy.TUI.Declarative.Tests/ZIndexResolutionTests.cs`
- `/src/Andy.TUI.Terminal/Rendering/TabSwitchOptimizer.cs`

### Phase 5: Performance Testing with Z-Index ⏳
**Files to Create**:
- `/tests/Andy.TUI.Terminal.Tests/SpatialPerformanceTests.cs`
- `/tests/Andy.TUI.Terminal.Tests/ZIndexPerformanceTests.cs`
- Benchmark z-only updates vs full spatial updates
- Measure tab switch performance

### Phase 6: Real-World Testing ⏳
**Scenarios to Test**:
- MultiSelectInput with z-aware spatial indexing
- TabView inside Modal (nested z-index resolution)
- Multiple overlapping modals with focus changes
- Complex nested TabViews
- Performance with 100+ layered elements

## Success Criteria

### Correctness Criteria
- [ ] **All movement tests pass**: 1D, 2D, and diagonal movement scenarios
- [ ] **Z-index resolution correct**: Relative + absolute z-indices compute properly
- [ ] **TabView behavior**: Tab selection changes z-order without position artifacts
- [ ] **Real-world fix**: MultiSelectInput duplication issue resolved

### Performance Criteria  
- [ ] **Spatial query performance**: O(log n) for 2D range queries vs O(n) linear
- [ ] **Z-index query performance**: O(log n + k) where k = elements in z-range
- [ ] **Z-only updates**: UpdateZIndex faster than remove+insert cycle
- [ ] **Tab switch performance**: <1ms for typical tab selection
- [ ] **Occlusion optimization**: Skip rendering completely occluded elements

### Architecture Criteria
- [ ] **Clean separation**: Spatial, z-index, and rendering logic properly isolated
- [ ] **Component abstraction**: TabView hides z-index complexity from users
- [ ] **Extensibility**: Easy to add new z-aware components (tooltips, dropdowns, etc.)

## Migration Strategy

1. **Implement R-Tree alongside existing system** (no breaking changes)
2. **Add feature flag** to switch between linear and spatial approaches
3. **Run parallel testing** to ensure correctness
4. **Performance comparison** to validate improvements
5. **Full migration** once validation complete

## Current Status

**Overall**: 15% Complete - Enhanced design phase
- ✅ **Basic Spatial Framework**: Rectangle, ISpatialIndex interface created
- ⏳ **Enhanced 3D R-Tree**: Design complete, implementation not started  
- ⏳ **Z-Index Integration**: Architecture designed, not implemented
- ⏳ **Occlusion Optimization**: Algorithms designed, not implemented
- ⏳ **2D Movement Patterns**: Test scenarios designed, not implemented
- ⏳ **3D Test Suite Creation**: Comprehensive scenarios designed, not implemented

**Next Immediate Steps**:
1. **Create RTree.cs** - Core R-Tree implementation with node management
2. **Implement ISpatialIndex interface** - Wrapper around R-Tree for UI elements
3. **Create comprehensive SpatialIndexTests.cs** - All detailed test scenarios above
4. **Benchmark implementation** - Validate O(log n) vs O(n) performance gains
5. **Integrate with VirtualDomRenderer** - Replace linear searches with spatial queries
6. **Test with MultiSelectInput example** - Verify real-world bug fix

**Detailed Implementation Order**:
1. `Rectangle.cs` - Core spatial primitive (✅ Already exists)
2. `ISpatialIndex<T>.cs` - Interface definition (✅ Already exists)
3. `RTreeNode.cs` - Internal tree node structure
4. `RTree<T>.cs` - Main R-Tree implementation
5. `SpatialIndexTests.cs` - Comprehensive test coverage
6. `VirtualDomRenderer.cs` integration - Replace linear with spatial queries
7. Real-world validation with MultiSelectInput test case