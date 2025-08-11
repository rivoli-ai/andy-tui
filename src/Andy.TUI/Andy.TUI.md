# Andy.TUI Project Documentation

## Overview

Andy.TUI is the main distribution package and entry point for the Andy Terminal User Interface framework. This project serves as the public-facing API surface, aggregating all framework components into a cohesive, publishable NuGet package for developers to consume.

## Project Configuration

### Target Framework
- **.NET 8.0** with modern C# features
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **Single-File Publishing**: Supported with IL3000 warnings suppressed

### NuGet Package Metadata
```
Package ID:      Andy.TUI
Version:         1.0.0
License:         Apache-2.0
Repository:      https://github.com/rivoli-ai/andy-tui
Tags:            tui, terminal, console, reactive, component-based
```

## Dependencies Architecture

```
                    Andy.TUI (Main Package)
                         │
        ┌────────────────┼────────────────┐
        │                │                │
        ▼                ▼                ▼
  Andy.TUI.Core   Andy.TUI.Declarative  Andy.TUI.Terminal
        │                │                │
        │                └────────┬───────┘
        │                         │
        └──────────────┬──────────┘
                       │
                External Packages:
                       │
            ┌──────────┴──────────┐
            │                     │
   Microsoft.Extensions    Microsoft.Extensions
      .Hosting            .Caching.Memory
      (8.0.0)                (8.0.1)
```

## Core Responsibilities

### 1. Package Aggregation
Bundles all Andy.TUI components into a single, distributable package:
- Includes all project references as part of the package
- Manages transitive dependencies
- Provides unified versioning

### 2. Public API Surface
Defines the main exports and entry points through `Exports.cs`:
- Framework initialization methods
- Builder patterns for application setup
- Extension methods for hosting integration

### 3. Host Integration
Integrates with Microsoft.Extensions.Hosting for modern .NET applications:
- Service registration
- Dependency injection support
- Application lifecycle management

### 4. Caching Infrastructure
Provides memory caching capabilities via Microsoft.Extensions.Caching.Memory:
- Component state caching
- Render optimization caching
- Resource pooling

## Package Structure

```
Andy.TUI.nupkg
│
├── lib/net8.0/
│   ├── Andy.TUI.dll
│   ├── Andy.TUI.Core.dll
│   ├── Andy.TUI.Declarative.dll
│   ├── Andy.TUI.Terminal.dll
│   ├── Andy.TUI.Layout.dll
│   ├── Andy.TUI.VirtualDom.dll
│   ├── Andy.TUI.Observable.dll
│   ├── Andy.TUI.Spatial.dll
│   └── Andy.TUI.Diagnostics.dll
│
├── README.md
├── andy_tui_icon.png
└── LICENSE
```

## Build Configuration

### Package Generation
```xml
<GeneratePackageOnBuild>false</GeneratePackageOnBuild>
```
Manual package generation for controlled releases.

### Symbol Package
```xml
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```
Generates .snupkg for debugging support.

### Custom Build Target
The project includes a custom MSBuild target to bundle referenced projects:
```
CopyProjectReferencesToPackage
```
This ensures all dependent assemblies are included in the NuGet package.

## Usage Examples

### Example 1: Basic Application Setup
```
Application Bootstrap → Andy.TUI → Host Builder → Service Registration → Run
```

### Example 2: Declarative UI Creation
```
UI Definition → Andy.TUI.Declarative → Component Tree → Rendering
```

### Example 3: Terminal Interaction
```
User Input → Andy.TUI.Terminal → Event Processing → UI Update
```

## Integration Patterns

### With ASP.NET Core
```
┌──────────────────┐
│  ASP.NET Core    │
│   Application    │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Andy.TUI Host   │
│   Integration    │
└──────────────────┘
```

### With Console Applications
```
┌──────────────────┐
│ Console App Main │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│   Andy.TUI       │
│   Initialize     │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│   Run Loop       │
└──────────────────┘
```

### With Worker Services
```
┌──────────────────┐
│  Worker Service  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Background TUI  │
│    Monitoring    │
└──────────────────┘
```

## Public API Surface

### Main Entry Points
1. **Application Builder**: Fluent API for configuring TUI applications
2. **Component Registration**: Methods for registering custom components
3. **Theme Configuration**: APIs for customizing appearance
4. **Event Handlers**: Global event subscription mechanisms

### Extension Methods
- Host builder extensions for service registration
- Terminal configuration extensions
- Rendering pipeline customization

## Performance Characteristics

### Memory Management
- Utilizes IMemoryCache for efficient resource management
- Implements object pooling for frequently allocated types
- Supports incremental rendering for large UIs

### Startup Performance
- Lazy initialization of subsystems
- On-demand component loading
- Minimal reflection usage

## Distribution

### NuGet Package
- Published to NuGet.org
- Supports package restore
- Compatible with .NET CLI, Visual Studio, and Rider

### Versioning Strategy
- Follows Semantic Versioning (SemVer)
- Major.Minor.Patch format
- Breaking changes increment major version

## Development Guidelines

### Adding New Public APIs
1. Define in Exports.cs or appropriate public class
2. Add XML documentation
3. Consider backward compatibility
4. Update package version appropriately

### Testing Package Distribution
1. Build package locally: `dotnet pack`
2. Test in sample application
3. Verify all dependencies are included
4. Check symbol package generation