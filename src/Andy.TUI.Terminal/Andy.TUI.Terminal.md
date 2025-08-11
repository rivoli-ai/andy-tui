# Andy.TUI.Terminal Project Documentation

## Overview

Andy.TUI.Terminal provides the low-level terminal interaction layer for the Andy TUI framework. This module handles direct terminal operations including ANSI escape sequences, cross-platform input management, buffered rendering, color management, and the core rendering pipeline that translates virtual DOM updates into efficient terminal output.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled

### Build Properties
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```
Strict compilation with full documentation and zero warnings.

### Dependencies
```xml
<ProjectReference Include="..\Andy.TUI.Core\Andy.TUI.Core.csproj" />
```

## Core Architecture

```
         Application Layer
               │
         Virtual DOM
               │
      RenderingSystem
               │
    ┌──────────┼──────────┐
    │          │          │
IRenderer  ITerminal  IInputManager
    │          │          │
    ▼          ▼          ▼
AnsiRenderer AnsiTerminal InputHandlers
    │          │          │
    └──────────┼──────────┘
               │
         Terminal I/O
         (stdout/stdin)
```

## Key Components

### 1. ITerminal Interface
Abstraction for terminal operations:
- Cursor positioning
- Screen clearing
- Size detection
- Color support detection
- Raw mode control

### 2. AnsiTerminal
ANSI escape sequence implementation:
- Cross-platform ANSI codes
- Terminal capability detection
- Buffered output
- Performance optimizations

### 3. IRenderer Interface
Rendering abstraction:
- Draw operations
- Clipping support
- Style management
- Batch rendering

### 4. AnsiRenderer
ANSI-based rendering implementation:
- Optimized escape sequences
- Differential updates
- Color management
- Style attributes

### 5. Buffer
Double-buffering system:
```
Front Buffer (Display) ←→ Back Buffer (Working)
         │                        │
    Current View            Next Frame
```

### 6. Cell
Individual terminal cell representation:
```
┌─────────────────────────┐
│        Cell             │
├─────────────────────────┤
│ • Character (Rune)      │
│ • Foreground Color      │
│ • Background Color      │
│ • Style Attributes      │
└─────────────────────────┘
```

### 7. Color
Advanced color management:
- 16-color support
- 256-color support
- True color (24-bit RGB)
- Color degradation for compatibility

## Input Management System

### Input Architecture
```
       Terminal Input
             │
    CrossPlatformInputManager
             │
    ┌────────┼────────┐
    │        │        │
Console  Enhanced  Custom
Handler   Handler  Handler
    │        │        │
    └────────┼────────┘
             │
        InputEvent
             │
      Event Processing
```

### Input Handlers

#### ConsoleInputHandler
Basic console key input:
- Standard key detection
- Modifier key support
- Special key mapping

#### EnhancedConsoleInputHandler
Advanced input features:
- Mouse support
- Extended key combinations
- Paste detection
- Focus events

#### CrossPlatformInputManager
Platform abstraction layer:
- OS-specific handling
- Input mode management
- Event normalization

### InputEvent Types
```
┌─────────────────────────────┐
│      InputEvent Types       │
├─────────────────────────────┤
│ • KeyPress                  │
│ • KeyRelease                │
│ • MouseMove                 │
│ • MouseClick                │
│ • MouseScroll               │
│ • WindowResize              │
│ • Focus/Blur                │
│ • Paste                     │
└─────────────────────────────┘
```

## Rendering Pipeline

### RenderingSystem
Coordinates the entire render process:
```
Frame Start
     │
     ▼
Collect Updates
     │
     ▼
Calculate Dirty Regions
     │
     ▼
Render to Buffer
     │
     ▼
Diff Buffers
     │
     ▼
Generate ANSI
     │
     ▼
Write to Terminal
     │
     ▼
Frame Complete
```

### RenderScheduler
Manages render timing and optimization:
```
Request Render ──► Queue ──► Batch ──► Execute
                     │         │          │
                   Defer    Merge     60 FPS
```

### IRenderingSystem Interface
Core rendering contract:
- Begin/End frame
- Draw primitives
- Clipping regions
- Style stack

## ANSI Escape Sequences

### Cursor Control
```
┌─────────────────────────────────┐
│   Operation    │   ANSI Code    │
├─────────────────────────────────┤
│ Move to X,Y    │ ESC[{Y};{X}H   │
│ Move Up        │ ESC[{n}A       │
│ Move Down      │ ESC[{n}B       │
│ Move Right     │ ESC[{n}C       │
│ Move Left      │ ESC[{n}D       │
│ Save Position  │ ESC[s          │
│ Restore        │ ESC[u          │
└─────────────────────────────────┘
```

### Color Codes
```
┌─────────────────────────────────┐
│    Type       │   ANSI Format   │
├─────────────────────────────────┤
│ 16 Colors     │ ESC[3{0-7}m     │
│ 256 Colors    │ ESC[38;5;{n}m   │
│ RGB Colors    │ ESC[38;2;R;G;Bm │
│ Background    │ ESC[48;...m     │
└─────────────────────────────────┘
```

### Style Attributes
```
┌─────────────────────────────────┐
│   Style       │   ANSI Code     │
├─────────────────────────────────┤
│ Bold          │ ESC[1m          │
│ Italic        │ ESC[3m          │
│ Underline     │ ESC[4m          │
│ Blink         │ ESC[5m          │
│ Reverse       │ ESC[7m          │
│ Strike        │ ESC[9m          │
│ Reset         │ ESC[0m          │
└─────────────────────────────────┘
```

## Buffer Management

### Double Buffering
```
Render Loop:
┌──────────────┐      ┌──────────────┐
│ Front Buffer │ ←──→ │ Back Buffer  │
│  (Display)   │      │   (Working)  │
└──────────────┘      └──────────────┘
        ↓                     ↑
    Terminal              Rendering
     Output               Operations
```

### Differential Updates
```
Old Buffer    New Buffer     Diff Result
┌─────────┐   ┌─────────┐   ┌─────────┐
│ A B C D │   │ A B X D │   │ · · X · │
│ E F G H │ → │ E Y G H │ = │ · Y · · │
│ I J K L │   │ I J K L │   │ · · · · │
└─────────┘   └─────────┘   └─────────┘
                             (Only X,Y sent)
```

## Performance Optimizations

### Batch Operations
```
Multiple Updates ──► Coalesce ──► Single Write
                        │              │
                    Reduce I/O     Atomic Update
```

### Smart Cursor Movement
```
Current: (10, 5)
Target:  (12, 5)

Naive:    ESC[5;12H  (8 bytes)
Optimized: ESC[2C    (4 bytes)
```

### Color Caching
```
Last Color ──► Same? ──► Skip ANSI
    │           │           │
    └── Cache   No      No Output
                │
                ▼
            Set New Color
```

## Cross-Platform Support

### Platform Detection
```
┌──────────────────────────┐
│   Platform Features      │
├──────────────────────────┤
│ Windows:                 │
│ • Windows Terminal       │
│ • ConHost               │
│ • ConPTY                │
├──────────────────────────┤
│ Linux/macOS:            │
│ • TTY detection         │
│ • terminfo support      │
│ • ANSI native           │
└──────────────────────────┘
```

### Capability Detection
```
Terminal ──► Query ──► Capabilities
              │            │
          Version      • Colors
          String       • Mouse
                      • Unicode
```

## Usage Examples

### Example 1: Basic Drawing
```
Terminal Initialize
      │
      ▼
Clear Screen
      │
      ▼
Draw Text at (10, 5)
      │
      ▼
Set Color to Red
      │
      ▼
Draw Box Border
      │
      ▼
Flush to Terminal
```

### Example 2: Input Loop
```
while (running) {
    Input Event ──► Process ──► Update State
                      │            │
                   Validate    Trigger Render
}
```

### Example 3: Animation
```
Frame 1 ──► Buffer ──► Diff ──► Update
   │
Frame 2 ──► Buffer ──► Diff ──► Update
   │
  ...   (60 FPS target)
```

## Error Handling

### Terminal State Recovery
```
Error Occurs ──► Save State ──► Reset Terminal
                      │              │
                  Attempt Fix    Restore State
```

### Graceful Degradation
```
True Color ──► Not Supported? ──► 256 Color
                     │                │
                     └──► 16 Color ◄──┘
```

## Testing Considerations

### Mock Terminal
- Virtual buffer for testing
- Simulated input events
- Predictable behavior
- No actual terminal I/O

### Performance Testing
- Render throughput
- Input latency
- Buffer efficiency
- Memory usage

## Best Practices

### For Rendering
1. Minimize cursor movements
2. Batch color changes
3. Use differential updates
4. Clear only changed regions
5. Respect terminal capabilities

### For Input
1. Handle platform differences
2. Provide keyboard shortcuts
3. Support mouse when available
4. Implement proper focus management
5. Handle paste events correctly

### For Performance
1. Use double buffering
2. Implement dirty region tracking
3. Optimize ANSI sequences
4. Cache color and style state
5. Profile hot paths