# Unified Testing Strategy

This document consolidates the layout test plan and diff/movement test plan into a single strategy for validating Andy.TUI correctness and performance.

## Goals
- Prevent visual duplication during movement/resize
- Ensure predictable, correct layout under varied constraints
- Verify renderer behaviors (dirty regions, clipping, z-index, spatial queries)
- Provide actionable diagnostics when tests fail

## Test Categories

### 1) Layout Calculation
- Constraint propagation: unconstrained, tight, mixed, zero, infinity
- Components: Box, VStack/HStack, Text, Grid, Spacer
- Flex behavior: grow/shrink/basis, gaps/margins/padding
- Content-based sizing and max/min constraints

Refer to existing unit suites and helpers (LayoutTestHelper, MockComponents) for creating tight/loose constraints and visualizing layouts.

### 2) Movement, Resize, and Overlap (Diff/Renderer)
- Single element moves (right/down/diagonal)
- Content expansion/shrink (width/height changes)
- Two-element interactions: non-overlapping, overlapping, chain reactions
- Nested hierarchies: parent moves, child expansion shifts siblings
- Real-world table/columns (e.g., MultiSelectInput column expansion)

Expected behavior: mark old regions dirty, clear them before drawing new content; handle z-order at overlaps.

### 3) Integration and Visual Regression
- Real-world layouts: forms, tables with scrolling, modals, dashboards
- Stress tests: deep nesting, many siblings, rapid changes, resizes
- Snapshot/ANSI rendering checks for stable output

### 4) Rendering Engine Focus (Current Priorities)
- Dirty region clearing happens before redraw
- Single rendering path (no dual element/visitor duplication)
- Clipping is consistent
- Occlusion-aware rendering (skip fully covered elements)

### 5) Z-Index and Spatial Index
- Absolute z-index propagation from `ViewInstance`
- R-Tree based spatial queries for dirty region redraws
- Revelation scenarios when top element moves

## Test Utilities
- LayoutTestHelper (tight/loose/unconstrained, assertions, visualization)
- Comprehensive logging: `ComprehensiveLoggingInitializer.BeginTestSession(testName)`
- Mock rendering systems for verifying clears/fills and draws

## Execution Order
1. Layout suites (Box/Stack/Text/Grid/Spacer)
2. Movement/resize suites (single and multi-element)
3. Nested and cascading layout movement
4. Real-world examples and snapshot tests

## Success Criteria
- No duplication artifacts on movement/resize
- All layout tests pass with predictable sizes and positions
- Overlaps respect z-order; occluded elements are skipped when possible
- Performance: no excessive clearing or redraw; spatial queries improve hotspots

## Status Rollups
- Layout suites: see latest summary in `LAYOUT_FIXES_SUMMARY.md`
- Renderer movement/overlap: tracked in `IMPLEMENTATION_STATUS_DETAILED.md`

For background and detailed scenarios, see `Z_INDEX_ARCHITECTURE.md` and `SPATIAL_INDEX_DESIGN.md`. 