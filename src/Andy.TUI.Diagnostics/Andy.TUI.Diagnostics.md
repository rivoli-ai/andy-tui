# Andy.TUI.Diagnostics Project Documentation

## Overview

Andy.TUI.Diagnostics provides comprehensive logging, debugging, and diagnostic capabilities for the Andy TUI framework. This lightweight module offers a flexible logging abstraction with multiple implementations, enabling developers to monitor application behavior, track performance, and debug issues effectively.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Namespace Configuration
```xml
<RootNamespace>Andy.TUI.Core.Diagnostics</RootNamespace>
<AssemblyName>Andy.TUI.Diagnostics</AssemblyName>
```
Note: Maintains backward compatibility with original namespace while using new assembly name.

## Architecture

```
           ILogger (Interface)
                │
    ┌───────────┼───────────┐
    │           │           │
    ▼           ▼           ▼
FileLogger  NullLogger  CustomLogger
    │                    (User Impl)
    │
    └──► DebugContext
         (Shared State)
```

## Core Components

### 1. ILogger Interface
The central abstraction for all logging operations:
- Defines standard logging methods (Info, Warning, Error, Debug)
- Supports structured logging with context
- Enables log level filtering

### 2. FileLogger
File-based logging implementation:
- Writes logs to persistent storage
- Supports log rotation
- Configurable output formatting
- Thread-safe write operations

### 3. NullLogger
No-operation logger for production scenarios:
- Zero performance overhead
- Useful for testing
- Default fallback implementation

### 4. DebugContext
Centralized debugging state management:
- Global debug flags
- Performance counters
- Diagnostic metadata storage
- Runtime inspection capabilities

## Usage Patterns

### Pattern 1: Development Logging
```
Development Mode
      │
      ▼
  FileLogger
      │
      ├──► Console Output
      └──► File Output
           (./logs/andy-tui.log)
```

### Pattern 2: Production Mode
```
Production Mode
      │
      ▼
  NullLogger
      │
      ▼
  No Output
  (Zero Overhead)
```

### Pattern 3: Custom Integration
```
External System
      │
      ▼
Custom ILogger Impl
      │
      ├──► Syslog
      ├──► Application Insights
      └──► Custom Backend
```

## Diagnostic Features

### Performance Tracking
```
┌─────────────────────────┐
│   Performance Timer     │
├─────────────────────────┤
│ • Frame render time     │
│ • Component update time │
│ • Event processing time │
│ • Memory allocations    │
└─────────────────────────┘
```

### Debug Context Capabilities
```
┌─────────────────────────┐
│     Debug Context       │
├─────────────────────────┤
│ • Component tree dump   │
│ • Event trace           │
│ • State snapshots       │
│ • Render statistics     │
└─────────────────────────┘
```

## Log Levels and Categories

### Severity Levels
1. **TRACE**: Detailed execution flow
2. **DEBUG**: Diagnostic information
3. **INFO**: General information
4. **WARNING**: Potential issues
5. **ERROR**: Recoverable errors
6. **FATAL**: Unrecoverable errors

### Log Categories
- **Rendering**: Frame updates, buffer operations
- **Events**: Input handling, event propagation
- **State**: Component state changes
- **Performance**: Timing and resource metrics
- **System**: Terminal operations, platform-specific

## Implementation Examples

### Example 1: Component Lifecycle Logging
```
Component Create → Log(INFO, "Component initialized")
State Change → Log(DEBUG, "State updated: {old} → {new}")
Component Destroy → Log(INFO, "Component disposed")
```

### Example 2: Performance Monitoring
```
Start Timer → Render Frame → Stop Timer → Log(PERF, "Frame: {ms}ms")
```

### Example 3: Error Tracking
```
Try Operation → Catch Exception → Log(ERROR, "Operation failed", exception)
```

## Testing Support

### Internal Visibility
```xml
<InternalsVisibleTo Include="Andy.TUI.Core.Tests" />
```
Allows comprehensive testing of diagnostic components.

### Test Scenarios
1. **Logger Output Verification**: Validate log formatting and content
2. **Performance Overhead**: Measure logging impact
3. **Thread Safety**: Concurrent logging operations
4. **Log Rotation**: File size and age-based rotation

## Configuration Options

### FileLogger Configuration
```
┌──────────────────────────┐
│   FileLogger Options     │
├──────────────────────────┤
│ • FilePath              │
│ • MaxFileSize           │
│ • RotationPolicy        │
│ • OutputFormat          │
│ • BufferSize            │
│ • AsyncWrite            │
└──────────────────────────┘
```

### Debug Context Configuration
```
┌──────────────────────────┐
│  Debug Context Options   │
├──────────────────────────┤
│ • EnableProfiling       │
│ • CaptureStackTraces    │
│ • MaxEventHistory       │
│ • DumpOnError           │
└──────────────────────────┘
```

## Performance Considerations

### Zero-Cost Abstractions
- NullLogger compiles to no-ops
- Conditional compilation for debug code
- Lazy evaluation of log messages

### Buffering Strategy
```
Log Call → Memory Buffer → Batch Write → File System
           (Lock-free)     (Async)       (Periodic)
```

### Memory Management
- Fixed-size circular buffers
- String interning for repeated messages
- Pooled StringBuilder instances

## Integration Points

### Framework Integration
- Automatically injected into all framework components
- Available through dependency injection
- Global static access for convenience

### External Tool Integration
- Structured logging format (JSON)
- ETW/EventSource support (Windows)
- Syslog compatibility (Unix)
- OpenTelemetry export

## Best Practices

### For Framework Development
1. Use appropriate log levels
2. Include contextual information
3. Avoid logging in hot paths
4. Use structured logging format

### For Application Developers
1. Configure appropriate logger for environment
2. Set up log rotation for production
3. Use debug context for troubleshooting
4. Monitor performance metrics

## Future Enhancements

Potential areas for expansion:
- Remote logging capabilities
- Real-time log streaming
- Advanced filtering rules
- Log aggregation support
- Metrics dashboard integration