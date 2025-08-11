# Diff/Renderer Movement & Overlap Testing (Consolidated)

This content has moved to the unified document: see `TESTING_STRATEGY.md`.

## Problem Statement

The current diff engine generates correct patches (UpdatePropsPatch) when element properties change, but the VirtualDomRenderer's patch application doesn't properly:
1. Clear old content when elements move or resize
2. Handle overlapping scenarios where content shifts position
3. Manage column expansion/contraction scenarios

## Detailed Test Case Categories

### Category 1: Single Element Operations

#### 1.1 Simple Position Movement

**Test**: `SingleElement_MoveRight_DetailedScenario`

**Initial State**:
```
Terminal Grid (20x5):
01234567890123456789
0 ·····················
1 ·····················  
2 ·····Hello·········
3 ·····················
4 ·····················
```
- Element: Text("Hello") at position (5,2), size (5x1)
- Rendered characters occupy columns 5-9 on row 2

**Change**: Move text right by 3 positions (x: 5→8)

**Expected Diff Engine Behavior**:
1. Detects position change in virtual DOM tree
2. Generates UpdatePropsPatch with old_x=5, new_x=8
3. Patch applied to VirtualDomRenderer

**Expected VirtualDomRenderer Behavior**:
1. Mark old region (5,2,5x1) as dirty for clearing
2. Update RenderedElement coordinates: x=5→8
3. Mark new region (8,2,5x1) as dirty for rendering
4. Call RenderDirtyRegions():
   - Clear (5,2,5x1) with spaces: "     "
   - Render "Hello" at (8,2)

**Expected Final State**:
```
Terminal Grid (20x5):
01234567890123456789
0 ·····················
1 ·····················  
2 ········Hello······
3 ·····················
4 ·····················
```

**Current Issue**: No clearing occurs, result shows:
```
2 ·····Hello···Hello··  ← DUPLICATION
```

---

**Test**: `SingleElement_MoveDown_DetailedScenario`

**Initial State**:
```
Terminal Grid (20x5):
01234567890123456789
0 ·····················
1 ·····················  
2 ·····Hello·········
3 ·····················
4 ·····················
```

**Change**: Move text down by 1 row (y: 2→3)

**Expected Process**:
1. Clear old position (5,2,5x1)
2. Render at new position (5,3)

**Expected Final State**:
```
Terminal Grid (20x5):
01234567890123456789
0 ·····················
1 ·····················  
2 ·····················
3 ·····Hello·········
4 ·····················
```

#### 1.2 Element Resize Operations

**Test**: `SingleElement_ExpandWidth_DetailedScenario`

**Initial State**:
```
2 ·····Hi············
```
- Text("Hi") at (5,2), occupies columns 5-6

**Change**: Content changes from "Hi" to "Hello World" (width: 2→11)

**Expected Process**:
1. Detect content change via UpdateTextPatch or UpdatePropsPatch
2. Clear old region (5,2,2x1) - insufficient!
3. **CRITICAL**: Must clear expanded region (5,2,11x1)
4. Render new content "Hello World" at (5,2)

**Expected Final State**:
```
2 ·····Hello World···
```

**Test**: `SingleElement_ShrinkWidth_DetailedScenario`

**Initial State**:
```
2 ·····Hello World···
```

**Change**: Content changes to "Hi" (width: 11→2)

**Expected Process**:
1. Clear full old region (5,2,11x1) - must clear extra chars
2. Render "Hi" at (5,2)

**Expected Final State**:
```
2 ·····Hi············
```

### Category 2: Two Element Interactions

#### 2.1 Non-Overlapping Movement

**Test**: `TwoElements_BothMove_NoOverlap_DetailedScenario`

**Initial State**:
```
Terminal Grid (20x5):
01234567890123456789
0 ·····················
1 ·····················  
2 ··AAA·····BBB······
3 ·····················
4 ·····················
```
- ElementA: Text("AAA") at (2,2), size (3x1)
- ElementB: Text("BBB") at (10,2), size (3x1)

**Change**: Both elements move right by 2 positions
- ElementA: (2,2) → (4,2)
- ElementB: (10,2) → (12,2)

**Expected Process**:
1. UpdatePropsPatch for ElementA: x=2→4
2. UpdatePropsPatch for ElementB: x=10→12
3. Clear old regions: (2,2,3x1) and (10,2,3x1)
4. Render at new positions: (4,2) and (12,2)

**Expected Final State**:
```
2 ····AAA·····BBB···
```

#### 2.2 Overlapping Movement (Complex) 🚫

**Test**: `TwoElements_OverlappingMovement_DetailedScenario`

**Initial State**:
```
Terminal Grid (20x5):
01234567890123456789
2 ··AAA·····BBB······
```
- ElementA: Text("AAA") at (2,2), size (3x1) [cols 2-4]
- ElementB: Text("BBB") at (10,2), size (3x1) [cols 10-12]

**Change**: 
- ElementA moves right to (8,2) [will occupy cols 8-10]
- ElementB moves left to (6,2) [will occupy cols 6-8]

**Critical Overlap Analysis**:
- ElementA new position (8,9,10) overlaps ElementB new position (6,7,8) at column 8
- Both elements need to clear their old positions
- Final render must handle the overlap correctly

**Expected Process** (Z-order dependent):
1. Clear ElementA old region (2,2,3x1)
2. Clear ElementB old region (10,2,3x1)
3. Render ElementA at (8,2): "AAA"
4. Render ElementB at (6,2): "BBB" 
5. Result at column 8: depends on render order

**Possible Final States** (depending on z-order):
```
# If ElementA renders last (higher z-index):
2 ······BBAAAA······
        ↑ overlap at column 8: 'A' wins

# If ElementB renders last (higher z-index):
2 ······BBBAA······
        ↑ overlap at column 8: 'B' wins
```

#### 2.3 Chain Reaction Movement 🚫

**Test**: `TwoElements_ChainReaction_DetailedScenario`

**Initial State**:
```
2 ··AAA·BBB·········
```
- ElementA: Text("AAA") at (2,2) [cols 2-4]
- ElementB: Text("BBB") at (6,2) [cols 6-8]

**Change**: ElementA expands to "AAAAA" (width 3→5)

**Expected Cascade Effect**:
1. ElementA expansion will occupy cols 2-6
2. This overlaps with ElementB at cols 6-8
3. Layout system should detect collision and move ElementB
4. ElementB should move right to avoid overlap: (6,2) → (7,2)

**Expected Process**:
1. ElementA content change triggers UpdateTextPatch
2. Layout recalculation detects ElementB collision
3. ElementB position change triggers UpdatePropsPatch
4. Clear old regions: (2,2,3x1) for ElementA, (6,2,3x1) for ElementB
5. Render: "AAAAA" at (2,2), "BBB" at (7,2)

**Expected Final State**:
```
2 ··AAAAA·BBB······
```

### Category 3: Nested Element Hierarchies

#### 3.1 Parent-Child Movement 🚫

**Test**: `NestedElements_ParentMoves_ChildrenFollow_DetailedScenario`

**Initial State**:
```
Virtual DOM Structure:
Container(x=2, y=1, width=8, height=3)
  ├─ HeaderText("Title") at relative (1,0) → absolute (3,1)
  └─ BodyText("Content") at relative (1,1) → absolute (3,2)

Terminal Grid:
01234567890123456789
0 ·····················
1 ···Title············  
2 ···Content·········
3 ·····················
```

**Change**: Container moves right by 3 positions (x: 2→5)

**Expected Cascading Updates**:
1. Container position: (2,1) → (5,1)
2. HeaderText absolute position: (3,1) → (6,1)
3. BodyText absolute position: (3,2) → (6,2)

**Expected Process**:
1. UpdatePropsPatch for Container: x=2→5
2. VirtualDomRenderer recalculates child positions
3. Clear old regions: (3,1,5x1), (3,2,7x1)
4. Render at new positions: (6,1), (6,2)

**Expected Final State**:
```
01234567890123456789
0 ·····················
1 ······Title·········  
2 ······Content······
3 ·····················
```

#### 3.2 Child Resize Affects Siblings 🚫

**Test**: `NestedElements_ChildExpands_SiblingsShift_DetailedScenario`

**Initial State**:
```
Virtual DOM Structure:
HStack(x=1, y=2)
  ├─ Label1("A") → (1,2)
  ├─ Label2("B") → (2,2)  
  └─ Label3("C") → (3,2)

Terminal Grid:
2 ·ABC················
```

**Change**: Label1 content expands from "A" to "EXPANDED" (width 1→8)

**Expected Layout Recalculation**:
1. Label1: (1,2) width 1→8, stays at (1,2)
2. Label2: shifts right (2,2) → (9,2)
3. Label3: shifts right (3,2) → (10,2)

**Expected Process**:
1. UpdateTextPatch for Label1 content change
2. HStack layout recalculation triggers position updates
3. UpdatePropsPatch for Label2: x=2→9
4. UpdatePropsPatch for Label3: x=3→10
5. Clear old regions: (1,2,1x1), (2,2,1x1), (3,2,1x1)
6. Render at new positions

**Expected Final State**:
```
2 ·EXPANDED·BC·······
```

### Category 4: Multi-Column Table Scenario (Real MultiSelectInput Issue)

#### 4.1 Three-Column Expansion (The Core Problem) 🚫

**Test**: `MultiColumn_FirstColumnExpands_DetailedScenario`

**Initial State** (MultiSelectInput with 3 columns):
```
Terminal Grid (50 chars wide):
01234567890123456789012345678901234567890123456789
2 Programming····Favorite Colors····Lucky Numbers··
  ↑col1 (11chars)↑col2 (15chars)   ↑col3 (13chars)
  positions:      positions:        positions:
  0-10           15-29             34-46
```

**User Action**: Add "Machine Learning" to Programming column

**Content Change**: 
- Column 1: "Programming" → "Programming\nMachine Learning" 
- Column width increases: 11 → 16 chars (longest line)

**Expected Layout Recalculation**:
1. Column 1: stays at x=0, width 11→16
2. Column 2: shifts right x=15→21 (0+16+5 padding)
3. Column 3: shifts right x=34→42 (21+15+6 padding)

**Critical Clearing Requirements**:
1. Column 2 old area (15-29) must be cleared completely
2. Column 3 old area (34-46) must be cleared completely
3. New content rendered at shifted positions

**Expected Process**:
1. Column 1 content change → UpdateTextPatch
2. Layout engine recalculates column positions
3. Column 2 position change → UpdatePropsPatch x=15→21
4. Column 3 position change → UpdatePropsPatch x=34→42
5. **CRITICAL CLEARING PHASE**:
   - Clear Column 2 old area: (15,2,15x1) with spaces
   - Clear Column 3 old area: (34,2,13x1) with spaces
6. **RENDERING PHASE**:
   - Render Column 1 expanded content at (0,2)
   - Render Column 2 content at (21,2)
   - Render Column 3 content at (42,2)

**Expected Final State**:
```
01234567890123456789012345678901234567890123456789
2 Programming·····Favorite Colors····Lucky Numbers··
3 Machine Learning···················
```

**Current Bug Result**:
```
01234567890123456789012345678901234567890123456789
2 Programming·····Favorite ColorsFavorite Colors····Lucky NumbersLucky Numbers··
3 Machine Learning···················              ↑ OLD TEXT NOT CLEARED
```

### Category 5: Complex Multi-Level Nested Scenarios

#### 5.1 Nested Table with Expanding Cells 🚫

**Test**: `NestedTable_CellExpansion_MultiLevel_DetailedScenario`

**Initial State**:
```
Table(2x2)
├─ Row1
│  ├─ Cell(0,0): HStack["A", "B"]
│  └─ Cell(1,0): Text("X")
└─ Row2  
   ├─ Cell(0,1): Text("C")
   └─ Cell(1,1): Text("Y")

Rendered:
01234567890
0 A·B···X···
1 C·····Y···
```

**Change**: Cell(0,0) HStack first element expands "A" → "ALPHA"

**Expected Cascade**:
1. HStack recalculates: "A","B" spacing changes
2. Cell(0,0) width increases
3. Table column 0 width increases
4. All Row1 Cell(1,0) shifts right
5. All Row2 Cell(1,1) shifts right (column alignment)

**Expected Process** (6 total patches):
1. UpdateTextPatch: "A" → "ALPHA"
2. UpdatePropsPatch: HStack "B" element shifts right
3. UpdatePropsPatch: Cell(1,0) shifts right
4. UpdatePropsPatch: Cell(1,1) shifts right
5. Clear all old positions of shifted elements
6. Render at new calculated positions

**Expected Final State**:
```
01234567890
0 ALPHA·B···X···
1 C·········Y···
```

This comprehensive breakdown shows exactly how each scenario should behave, what clearing operations are required, and what the spatial index needs to detect efficiently.

## Technical Implementation Plan

### Phase 1: Fix VirtualDomRenderer.VisitUpdateProps ⚠️

**Current Status**: COMPLETED BUT TESTS STILL FAILING
**File**: `/Users/samibengrine/Devel/rivoli-ai/andy-tui/src/Andy.TUI.Terminal/Rendering/VirtualDomRenderer.cs:275`

**Changes Made**:
```csharp
public void VisitUpdateProps(UpdatePropsPatch patch)
{
    // Mark old position as dirty (for clearing)
    _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));
    
    // Track position/size changes and update RenderedElement coordinates
    if (positionChanged)
    {
        element.X = newX; element.Y = newY; 
        element.Width = newWidth; element.Height = newHeight;
        // Mark new position as dirty (for rendering)
        _dirtyRegionTracker.MarkDirty(new Rectangle(newX, newY, newWidth, newHeight));
    }
}
```

**Result**: ✅ Build succeeds, 🚫 Tests still fail - no clearing operations performed

**Issue Identified**: Dirty regions are being marked, but `RenderDirtyRegions()` isn't performing fills.
Possible causes:
1. `RenderDirtyRegions()` method not being called during patch application
2. MockRenderingSystem not capturing `FillRect` calls correctly
3. `DirtyRegionTracker` not properly tracking dirty regions

### Phase 2: Verify Dirty Region Clearing

**Status**: Pending Phase 1 completion
- Ensure `RenderDirtyRegions()` properly clears marked areas with spaces
- Verify clearing happens before new content is drawn

### Phase 3: Handle Complex Overlapping Scenarios  

**Status**: Pending Phase 2 completion
- Test overlapping movement scenarios
- Ensure clearing strategy works for partial overlaps

### Phase 4: Multi-Component Layout Shifts

**Status**: Pending Phase 3 completion  
- Test the real-world MultiSelectInput column expansion scenario
- Verify all shifted columns are properly cleared and redrawn

## Test Execution Order

1. **Build and verify VirtualDomRenderer fix compiles**
2. **Run Category 2 tests** - Should pass after VisitUpdateProps fix
3. **Run Category 3 tests** - Content size changes
4. **Run Category 4 tests** - Overlapping scenarios  
5. **Run Category 5 tests** - Multi-component shifts (the main issue)
6. **Run Category 6 tests** - Complex grid scenarios

## Success Criteria

- [ ] All movement tests pass
- [ ] No visual duplication in MultiSelectInput demo
- [ ] Text components showing bound data update reactively
- [ ] Performance remains acceptable (no excessive clearing/redrawing)

## Files Involved

### Test Files
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/tests/Andy.TUI.Declarative.Tests/DiffEngineMovementTests.cs`
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/tests/Andy.TUI.Declarative.Tests/DiffEngineOverlapTests.cs` (legacy)

### Implementation Files  
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/src/Andy.TUI.Terminal/Rendering/VirtualDomRenderer.cs`
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/src/Andy.TUI.Core/VirtualDom/DiffEngine.cs`
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/src/Andy.TUI.Declarative/Rendering/DeclarativeRenderer.cs`

### Example Files (For Testing)
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/examples/Andy.TUI.Examples.Input/MultiSelectInputTest.cs`
- `/Users/samibengrine/Devel/rivoli-ai/andy-tui/examples/Andy.TUI.Examples.Input/UIComponentsShowcase.cs`

## Current Status Summary

**Overall Progress**: 15% Complete
- ✅ **Diff engine patch generation**: Working correctly
- ⚠️ **VirtualDomRenderer patch application**: Enhanced but tests still failing
- 🚫 **Movement test execution**: Failing - no clearing operations performed
- 🚫 **Real-world testing**: Pending test completion

**STRATEGIC PIVOT**: Moving to Z-Aware Spatial Index Approach
- 📋 **Decision**: Implement Enhanced 3D R-Tree with relative/absolute z-index support
- 📋 **Rationale**: Linear search + flat z-index has fundamental scalability issues
- 📋 **New Requirements**: Support hierarchical z-index resolution for nested components
- 📋 **Reference**: See `SPATIAL_INDEX_DESIGN.md` and `Z_INDEX_ARCHITECTURE.md`

**Next Immediate Actions**:
1. ✅ Create `SPATIAL_INDEX_DESIGN.md` with R-Tree implementation plan
2. 🔄 Implement basic R-Tree with core spatial operations testing
3. 🔄 Integrate spatial index with VirtualDomRenderer
4. 🔄 Re-run movement tests with spatial approach
5. 🔄 Validate with MultiSelectInput real-world scenario

**Target**: Fix the MultiSelectInput duplication issue using efficient spatial indexing for overlap detection and clearing.
