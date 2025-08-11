# Debug Logging System

Andy.TUI includes a comprehensive debug logging system to help diagnose issues during development.

## Overview

The debug logging system provides:
- File-based logging with automatic rotation
- Category-based organization
- Configurable log levels
- Cross-platform support
- Minimal performance impact when disabled

## Architecture

### Core Components

#### ILogger Interface
```csharp
public interface ILogger
{
    void Debug(string message, params object[] args);
    void Info(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Error(string message, params object[] args);
    void Error(Exception exception, string message, params object[] args);
    ILogger ForCategory(string category);
}
```

#### DebugContext
Global static class that manages logging initialization:
- Checks `ANDY_TUI_DEBUG` environment variable
- Creates log directory structure
- Initializes file logger
- Provides global `Logger` instance

#### FileLogger
Concrete implementation that:
- Writes logs to timestamped files
- Creates separate files per category
- Handles thread-safe file operations
- Formats log entries with timestamps and levels

## Usage

### Enabling Debug Logging (environment variables)

```bash
# Enable with default level (Debug)
export ANDY_TUI_DEBUG=1

# Set specific log level (Debug, Info, Warning, Error)
export ANDY_TUI_DEBUG=Info

# Custom log directory
export ANDY_TUI_DEBUG_DIR=/path/to/logs

# Run application
dotnet run --project examples/Andy.TUI.Examples.Input
```

### Comprehensive logging initializer (tests and apps)

In addition to the lightweight `DebugContext`, the diagnostics package provides a comprehensive initializer that writes categorized logs, disables console noise in tests, and exports failure logs automatically.

```csharp
using Andy.TUI.Diagnostics;

// One-time app startup
ComprehensiveLoggingInitializer.Initialize();

// For tests
using (ComprehensiveLoggingInitializer.BeginTestSession("MyTest"))
{
    // ... test code ...
}

// Or explicitly enable test mode
ComprehensiveLoggingInitializer.Initialize(isTestMode: true, customLogPath: "./TestLogs");
```

### In Your Code

```csharp
public class MyComponent : ViewInstance
{
    private readonly ILogger _logger;
    
    public MyComponent(string id) : base(id)
    {
        _logger = DebugContext.Logger.ForCategory("MyComponent");
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        _logger.Debug("Updating component with {0}", viewDeclaration.GetType().Name);
        
        try
        {
            // Component logic
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to update component");
        }
    }
}
```

### Log Output

Logs are organized by timestamp and category (DebugContext) or under `TestLogs/` when using ComprehensiveLoggingInitializer:
```
/tmp/andy-tui-debug/20250804_175311/
├── DeclarativeRenderer.log
├── EventRouter.log
├── FocusManager.log
├── SelectInputInstance.log
└── ViewInstanceManager.log
```

Example log format:
```
[2025-08-04 17:53:11.123] [DEBUG] [DeclarativeRenderer] Render requested, executing render cycle
[2025-08-04 17:53:11.124] [DEBUG] [DeclarativeRenderer] Virtual DOM rendered
[2025-08-04 17:53:11.125] [INFO] [EventRouter] Key pressed: DownArrow (Modifiers: None)
[2025-08-04 17:53:11.126] [DEBUG] [FocusManager] Routing key to focused component: SelectInputInstance_1
```

## Categories

### Core Categories
- **DeclarativeRenderer**: Rendering pipeline and virtual DOM updates
- **VirtualDomRenderer**: Low-level DOM rendering operations
- **EventRouter**: Event dispatching and handling
- **FocusManager**: Focus state and keyboard navigation
- **ViewInstanceManager**: Component lifecycle management

### Component Categories
- **SelectInputInstance**: SelectInput-specific behavior
- **TextFieldInstance**: TextField input handling
- **ButtonInstance**: Button interaction
- Add more as needed using `ForCategory()`

## Best Practices

### Performance
- Logging has minimal impact when disabled
- Use appropriate log levels:
  - `Debug`: Detailed flow information
  - `Info`: Important state changes
  - `Warning`: Potential issues
  - `Error`: Actual failures

### What to Log
- Component lifecycle events (creation, updates, disposal)
- State changes that affect rendering
- Event routing decisions
- Performance metrics (render times, patch counts)
- Error conditions with context

### What NOT to Log
- Every property access
- Frequent operations (e.g., every mouse move)
- Sensitive user data
- Large data structures (use summaries)

## Troubleshooting Common Issues

### UI Not Updating
Check `DeclarativeRenderer.log` for:
- "Render requested" entries
- "Diff completed - N patches" showing changes detected
- "Forced render flush" confirming render execution

### Keyboard Input Not Working
Check `EventRouter.log` and `FocusManager.log` for:
- Key press events being received
- Focus state of components
- Event routing to correct component

### Performance Issues
Look for:
- Excessive render cycles in `DeclarativeRenderer.log`
- Large patch counts indicating inefficient updates
- Repeated component recreations in `ViewInstanceManager.log`

## Integration with Tests

Enable debug logging in tests:
```csharp
// Preferred: structured, per-test session
using (ComprehensiveLoggingInitializer.BeginTestSession(nameof(MyTest)))
{
    // Test code
}

// Lightweight alternative:
Environment.SetEnvironmentVariable("ANDY_TUI_DEBUG", "Debug");
DebugContext.Initialize();
try { /* test */ } finally { DebugContext.Shutdown(); }
```

## Future Enhancements

- [ ] Structured logging with JSON output
- [ ] Log viewer TUI application
- [ ] Remote logging support
- [ ] Performance profiling integration
- [ ] Log filtering and search capabilities