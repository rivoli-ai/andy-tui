using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Input;

/// <summary>
/// A button component that can be clicked or activated with keyboard.
/// </summary>
public class Button : InputComponent
{
    private bool _isPressed;
    
    /// <summary>
    /// Gets or sets the button text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the button style.
    /// </summary>
    public ButtonStyle Style { get; set; } = ButtonStyle.Default;
    
    /// <summary>
    /// Gets or sets whether the button is the default action button.
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets whether the button is a cancel button.
    /// </summary>
    public bool IsCancel { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum width of the button.
    /// </summary>
    public new int MinWidth { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the icon to display before the text.
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// Gets whether the button is currently pressed.
    /// </summary>
    public bool IsPressed => _isPressed;
    
    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    public event EventHandler? Click;
    
    protected override Size MeasureCore(Size availableSize)
    {
        var textWidth = Text.Length;
        if (!string.IsNullOrEmpty(Icon))
            textWidth += Icon.Length + 1; // Icon + space
            
        var width = Math.Max(textWidth + Padding.Horizontal + 2, MinWidth); // +2 for borders
        var height = 1 + Padding.Vertical + 2; // +2 for borders
        
        return new Size(
            Math.Min(width, availableSize.Width),
            Math.Min(height, availableSize.Height));
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        // Base handles bounds
    }
    
    protected override VirtualNode OnRender()
    {
        var children = new List<VirtualNode>();
        var contentBounds = ContentBounds;
        
        // Background
        var bgColor = GetBackgroundColor();
        if (bgColor != Color.None)
        {
            children.Add(CreateBackground(bgColor));
        }
        
        // Button content
        var buttonText = GetButtonText();
        var textColor = GetTextColor();
        
        // Center the text
        var textX = contentBounds.X + (contentBounds.Width - buttonText.Length) / 2;
        var textY = contentBounds.Y + contentBounds.Height / 2;
        
        children.Add(CreateText(textX, textY, buttonText, textColor));
        
        // Border
        var borderStyle = GetBorderStyle();
        var borderColor = GetBorderColor();
        children.Add(CreateBorder(borderStyle, borderColor));
        
        // Focus indicator
        if (IsFocused)
        {
            children.Add(CreateFocusIndicator());
        }
        
        return CreateLayoutNode("button", children.ToArray());
    }
    
    protected override bool OnKeyPress(KeyEventArgs args)
    {
        switch (args.Key)
        {
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                Press();
                return true;
                
            case ConsoleKey.Escape when IsCancel:
                Press();
                return true;
                
            default:
                return false;
        }
    }
    
    protected override bool OnMouseEvent(MouseEventArgs args)
    {
        switch (args.Button)
        {
            case MouseButton.Left when args is { Button: MouseButton.Left }:
                if (IsPointInBounds(args.X, args.Y))
                {
                    Focus();
                    Press();
                    return true;
                }
                break;
        }
        
        return base.OnMouseEvent(args);
    }
    
    /// <summary>
    /// Programmatically clicks the button.
    /// </summary>
    public void PerformClick()
    {
        if (IsEnabled)
        {
            OnClick();
        }
    }
    
    private void Press()
    {
        if (!IsEnabled)
            return;
            
        _isPressed = true;
        RequestRender();
        
        // Simulate button release after a short delay
        Task.Delay(100).ContinueWith(_ =>
        {
            _isPressed = false;
            RequestRender();
            OnClick();
        });
    }
    
    private void OnClick()
    {
        Click?.Invoke(this, EventArgs.Empty);
    }
    
    private string GetButtonText()
    {
        var text = Text;
        
        if (!string.IsNullOrEmpty(Icon))
        {
            text = $"{Icon} {text}";
        }
        
        // Add padding to reach minimum width
        var currentWidth = text.Length + Padding.Horizontal + 2;
        if (currentWidth < MinWidth)
        {
            var paddingNeeded = MinWidth - currentWidth;
            var leftPad = paddingNeeded / 2;
            var rightPad = paddingNeeded - leftPad;
            text = new string(' ', leftPad) + text + new string(' ', rightPad);
        }
        
        return text;
    }
    
    private Color GetBackgroundColor()
    {
        if (!IsEnabled)
            return Color.DarkGray;
            
        if (_isPressed)
            return Style switch
            {
                ButtonStyle.Primary => Color.DarkBlue,
                ButtonStyle.Success => Color.DarkGreen,
                ButtonStyle.Warning => Color.DarkYellow,
                ButtonStyle.Danger => Color.DarkRed,
                _ => Color.DarkGray
            };
            
        if (IsFocused)
            return Style switch
            {
                ButtonStyle.Primary => Color.Blue,
                ButtonStyle.Success => Color.Green,
                ButtonStyle.Warning => Color.Yellow,
                ButtonStyle.Danger => Color.Red,
                _ => Color.Gray
            };
            
        return Style switch
        {
            ButtonStyle.Primary => Color.DarkBlue,
            ButtonStyle.Success => Color.DarkGreen,
            ButtonStyle.Warning => Color.DarkYellow,
            ButtonStyle.Danger => Color.DarkRed,
            _ => Color.None
        };
    }
    
    private Color GetTextColor()
    {
        if (!IsEnabled)
            return Color.Gray;
            
        return Style switch
        {
            ButtonStyle.Warning => Color.Black,
            _ => Color.White
        };
    }
    
    private BorderStyle GetBorderStyle()
    {
        if (IsDefault)
            return BorderStyle.Double;
            
        return IsFocused ? BorderStyle.Single : BorderStyle.Single;
    }
    
    private Color GetBorderColor()
    {
        if (!IsEnabled)
            return Color.DarkGray;
            
        if (IsFocused)
            return Color.Cyan;
            
        return Style switch
        {
            ButtonStyle.Primary => Color.Blue,
            ButtonStyle.Success => Color.Green,
            ButtonStyle.Warning => Color.Yellow,
            ButtonStyle.Danger => Color.Red,
            _ => Color.Gray
        };
    }
    
    private VirtualNode CreateBackground(Color color)
    {
        return new ElementNode("rect", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height,
            ["fill"] = color
        });
    }
    
    private VirtualNode CreateText(int x, int y, string text, Color color)
    {
        return new ElementNode("text", new Dictionary<string, object?>
        {
            ["x"] = x,
            ["y"] = y,
            ["color"] = color
        }, new TextNode(text));
    }
    
    private VirtualNode CreateBorder(BorderStyle style, Color color)
    {
        var chars = GetBorderChars(style);
        var nodes = new List<VirtualNode>();
        
        // Top border
        nodes.Add(CreateText(Bounds.X, Bounds.Y, chars.TopLeft.ToString(), color));
        for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, Bounds.Y, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(Bounds.Right - 1, Bounds.Y, chars.TopRight.ToString(), color));
        
        // Side borders
        for (int y = Bounds.Y + 1; y < Bounds.Bottom - 1; y++)
        {
            nodes.Add(CreateText(Bounds.X, y, chars.Vertical.ToString(), color));
            nodes.Add(CreateText(Bounds.Right - 1, y, chars.Vertical.ToString(), color));
        }
        
        // Bottom border
        nodes.Add(CreateText(Bounds.X, Bounds.Bottom - 1, chars.BottomLeft.ToString(), color));
        for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
        {
            nodes.Add(CreateText(x, Bounds.Bottom - 1, chars.Horizontal.ToString(), color));
        }
        nodes.Add(CreateText(Bounds.Right - 1, Bounds.Bottom - 1, chars.BottomRight.ToString(), color));
        
        return new FragmentNode(nodes);
    }
    
    private VirtualNode CreateFocusIndicator()
    {
        // Add brackets around the button when focused
        var nodes = new List<VirtualNode>
        {
            CreateText(Bounds.X - 1, ContentBounds.Y + ContentBounds.Height / 2, "[", Color.Cyan),
            CreateText(Bounds.Right, ContentBounds.Y + ContentBounds.Height / 2, "]", Color.Cyan)
        };
        
        return new FragmentNode(nodes);
    }
    
    private (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => ('╭', '╮', '╰', '╯', '─', '│'),
            _ => ('+', '+', '+', '+', '-', '|')
        };
    }
}

/// <summary>
/// Defines button styles.
/// </summary>
public enum ButtonStyle
{
    /// <summary>
    /// Default button style.
    /// </summary>
    Default,
    
    /// <summary>
    /// Primary action button style.
    /// </summary>
    Primary,
    
    /// <summary>
    /// Success/positive action button style.
    /// </summary>
    Success,
    
    /// <summary>
    /// Warning button style.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Danger/destructive action button style.
    /// </summary>
    Danger
}