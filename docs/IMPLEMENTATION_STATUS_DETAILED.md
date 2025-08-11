# Detailed Implementation Status and Test Scenarios

## Overview

This document tracks the detailed implementation status for all UI element movement, overlap, and spatial indexing scenarios in the Andy.TUI framework.

## Single Element Operation Test Cases

### 1. Position Movement Scenarios

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Single Element Move Right** | 🚫 FAILING | `DiffEngineMovementTests.cs:27` | Text moves from (5,2) to (8,2), old position cleared |
| **Single Element Move Down** | 🚫 NOT TESTED | - | Text moves from (5,2) to (5,4), old position cleared |
| **Single Element Diagonal Move** | 🚫 NOT TESTED | - | Text moves from (5,2) to (10,5), old position cleared |

#### Detailed Scenario: Single Element Move Right

**Initial State**:
```
Terminal Grid (20x5):
01234567890123456789
2 •••••Hello•••••••••
```

**Action**: Move text right by 3 positions (x: 5→8)

**Current Bug**: Results in duplication
```
2 •••••Hello••Hello•••  ← DUPLICATION
```

**Expected Result**:
```
2 ••••••••Hello•••••••  ← Clean movement
```

**Root Cause**: `VirtualDomRenderer` applies patches via both an element traversal path and a visitor path. In `VisitUpdateProps`, old regions are not reliably cleared before drawing, which, combined with dual paths, causes duplication. Renderer unification is planned to resolve this.

### 2. Element Resize Scenarios

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Content Expansion** | 🚫 NOT TESTED | - | "Hi" expands to "Hello World", old area cleared |
| **Content Shrinking** | 🚫 NOT TESTED | - | "Hello World" shrinks to "Hi", extra chars cleared |
| **Width Expansion** | 🚫 NOT TESTED | - | Element width increases, old boundaries cleared |
| **Height Expansion** | 🚫 NOT TESTED | - | Element height increases, affects vertical layout |

#### Detailed Scenario: Content Expansion

**Initial State**:
```
2 •••••Hi•••••••••••••
```

**Action**: Content changes to "Hello World" (width: 2→11)

**Critical**: Must clear full expanded region, not just old size

**Expected Process**:
1. Detect content change via UpdateTextPatch
2. Calculate new content bounds: (5,2,11x1)
3. Clear full new region: (5,2,11x1)
4. Render "Hello World" at (5,2)

**Expected Result**:
```
2 •••••Hello World••••
```

## Two Element Interaction Test Cases

### 1. Non-Overlapping Movement (1D Scenarios)

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Both Move Right** | 🚫 NOT TESTED | - | Two elements move independently, no interference |
| **One Up, One Down** | 🚫 NOT TESTED | - | Vertical movement with horizontal elements |
| **Parallel Movement** | 🚫 NOT TESTED | - | Multiple elements move in same direction |

#### Detailed Scenario: Both Elements Move Right

**Initial State**:
```
2 ••AAA•••••BBB••••••
```
- ElementA: "AAA" at (2,2), size (3x1)
- ElementB: "BBB" at (10,2), size (3x1)

**Action**: Both move right by 2 positions

**Expected Process**:
1. ElementA: (2,2) → (4,2)
2. ElementB: (10,2) → (12,2)
3. Clear old positions: (2,2,3x1) and (10,2,3x1)
4. Render at new positions

**Expected Result**:
```
2 ••••AAA•••••BBB••••
```

### 2. Two-Dimensional Movement Patterns

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|  
| **Diagonal Element Movement** | 🚫 NOT TESTED | - | Element moves diagonally, clearing 2D path |
| **Grid Element Swap** | 🚫 NOT TESTED | - | Two elements swap positions in 2D grid |
| **Radial Expansion** | 🚫 NOT TESTED | - | Element expands outward in all directions |
| **Circular Movement Pattern** | 🚫 NOT TESTED | - | Elements move in circular/spiral patterns |
| **2D Table Cell Migration** | 🚫 NOT TESTED | - | Table cells shift both horizontally and vertically |

#### Detailed Scenario: Diagonal Element Movement

**Initial State**:
```
Terminal Grid (15x8):
012345678901234
0 •••••••••••••••
1 •••••••••••••••  
2 ••AAA•••••••••• 
3 •••••••••••••••
4 •••••••••••••••
5 •••••••••••••••
6 •••••••••••••••
7 •••••••••••••••
```
- ElementA: "AAA" at (2,2), size (3x1)

**Action**: Move diagonally to (7,5) - both X and Y change

**2D Path Analysis**:
- Old position: (2,2,3x1) 
- New position: (7,5,3x1)
- **No overlap** between old and new rectangles
- **Path clearing**: Need to clear old rectangle completely
- **2D spatial query**: Check for elements in diagonal path

**Expected Process**:
1. UpdatePropsPatch: x=2→7, y=2→5
2. Calculate 2D movement vector: (+5, +3)
3. Clear old region: (2,2,3x1)
4. Render at new position: (7,5,3x1)

**Expected Final State**:
```
012345678901234
0 •••••••••••••••
1 •••••••••••••••  
2 •••••••••••••••  ← Old position cleared
3 •••••••••••••••
4 •••••••••••••••
5 •••••••AAA••••• ← New position
6 •••••••••••••••
7 •••••••••••••••
```

#### Detailed Scenario: 2x2 Grid Element Swap

**Initial State**:
```
012345678901234
2 •A••B••••••••••
3 •••••••••••••••
4 •C••D••••••••••

Grid Layout:
  Col1  Col2
Row1: A(1,2) B(4,2)
Row2: C(1,4) D(4,4)
```

**Action**: Swap A↔D diagonally (complex 2D operation)
- A: (1,2) → (4,4)
- D: (4,4) → (1,2)

**2D Spatial Complexity**:
- **Overlapping paths**: A's destination overlaps D's origin
- **Timing dependency**: Must clear both old positions before rendering new
- **No intermediate collision**: A and D don't collide during movement

**Expected Process**:
1. Two UpdatePropsPatch operations simultaneously
2. Clear both old positions: (1,2,1x1) and (4,4,1x1)
3. Render both at new positions

**Expected Final State**:
```
2 •D••B••••••••••
3 •••••••••••••••
4 •A••C••••••••••
```

### 3. Overlapping Movement (Complex 2D)

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Elements Cross Paths** | 🚫 NOT TESTED | - | Elements move past each other, proper z-order handling |
| **Partial Overlap** | 🚫 NOT TESTED | - | Elements partially overlap, correct rendering order |
| **Complete Overlap** | 🚫 NOT TESTED | - | One element completely covers another |

#### Detailed Scenario: Elements Cross Paths

**Initial State**:
```
2 ••AAA•••••BBB••••••
```

**Action**: 
- ElementA moves right to (8,2) [will occupy cols 8-10]
- ElementB moves left to (6,2) [will occupy cols 6-8]

**Critical Analysis**:
- Overlap at column 8
- Z-order determines final render result
- Both old positions must be cleared

**Expected Process**:
1. Clear ElementA old: (2,2,3x1)
2. Clear ElementB old: (10,2,3x1)
3. Render with z-order: depends on element priority

**Possible Results** (z-order dependent):
```
# ElementA higher z-index:
2 ••••••BBAAAA••••••

# ElementB higher z-index:
2 ••••••BBBAA•••••••
```

### 3. Chain Reaction Movement

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Element Expansion Pushes Others** | 🚫 NOT TESTED | - | First element expands, others shift to avoid overlap |
| **Cascading Layout Changes** | 🚫 NOT TESTED | - | One change triggers multiple element repositioning |
| **Container Resize Effects** | 🚫 NOT TESTED | - | Parent element resize affects all children positions |

#### Detailed Scenario: Element Expansion Chain Reaction

**Initial State**:
```
2 ••AAA•BBB••••••••••
```
- ElementA: "AAA" at (2,2) [cols 2-4]
- ElementB: "BBB" at (6,2) [cols 6-8]

**Action**: ElementA expands to "AAAAA" (width 3→5)

**Expected Cascade**:
1. ElementA expansion will occupy cols 2-6
2. Collision detected with ElementB at col 6
3. ElementB must shift right: (6,2) → (7,2)

**Expected Process**:
1. ElementA content change → UpdateTextPatch
2. Layout system detects ElementB collision
3. ElementB position change → UpdatePropsPatch
4. Clear old regions: (2,2,3x1), (6,2,3x1)
5. Render: "AAAAA" at (2,2), "BBB" at (7,2)

**Expected Result**:
```
2 ••AAAAA•BBB••••••••
```

## Nested Element Hierarchy Test Cases

### 1. Parent-Child Movement

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Parent Moves, Children Follow** | 🚫 NOT TESTED | - | Container movement cascades to all child elements |
| **Child Expands Within Parent** | 🚫 NOT TESTED | - | Child growth constrained by parent boundaries |
| **Nested Container Resize** | 🚫 NOT TESTED | - | Multiple levels of parent-child relationships |

#### Detailed Scenario: Parent Container Movement

**Initial DOM Structure**:
```
Container(x=2, y=1, width=8, height=3)
  ├─ HeaderText("Title") at relative (1,0) → absolute (3,1)
  └─ BodyText("Content") at relative (1,1) → absolute (3,2)
```

**Initial Terminal Display**:
```
01234567890123456789
1 •••Title•••••••••••
2 •••Content•••••••••
```

**Action**: Container moves right by 3 positions (x: 2→5)

**Expected Cascading Updates**:
1. Container: (2,1) → (5,1)
2. HeaderText absolute: (3,1) → (6,1)
3. BodyText absolute: (3,2) → (6,2)

**Expected Process**:
1. UpdatePropsPatch for Container: x=2→5
2. Child position recalculation
3. Clear old regions: (3,1,5x1), (3,2,7x1)
4. Render at new positions: (6,1), (6,2)

**Expected Result**:
```
01234567890123456789
1 ••••••Title••••••••
2 ••••••Content••••••
```

### 2. Sibling Interaction Within Container

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Child Resize Affects Siblings** | 🚫 NOT TESTED | - | One child expansion pushes siblings within container |
| **HStack Child Expansion** | 🚫 NOT TESTED | - | Horizontal stack recalculates child positions |
| **VStack Child Expansion** | 🚫 NOT TESTED | - | Vertical stack recalculates child positions |

#### Detailed Scenario: HStack Child Expansion

**Initial DOM Structure**:
```
HStack(x=1, y=2)
  ├─ Label1("A") → (1,2)
  ├─ Label2("B") → (2,2)
  └─ Label3("C") → (3,2)
```

**Initial Terminal Display**:
```
2 •ABC•••••••••••••••
```

**Action**: Label1 content expands "A" → "EXPANDED" (width 1→8)

**Expected Layout Recalculation**:
1. Label1: (1,2) width 1→8, stays at (1,2)
2. Label2: shifts right (2,2) → (9,2)
3. Label3: shifts right (3,2) → (10,2)

**Expected Process**:
1. UpdateTextPatch for Label1 content
2. HStack layout recalculation
3. UpdatePropsPatch for Label2: x=2→9
4. UpdatePropsPatch for Label3: x=3→10
5. Clear old regions: (1,2,1x1), (2,2,1x1), (3,2,1x1)
6. Render at new positions

**Expected Result**:
```
2 •EXPANDED•BC••••••••
```

## Z-Index Layering Scenarios (3D Rendering)

### 1. Occlusion and Layered Rendering

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|  
| **Fully Occluded Element Changes** | 🚫 NOT TESTED | - | Skip rendering completely covered elements |
| **Partially Occluded Element Updates** | 🚫 NOT TESTED | - | Render only visible portions of covered elements |
| **Z-Order Element Revelation** | 🚫 NOT TESTED | - | Moving top element reveals elements underneath |
| **Modal Dialog Over Content** | 🚫 NOT TESTED | - | Dialog movement affects underlying content visibility |
| **Stacked Panel Management** | 🚫 NOT TESTED | - | Multiple overlapping panels with focus changes |

#### Detailed Scenario: Fully Occluded Element Optimization

**Initial State** (3D layered view):
```
2D Terminal View:
012345678901234
2 •••••MODAL••••• 
3 •••••MODAL•••••
4 •••••MODAL•••••

Z-Index Layer Analysis:
Z=20: Modal Dialog at (5,2,5x3) - "MODAL" 
Z=10: Button at (6,3,3x1) - "BTN" (COMPLETELY COVERED)
Z=5:  Background at (7,3,4x1) - "BACK" (COMPLETELY COVERED)

Actual Rendered Result: Only Modal visible
```

**Action**: Background element content changes "BACK" → "BACKGROUND"

**Critical Optimization Question**: 
Should we render the Background element at all since it's completely occluded?

**Traditional Approach** (wasteful):
1. UpdateTextPatch for Background element
2. Clear Background old region: (7,3,4x1)
3. Render "BACKGROUND" at (7,3)
4. **But it's completely covered by Modal!** - Wasted rendering

**Z-Index Optimized Approach**:
1. UpdateTextPatch for Background element 
2. **Occlusion check**: Query spatial index for elements with higher z-index at same region
3. **Find**: Modal (z=20) completely covers Background (z=5)
4. **Skip rendering**: Background change has no visual effect
5. **Update internal state only**: Keep Background's new content for later revelation

**Expected Process**:
```csharp
public void VisitUpdateText(UpdateTextPatch patch)
{
    var element = GetElement(patch.Path);
    var elementBounds = GetElementBounds(element);
    
    // Update element content
    element.UpdateContent(patch.NewText);
    
    // Z-Index Occlusion Check
    var occludingElements = _spatialIndex.Query(elementBounds)
        .Where(e => e.ZIndex > element.ZIndex && 
                   GetElementBounds(e).Contains(elementBounds));
    
    if (occludingElements.Any())
    {
        // Element is completely occluded - skip rendering
        // But keep internal state updated for potential future revelation
        return; 
    }
    
    // Element is visible - proceed with normal rendering
    _dirtyRegionTracker.MarkDirty(elementBounds);
}
```

#### Detailed Scenario: Z-Order Element Revelation

**Initial State**:
```
2D View:
012345678901234
2 •••••MODAL••••• 
3 •••••MODAL•••••

Z-Layers:
Z=20: Modal at (5,2,5x2) - "MODAL"
Z=10: Hidden Button at (6,2,3x1) - "BTN" (covered)
Z=5:  Hidden Text at (7,3,4x1) - "TEXT" (covered) 
```

**Action**: Modal moves right by 3 positions (5,2) → (8,2)

**Z-Index Revelation Process**:
1. Modal UpdatePropsPatch: x=5→8
2. **Revelation Detection**: Query elements with lower z-index in Modal's old region
3. **Found**: Button (z=10) and Text (z=5) were previously covered
4. **Mark for revelation**: Button and Text regions need rendering

**Expected Final State**:
```
2D View:
012345678901234
2 ••••••BTN•MODAL 
3 •••••••TEXT•••••

Z-Layers now visible:
Z=20: Modal at (8,2,5x2) - "MODAL" (moved)
Z=10: Button at (6,2,3x1) - "BTN" (now visible!)
Z=5:  Text at (7,3,4x1) - "TEXT" (now visible!)
```

**Expected Process**:
```csharp
public void VisitUpdateProps(UpdatePropsPatch patch)
{
    var element = GetElement(patch.Path);
    var oldBounds = GetElementBounds(element);
    
    // Update element position
    UpdateElementProps(element, patch);
    var newBounds = GetElementBounds(element);
    
    // Standard spatial index update
    _spatialIndex.Update(oldBounds, newBounds, element);
    
    // Mark moved element's regions for rendering
    _dirtyRegionTracker.MarkDirty(oldBounds);  // Clear old
    _dirtyRegionTracker.MarkDirty(newBounds);  // Draw new
    
    // Z-INDEX REVELATION CHECK
    // Find elements that were previously occluded by this element
    var revealedElements = _spatialIndex.Query(oldBounds)
        .Where(e => e.ZIndex < element.ZIndex);
    
    foreach (var revealedElement in revealedElements)
    {
        var revealedBounds = GetElementBounds(revealedElement);
        var intersection = Rectangle.Intersect(oldBounds, revealedBounds);
        
        if (!intersection.IsEmpty)
        {
            // This element is now partially/fully revealed
            _dirtyRegionTracker.MarkDirty(revealedBounds);
        }
    }
}
```

### 2. Complex Z-Index Interaction Scenarios

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|  
| **Cascading Z-Order Changes** | 🚫 NOT TESTED | - | Element z-index change affects multiple layer visibility |
| **Transparent Element Layering** | 🚫 NOT TESTED | - | Semi-transparent elements require blended rendering |
| **Window Focus Z-Order Management** | 🚫 NOT TESTED | - | Focus changes reorder z-indices of multiple elements |

## Multi-Column Table Scenarios (Real-World)

### 1. MultiSelectInput Column Expansion (Core Bug)

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Three Column Expansion** | 🚫 FAILING | Example 16 MultiSelectInputTest | First column expands, others shift without duplication |
| **Dynamic Width Calculation** | 🚫 NOT TESTED | - | Column widths recalculate based on content |
| **Overflow Handling** | 🚫 NOT TESTED | - | Content exceeding terminal width handled gracefully |

#### Detailed Scenario: Three-Column Expansion (The Core Issue)

**Initial State** (MultiSelectInput with 3 columns):
```
Terminal Grid (50 chars wide):
01234567890123456789012345678901234567890123456789
2 Programming••••Favorite Colors••••Lucky Numbers••
  ↑col1 (11chars) ↑col2 (15chars)   ↑col3 (13chars)
  positions:      positions:        positions:
  0-10           15-29             34-46
```

**User Action**: Add "Machine Learning" to Programming column

**Content Change**:
- Column 1: "Programming" → "Programming\nMachine Learning"
- Column width: 11 → 16 chars (longest line)

**Expected Layout Recalculation**:
1. Column 1: x=0, width 11→16
2. Column 2: x=15→21 (0+16+5 padding)
3. Column 3: x=34→42 (21+15+6 padding)

**Critical Clearing Requirements**:
- Column 2 old area (15-29) must be **completely cleared**
- Column 3 old area (34-46) must be **completely cleared**
- New content rendered at shifted positions

**Expected Process**:
1. Column 1 content change → UpdateTextPatch
2. Layout recalculation → UpdatePropsPatch for columns 2&3
3. **CRITICAL CLEARING PHASE**:
   - Clear Column 2 old: (15,2,15x1) with spaces
   - Clear Column 3 old: (34,2,13x1) with spaces
4. **RENDERING PHASE**:
   - Render Column 1 expanded at (0,2)
   - Render Column 2 at (21,2)
   - Render Column 3 at (42,2)

**Expected Final State**:
```
01234567890123456789012345678901234567890123456789
2 Programming•••••Favorite Colors••••Lucky Numbers••
3 Machine Learning•••••••••••••••••••••••••••••••••
```

**Current Bug Result** (FAILING):
```
01234567890123456789012345678901234567890123456789
2 Programming•••••Favorite ColorsFavorite Colors••••Lucky NumbersLucky Numbers••
3 Machine Learning•••••••••••••••••••••••••••••••••
                       ↑ DUPLICATION - OLD TEXT NOT CLEARED
```

## Advanced 2D and 3D Spatial Index Integration

### 1. Multi-Dimensional Spatial Operations

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|  
| **2D Range Queries** | ⏳ PLACEHOLDER | - | Efficiently find all elements in rectangular region |
| **3D Z-Index Queries** | 🚫 NOT IMPLEMENTED | - | Query elements by region AND z-index range |
| **Occlusion Detection Queries** | 🚫 NOT IMPLEMENTED | - | Find all elements occluded by a given element |
| **Revelation Impact Analysis** | 🚫 NOT IMPLEMENTED | - | Determine rendering impact when element moves/resizes |

### 2. R-Tree Core Operations (Enhanced)

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Insert Single Element** | ⏳ PLACEHOLDER | SpatialIndexTests.cs:98 | Element queryable at inserted position |
| **Remove Element** | ⏳ PLACEHOLDER | SpatialIndexTests.cs:114 | Element no longer queryable after removal |
| **Update Element Position** | ⏳ PLACEHOLDER | SpatialIndexTests.cs:121 | Element queryable at new position only |
| **Range Query** | ⏳ PLACEHOLDER | SpatialIndexTests.cs:128 | Returns all intersecting elements |
| **Point Query** | ⏳ PLACEHOLDER | SpatialIndexTests.cs:135 | Returns elements containing point |

### 2. Enhanced Spatial Performance Benchmarks

| Test Case | Status | Implementation File | Expected Performance |
|-----------|--------|-------------------|---------------------|
| **2D Movement Pattern Performance** | ⏳ NOT IMPLEMENTED | - | Complex diagonal/circular movements |
| **Z-Index Occlusion Query Performance** | ⏳ NOT IMPLEMENTED | - | O(log n) occlusion detection vs O(n²) naive |
| **100 Elements with Z-Layers** | ⏳ NOT IMPLEMENTED | - | 3D spatial queries with layering |
| **1000 Elements Multi-Layer** | ⏳ NOT IMPLEMENTED | - | Complex layered UI performance |
| **Bulk Z-Order Operations** | ⏳ NOT IMPLEMENTED | - | Efficient batch z-index updates |

### 3. Integration with VirtualDomRenderer

| Test Case | Status | Implementation File | Expected Result |
|-----------|--------|-------------------|-----------------|
| **Spatial Query in RenderDirtyRegions** | ⏳ NOT IMPLEMENTED | - | Replace linear search with O(log n) spatial query |
| **Overlap Detection in VisitUpdateProps** | ⏳ NOT IMPLEMENTED | - | Efficient detection of elements affected by position changes |
| **Multi-Element Movement Optimization** | ⏳ NOT IMPLEMENTED | - | Batch spatial operations for complex layout changes |

## Implementation Priority and Dependencies

### Phase 1: Fix Current Linear Approach ⚠️ 
**Status**: ATTEMPTED BUT FAILING
- **Issue**: VirtualDomRenderer clearing operations not working
- **Files**: `VirtualDomRenderer.cs:275` (VisitUpdateProps method)
- **Tests**: All movement tests in `DiffEngineMovementTests.cs` failing

### Phase 2: Spatial Index Implementation ⏳
**Status**: IN PROGRESS
- **Current**: Basic framework created (ISpatialIndex, Rectangle, test structure)
- **Next**: Implement R-Tree core functionality
- **Files**: Need to create `RTree.cs`, `RTreeNode.cs`

### Phase 3: Integration and Validation ⏳
**Status**: PENDING PHASE 2
- **Target**: Replace linear searches with spatial queries
- **Validation**: All test cases above must pass
- **Real-world test**: MultiSelectInput duplication issue resolved

## Success Metrics

### Correctness Metrics
- [ ] **All movement tests pass**: Single element, two element, nested scenarios
- [ ] **No visual duplication**: MultiSelectInput and other real-world examples work correctly
- [ ] **Consistent behavior**: Same results regardless of element count or complexity

### Performance Metrics  
- [ ] **O(log n) query performance**: Spatial queries significantly faster than linear search
- [ ] **Memory efficiency**: Spatial index memory overhead acceptable
- [ ] **Rendering speed**: No regression in overall rendering performance

### Maintainability Metrics
- [ ] **Clean separation**: Spatial logic isolated from rendering logic
- [ ] **Comprehensive tests**: All detailed scenarios covered with automated tests
- [ ] **Documentation**: Clear examples and usage patterns for future developers

## Current Blockers

1. **VirtualDomRenderer clearing not working**: VisitUpdateProps not actually clearing old regions
2. **MockRenderingSystem test limitations**: May not be capturing FillRect operations correctly  
3. **DirtyRegionTracker implementation**: Unclear if regions are being tracked/processed correctly
4. **R-Tree implementation missing**: Need core spatial index functionality

## Next Immediate Actions

1. **Debug VirtualDomRenderer clearing issue**: Determine why marked dirty regions aren't being cleared
2. **Implement basic R-Tree**: Create minimal working spatial index
3. **Create comprehensive test scenarios**: Implement all detailed test cases listed above
4. **Performance benchmarking**: Validate spatial approach performance gains
5. **Real-world validation**: Test with MultiSelectInput and other complex examples