# Layout Testing Plan (Consolidated)

This content has moved to the unified document: see `TESTING_STRATEGY.md`.

## Implementation Status (Updated: January 2025)
✅ **Phase 1 Complete**: Test infrastructure has been successfully implemented
- Created comprehensive test helpers and mock components
- Built constraint propagation test suite with 10 passing tests
- Implemented component-specific layout tests for Box, Stack, Text, and Grid

✅ **Phase 2 Complete**: Fixed majority of failing tests
- Fixed Box flex properties (FlexGrow, FlexShrink, FlexBasis)
- Fixed Text component MaxWidth handling
- Improved Box auto-sizing with tight constraints
- **Reduced failing tests from 41 to 18 (56% improvement)**

### Latest Progress (January 2025)
- ✅ Fixed Grid auto-placement with proper cursor advancement
- ✅ Fixed Grid row calculation with placement simulation for spans
- ✅ Fixed Grid tight constraints with Fr column/row distribution
- ✅ Fixed Grid spanning to fill allocated cell space
- ✅ **All Grid layout tests now pass!**
- ✅ Fixed Spacer implementation to respect tight constraints
- ✅ Added HStack FlexBasis and FlexGrow support

## Current Issues
- VStack needs FlexGrow support (3 failing tests)
- Text edge cases with off-by-one width calculations (5 failing tests)
- Box JustifyContent calculation (1 failing test)
- Various focus handling and integration tests (9 failing tests)

## Testing Categories

### 1. Constraint Propagation Tests
Test how constraints flow through the component tree in various scenarios.

#### 1.1 Basic Constraint Tests
- [x] Unconstrained parent with constrained children
- [x] Constrained parent with unconstrained children  
- [x] Mixed constraints at different tree levels
- [x] Infinity handling in constraints
- [x] Zero-size constraint handling

#### 1.2 Auto-Sizing Tests
- [x] Box with auto width and fixed height
- [x] Box with fixed width and auto height
- [x] Box with both auto dimensions
- [x] Nested auto-sized boxes
- [x] Auto-sized boxes with padding/margin

#### 1.3 Edge Case Tests
- [x] Components with zero size
- [x] Components with infinite preferred size
- [x] Deeply nested constraint propagation
- [x] Circular dependency detection

### 2. Component Layout Tests

#### 2.1 Box Component
- [x] Empty box layout
- [x] Box with single child
- [x] Box with multiple children
- [x] Box with flex properties ✅ (Fixed FlexGrow, FlexShrink, FlexBasis)
- [x] Box with gap/spacing
- [x] Box with padding variations
- [x] Box with margin variations
- [ ] Box with border considerations
- [x] Box with auto dimensions and tight constraints ✅ (Fixed)

#### 2.2 VStack/HStack Components
- [x] Empty stack
- [x] Stack with uniform children
- [x] Stack with mixed-size children
- [x] Stack with spacers
- [x] Stack with auto-sized children
- [x] Stack overflow handling
- [x] Stack with flex children

#### 2.3 Text Component
- [x] Single line text
- [x] Multi-line text without wrapping
- [x] Word wrap scenarios
- [x] Character wrap scenarios
- [x] Text with max width ✅ (Fixed to use MaxWidth as layout width)
- [x] Text with max lines
- [x] Text truncation modes (head, middle, tail)
- [x] Text in constrained containers

#### 2.4 Grid Component
- [x] Fixed columns and rows
- [x] Auto-sized columns and rows
- [x] Fractional units (fr)
- [x] Mixed sizing strategies
- [x] Grid gaps
- [x] Grid item spanning
- [x] Grid overflow scenarios

### 3. Layout Algorithm Tests

#### 3.1 Measurement Phase
- [ ] Natural size calculation
- [ ] Preferred size vs actual size
- [ ] Content-based sizing
- [ ] Min/max constraint application

#### 3.2 Layout Phase
- [ ] Position calculation
- [ ] Size finalization
- [ ] Absolute position propagation
- [ ] Coordinate system (0-based vs 1-based)

#### 3.3 Render Phase
- [ ] Virtual DOM generation
- [ ] Coordinate transformation
- [ ] Clipping and overflow

### 4. Integration Tests

#### 4.1 Real-World Scenarios
- [ ] Form layouts with labels and inputs
- [ ] Card layouts with headers and content
- [ ] Table layouts with scrolling
- [ ] Modal dialogs with proper centering
- [ ] Sidebar layouts
- [ ] Dashboard layouts with multiple panels

#### 4.2 Stress Tests
- [ ] Deeply nested components (>10 levels)
- [ ] Large number of siblings (>100)
- [ ] Rapid layout changes
- [ ] Extreme aspect ratios
- [ ] Terminal resize handling

### 5. Visual Regression Tests

#### 5.1 Snapshot Tests
- [ ] Component visual snapshots
- [ ] Layout boundary tests
- [ ] Spacing consistency tests
- [ ] Alignment verification

#### 5.2 Rendering Tests
- [ ] ANSI escape sequence generation
- [ ] Terminal coordinate mapping
- [ ] Style application with layout

## Implementation Strategy

### Phase 1: Test Infrastructure (Week 1) ✅ COMPLETE
1. ✅ Create layout test utilities (LayoutTestHelper.cs)
2. ✅ Implement constraint assertion helpers
3. ✅ Create visual diff tools for terminal output (VisualizeLayout)
4. ⚠️  Set up snapshot testing framework (partial - CreateSnapshot implemented)

### Phase 2: Unit Tests (Week 2-3) ✅ COMPLETE
1. ✅ Implement constraint propagation tests (10 tests, all passing)
2. ✅ Test individual component layout logic (51 tests total)
3. ⚠️  Test measurement and layout phases (partial coverage)
4. ✅ Add edge case coverage (NaN, infinity, zero handling)

### Phase 3: Integration Tests (Week 4)
1. Build real-world scenario tests
2. Add stress tests
3. Implement visual regression tests
4. Performance benchmarks

### Phase 4: Fixes and Validation (Week 5-6) ⚠️ IN PROGRESS
1. ✅ Fixed Box flex properties (FlexGrow, FlexShrink, FlexBasis)
2. ✅ Fixed Text MaxWidth handling
3. ✅ Improved auto-sizing with tight constraints
4. ⚠️ Grid spanning implementation needed
5. ⚠️ Stack Spacer implementation needed
6. Document layout behavior
7. Create layout debugging tools

## Test Utilities Needed ✅ IMPLEMENTED

### LayoutTestHelper ✅
```csharp
public class LayoutTestHelper
{
    // Constraint creation helpers
    public static LayoutConstraints Tight(float width, float height); ✅
    public static LayoutConstraints Loose(float width, float height); ✅
    public static LayoutConstraints Unconstrained(); ✅
    
    // Assertion helpers
    public static void AssertLayoutBox(LayoutBox actual, LayoutBox expected); ✅
    public static void AssertNotInfinite(float value, string message); ✅
    public static void AssertReasonableSize(float value, float min, float max); ✅
    
    // Debug helpers
    public static string VisualizeLayout(ViewInstance root); ✅
    public static void DumpConstraintTree(ViewInstance root); ✅
}
```

### MockComponents ✅
```csharp
public class FixedSizeComponent : ISimpleComponent ✅
{
    public float Width { get; set; }
    public float Height { get; set; }
}

public class AutoSizeComponent : ISimpleComponent ✅
{
    public float? PreferredWidth { get; set; }
    public float? PreferredHeight { get; set; }
}

// Additional implemented components:
public class TestContainer : ISimpleComponent ✅
public class ExtremeValueComponent : ISimpleComponent ✅
```

## Success Criteria

1. **No Infinite Propagation**: Layout calculations should never result in infinite sizes unless explicitly intended
2. **No Zero Collapse**: Components should not collapse to zero size unless empty or explicitly sized to zero
3. **Predictable Behavior**: Same input constraints should always produce same output layout
4. **Performance**: Layout calculation should complete in <1ms for typical UIs
5. **Visual Correctness**: Rendered output should match expected visual layout

## Known Issues to Address

1. Text appearing on same line as headers in Box components ⚠️ (tests written, awaiting fix)
2. Absolute position calculation timing issues
3. Inconsistent handling of auto-sized components ⚠️ (tests reveal implementation gaps)
4. Missing validation for extreme constraint values ✅ (tests added)

## Test Results Summary (January 2025)
- **Constraint Propagation Tests**: 10/10 passing ✅
- **Component Layout Tests**: Major improvements ✅
  - Box: **23/23 tests passing** ✅ (JustifyContent fixed!)
  - Stack: **20/20 tests passing** ✅ (All Stack issues resolved!)
  - Text: **27/27 tests passing** ✅ (All Text layout issues fixed!)
  - Grid: **21/21 tests passing** ✅ (All Grid issues resolved!)
- **Total Tests**: 344 implemented, 332 passing
- **Overall Progress**: **Reduced failing tests from 41 to 12 (71% improvement)** 🎉

## Future Enhancements

1. Layout debugging visualizer
2. Performance profiler for layout calculations
3. Layout constraint validator
4. Visual layout designer tool

## Timeline

- **Month 1**: Complete test infrastructure and unit tests
- **Month 2**: Complete integration tests and fixes
- **Month 3**: Visual regression tests and documentation
- **Month 4**: Performance optimization and tooling

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Breaking existing layouts | High | Comprehensive snapshot tests |
| Performance regression | Medium | Benchmark suite |
| Complex edge cases | Medium | Extensive test coverage |
| Terminal compatibility | Low | Multi-terminal testing |

## Conclusion

This comprehensive testing plan will ensure the Andy.TUI layout system is robust, predictable, and performant. By systematically testing all aspects of layout calculation, we can prevent edge cases and provide a solid foundation for complex UI layouts.