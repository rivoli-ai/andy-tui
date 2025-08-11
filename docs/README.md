# Documentation Index

A guided index of the documentation for Andy.TUI. This page lists the most useful docs by topic and audience.

## Getting Oriented
- Architecture: DECLARATIVE_ARCHITECTURE.md — Overview of the declarative architecture, components, layout, and pipeline.
- Comparison: andy-vs-bubbletea.md — Side-by-side feature comparison with Charm's Bubble Tea.

## Project Documentation
Detailed documentation for each project in the framework:
- [Andy.TUI.Core](../src/Andy.TUI.Core/Andy.TUI.Core.md) — Core orchestration and framework integration
- [Andy.TUI.Terminal](../src/Andy.TUI.Terminal/Andy.TUI.Terminal.md) — Low-level terminal operations and rendering
- [Andy.TUI.Declarative](../src/Andy.TUI.Declarative/Andy.TUI.Declarative.md) — SwiftUI-inspired declarative API
- [Andy.TUI.Layout](../src/Andy.TUI.Layout/Andy.TUI.Layout.md) — Flexbox-based layout engine with constraints
- [Andy.TUI.VirtualDom](../src/Andy.TUI.VirtualDom/Andy.TUI.VirtualDom.md) — Virtual DOM diffing and patching
- [Andy.TUI.Observable](../src/Andy.TUI.Observable/Andy.TUI.Observable.md) — Reactive state management
- [Andy.TUI.Spatial](../src/Andy.TUI.Spatial/Andy.TUI.Spatial.md) — Spatial indexing and occlusion culling
- [Andy.TUI.Diagnostics](../src/Andy.TUI.Diagnostics/Andy.TUI.Diagnostics.md) — Logging and debugging tools
- [Andy.TUI](../src/Andy.TUI/Andy.TUI.md) — Main distribution package

## Core APIs
- Virtual DOM API: VIRTUAL_DOM_API.md — Virtual nodes, builder API, diff engine, patches, and usage patterns.
- Terminal API: TERMINAL_API.md — `ITerminal`, `AnsiTerminal`, styles, buffers, and usage examples.
- Rendering System: RENDERING_SYSTEM_USAGE.md — `RenderingSystem`, `RenderScheduler`, dirty region updates, and patterns.
- Observable API: OBSERVABLE_API.md — Observable properties, computed properties, collections, and subscription patterns.

## Advanced Topics
- Z-Index Architecture: Z_INDEX_ARCHITECTURE.md — Relative vs absolute z-index, propagation, and rendering strategies.
- Spatial Index Design: SPATIAL_INDEX_DESIGN.md — 3D spatial index, occlusion handling, and performance considerations.
- Layout Testing Plan: LAYOUT_TESTING_PLAN.md — Strategy for validating layout rules and edge cases.
- Diff Engine Testing Plan: DIFF_ENGINE_TESTING_PLAN.md — Test strategy for movements, resizing, and overlap scenarios.

## Tooling & Debugging
- Debug Logging: DEBUG_LOGGING.md — Enabling logs, categories, locations, and best practices.

## Examples
- Observable: `examples/Andy.TUI.Examples/ObservableSystemExample.cs`
- Observable Collection: `examples/Andy.TUI.Examples/ObservableCollectionExample.cs`
- Virtual DOM: `examples/VirtualDom/BasicVirtualDomExample.cs`, `AdvancedVirtualDomExample.cs`, `ReactiveVirtualDomExample.cs`
- Terminal basics: `examples/Andy.TUI.Examples.Terminal/BasicTerminalExample.cs`
- Rendering system: `examples/Andy.TUI.Examples.Terminal/RenderingSystemExample.cs`, `AdvancedRenderingExample.cs`
- Styled text: `examples/Andy.TUI.Examples.Terminal/StyledTextExample.cs`
- Declarative menu: `examples/Andy.TUI.Examples.Input/Program.cs`
- Declarative widgets: `examples/Andy.TUI.Examples.Input/UIComponentsShowcase.cs`, `TableTest.cs`, `SelectInputTest.cs`
- Z-Index and tabs: `examples/Andy.TUI.Examples.ZIndex/TabViewWorkingExample.cs`, `MinimalZIndexExample.cs`

## Status & Planning
- Implementation Status (Detailed): IMPLEMENTATION_STATUS_DETAILED.md — Current status and progress.
- Declarative Implementation Plan: DECLARATIVE_IMPLEMENTATION_PLAN.md — Phases and roadmap for declarative stack.
- Layout Fixes Summary: LAYOUT_FIXES_SUMMARY.md — Summary of layout changes and fixes.
- Next Steps: NEXT_STEPS.md — Short-term action items and priorities.

## Deprecated
- Historical feature comparison (removed): superseded by `andy-vs-bubbletea.md`.

If you spot inconsistencies, open an issue or PR — keeping docs aligned with code is a priority.
