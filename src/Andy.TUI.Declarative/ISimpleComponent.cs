using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Declarative;

/// <summary>
/// Interface for simple declarative components.
/// </summary>
public interface ISimpleComponent
{
    VirtualNode Render();
}