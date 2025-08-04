# Declarative UI Implementation Plan

## Overview

This document provides a concrete, step-by-step implementation plan for the Andy.TUI declarative framework, combining Ink's functionality with SwiftUI/WPF patterns.

## Current Status

### ✅ Completed
- Basic declarative component infrastructure (Text, Button, TextField, Dropdown)
- ViewInstance architecture separating declarations from runtime
- Basic HStack/VStack layout components
- Two-way data binding with Binding<T>
- Focus management and keyboard navigation
- Flexbox layout types (FlexDirection, JustifyContent, AlignItems, etc.)
- Length type for flexible sizing (pixels, percentage, auto)
- Spacing type for margin/padding
- Box component with flexbox properties
- Layout calculation phase in ViewInstance
- Phase 1: Complete Layout System ✅
- Phase 2: Core Component Library ✅
  - All text components (Text with wrapping, Newline, Transform, Gradient, BigText)
  - All input components (TextArea, SelectInput, MultiSelectInput, Checkbox, RadioGroup, Slider)
  - All display components (Table, List, ProgressBar, Spinner, Badge, Modal)

### 🚧 In Progress
- Unit testing for Phase 2 components

### 📋 Not Started
- Hook system for state management (Phase 3)
- Animation system (Phase 4)
- Complete testing framework (Phase 5)

## Implementation Phases

## Phase 1: Complete Layout System (Week 1) ✅ COMPLETED

### 1.1 Update All Existing Components (2 days) ✅
- [x] Update HStackInstance to use LayoutBox and flexbox properties
- [x] Update VStackInstance to use LayoutBox and flexbox properties
- [x] Update ButtonInstance with proper layout calculation
- [x] Update TextFieldInstance with proper layout calculation
- [x] Update DropdownInstance with proper layout calculation
- [x] Test all components with new layout system

### 1.2 Implement Layout Algorithm Improvements (3 days) ✅
- [x] Add proper flex-shrink handling
- [x] Implement flex-basis support
- [x] Add overflow handling (visible, hidden, scroll)
- [x] Implement proper percentage-based sizing
- [ ] Add layout caching for performance (deferred)
- [x] Handle nested flex containers correctly

### 1.3 Add Missing Layout Features (2 days) ✅
- [x] Implement ZStack for layered layouts
- [x] Add Grid component with CSS Grid-like behavior
- [ ] Implement ScrollView with viewport management (deferred)
- [x] Add Spacer component for flexible space
- [ ] Create Divider component (deferred)

## Phase 2: Core Component Library (Week 2) ✅ COMPLETED

### 2.1 Text Components (2 days) ✅ COMPLETED
- [x] Enhance Text with wrapping and truncation
  ```csharp
  new Text("Long text...")
      .Wrap(TextWrap.Word)
      .MaxLines(3)
      .TruncationMode(TruncationMode.Ellipsis)
  ```
- [x] Add Newline component
- [x] Add Transform component for text transformations
  ```csharp
  new Transform(TransformType.Uppercase) {
      new Text("hello") // renders as "HELLO"
  }
  ```
- [x] Add Gradient component for color gradients
- [x] Add BigText component for ASCII art

### 2.2 Input Components (3 days) ✅ COMPLETED
- [x] Create TextArea for multi-line input
  ```csharp
  new TextArea(this.Bind(() => description))
      .Rows(5)
      .MaxLength(500)
      .Placeholder("Enter description...")
  ```
- [x] Create SelectInput for keyboard-navigable lists
  ```csharp
  new SelectInput<Country>(countries, this.Bind(() => selected))
      .ItemDisplay(c => $"{c.Flag} {c.Name}")
      .OnSelect(HandleSelection)
  ```
- [x] Create MultiSelectInput with checkboxes
- [x] Create Checkbox component
- [x] Create RadioGroup component
- [x] Create Slider for numeric input

### 2.3 Display Components (2 days) ✅ COMPLETED
- [x] Create Table with sorting and selection
  ```csharp
  new Table<User>(users) {
      new Column<User>("Name", u => u.Name).Sortable(),
      new Column<User>("Email", u => u.Email).Width(30),
      new Column<User>("Role", u => u.Role).Align(Alignment.Center)
  }
  ```
- [x] Create List with markers
  ```csharp
  new List(ListType.Ordered) {
      new Text("First item"),
      new Text("Second item")
  }
  ```
- [x] Create ProgressBar with styles
- [x] Create Spinner with multiple styles
- [x] Create Badge for status indicators
- [x] Create Modal/Dialog system

## Phase 3: State Management & Hooks (Week 3)

### 3.1 Hook Infrastructure (2 days)
- [ ] Create HookContext for managing hook state
- [ ] Implement hook call order validation
- [ ] Add hook cleanup on component disposal
- [ ] Create base Hook class

### 3.2 Core Hooks (3 days)
- [ ] Implement UseState<T>
  ```csharp
  protected override ISimpleComponent Body()
  {
      var (count, setCount) = UseState(0);
      return new Text($"Count: {count}");
  }
  ```
- [ ] Implement UseEffect with dependencies
  ```csharp
  UseEffect(() => {
      var timer = SetInterval(() => setCount(c => c + 1), 1000);
      return () => ClearInterval(timer);
  }, new[] { intervalMs });
  ```
- [ ] Implement UseMemo<T> for expensive computations
- [ ] Implement UseCallback for callback memoization
- [ ] Implement UseRef<T> for mutable references

### 3.3 Specialized Hooks (2 days)
- [ ] Implement UseInput for keyboard handling
  ```csharp
  UseInput((input, key) => {
      if (key.Key == ConsoleKey.Q) Exit();
  });
  ```
- [ ] Implement UseFocus with focus management
- [ ] Implement UseApp for app lifecycle
- [ ] Implement UseStdout/UseStdin
- [ ] Implement UseContext<T> for context values

## Phase 4: Advanced Features (Week 4)

### 4.1 Animation System (3 days)
- [ ] Create Animation base class
- [ ] Implement property animations
  ```csharp
  new Box()
      .Width(isExpanded ? 100 : 50)
      .Animate(duration: 300, easing: Easing.EaseInOut)
  ```
- [ ] Add spring animations
- [ ] Implement animation chaining
- [ ] Create transition effects for enter/exit

### 4.2 Theme System (2 days)
- [ ] Create Theme class with style definitions
- [ ] Implement ThemeProvider component
- [ ] Add UseTheme hook
- [ ] Create built-in themes (light, dark, high contrast)
- [ ] Support custom theme creation

## Phase 2.5: Testing for Core Components (1 Week) ✅ COMPLETED

### Unit Tests Completed
- [x] RadioGroup component tests ✅
- [x] List component tests ✅
- [x] ProgressBar component tests ✅
- [x] Spinner component tests ✅
- [x] Gradient component tests ✅
- [x] BigText component tests ✅
- [x] Slider component tests ✅
- [x] Badge component tests ✅

### Test Coverage Summary
- Created comprehensive unit tests for all 8 components that were missing tests
- All tests follow established patterns from existing test files
- Tests cover basic creation, all parameters, public methods, and real usage scenarios from examples
- Total tests passing: 150 new tests added and passing

### Examples Status
- [x] UIComponentsShowcase.cs - demonstrates Checkbox, RadioGroup, List, ProgressBar, Spinner
- [x] FinalComponentsShowcase.cs - demonstrates Gradient, BigText, Slider, Badge
- [x] Individual test files for Text, TextArea, SelectInput, Table, Modal, Transform, Newline, MultiSelectInput

## Phase 5: Testing & Tools (Week 5)

### 5.1 Testing Framework (3 days)
- [ ] Create TestRenderer for component testing
- [ ] Add assertion helpers for component state
- [ ] Implement mock terminal for unit tests
- [ ] Create visual regression testing tools
- [ ] Add performance benchmarking

### 5.2 Developer Tools (2 days)
- [ ] Create component inspector
- [ ] Add layout debugger overlay
- [ ] Implement performance profiler
- [ ] Add hot reload support
- [ ] Create VS Code extension for syntax highlighting

## Phase 6: Examples & Documentation (Week 6)

### 6.1 Example Applications (3 days)
- [ ] Port Ink examples to Andy.TUI
- [ ] Create dashboard example
- [ ] Build file explorer example
- [ ] Create form builder example
- [ ] Build real-time data visualization

### 6.2 Documentation (2 days)
- [ ] Write component API reference
- [ ] Create getting started guide
- [ ] Write migration guide from imperative
- [ ] Create cookbook with patterns
- [ ] Record video tutorials

## Success Criteria

### Performance Targets
- Layout calculation: < 1ms for 100 components
- Render cycle: < 16ms for smooth 60fps
- Memory usage: < 50MB for typical applications
- Startup time: < 100ms

### Feature Parity
- ✅ All Ink components have Andy.TUI equivalents
- ✅ Layout capabilities match or exceed Yoga
- ✅ Type safety exceeds TypeScript definitions
- ✅ Developer experience on par with SwiftUI

### Code Quality
- Test coverage > 80%
- All public APIs documented
- Examples for every component
- No breaking changes after 1.0

## Migration Strategy

### From Current Implementation
1. Existing ViewInstance-based components continue to work
2. New layout system is opt-in per component
3. Gradual migration path with compatibility layer
4. Clear deprecation warnings and migration guides

### For New Projects
1. Start with declarative API immediately
2. Use Box as the foundation for all layouts
3. Leverage hooks for state management
4. Follow SwiftUI-like patterns

## Risk Mitigation

### Performance Risks
- **Risk**: Complex layouts may be slow
- **Mitigation**: Implement layout caching, optimize hot paths

### Compatibility Risks
- **Risk**: Breaking existing code
- **Mitigation**: Maintain backward compatibility, gradual migration

### Complexity Risks
- **Risk**: API becomes too complex
- **Mitigation**: Regular API reviews, user feedback, simplification passes

This plan provides a clear path to building a world-class terminal UI framework that combines the best ideas from modern UI libraries while leveraging .NET's strengths.