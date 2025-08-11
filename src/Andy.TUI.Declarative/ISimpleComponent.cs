using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative;

/// <summary>
/// Interface for simple declarative components.
/// </summary>
public interface ISimpleComponent
{
    VirtualNode Render();
}