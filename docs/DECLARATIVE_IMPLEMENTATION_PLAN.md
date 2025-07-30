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

### 🚧 In Progress
- Updating existing ViewInstances to use new layout system

### 📋 Not Started
- Complete component library matching Ink
- Hook system for state management
- Animation system
- Testing framework

## Implementation Phases

## Phase 1: Complete Layout System (Week 1)

### 1.1 Update All Existing Components (2 days)
- [ ] Update HStackInstance to use LayoutBox and flexbox properties
- [ ] Update VStackInstance to use LayoutBox and flexbox properties
- [ ] Update ButtonInstance with proper layout calculation
- [ ] Update TextFieldInstance with proper layout calculation
- [ ] Update DropdownInstance with proper layout calculation
- [ ] Test all components with new layout system

### 1.2 Implement Layout Algorithm Improvements (3 days)
- [ ] Add proper flex-shrink handling
- [ ] Implement flex-basis support
- [ ] Add overflow handling (visible, hidden, scroll)
- [ ] Implement proper percentage-based sizing
- [ ] Add layout caching for performance
- [ ] Handle nested flex containers correctly

### 1.3 Add Missing Layout Features (2 days)
- [ ] Implement ZStack for layered layouts
- [ ] Add Grid component with CSS Grid-like behavior
- [ ] Implement ScrollView with viewport management
- [ ] Add Spacer component for flexible space
- [ ] Create Divider component

## Phase 2: Core Component Library (Week 2)

### 2.1 Text Components (2 days)
- [ ] Enhance Text with wrapping and truncation
  ```csharp
  new Text("Long text...")
      .Wrap(TextWrap.Word)
      .MaxLines(3)
      .TruncationMode(TruncationMode.Ellipsis)
  ```
- [ ] Add Newline component
- [ ] Add Transform component for text transformations
  ```csharp
  new Transform(TransformType.Uppercase) {
      new Text("hello") // renders as "HELLO"
  }
  ```
- [ ] Add Gradient component for color gradients
- [ ] Add BigText component for ASCII art

### 2.2 Input Components (3 days)
- [ ] Create TextArea for multi-line input
  ```csharp
  new TextArea(this.Bind(() => description))
      .Rows(5)
      .MaxLength(500)
      .Placeholder("Enter description...")
  ```
- [ ] Create SelectInput for keyboard-navigable lists
  ```csharp
  new SelectInput<Country>(countries, this.Bind(() => selected))
      .ItemDisplay(c => $"{c.Flag} {c.Name}")
      .OnSelect(HandleSelection)
  ```
- [ ] Create MultiSelectInput with checkboxes
- [ ] Create Checkbox component
- [ ] Create RadioGroup component
- [ ] Create Slider for numeric input

### 2.3 Display Components (2 days)
- [ ] Create Table with sorting and selection
  ```csharp
  new Table<User>(users) {
      new Column<User>("Name", u => u.Name).Sortable(),
      new Column<User>("Email", u => u.Email).Width(30),
      new Column<User>("Role", u => u.Role).Align(Alignment.Center)
  }
  ```
- [ ] Create List with markers
  ```csharp
  new List(ListType.Ordered) {
      new Text("First item"),
      new Text("Second item")
  }
  ```
- [ ] Create ProgressBar with styles
- [ ] Create Spinner with multiple styles
- [ ] Create Badge for status indicators
- [ ] Create Modal/Dialog system

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