using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Spinner animation styles.
/// </summary>
public enum SpinnerStyle
{
    Dots,       // ⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏
    Line,       // ─\|/
    Arrow,      // ←↖↑↗→↘↓↙
    Box,        // ▖▘▝▗
    Circle,     // ◐◓◑◒
    Star,       // ✶✸✹✺✹✸
    Bounce,     // ⠁⠂⠄⠂
    Pulse,      // ▁▃▅▇▅▃
    Custom
}

/// <summary>
/// An animated spinner component.
/// </summary>
public class Spinner : ISimpleComponent
{
    private readonly SpinnerStyle _style;
    private readonly string[] _customFrames;
    private readonly Color _color;
    private readonly string _label;
    private readonly bool _labelFirst;
    private readonly int _frameDelay;
    
    public Spinner(
        SpinnerStyle style = SpinnerStyle.Dots,
        string[]? customFrames = null,
        Color? color = null,
        string label = "",
        bool labelFirst = false,
        int frameDelay = 80)
    {
        _style = style;
        _customFrames = customFrames ?? Array.Empty<string>();
        _color = color ?? Color.Cyan;
        _label = label ?? "";
        _labelFirst = labelFirst;
        _frameDelay = Math.Max(10, frameDelay);
    }
    
    // Internal accessors for view instance
    internal SpinnerStyle GetStyle() => _style;
    internal string[] GetCustomFrames() => _customFrames;
    internal Color GetColor() => _color;
    internal string GetLabel() => _label;
    internal bool GetLabelFirst() => _labelFirst;
    internal int GetFrameDelay() => _frameDelay;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("Spinner declarations should not be rendered directly. Use ViewInstanceManager.");
    }
    
    // Helper method to get spinner frames
    public static string[] GetFrames(SpinnerStyle style)
    {
        return style switch
        {
            SpinnerStyle.Dots => new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" },
            SpinnerStyle.Line => new[] { "─", "\\", "|", "/" },
            SpinnerStyle.Arrow => new[] { "←", "↖", "↑", "↗", "→", "↘", "↓", "↙" },
            SpinnerStyle.Box => new[] { "▖", "▘", "▝", "▗" },
            SpinnerStyle.Circle => new[] { "◐", "◓", "◑", "◒" },
            SpinnerStyle.Star => new[] { "✶", "✸", "✹", "✺", "✹", "✸" },
            SpinnerStyle.Bounce => new[] { "⠁", "⠂", "⠄", "⠂" },
            SpinnerStyle.Pulse => new[] { "▁", "▃", "▅", "▇", "▅", "▃" },
            SpinnerStyle.Custom => Array.Empty<string>(),
            _ => new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" }
        };
    }
}