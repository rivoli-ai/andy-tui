using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Layout;

/// <summary>
/// A container component that provides padding, margin, borders, and background styling.
/// </summary>
public class Box : LayoutComponent
{
    private VirtualNode? _content;
    private VirtualNode? _cachedRenderedContent;
    private bool _contentCached = false;
    
    /// <summary>
    /// Gets or sets the border configuration.
    /// </summary>
    public Border Border { get; set; } = Border.None;
    
    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public Color? BackgroundColor { get; set; }
    
    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public Color? ForegroundColor { get; set; }
    
    /// <summary>
    /// Gets or sets the border color.
    /// </summary>
    public Color? BorderColor { get; set; }
    
    /// <summary>
    /// Gets or sets the content alignment within the box.
    /// </summary>
    public Alignment ContentHorizontalAlignment { get; set; } = Alignment.Stretch;
    
    /// <summary>
    /// Gets or sets the vertical content alignment within the box.
    /// </summary>
    public Alignment ContentVerticalAlignment { get; set; } = Alignment.Stretch;
    
    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    public VirtualNode? Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                _contentCached = false;
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a render function for dynamic content.
    /// </summary>
    public Func<VirtualNode>? RenderContent { get; set; }
    
    private VirtualNode? GetEffectiveContent()
    {
        // Cache content for the current render cycle
        if (!_contentCached)
        {
            _cachedRenderedContent = RenderContent?.Invoke() ?? Content;
            _contentCached = true;
        }
        return _cachedRenderedContent;
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        
        var borderSize = Border.Style != BorderStyle.None ? 2 : 0; // 1 for each side
        var contentAvailableSize = new Size(
            Math.Max(0, availableSize.Width - Padding.Horizontal - borderSize),
            Math.Max(0, availableSize.Height - Padding.Vertical - borderSize));
        
        // Measure content if it's a component
        var contentSize = Size.Zero;
        // During measurement, use Content directly without invoking RenderContent
        var content = Content;
        
        if (content is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            contentSize = layoutComponent.Measure(contentAvailableSize);
        }
        else if (content != null)
        {
            // For non-layout content, estimate size based on content
            contentSize = EstimateContentSize(content, contentAvailableSize);
        }
        
        return new Size(
            contentSize.Width + Padding.Horizontal + borderSize,
            contentSize.Height + Padding.Vertical + borderSize);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        // The bounds are already set by the base class
        // During arrangement, use Content directly without invoking RenderContent
        var content = Content;
        
        if (content is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            var borderSize = Border.Style != BorderStyle.None ? 2 : 0;
            var contentBounds = new Rectangle(
                bounds.X + Padding.Left + (Border.Left ? 1 : 0),
                bounds.Y + Padding.Top + (Border.Top ? 1 : 0),
                Math.Max(0, bounds.Width - Padding.Horizontal - borderSize),
                Math.Max(0, bounds.Height - Padding.Vertical - borderSize));
            
            // Apply content alignment
            var contentSize = layoutComponent.Measure(new Size(contentBounds.Width, contentBounds.Height));
            var alignedBounds = AlignContent(contentBounds, contentSize);
            
            layoutComponent.Arrange(alignedBounds);
        }
    }
    
    protected override VirtualNode OnRender()
    {
        // Reset content cache for this render cycle
        _contentCached = false;
        
        var children = new List<VirtualNode>();
        
        // Add background if specified
        if (BackgroundColor.HasValue)
        {
            children.Add(CreateBackground());
        }
        
        // Add border if specified
        if (Border.Style != BorderStyle.None)
        {
            children.Add(CreateBorder());
        }
        
        // Add content
        var content = GetEffectiveContent();
        if (content != null)
        {
            children.Add(CreateContentWrapper(content));
        }
        
        return CreateLayoutNode("box", children.ToArray());
    }
    
    private VirtualNode CreateBackground()
    {
        return new ElementNode("rect", new Dictionary<string, object?>
        {
            ["x"] = Bounds.X,
            ["y"] = Bounds.Y,
            ["width"] = Bounds.Width,
            ["height"] = Bounds.Height,
            ["fill"] = BackgroundColor
        });
    }
    
    private VirtualNode CreateBorder()
    {
        var chars = GetBorderCharacters(Border.Style);
        var nodes = new List<VirtualNode>();
        
        var color = BorderColor ?? ForegroundColor;
        Style? style = color.HasValue ? new Style { Foreground = color.Value } : null;
        
        // Top border
        if (Border.Top)
        {
            for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
            {
                nodes.Add(CreateTextNode(chars.Horizontal, x, Bounds.Y, style));
            }
        }
        
        // Bottom border
        if (Border.Bottom)
        {
            for (int x = Bounds.X + 1; x < Bounds.Right - 1; x++)
            {
                nodes.Add(CreateTextNode(chars.Horizontal, x, Bounds.Bottom - 1, style));
            }
        }
        
        // Left border
        if (Border.Left)
        {
            for (int y = Bounds.Y + 1; y < Bounds.Bottom - 1; y++)
            {
                nodes.Add(CreateTextNode(chars.Vertical, Bounds.X, y, style));
            }
        }
        
        // Right border
        if (Border.Right)
        {
            for (int y = Bounds.Y + 1; y < Bounds.Bottom - 1; y++)
            {
                nodes.Add(CreateTextNode(chars.Vertical, Bounds.Right - 1, y, style));
            }
        }
        
        // Corners
        if (Border.Top && Border.Left)
            nodes.Add(CreateTextNode(chars.TopLeft, Bounds.X, Bounds.Y, style));
        if (Border.Top && Border.Right)
            nodes.Add(CreateTextNode(chars.TopRight, Bounds.Right - 1, Bounds.Y, style));
        if (Border.Bottom && Border.Left)
            nodes.Add(CreateTextNode(chars.BottomLeft, Bounds.X, Bounds.Bottom - 1, style));
        if (Border.Bottom && Border.Right)
            nodes.Add(CreateTextNode(chars.BottomRight, Bounds.Right - 1, Bounds.Bottom - 1, style));
        
        return new FragmentNode(nodes);
    }
    
    private VirtualNode CreateContentWrapper(VirtualNode content)
    {
        var borderOffset = Border.Style != BorderStyle.None ? 1 : 0;
        var contentX = Bounds.X + Padding.Left + (Border.Left ? borderOffset : 0);
        var contentY = Bounds.Y + Padding.Top + (Border.Top ? borderOffset : 0);
        
        var attributes = new Dictionary<string, object?>
        {
            ["x"] = contentX,
            ["y"] = contentY
        };
        
        if (ForegroundColor.HasValue)
        {
            attributes["color"] = ForegroundColor.Value;
        }
        
        return new ElementNode("content", attributes, content);
    }
    
    private VirtualNode CreateTextNode(char character, int x, int y, Style? style)
    {
        var attributes = new Dictionary<string, object?>
        {
            ["x"] = x,
            ["y"] = y
        };
        
        if (style != null)
        {
            attributes["style"] = style;
        }
        
        return new ElementNode("text", attributes, new TextNode(character.ToString()));
    }
    
    private Rectangle AlignContent(Rectangle available, Size contentSize)
    {
        var x = available.X;
        var y = available.Y;
        var width = contentSize.Width;
        var height = contentSize.Height;
        
        // Apply horizontal alignment
        switch (ContentHorizontalAlignment)
        {
            case Alignment.Center:
                x += (available.Width - width) / 2;
                break;
            case Alignment.End:
                x += available.Width - width;
                break;
            case Alignment.Stretch:
                width = available.Width;
                break;
        }
        
        // Apply vertical alignment
        switch (ContentVerticalAlignment)
        {
            case Alignment.Center:
                y += (available.Height - height) / 2;
                break;
            case Alignment.End:
                y += available.Height - height;
                break;
            case Alignment.Stretch:
                height = available.Height;
                break;
        }
        
        return new Rectangle(x, y, width, height);
    }
    
    private Size EstimateContentSize(VirtualNode node, Size availableSize)
    {
        // Simple estimation for text content
        if (node is TextNode textNode)
        {
            var lines = textNode.Content.Split('\n');
            var width = lines.Length > 0 ? lines.Max(line => line.Length) : 0;
            var height = lines.Length;
            return new Size(width, height);
        }
        
        // For other nodes, use available size
        return availableSize;
    }
    
    private (char TopLeft, char TopRight, char BottomLeft, char BottomRight, char Horizontal, char Vertical) GetBorderCharacters(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ('┌', '┐', '└', '┘', '─', '│'),
            BorderStyle.Double => ('╔', '╗', '╚', '╝', '═', '║'),
            BorderStyle.Rounded => ('╭', '╮', '╰', '╯', '─', '│'),
            BorderStyle.Heavy => ('┏', '┓', '┗', '┛', '━', '┃'),
            BorderStyle.Dashed => ('┌', '┐', '└', '┘', '╌', '╎'),
            _ => (' ', ' ', ' ', ' ', ' ', ' ')
        };
    }
}