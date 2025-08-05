# Layout System Comprehensive Testing Plan

## Overview
This document outlines a comprehensive testing strategy for the Andy.TUI layout system to ensure robust and predictable layout calculations across all scenarios. The goal is to prevent edge cases where measurements unintentionally become extreme values (infinity, zero) when they should have reasonable values.

## Current Issues
- Text content sometimes appears on the same line as headers in Box components
- Auto-sized boxes can propagate infinite constraints to children
- Layout calculations don't always handle edge cases properly
- Missing test coverage for complex layout scenarios

## Testing Categories

### 1. Constraint Propagation Tests
Test how constraints flow through the component tree in various scenarios.

#### 1.1 Basic Constraint Tests
- [ ] Unconstrained parent with constrained children
- [ ] Constrained parent with unconstrained children  
- [ ] Mixed constraints at different tree levels
- [ ] Infinity handling in constraints
- [ ] Zero-size constraint handling

#### 1.2 Auto-Sizing Tests
- [ ] Box with auto width and fixed height
- [ ] Box with fixed width and auto height
- [ ] Box with both auto dimensions
- [ ] Nested auto-sized boxes
- [ ] Auto-sized boxes with padding/margin

#### 1.3 Edge Case Tests
- [ ] Components with zero size
- [ ] Components with infinite preferred size
- [ ] Deeply nested constraint propagation
- [ ] Circular dependency detection

### 2. Component Layout Tests

#### 2.1 Box Component
- [ ] Empty box layout
- [ ] Box with single child
- [ ] Box with multiple children
- [ ] Box with flex properties
- [ ] Box with gap/spacing
- [ ] Box with padding variations
- [ ] Box with margin variations
- [ ] Box with border considerations

#### 2.2 VStack/HStack Components
- [ ] Empty stack
- [ ] Stack with uniform children
- [ ] Stack with mixed-size children
- [ ] Stack with spacers
- [ ] Stack with auto-sized children
- [ ] Stack overflow handling
- [ ] Stack with flex children

#### 2.3 Text Component
- [ ] Single line text
- [ ] Multi-line text without wrapping
- [ ] Word wrap scenarios
- [ ] Character wrap scenarios
- [ ] Text with max width
- [ ] Text with max lines
- [ ] Text truncation modes (head, middle, tail)
- [ ] Text in constrained containers

#### 2.4 Grid Component
- [ ] Fixed columns and rows
- [ ] Auto-sized columns and rows
- [ ] Fractional units (fr)
- [ ] Mixed sizing strategies
- [ ] Grid gaps
- [ ] Grid item spanning
- [ ] Grid overflow scenarios

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

### Phase 1: Test Infrastructure (Week 1)
1. Create layout test utilities
2. Implement constraint assertion helpers
3. Create visual diff tools for terminal output
4. Set up snapshot testing framework

### Phase 2: Unit Tests (Week 2-3)
1. Implement constraint propagation tests
2. Test individual component layout logic
3. Test measurement and layout phases
4. Add edge case coverage

### Phase 3: Integration Tests (Week 4)
1. Build real-world scenario tests
2. Add stress tests
3. Implement visual regression tests
4. Performance benchmarks

### Phase 4: Fixes and Validation (Week 5-6)
1. Fix issues discovered during testing
2. Refactor layout algorithms as needed
3. Document layout behavior
4. Create layout debugging tools

## Test Utilities Needed

### LayoutTestHelper
```csharp
public class LayoutTestHelper
{
    // Constraint creation helpers
    public static LayoutConstraints Tight(float width, float height);
    public static LayoutConstraints Loose(float width, float height);
    public static LayoutConstraints Unconstrained();
    
    // Assertion helpers
    public static void AssertLayoutBox(LayoutBox actual, LayoutBox expected);
    public static void AssertNotInfinite(float value, string message);
    public static void AssertReasonableSize(float value, float min, float max);
    
    // Debug helpers
    public static string VisualizeLayout(ViewInstance root);
    public static void DumpConstraintTree(ViewInstance root);
}
```

### MockComponents
```csharp
public class FixedSizeComponent : ISimpleComponent
{
    public float Width { get; set; }
    public float Height { get; set; }
}

public class AutoSizeComponent : ISimpleComponent
{
    public float? PreferredWidth { get; set; }
    public float? PreferredHeight { get; set; }
}
```

## Success Criteria

1. **No Infinite Propagation**: Layout calculations should never result in infinite sizes unless explicitly intended
2. **No Zero Collapse**: Components should not collapse to zero size unless empty or explicitly sized to zero
3. **Predictable Behavior**: Same input constraints should always produce same output layout
4. **Performance**: Layout calculation should complete in <1ms for typical UIs
5. **Visual Correctness**: Rendered output should match expected visual layout

## Known Issues to Address

1. Text appearing on same line as headers in Box components
2. Absolute position calculation timing issues
3. Inconsistent handling of auto-sized components
4. Missing validation for extreme constraint values

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