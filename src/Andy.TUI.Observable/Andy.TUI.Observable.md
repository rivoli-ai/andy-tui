# Andy.TUI.Observable Project Documentation

## Overview

Andy.TUI.Observable implements a comprehensive reactive state management system for the Andy TUI framework. This module provides observable properties, computed values, and reactive collections that automatically track dependencies and propagate changes throughout the UI, enabling efficient re-rendering and state synchronization.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Namespace Configuration
```xml
<RootNamespace>Andy.TUI.Core.Observable</RootNamespace>
<AssemblyName>Andy.TUI.Observable</AssemblyName>
```
Maintains backward compatibility while using new assembly structure.

## Core Architecture

```
         Dependency Tracker
                │
    ┌───────────┼───────────────┐
    │           │               │
    ▼           ▼               ▼
Observable  Computed      Observable
Property    Property      Collection
    │           │               │
    └───────────┴───────────────┘
                │
                ▼
        Change Notifications
                │
                ▼
        Component Updates
```

## Key Components

### 1. ObservableProperty<T>
Basic reactive property implementation:
- Automatic change detection
- Event-based notifications
- Value equality checking
- Thread-safe operations

### 2. ComputedProperty<T>
Derived values with automatic recalculation:
- Dependency tracking
- Lazy evaluation
- Memoization
- Circular dependency detection

### 3. ObservableCollection<T>
Reactive list implementation:
- Collection change notifications
- Batch update support
- Index-based access tracking
- LINQ integration

### 4. DependencyTracker
Manages reactive dependency graph:
- Automatic subscription management
- Efficient update propagation
- Memory leak prevention
- Debug visualization

## Reactive Patterns

### Pattern 1: Simple Property Binding
```
ObservableProperty<string> name
         │
         ├──► UI Component
         │     (Auto-updates)
         │
         └──► Computed Property
               (Recalculates)
```

### Pattern 2: Computed Chain
```
Observable A ──┐
               ├──► Computed C ──► Computed D ──► UI
Observable B ──┘
```

### Pattern 3: Collection Binding
```
ObservableCollection<Item>
         │
         ├──► List Component
         │     (Add/Remove/Update)
         │
         └──► Computed Count
               (Auto-updates)
```

## Dependency Tracking Mechanism

```
┌─────────────────────────────────────┐
│        Dependency Graph             │
├─────────────────────────────────────┤
│                                     │
│   Property A ◄──── Computed X      │
│       │              │              │
│       │              ▼              │
│       └─────► Computed Y           │
│                     │               │
│                     ▼               │
│               Component UI          │
│                                     │
└─────────────────────────────────────┘
```

## Usage Examples

### Example 1: Form Validation
```
Email (Observable) ──┬──► IsValid (Computed)
Password (Observable)┘         │
                              ▼
                        Submit Button
                         (Enabled)
```

### Example 2: Filtered List
```
Items (ObservableCollection) ──┬──► FilteredItems (Computed)
SearchTerm (Observable) ────────┘           │
                                           ▼
                                     ListView Component
```

### Example 3: Real-time Calculations
```
Quantity (Observable) ──┬──► Total (Computed)
Price (Observable) ─────┘         │
                                 ▼
                           Display Component
```

## Performance Optimizations

### 1. Lazy Evaluation
```
Computed Property
      │
      ├──► Not Accessed = No Calculation
      │
      └──► First Access = Calculate & Cache
```

### 2. Batch Updates
```
StartBatch()
  │
  ├──► Update Property 1
  ├──► Update Property 2
  ├──► Update Property 3
  │
EndBatch() ──► Single Notification
```

### 3. Weak References
```
Component ◄──weak──► Observable
    │                    │
    └── Can be GC'd ────┘
```

## Collection Operations

### Supported Operations
```
┌─────────────────────────────────┐
│   ObservableCollection<T>       │
├─────────────────────────────────┤
│ • Add(T item)                   │
│ • Remove(T item)                │
│ • Insert(int index, T item)     │
│ • Clear()                       │
│ • Replace(T old, T new)         │
│ • Move(int from, int to)        │
│ • Sort()                        │
│ • Filter()                      │
└─────────────────────────────────┘
```

### Collection Extensions
Provided through ObservableCollectionExtensions:
- Reactive LINQ operators
- Projection methods
- Aggregation functions
- Transformation utilities

## Memory Management

### Subscription Cleanup
```
Component Subscribe ──► Observable
         │                   │
    Dispose() ──► Unsubscribe
                      │
                  Release Memory
```

### Object Pooling
- Reusable event args
- Pooled collection enumerators
- Cached delegate instances

## Thread Safety

### Synchronization Strategy
```
┌──────────────────────────┐
│   Read Operations        │──► Lock-free
├──────────────────────────┤
│   Write Operations       │──► Fine-grained locks
├──────────────────────────┤
│   Batch Operations       │──► Exclusive lock
└──────────────────────────┘
```

### Cross-thread Notifications
- Automatic marshaling to UI thread
- Configurable synchronization context
- Async-safe operations

## Testing Support

### Internal Visibility
```xml
<InternalsVisibleTo Include="Andy.TUI.Core.Tests" />
<InternalsVisibleTo Include="Andy.TUI.Observable.Tests" />
```

### Test Scenarios
1. **Property Changes**: Value updates and notifications
2. **Computed Updates**: Dependency tracking and recalculation
3. **Collection Operations**: Add/remove/update scenarios
4. **Memory Leaks**: Subscription cleanup verification
5. **Thread Safety**: Concurrent access tests

## Integration Examples

### With UI Components
```
public class TextBox : Component
{
    ObservableProperty<string> Text;
    ComputedProperty<bool> IsEmpty;
    
    Text changes → Component re-renders
}
```

### With Data Binding
```
Model Property ◄──► Observable ◄──► UI Element
                Two-way Binding
```

### With Validation
```
Input Fields ──► Computed Validation ──► Error Display
                     Rules
```

## Best Practices

### For Property Design
1. Use ObservableProperty for mutable state
2. Use ComputedProperty for derived values
3. Avoid circular dependencies
4. Implement proper equality comparison

### For Collection Usage
1. Use batch operations for multiple changes
2. Prefer immutable operations when possible
3. Clean up subscriptions properly
4. Consider performance for large collections

### For Memory Management
1. Dispose subscriptions when done
2. Use weak references for long-lived observables
3. Avoid capturing unnecessary closures
4. Profile memory usage in production

## Advanced Features

### Custom Observable Types
- Create specialized observable implementations
- Custom change detection logic
- Domain-specific notifications

### Reactive Extensions
- Time-based operations (throttle, debounce)
- Combination operators (merge, combine)
- Transformation operators (map, filter)

### Debug Support
- Dependency graph visualization
- Change history tracking
- Performance profiling hooks