// This file ensures that the public APIs from Core, Declarative, and Terminal are accessible
// when users reference the Andy.TUI NuGet package.

// Re-export namespaces for easier access
global using Andy.TUI.Core;
global using Andy.TUI.Core.Observable;
global using Andy.TUI.Core.VirtualDom;
global using Andy.TUI.Core.Diagnostics;
global using Andy.TUI.Declarative;
global using Andy.TUI.Declarative.Components;
global using Andy.TUI.Declarative.Extensions;
global using Andy.TUI.Declarative.Layout;
global using Andy.TUI.Declarative.Layout; // kept for backward-compat imports
global using Andy.TUI.Declarative.Layout; // aliasing remains
global using Andy.TUI.Declarative.Rendering;
global using Andy.TUI.Declarative.State;
global using Andy.TUI.Terminal;