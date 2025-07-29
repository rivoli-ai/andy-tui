// This file ensures that the public APIs from Core, Components, and Terminal are accessible
// when users reference the Andy.TUI NuGet package.

// Re-export namespaces for easier access
global using Andy.TUI.Core;
global using Andy.TUI.Components;
global using Andy.TUI.Components.EventHandling;
global using Andy.TUI.Core.Observable;
global using Andy.TUI.Core.VirtualDom;
global using Andy.TUI.Terminal;