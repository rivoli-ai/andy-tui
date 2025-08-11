# Andy.TUI.Core Project Documentation

## Overview

Andy.TUI.Core is the foundational layer of the Andy TUI framework, serving as the orchestration hub that integrates all core subsystems. It acts as the central coordinator between virtual DOM management, reactive state handling, spatial indexing, and diagnostic capabilities.

## Project Configuration

### Target Framework
- **.NET 8.0** with C# 12 features
- **Nullable Reference Types**: Enabled for enhanced type safety
- **Implicit Usings**: Enabled for cleaner code

### Dependencies Architecture

```
Andy.TUI.Core
     │
     ├── Andy.TUI.VirtualDom    (Virtual DOM implementation)
     ├── Andy.TUI.Observable    (Reactive state management)
     ├── Andy.TUI.Spatial       (Spatial indexing & occlusion)
     └── Andy.TUI.Diagnostics   (Logging & debugging)
```

## Core Responsibilities

### 1. Framework Integration
The project acts as the glue layer that brings together all fundamental subsystems:
- Coordinates between virtual DOM updates and spatial indexing
- Manages reactive state propagation through the component tree
- Handles diagnostic information flow across all subsystems

### 2. Rendering Pipeline
Establishes the core rendering abstractions and pipeline:
- Defines rendering interfaces and base implementations
- Manages the render cycle coordination
- Handles buffer management and optimization

### 3. Component Lifecycle
Provides base component abstractions and lifecycle management:
- Component initialization and disposal
- State synchronization between components
- Event propagation and handling

## Assembly Configuration

### Internal Visibility
```xml
<InternalsVisibleTo Include="Andy.TUI.Core.Tests" />
```
Exposes internal members to the test assembly for comprehensive unit testing.

## Architectural Flow

```
┌─────────────────────────────────────────────────────┐
│                  Application Layer                   │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│                  Andy.TUI.Core                       │
│                                                      │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────┐ │
│  │  Rendering  │  │  Component   │  │   Event    │ │
│  │  Pipeline   │◄─┤  Management  │◄─┤  Handling  │ │
│  └──────┬──────┘  └──────┬───────┘  └─────┬──────┘ │
│         │                │                 │        │
└─────────┼────────────────┼─────────────────┼────────┘
          │                │                 │
          ▼                ▼                 ▼
    ┌──────────┐    ┌──────────┐     ┌──────────┐
    │ VirtualDom│    │Observable│     │ Spatial  │
    └──────────┘    └──────────┘     └──────────┘
```

## Usage Examples

### Example 1: Component Creation
The Core project provides base classes for creating TUI components:
```
Component Definition → Core Processing → Virtual DOM Update → Render
```

### Example 2: State Management Flow
Reactive state changes flow through the Core layer:
```
State Change → Observable Notification → Core Coordinator → Component Update → Re-render
```

### Example 3: Spatial Optimization
Occlusion culling and spatial queries are coordinated through Core:
```
Render Request → Core Pipeline → Spatial Query → Occlusion Check → Optimized Render
```

## Build Characteristics

### Compilation
- Produces: `Andy.TUI.Core.dll`
- Type: Class Library
- No executable output

### Testing Support
- Full internal member access for `Andy.TUI.Core.Tests`
- Designed for comprehensive unit and integration testing

## Integration Points

### Upstream Dependencies
None - this is a foundational layer that depends only on its subsystem modules.

### Downstream Consumers
- **Andy.TUI**: Main framework assembly that packages Core
- **Andy.TUI.Terminal**: Uses Core for rendering abstractions
- **Andy.TUI.Declarative**: Builds on Core's component model

## Performance Considerations

1. **Zero Allocation Patterns**: Core establishes patterns for minimal GC pressure
2. **Pooling Infrastructure**: Provides object pooling for frequently allocated types
3. **Lazy Initialization**: Components and resources are initialized on-demand
4. **Efficient Event Propagation**: Optimized event bubbling and capturing

## Development Guidelines

### Adding New Features
1. Maintain separation of concerns between subsystems
2. Use dependency injection patterns for extensibility
3. Ensure all public APIs have XML documentation
4. Follow existing architectural patterns

### Testing Requirements
- Unit tests for all public APIs
- Integration tests for subsystem coordination
- Performance benchmarks for critical paths