using System;
using System.Threading;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for Spinner component.
/// </summary>
public class SpinnerInstance : ViewInstance
{
    private SpinnerStyle _style = SpinnerStyle.Dots;
    private string[] _frames = Array.Empty<string>();
    private Color _color = Color.Cyan;
    private string _label = "";
    private bool _labelFirst = false;
    private int _frameDelay = 80;
    private int _currentFrame = 0;
    private DateTime _lastFrameTime = DateTime.UtcNow;
    private Timer? _animationTimer;

    public SpinnerInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Spinner spinner)
            throw new InvalidOperationException($"Expected Spinner, got {viewDeclaration.GetType()}");

        _style = spinner.GetStyle();
        _color = spinner.GetColor();
        _label = spinner.GetLabel();
        _labelFirst = spinner.GetLabelFirst();
        _frameDelay = spinner.GetFrameDelay();

        // Get frames based on style
        if (_style == SpinnerStyle.Custom)
        {
            _frames = spinner.GetCustomFrames();
        }
        else
        {
            _frames = Spinner.GetFrames(_style);
        }

        // Start animation if not already running
        if (_animationTimer == null && _frames.Length > 0)
        {
            _animationTimer = new Timer(OnAnimationTick, null, _frameDelay, _frameDelay);
        }
    }

    private void OnAnimationTick(object? state)
    {
        _currentFrame = (_currentFrame + 1) % _frames.Length;
        InvalidateView();
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var frameWidth = _frames.Length > 0 ? _frames[0].Length : 1;
        var labelWidth = _label.Length;
        var spacing = string.IsNullOrEmpty(_label) ? 0 : 1;

        var totalWidth = frameWidth + spacing + labelWidth;

        return new LayoutBox
        {
            Width = Math.Min(totalWidth, constraints.MaxWidth),
            Height = 1
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_frames.Length == 0)
        {
            return Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode("(No frames)"))
                .Build();
        }

        var frame = _frames[_currentFrame];
        var text = "";

        if (_labelFirst && !string.IsNullOrEmpty(_label))
        {
            text = $"{_label} {frame}";
        }
        else if (!string.IsNullOrEmpty(_label))
        {
            text = $"{frame} {_label}";
        }
        else
        {
            text = frame;
        }

        return Element("text")
            .WithProp("style", Style.Default.WithForegroundColor(_color))
            .WithProp("x", (int)layout.AbsoluteX)
            .WithProp("y", (int)layout.AbsoluteY)
            .WithChild(new TextNode(text))
            .Build();
    }

    public override void Dispose()
    {
        _animationTimer?.Dispose();
        _animationTimer = null;
        base.Dispose();
    }
}