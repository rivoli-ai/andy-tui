# RenderingSystem Usage Guide

This guide explains how to properly use the RenderingSystem for different scenarios.

## Overview

The RenderingSystem provides a high-level API for terminal rendering with:
- Double buffering for smooth updates
- Dirty region tracking for efficiency
- Frame rate limiting via RenderScheduler
- ANSI color and style support

## Key Components

1. **TerminalBuffer**: Manages double buffering and dirty region tracking
2. **AnsiRenderer**: Generates ANSI escape sequences
3. **RenderScheduler**: Controls frame rate and batches updates
4. **RenderingSystem**: High-level coordinating API

## Usage Patterns

### 1. Static Content (One-time Render)

For content that doesn't change (like system information banners):

```csharp
var terminal = new AnsiTerminal();
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Draw your content
renderingSystem.Clear();
renderingSystem.DrawBox(0, 0, 80, 24, Style.Default, BoxStyle.Single);
renderingSystem.WriteText(10, 10, "Hello World", Style.WithForeground(Color.Green));

// Render once
renderingSystem.Render();

// Wait for user input
Console.ReadKey();

renderingSystem.Shutdown();
```

### 2. Event-Driven Updates

For interactive applications that update only when user input occurs:

```csharp
var terminal = new AnsiTerminal();
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Set a reasonable FPS for responsiveness
renderingSystem.Scheduler.TargetFps = 30;

// Define render function
Action redrawUI = () =>
{
    renderingSystem.Clear();
    // Draw your UI here
};

// Initial draw
redrawUI();
renderingSystem.Render();

// Setup input handler
inputHandler.KeyPressed += (_, e) =>
{
    // Update state based on input
    
    // Queue a redraw
    renderingSystem.Scheduler.QueueRender(redrawUI);
};

// Main loop
while (!exit)
{
    inputHandler.Poll();
    Thread.Sleep(10);
}
```

### 3. Continuous Animation

For applications with continuous animation:

```csharp
var terminal = new AnsiTerminal();
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Set target FPS
renderingSystem.Scheduler.TargetFps = 60;

// Animation state
var frameCount = 0;

// Animation function
Action? animateFrame = null;
animateFrame = () =>
{
    if (shouldExit)
        return;
        
    // Clear and draw frame
    renderingSystem.Clear();
    
    // Update animation state
    frameCount++;
    
    // Draw animated content
    DrawAnimatedContent(renderingSystem, frameCount);
    
    // Queue next frame - let the scheduler control timing!
    renderingSystem.Scheduler.QueueRender(animateFrame);
};

// Start animation
renderingSystem.Scheduler.QueueRender(animateFrame);

// Wait for exit condition
while (!shouldExit)
{
    Thread.Sleep(50);
}

renderingSystem.Shutdown();
```

## Important Notes

### DO NOT:
- Call `QueueRender` in a tight loop with your own timing
- Mix manual `Thread.Sleep` timing with the RenderScheduler
- Call drawing methods directly in a loop without using the scheduler

### DO:
- Let the RenderScheduler control all timing for animations
- Use `QueueRender` with callbacks for all drawing operations
- Queue the next frame from within the current frame's callback
- Use event-driven rendering for interactive applications

## Common Mistakes

### ❌ Wrong: Fighting with the scheduler
```csharp
while (!exit)
{
    renderingSystem.Scheduler.QueueRender(() =>
    {
        // Draw frame
    });
    Thread.Sleep(16); // Don't do this!
}
```

### ✅ Correct: Let the scheduler control timing
```csharp
Action? renderFrame = null;
renderFrame = () =>
{
    if (exit) return;
    
    // Draw frame
    
    // Queue next frame
    renderingSystem.Scheduler.QueueRender(renderFrame);
};

renderingSystem.Scheduler.QueueRender(renderFrame);
```

## Performance Tips

1. **Batch Operations**: All drawing operations within a single `QueueRender` callback are batched together
2. **Dirty Region Tracking**: The system automatically tracks what has changed
3. **Frame Rate**: Set appropriate FPS - 30 FPS for interactive apps, 60 FPS for smooth animations
4. **Buffer Access**: Use `Buffer.SetCell` for individual character updates instead of `WriteText` for single chars

## Migration from TerminalRenderer

If migrating from the old `TerminalRenderer`:

1. Replace `BeginFrame`/`EndFrame` pattern with `QueueRender` callbacks
2. Remove manual frame timing loops
3. Let the RenderScheduler handle all timing
4. Use event-driven patterns where appropriate