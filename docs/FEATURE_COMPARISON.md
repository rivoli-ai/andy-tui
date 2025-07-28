# Feature Comparison: Andy UI vs Qwen-Code vs Claude Code

## Overview

This document compares the UI features and approaches of the current Andy UI implementation, Qwen-Code (which uses React/Ink), and Claude Code to inform the design of the new Andy.TUI library.

## UI Architecture Comparison

### Andy (Current)
- **Framework**: Custom .NET-based reactive system
- **Rendering**: Direct terminal manipulation with Spectre.Console
- **State Management**: ObservableProperty pattern
- **Component Model**: Custom ComponentBase with virtual DOM concepts
- **Strengths**: 
  - Native .NET performance
  - Strong typing
  - Integrated with .NET ecosystem
- **Weaknesses**:
  - Limited component library
  - Less mature than React ecosystem

### Qwen-Code
- **Framework**: React with Ink for terminal rendering
- **Rendering**: React reconciliation + Ink's terminal renderer
- **State Management**: React hooks (useState, useEffect, etc.)
- **Component Model**: Standard React components
- **Strengths**:
  - Mature ecosystem
  - Familiar React patterns
  - Rich component library
- **Weaknesses**:
  - JavaScript performance overhead
  - Node.js dependency

### Claude Code
- **Framework**: Not directly observable, but appears to use advanced TUI
- **Features Observed**:
  - Real-time header updates with tool status
  - Token counter with directional indicators
  - Collapsible tool execution sections
  - Smooth streaming text
  - Progress indicators

## Feature Breakdown

### 1. Layout System

| Feature | Andy (Current) | Qwen-Code | Claude Code | Andy.TUI (Planned) |
|---------|---------------|-----------|-------------|-------------------|
| Flexbox-like layout | ✓ Basic | ✓ Full (via Ink) | ✓ | ✓ Full |
| Grid layout | ✗ | ✓ Limited | Unknown | ✓ |
| Absolute positioning | ✓ | ✓ | ✓ | ✓ |
| Responsive sizing | ✓ Basic | ✓ | ✓ | ✓ Advanced |
| Scrollable regions | ✓ | ✓ | ✓ | ✓ |

### 2. Component Library

| Component | Andy (Current) | Qwen-Code | Claude Code | Andy.TUI (Planned) |
|-----------|---------------|-----------|-------------|-------------------|
| Text input | ✓ | ✓ | ✓ | ✓ Enhanced |
| Multi-line input | ✗ | ✓ | ✓ | ✓ |
| Buttons | ✓ Basic | ✓ | ✓ | ✓ |
| Progress bars | ✓ | ✓ | ✓ | ✓ Animated |
| Spinners | ✓ | ✓ Multiple styles | ✓ | ✓ Customizable |
| Tables | ✗ | ✓ | ✓ | ✓ |
| Modals | ✗ | ✓ | ✓ | ✓ |
| Tabs | ✗ | ✓ | Unknown | ✓ |
| Status bar | ✓ Basic | ✓ | ✓ | ✓ Advanced |
| Diff viewer | ✗ | ✓ | ✓ | ✓ |

### 3. Real-time Features

| Feature | Andy (Current) | Qwen-Code | Claude Code | Andy.TUI (Planned) |
|---------|---------------|-----------|-------------|-------------------|
| Streaming text | ✓ Basic | ✓ | ✓ Smooth | ✓ Optimized |
| Live updates | ✓ | ✓ | ✓ | ✓ |
| Background tasks | ✓ | ✓ | ✓ | ✓ |
| Tool status indicators | ✗ | ✓ | ✓ Advanced | ✓ |
| Collapsible sections | ✗ | ✓ | ✓ | ✓ |
| Auto-scrolling | ✓ | ✓ | ✓ | ✓ Smart |

### 4. User Input

| Feature | Andy (Current) | Qwen-Code | Claude Code | Andy.TUI (Planned) |
|---------|---------------|-----------|-------------|-------------------|
| Keyboard navigation | ✓ | ✓ | ✓ | ✓ |
| Mouse support | ✗ | ✓ | ✓ | ✓ |
| Copy/paste | ✓ Basic | ✓ | ✓ | ✓ |
| Text selection | ✗ | ✓ | ✓ | ✓ |
| Shortcuts | ✓ | ✓ | ✓ | ✓ Customizable |

### 5. Visual Features

| Feature | Andy (Current) | Qwen-Code | Claude Code | Andy.TUI (Planned) |
|---------|---------------|-----------|-------------|-------------------|
| Syntax highlighting | ✓ | ✓ | ✓ | ✓ |
| Themes | ✓ Basic | ✓ Multiple | ✓ | ✓ Extensible |
| Animations | ✗ | ✓ Limited | ✓ | ✓ |
| Icons/Emojis | ✓ | ✓ | ✓ | ✓ |
| Borders/Boxes | ✓ | ✓ Various styles | ✓ | ✓ |

## Key Innovations to Implement

### From Claude Code
1. **Smart Header Bar**
   - Tool execution status with icons
   - Real-time token counting
   - Bidirectional indicators (↑↓ for in/out)
   - Context-aware information display

2. **Collapsible Tool Sections**
   - Expandable/collapsible tool execution logs
   - Nested execution visualization
   - Progress indicators within sections

3. **Smooth Streaming**
   - Character-by-character rendering
   - No flicker or jumping
   - Proper word wrapping

### From Qwen-Code
1. **React-like Component Model**
   - Hooks-inspired state management
   - Component composition
   - Effect system

2. **Rich Input Components**
   - Multi-line text editing
   - Syntax-aware input
   - Auto-completion support

3. **Flexible Theming**
   - Multiple built-in themes
   - Easy theme switching
   - Comprehensive style system

### Unique Andy.TUI Features
1. **.NET Native Performance**
   - Zero JavaScript overhead
   - Direct terminal access
   - Minimal memory footprint

2. **Deep Integration**
   - Native .NET async/await
   - LINQ support in components
   - Source generators for performance

3. **Advanced Observables**
   - Computed properties
   - Dependency tracking
   - Automatic disposal

## Implementation Priority

### Phase 1: Core Features (MVP)
- Basic layout system (Box, Stack)
- Text input component
- Message display with streaming
- Simple progress indicators
- Basic theme support

### Phase 2: Enhanced UI
- Header bar with live updates
- Status bar with git info
- Collapsible sections
- Modal dialogs
- Syntax highlighting

### Phase 3: Advanced Features
- Mouse support
- Animations
- Diff viewer
- Table component
- Custom themes

### Phase 4: Polish
- Smooth scrolling
- Text selection
- Copy/paste enhancements
- Accessibility features
- Performance optimizations

## Technical Decisions

### Rendering Strategy
- **Virtual DOM**: Yes, for efficient updates
- **Double Buffering**: Yes, to prevent flicker
- **Dirty Tracking**: Yes, for minimal redraws
- **Async Rendering**: Yes, for non-blocking updates

### State Management
- **Observable Pattern**: Core primitive
- **Computed Properties**: For derived state
- **Effect System**: For side effects
- **Context API**: For dependency injection

### Component Model
- **Functional-style**: With hooks-like patterns
- **Class-based**: For complex components
- **Composition**: Over inheritance
- **Lifecycle**: Initialize, Render, Update, Dispose

## Success Criteria

The new Andy.TUI should:
1. Match or exceed Qwen-Code's component richness
2. Achieve Claude Code's smooth user experience
3. Maintain Andy's .NET-native performance
4. Provide a familiar API for .NET developers
5. Support all planned UI requirements for Andy CLI