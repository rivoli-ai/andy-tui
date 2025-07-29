using System;
using System.Collections.Generic;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Display;

/// <summary>
/// An animated spinner component for indicating loading or processing.
/// </summary>
public class Spinner : LayoutComponent
{
    private static readonly Dictionary<SpinnerStyle, string[]> SpinnerFrames = new()
    {
        [SpinnerStyle.Dots] = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" },
        [SpinnerStyle.Line] = new[] { "-", "\\", "|", "/" },
        [SpinnerStyle.Arrow] = new[] { "←", "↖", "↑", "↗", "→", "↘", "↓", "↙" },
        [SpinnerStyle.Bar] = new[] { "▁", "▂", "▃", "▄", "▅", "▆", "▇", "█", "▇", "▆", "▅", "▄", "▃", "▂" },
        [SpinnerStyle.Circle] = new[] { "◐", "◓", "◑", "◒" },
        [SpinnerStyle.Square] = new[] { "◰", "◳", "◲", "◱" },
        [SpinnerStyle.Clock] = new[] { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛" },
        [SpinnerStyle.Bounce] = new[] { "⠁", "⠂", "⠄", "⠂" },
        [SpinnerStyle.Box] = new[] { "▖", "▘", "▝", "▗" },
        [SpinnerStyle.Star] = new[] { "✶", "✸", "✹", "✺", "✹", "✸" },
        [SpinnerStyle.Dots2] = new[] { "⣾", "⣽", "⣻", "⢿", "⡿", "⣟", "⣯", "⣷" },
        [SpinnerStyle.Dots3] = new[] { "⠋", "⠙", "⠚", "⠞", "⠖", "⠦", "⠴", "⠲", "⠳", "⠓" },
        [SpinnerStyle.Pipe] = new[] { "┤", "┘", "┴", "└", "├", "┌", "┬", "┐" },
        [SpinnerStyle.SimpleDots] = new[] { ".  ", ".. ", "...", "   " },
        [SpinnerStyle.SimpleDotsScrolling] = new[] { ".  ", ".. ", "...", " ..", "  .", "   " },
        [SpinnerStyle.GrowingDots] = new[] { "·", "••", "•••", "••••", "•••", "••", "·", " " },
        [SpinnerStyle.Balloon] = new[] { " ", ".", "o", "O", "@", "*", " " },
        [SpinnerStyle.Flip] = new[] { "_", "_", "_", "-", "`", "`", "'", "´", "-", "_", "_", "_" },
        [SpinnerStyle.Hamburger] = new[] { "☱", "☲", "☴" },
        [SpinnerStyle.GrowingBlock] = new[] { "▁", "▃", "▄", "▅", "▆", "▇", "█", "▇", "▆", "▅", "▄", "▃" },
        [SpinnerStyle.Arc] = new[] { "◜", "◠", "◝", "◞", "◡", "◟" },
        [SpinnerStyle.Toggle] = new[] { "⊶", "⊷" },
        [SpinnerStyle.Boxes] = new[] { "▌", "▀", "▐", "▄" },
        [SpinnerStyle.Earth] = new[] { "🌍", "🌎", "🌏" },
        [SpinnerStyle.Moon] = new[] { "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘" },
        [SpinnerStyle.Runner] = new[] { "🚶", "🏃" },
        [SpinnerStyle.Pong] = new[] { "▐⠂       ▌", "▐⠈       ▌", "▐ ⠂      ▌", "▐ ⠠      ▌", "▐  ⡀     ▌", "▐  ⠠     ▌", "▐   ⠂    ▌", "▐   ⠈    ▌", "▐    ⠂   ▌", "▐    ⠠   ▌", "▐     ⡀  ▌", "▐     ⠠  ▌", "▐      ⠂ ▌", "▐      ⠈ ▌", "▐       ⠂▌", "▐       ⠠▌", "▐       ⡀▌", "▐      ⠠ ▌", "▐      ⠂ ▌", "▐     ⠈  ▌", "▐     ⠂  ▌", "▐    ⠠   ▌", "▐    ⡀   ▌", "▐   ⠠    ▌", "▐   ⠂    ▌", "▐  ⠈     ▌", "▐  ⠂     ▌", "▐ ⠠      ▌", "▐ ⡀      ▌", "▐⠠       ▌" }
    };
    
    private System.Timers.Timer? _animationTimer;
    private int _currentFrame = 0;
    private SpinnerStyle _style = SpinnerStyle.Dots;
    private bool _isAnimating = false;
    private string _text = string.Empty;
    private int _animationSpeed = 100;
    private Color _color = Color.Cyan;
    
    /// <summary>
    /// Gets or sets the spinner style.
    /// </summary>
    public SpinnerStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            _currentFrame = 0;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the text to display next to the spinner.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value ?? string.Empty;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets the animation speed in milliseconds.
    /// </summary>
    public int AnimationSpeed
    {
        get => _animationSpeed;
        set
        {
            _animationSpeed = Math.Max(50, value);
            if (_animationTimer != null)
            {
                _animationTimer.Interval = _animationSpeed;
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the spinner color.
    /// </summary>
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets or sets whether the spinner is animating.
    /// </summary>
    public bool IsAnimating
    {
        get => _isAnimating;
        set
        {
            if (_isAnimating != value)
            {
                _isAnimating = value;
                if (_isAnimating)
                    StartAnimation();
                else
                    StopAnimation();
            }
        }
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        
        _animationTimer = new System.Timers.Timer(_animationSpeed);
        _animationTimer.Elapsed += OnAnimationTick;
        
        if (_isAnimating)
            StartAnimation();
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        var frames = GetFrames();
        var maxFrameWidth = 0;
        
        foreach (var frame in frames)
        {
            maxFrameWidth = Math.Max(maxFrameWidth, frame.Length);
        }
        
        var width = maxFrameWidth;
        if (!string.IsNullOrEmpty(_text))
            width += 1 + _text.Length; // Space + text
            
        return new Size(width, 1);
    }
    
    protected override void ArrangeCore(Rectangle finalRect)
    {
        // Base arranges the bounds
    }
    
    protected override VirtualNode OnRender()
    {
        var frames = GetFrames();
        if (frames.Length == 0)
            return new ElementNode("spinner", new Dictionary<string, object?>
            {
                ["x"] = Bounds.X,
                ["y"] = Bounds.Y,
                ["width"] = Bounds.Width,
                ["height"] = Bounds.Height
            });
            
        var frame = frames[_currentFrame % frames.Length];
        var content = frame;
        
        if (!string.IsNullOrEmpty(_text))
            content += " " + _text;
            
        return new ElementNode("spinner", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height
        }, new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = 0,
            ["y"] = 0,
            ["style"] = Terminal.Style.Default.WithForegroundColor(_color)
        }, new TextNode(content)));
    }
    
    private string[] GetFrames()
    {
        return SpinnerFrames.TryGetValue(_style, out var frames) ? frames : Array.Empty<string>();
    }
    
    private void StartAnimation()
    {
        _animationTimer?.Start();
    }
    
    private void StopAnimation()
    {
        _animationTimer?.Stop();
        _isAnimating = false;
    }
    
    private void OnAnimationTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _currentFrame++;
        
        // Request render on UI thread
        RequestRender();
    }
    
    /// <summary>
    /// Starts the spinner animation.
    /// </summary>
    public void Start()
    {
        IsAnimating = true;
    }
    
    /// <summary>
    /// Stops the spinner animation.
    /// </summary>
    public void Stop()
    {
        IsAnimating = false;
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopAnimation();
            _animationTimer?.Dispose();
            _animationTimer = null;
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Defines spinner animation styles.
/// </summary>
public enum SpinnerStyle
{
    /// <summary>
    /// Braille dots pattern.
    /// </summary>
    Dots,
    
    /// <summary>
    /// Simple line rotation.
    /// </summary>
    Line,
    
    /// <summary>
    /// Rotating arrow.
    /// </summary>
    Arrow,
    
    /// <summary>
    /// Growing/shrinking bar.
    /// </summary>
    Bar,
    
    /// <summary>
    /// Rotating circle quarters.
    /// </summary>
    Circle,
    
    /// <summary>
    /// Rotating square corners.
    /// </summary>
    Square,
    
    /// <summary>
    /// Clock faces (requires emoji support).
    /// </summary>
    Clock,
    
    /// <summary>
    /// Bouncing dots.
    /// </summary>
    Bounce,
    
    /// <summary>
    /// Box corners.
    /// </summary>
    Box,
    
    /// <summary>
    /// Star animation.
    /// </summary>
    Star,
    
    /// <summary>
    /// Braille dots pattern 2.
    /// </summary>
    Dots2,
    
    /// <summary>
    /// Braille dots pattern 3.
    /// </summary>
    Dots3,
    
    /// <summary>
    /// Pipe characters.
    /// </summary>
    Pipe,
    
    /// <summary>
    /// Simple dots.
    /// </summary>
    SimpleDots,
    
    /// <summary>
    /// Simple dots scrolling.
    /// </summary>
    SimpleDotsScrolling,
    
    /// <summary>
    /// Growing dots.
    /// </summary>
    GrowingDots,
    
    /// <summary>
    /// Balloon animation.
    /// </summary>
    Balloon,
    
    /// <summary>
    /// Flip animation.
    /// </summary>
    Flip,
    
    /// <summary>
    /// Hamburger menu animation.
    /// </summary>
    Hamburger,
    
    /// <summary>
    /// Growing block.
    /// </summary>
    GrowingBlock,
    
    /// <summary>
    /// Arc animation.
    /// </summary>
    Arc,
    
    /// <summary>
    /// Toggle animation.
    /// </summary>
    Toggle,
    
    /// <summary>
    /// Box rotation.
    /// </summary>
    Boxes,
    
    /// <summary>
    /// Earth emoji (requires emoji support).
    /// </summary>
    Earth,
    
    /// <summary>
    /// Moon phases (requires emoji support).
    /// </summary>
    Moon,
    
    /// <summary>
    /// Runner emoji (requires emoji support).
    /// </summary>
    Runner,
    
    /// <summary>
    /// Pong game animation.
    /// </summary>
    Pong
}