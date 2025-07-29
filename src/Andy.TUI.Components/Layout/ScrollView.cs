using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Layout;

/// <summary>
/// A scrollable container component that provides viewport management and scrolling.
/// </summary>
public class ScrollView : LayoutComponent
{
    private VirtualNode? _content;
    private Size _contentSize = Size.Zero;
    private int _scrollX = 0;
    private int _scrollY = 0;
    private bool _showHorizontalScrollbar = true;
    private bool _showVerticalScrollbar = true;
    
    /// <summary>
    /// Gets or sets the content of the scroll view.
    /// </summary>
    public VirtualNode? Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets whether horizontal scrolling is enabled.
    /// </summary>
    public bool HorizontalScrollEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether vertical scrolling is enabled.
    /// </summary>
    public bool VerticalScrollEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to show the horizontal scrollbar.
    /// </summary>
    public bool ShowHorizontalScrollbar
    {
        get => _showHorizontalScrollbar && HorizontalScrollEnabled;
        set => _showHorizontalScrollbar = value;
    }
    
    /// <summary>
    /// Gets or sets whether to show the vertical scrollbar.
    /// </summary>
    public bool ShowVerticalScrollbar
    {
        get => _showVerticalScrollbar && VerticalScrollEnabled;
        set => _showVerticalScrollbar = value;
    }
    
    /// <summary>
    /// Gets or sets the scrollbar style.
    /// </summary>
    public ScrollbarStyle ScrollbarStyle { get; set; } = ScrollbarStyle.Simple;
    
    /// <summary>
    /// Gets or sets the horizontal scroll position.
    /// </summary>
    public int ScrollX
    {
        get => _scrollX;
        set
        {
            var newValue = Math.Max(0, Math.Min(value, MaxScrollX));
            if (_scrollX != newValue)
            {
                _scrollX = newValue;
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the vertical scroll position.
    /// </summary>
    public int ScrollY
    {
        get => _scrollY;
        set
        {
            var newValue = Math.Max(0, Math.Min(value, MaxScrollY));
            if (_scrollY != newValue)
            {
                _scrollY = newValue;
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets the maximum horizontal scroll position.
    /// </summary>
    public int MaxScrollX => Math.Max(0, _contentSize.Width - ViewportWidth);
    
    /// <summary>
    /// Gets the maximum vertical scroll position.
    /// </summary>
    public int MaxScrollY => Math.Max(0, _contentSize.Height - ViewportHeight);
    
    /// <summary>
    /// Gets the viewport width (excluding scrollbars).
    /// </summary>
    public int ViewportWidth
    {
        get
        {
            var width = ContentBounds.Width;
            if (ShowVerticalScrollbar)
                width -= 1; // Reserve space for scrollbar
            return Math.Max(0, width);
        }
    }
    
    /// <summary>
    /// Gets the viewport height (excluding scrollbars).
    /// </summary>
    public int ViewportHeight
    {
        get
        {
            var height = ContentBounds.Height;
            if (ShowHorizontalScrollbar)
                height -= 1; // Reserve space for scrollbar
            return Math.Max(0, height);
        }
    }
    
    /// <summary>
    /// Scrolls to make the specified area visible.
    /// </summary>
    public void ScrollToArea(Rectangle area)
    {
        // Horizontal scrolling
        if (area.X < ScrollX)
        {
            ScrollX = area.X;
        }
        else if (area.Right > ScrollX + ViewportWidth)
        {
            ScrollX = area.Right - ViewportWidth;
        }
        
        // Vertical scrolling
        if (area.Y < ScrollY)
        {
            ScrollY = area.Y;
        }
        else if (area.Bottom > ScrollY + ViewportHeight)
        {
            ScrollY = area.Bottom - ViewportHeight;
        }
    }
    
    /// <summary>
    /// Scrolls by the specified delta.
    /// </summary>
    public void ScrollBy(int deltaX, int deltaY)
    {
        if (HorizontalScrollEnabled)
            ScrollX += deltaX;
        if (VerticalScrollEnabled)
            ScrollY += deltaY;
    }
    
    /// <summary>
    /// Scrolls to the top.
    /// </summary>
    public void ScrollToTop() => ScrollY = 0;
    
    /// <summary>
    /// Scrolls to the bottom.
    /// </summary>
    public void ScrollToBottom() => ScrollY = MaxScrollY;
    
    /// <summary>
    /// Scrolls to the left.
    /// </summary>
    public void ScrollToLeft() => ScrollX = 0;
    
    /// <summary>
    /// Scrolls to the right.
    /// </summary>
    public void ScrollToRight() => ScrollX = MaxScrollX;
    
    protected override Size MeasureCore(Size availableSize)
    {
        if (_content == null)
            return new Size(Padding.Horizontal, Padding.Vertical);
        
        // Measure content with unlimited size to get true content size
        var unlimitedSize = new Size(
            HorizontalScrollEnabled ? int.MaxValue : availableSize.Width - Padding.Horizontal,
            VerticalScrollEnabled ? int.MaxValue : availableSize.Height - Padding.Vertical);
        
        _contentSize = MeasureChild(_content, unlimitedSize);
        
        // Return the constrained size
        var width = Math.Min(_contentSize.Width, availableSize.Width - Padding.Horizontal);
        var height = Math.Min(_contentSize.Height, availableSize.Height - Padding.Vertical);
        
        return new Size(width + Padding.Horizontal, height + Padding.Vertical);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        if (_content == null)
            return;
        
        // Arrange content at its full size, offset by scroll position
        var contentBounds = new Rectangle(
            bounds.X + Padding.Left - ScrollX,
            bounds.Y + Padding.Top - ScrollY,
            _contentSize.Width,
            _contentSize.Height);
        
        ArrangeChild(_content, contentBounds);
    }
    
    protected override VirtualNode OnRender()
    {
        var children = new List<VirtualNode>();
        
        // Add viewport with clipping
        var viewportBounds = new Rectangle(
            Bounds.X + Padding.Left,
            Bounds.Y + Padding.Top,
            ViewportWidth,
            ViewportHeight);
        
        var viewportAttrs = new Dictionary<string, object?>
        {
            ["x"] = viewportBounds.X,
            ["y"] = viewportBounds.Y,
            ["width"] = viewportBounds.Width,
            ["height"] = viewportBounds.Height,
            ["clip"] = true
        };
        
        if (_content != null)
        {
            // Wrap content with scroll offset
            var contentWrapper = new ElementNode("scroll-content", new Dictionary<string, object?>
            {
                ["offset-x"] = -ScrollX,
                ["offset-y"] = -ScrollY
            }, _content);
            
            children.Add(new ElementNode("viewport", viewportAttrs, contentWrapper));
        }
        else
        {
            children.Add(new ElementNode("viewport", viewportAttrs));
        }
        
        // Add scrollbars if needed
        if (ShowVerticalScrollbar && _contentSize.Height > ViewportHeight)
        {
            children.Add(CreateVerticalScrollbar());
        }
        
        if (ShowHorizontalScrollbar && _contentSize.Width > ViewportWidth)
        {
            children.Add(CreateHorizontalScrollbar());
        }
        
        return CreateLayoutNode("scrollview", children.ToArray());
    }
    
    private VirtualNode CreateVerticalScrollbar()
    {
        var x = Bounds.Right - 1;
        var y = Bounds.Y + Padding.Top;
        var height = ViewportHeight;
        
        var scrollbarNodes = new List<VirtualNode>();
        
        // Track background
        for (int i = 0; i < height; i++)
        {
            scrollbarNodes.Add(CreateTextNode('│', x, y + i, null));
        }
        
        // Thumb
        if (height > 2 && MaxScrollY > 0)
        {
            var thumbHeight = Math.Max(1, (int)(height * (double)ViewportHeight / _contentSize.Height));
            var thumbY = y + (int)((height - thumbHeight) * (double)ScrollY / MaxScrollY);
            
            for (int i = 0; i < thumbHeight; i++)
            {
                var thumbChar = ScrollbarStyle == ScrollbarStyle.Simple ? '█' : '▓';
                scrollbarNodes.Add(CreateTextNode(thumbChar, x, thumbY + i, 
                    new Style { Foreground = Color.White }));
            }
        }
        
        return new FragmentNode(scrollbarNodes);
    }
    
    private VirtualNode CreateHorizontalScrollbar()
    {
        var x = Bounds.X + Padding.Left;
        var y = Bounds.Bottom - 1;
        var width = ViewportWidth;
        
        var scrollbarNodes = new List<VirtualNode>();
        
        // Track background
        for (int i = 0; i < width; i++)
        {
            scrollbarNodes.Add(CreateTextNode('─', x + i, y, null));
        }
        
        // Thumb
        if (width > 2 && MaxScrollX > 0)
        {
            var thumbWidth = Math.Max(1, (int)(width * (double)ViewportWidth / _contentSize.Width));
            var thumbX = x + (int)((width - thumbWidth) * (double)ScrollX / MaxScrollX);
            
            for (int i = 0; i < thumbWidth; i++)
            {
                var thumbChar = ScrollbarStyle == ScrollbarStyle.Simple ? '█' : '▓';
                scrollbarNodes.Add(CreateTextNode(thumbChar, thumbX + i, y,
                    new Style { Foreground = Color.White }));
            }
        }
        
        return new FragmentNode(scrollbarNodes);
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
    
    private Size MeasureChild(VirtualNode child, Size availableSize)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            return layoutComponent.Measure(availableSize);
        }
        
        // For non-layout nodes, estimate size
        if (child is TextNode textNode)
        {
            var lines = textNode.Content.Split('\n');
            return new Size(lines.Length > 0 ? lines.Max(l => l.Length) : 0, lines.Length);
        }
        
        return new Size(1, 1);
    }
    
    private void ArrangeChild(VirtualNode child, Rectangle bounds)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            layoutComponent.Arrange(bounds);
        }
    }
}

/// <summary>
/// Scrollbar visual style.
/// </summary>
public enum ScrollbarStyle
{
    Simple,
    Shaded
}