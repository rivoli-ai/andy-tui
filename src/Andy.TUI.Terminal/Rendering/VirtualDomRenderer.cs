using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Terminal.Rendering;

/// <summary>
/// Renders virtual DOM trees to the terminal with z-order management and efficient updates.
/// </summary>
public class VirtualDomRenderer : IVirtualNodeVisitor, IPatchVisitor
{
    private readonly IRenderingSystem _renderingSystem;
    private readonly Dictionary<int[], RenderedElement> _renderedElements = new(new PathComparer());
    private readonly DirtyRegionTracker _dirtyRegionTracker = new();
    private VirtualNode? _currentTree;
    private RenderedElement? _rootElement;

    /// <summary>
    /// Represents a rendered element with its position and z-order.
    /// </summary>
    private class RenderedElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ZIndex { get; set; }
        public VirtualNode Node { get; set; } = null!;
        public List<RenderedElement> Children { get; } = new();
    }

    public VirtualDomRenderer(IRenderingSystem renderingSystem)
    {
        _renderingSystem = renderingSystem ?? throw new ArgumentNullException(nameof(renderingSystem));
    }

    /// <summary>
    /// Renders a complete virtual DOM tree to the terminal.
    /// </summary>
    public void Render(VirtualNode tree)
    {
        _currentTree = tree;
        _renderedElements.Clear();
        _dirtyRegionTracker.Clear();

        // Clear the entire render area to remove any stale content from previous frames
        for (int y = 0; y < _renderingSystem.Height; y++)
        {
            _renderingSystem.FillRect(0, y, _renderingSystem.Width, 1, ' ', Style.Default);
        }

        // First pass: build the render tree with positions and z-indices
        _rootElement = BuildRenderTree(tree, 0, 0, Array.Empty<int>());

        // Second pass: render elements in z-order
        RenderInZOrder(_rootElement);
    }

    /// <summary>
    /// Applies patches to update the rendered output efficiently.
    /// </summary>
    public void ApplyPatches(IReadOnlyList<Patch> patches)
    {
        if (_currentTree == null)
            throw new InvalidOperationException("No tree has been rendered yet.");

        // Console.Error.WriteLine($"[VirtualDomRenderer] ApplyPatches: {patches.Count} patches");

        // Apply patches and track dirty regions
        foreach (var patch in patches)
        {
            // Console.Error.WriteLine($"[VirtualDomRenderer] Applying patch: {patch.GetType().Name}");
            patch.Accept(this);
        }

        // Re-render only dirty regions
        RenderDirtyRegions();
    }

    private RenderedElement BuildRenderTree(VirtualNode node, int x, int y, int[] path)
    {
        var element = new RenderedElement
        {
            X = x,
            Y = y,
            Node = node
        };

        // Extract position and size from props
        if (node.Props.TryGetValue("x", out var xProp) && xProp is int xVal)
            element.X = xVal;
        if (node.Props.TryGetValue("y", out var yProp) && yProp is int yVal)
            element.Y = yVal;
        if (node.Props.TryGetValue("width", out var wProp) && wProp is int width)
            element.Width = width;
        if (node.Props.TryGetValue("height", out var hProp) && hProp is int height)
            element.Height = height;
        if (node.Props.TryGetValue("z-index", out var zProp) && zProp is int zIndex)
            element.ZIndex = zIndex;

        // For text elements without explicit dimensions, compute based on content
        if (element.Width == 0 && node is ElementNode elementNode && elementNode.TagName.ToLower() == "text")
        {
            // Calculate dimensions based on text content
            var textContent = GetTextContent(node);
            if (!string.IsNullOrEmpty(textContent))
            {
                var lines = textContent.Split('\n');
                element.Height = Math.Max(1, lines.Length);
                element.Width = lines.Max(line => line.Length);
            }
            else
            {
                // Default minimum size for empty text elements
                element.Height = 1;
                element.Width = 1;
            }

            // Console.Error.WriteLine($"[VirtualDomRenderer] Text element at ({element.X},{element.Y}) computed size: {element.Width}x{element.Height}, content: '{textContent}'");
        }

        _renderedElements[path] = element;

        // Process children
        var childIndex = 0;
        foreach (var child in node.Children)
        {
            var childPath = path.Concat(new[] { childIndex }).ToArray();
            var childElement = BuildRenderTree(child, element.X, element.Y, childPath);
            element.Children.Add(childElement);
            childIndex++;
        }

        return element;
    }

    private void RenderInZOrder(RenderedElement root)
    {
        // Collect all elements with their absolute positions
        var allElements = new List<(RenderedElement Element, int AbsoluteX, int AbsoluteY)>();
        CollectElements(root, 0, 0, allElements);

        // Sort by z-index (lower z-index renders first)
        var sortedElements = allElements.OrderBy(e => e.Element.ZIndex).ToList();

        // Render each element
        foreach (var (element, absX, absY) in sortedElements)
        {
            RenderElement(element, absX, absY);
        }
    }

    private void CollectElements(RenderedElement element, int parentX, int parentY,
        List<(RenderedElement, int, int)> allElements)
    {
        // Check if this node has explicit position props (absolute positioning)
        bool hasExplicitX = element.Node.Props.ContainsKey("x");
        bool hasExplicitY = element.Node.Props.ContainsKey("y");

        // For fragment nodes, don't accumulate position since they're just containers
        var isFragment = element.Node is FragmentNode;

        // If node has explicit x/y, use those as absolute positions
        // Otherwise, compute relative to parent
        var absX = hasExplicitX ? element.X : (isFragment ? element.X : parentX + element.X);
        var absY = hasExplicitY ? element.Y : (isFragment ? element.Y : parentY + element.Y);

        // Debug logging removed for clarity

        // Don't add fragment nodes to the render list
        // Also don't add text nodes separately - they'll be rendered as part of their parent
        if (!isFragment && !(element.Node is TextNode))
        {
            allElements.Add((element, absX, absY));
        }

        // Only recurse into children for fragments and other containers
        // Don't recurse into text element children since we'll render them together
        if (isFragment || (element.Node is ElementNode elem && elem.TagName.ToLower() != "text"))
        {
            foreach (var child in element.Children)
            {
                // Pass the computed absolute position as parent position for children
                CollectElements(child, absX, absY, allElements);
            }
        }
    }

    private void RenderElement(RenderedElement element, int x, int y)
    {
        _currentRenderContext = new RenderContext { X = x, Y = y, Element = element };
        // Debug: RenderElement at ({x},{y})
        element.Node.Accept(this);
        _currentRenderContext = null;
    }

    private void RenderDirtyRegions()
    {
        var dirtyRegions = _dirtyRegionTracker.GetDirtyRegions().ToList();

        foreach (var region in dirtyRegions)
        {
            // Clear the dirty region
            for (int y = region.Y; y < region.Y + region.Height; y++)
            {
                _renderingSystem.FillRect(region.X, y, region.Width, 1, ' ', Style.Default);
            }

            // Re-render intersecting elements in z-order (low to high)
            if (_rootElement != null)
            {
                var all = new List<(RenderedElement Element, int AbsX, int AbsY)>();
                CollectElements(_rootElement, 0, 0, all);
                foreach (var (element, absX, absY) in all
                    .Where(e => new Rectangle(e.AbsX, e.AbsY, e.Element.Width, e.Element.Height).IntersectsWith(region))
                    .OrderBy(e => e.Element.ZIndex))
                {
                    RenderElement(element, absX, absY);
                }
            }
        }

        _dirtyRegionTracker.Clear();
    }

    // Obsolete after z-ordered region rendering, keep for reference
    private void RenderElementsInRegion(RenderedElement element, Rectangle region, int parentX = 0, int parentY = 0) { }

    #region IVirtualNodeVisitor Implementation

    public void VisitText(TextNode node)
    {
        // Text nodes render their content at the current position
        // Position comes from the parent element's position in the render tree
        if (_currentRenderContext != null)
        {
            // Get style from parent element if it's a text element
            var style = Style.Default;
            if (_currentRenderContext.Element?.Node is ElementNode elementNode &&
                elementNode.TagName.ToLower() == "text")
            {
                style = GetStyleProp(elementNode, "style", Style.Default);
            }

            _renderingSystem.WriteText(_currentRenderContext.X, _currentRenderContext.Y,
                node.Content, style);
        }
    }

    public void VisitElement(ElementNode node)
    {
        var tagName = node.TagName.ToLower();
        // Debug: VisitElement {tagName}

        // Handle different element types
        switch (tagName)
        {
            case "rect":
                RenderRect(node);
                break;
            case "text":
                RenderText(node);
                break;
            case "box":
                RenderBox(node);
                break;
            case "table":
                RenderTable(node);
                break;
            default:
                // Unknown elements don't render anything themselves
                // Their children are handled by the render tree
                break;
        }
    }

    public void VisitComponent(ComponentNode node)
    {
        // Components should be expanded to their rendered output before reaching here
        throw new NotSupportedException("Component nodes should be expanded before rendering.");
    }

    public void VisitFragment(FragmentNode node)
    {
        // Fragment nodes don't render anything themselves
        // Their children are already handled by the render tree
    }

    public void VisitEmpty(EmptyNode node)
    {
        // Empty nodes render nothing
    }

    public void VisitClipping(ClippingNode node)
    {
        // Clipping nodes constrain their children to a rectangular area
        // For terminal rendering, we'll need to handle this at a higher level
        // since we can't truly clip in a terminal. We'll track the clipping bounds
        // and prevent rendering outside them.

        // The actual clipping logic is handled in BuildRenderTree and CollectElements
        // This visitor method is just a placeholder for now
    }

    #endregion

    #region IPatchVisitor Implementation

    public void VisitReplace(ReplacePatch patch)
    {
        if (_renderedElements.TryGetValue(patch.Path, out var element))
        {
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));
            element.Node = patch.NewNode;
        }
    }

    public void VisitUpdateProps(UpdatePropsPatch patch)
    {
        if (_renderedElements.TryGetValue(patch.Path, out var element))
        {
            // Mark old position as dirty (for clearing)
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));

            // Track if position or size properties changed
            bool positionChanged = false;
            int newX = element.X, newY = element.Y;
            int newWidth = element.Width, newHeight = element.Height;

            // Update props and check for position/size changes
            foreach (var (key, value) in patch.PropsToSet)
            {
                element.Node.Props[key] = value;

                // Check for position/size property changes
                if (key == "x" && value is int x) { newX = x; positionChanged = true; }
                else if (key == "y" && value is int y) { newY = y; positionChanged = true; }
                else if (key == "width" && value is int w) { newWidth = w; positionChanged = true; }
                else if (key == "height" && value is int h) { newHeight = h; positionChanged = true; }
            }

            // Remove props
            foreach (var key in patch.PropsToRemove)
            {
                element.Node.Props.Remove(key);
            }

            // Check if style property changed (requires re-render even if position didn't change)
            bool styleChanged = patch.PropsToSet.Any(p => p.Key == "style");

            // If position/size changed, update element and mark new area as dirty
            if (positionChanged)
            {
                element.X = newX;
                element.Y = newY;
                element.Width = newWidth;
                element.Height = newHeight;

                // Mark new position as dirty (for rendering)
                _dirtyRegionTracker.MarkDirty(new Rectangle(newX, newY, newWidth, newHeight));
            }
            else if (styleChanged || patch.PropsToSet.Count > 0 || patch.PropsToRemove.Count > 0)
            {
                // Even if position didn't change, we need to re-render if any props changed
                var rect = new Rectangle(element.X, element.Y, element.Width, element.Height);
                // Console.Error.WriteLine($"[VirtualDomRenderer] VisitUpdateProps marking dirty: ({rect.X},{rect.Y}) {rect.Width}x{rect.Height}");
                _dirtyRegionTracker.MarkDirty(rect);
            }
        }
    }

    public void VisitUpdateText(UpdateTextPatch patch)
    {
        // Text nodes are not stored directly in _renderedElements
        // We need to find the parent element that contains this text node
        // Console.Error.WriteLine($"[VirtualDomRenderer] VisitUpdateText: path length={patch.Path.Length}, newText='{patch.NewText}'");

        if (patch.Path.Length > 0)
        {
            var parentPath = patch.Path.Take(patch.Path.Length - 1).ToArray();
            // Console.Error.WriteLine($"[VirtualDomRenderer] Looking for parent at path: [{string.Join(",", parentPath)}]");

            if (_renderedElements.TryGetValue(parentPath, out var parentElement))
            {
                // Compute clear width as max of old and new to ensure trailing chars are cleared
                var newWidth = patch.NewText?.Length ?? 0;
                var oldWidth = Math.Max(0, parentElement.Width);
                var height = parentElement.Height > 0 ? parentElement.Height : 1;
                var clearWidth = Math.Max(oldWidth, newWidth);
                var rect = new Rectangle(parentElement.X, parentElement.Y, clearWidth, height);
                _dirtyRegionTracker.MarkDirty(rect);

                // Update the text node in the parent's children
                var childIndex = patch.Path[patch.Path.Length - 1];
                if (childIndex < parentElement.Node.Children.Count &&
                    parentElement.Node.Children[childIndex] is TextNode)
                {
                    // Replace the text node (they're immutable)
                    var children = parentElement.Node.Children.ToList();
                    children[childIndex] = new TextNode(patch.NewText ?? string.Empty);

                    // Update the parent node's children and cached size
                    if (parentElement.Node is ElementNode elementNode)
                    {
                        parentElement.Node = new ElementNode(elementNode.TagName, elementNode.Props, children.ToArray());
                        parentElement.Width = newWidth;
                        parentElement.Height = height;
                    }
                }
            }
        }
    }

    public void VisitInsert(InsertPatch patch)
    {
        // Find parent element
        var parentPath = patch.Path.Take(patch.Path.Length - 1).ToArray();
        if (_renderedElements.TryGetValue(parentPath, out var parent))
        {
            var newElement = BuildRenderTree(patch.Node, parent.X, parent.Y, patch.Path);
            var insertIndex = Math.Max(0, Math.Min(patch.Index, parent.Children.Count));
            parent.Children.Insert(insertIndex, newElement);

            // Mark the area of the newly inserted element as dirty
            // Use its measured bounds so we actually render it even if the parent has 0x0 size
            var newRect = new Rectangle(newElement.X, newElement.Y, newElement.Width, newElement.Height);
            _dirtyRegionTracker.MarkDirty(newRect);

            // Also mark parent if it has non-zero bounds (helps when containers need redraw)
            if (parent.Width > 0 && parent.Height > 0)
            {
                _dirtyRegionTracker.MarkDirty(new Rectangle(parent.X, parent.Y, parent.Width, parent.Height));
            }
        }
    }

    public void VisitRemove(RemovePatch patch)
    {
        if (_renderedElements.TryGetValue(patch.Path, out var element))
        {
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));
            _renderedElements.Remove(patch.Path);

            // Remove from parent's children
            var parentPath = patch.Path.Take(patch.Path.Length - 1).ToArray();
            if (_renderedElements.TryGetValue(parentPath, out var parent))
            {
                parent.Children.RemoveAt(patch.Index);
            }
        }
    }

    public void VisitMove(MovePatch patch)
    {
        // Mark both old and new positions as dirty
        if (_renderedElements.TryGetValue(patch.Path, out var element))
        {
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));
            // Update position would happen here
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));
        }
    }

    public void VisitReorder(ReorderPatch patch)
    {
        if (_renderedElements.TryGetValue(patch.Path, out var element))
        {
            _dirtyRegionTracker.MarkDirty(new Rectangle(element.X, element.Y, element.Width, element.Height));

            // Apply moves to reorder children
            foreach (var (from, to) in patch.Moves)
            {
                if (from < element.Children.Count && to < element.Children.Count)
                {
                    var child = element.Children[from];
                    element.Children.RemoveAt(from);
                    element.Children.Insert(to, child);
                }
            }
        }
    }

    #endregion

    #region Element Rendering Methods

    private RenderContext? _currentRenderContext;

    private class RenderContext
    {
        public int X { get; set; }
        public int Y { get; set; }
        public RenderedElement? Element { get; set; }
    }

    private void RenderRect(ElementNode node)
    {
        var x = GetIntProp(node, "x", _currentRenderContext?.X ?? 0);
        var y = GetIntProp(node, "y", _currentRenderContext?.Y ?? 0);
        var width = GetIntProp(node, "width", 0);
        var height = GetIntProp(node, "height", 0);
        var fill = GetColorProp(node, "fill");

        if (fill.HasValue)
        {
            var style = Style.Default.WithBackgroundColor(fill.Value);
            _renderingSystem.FillRect(x, y, width, height, ' ', style);
        }
    }

    private void RenderText(ElementNode node)
    {
        // Get position from node props, falling back to context position
        var x = GetIntProp(node, "x", _currentRenderContext?.X ?? 0);
        var y = GetIntProp(node, "y", _currentRenderContext?.Y ?? 0);

        // Render text element and its children
        var style = GetStyleProp(node, "style", Style.Default);

        // Render all text content from child nodes
        foreach (var child in node.Children)
        {
            if (child is TextNode textNode)
            {
                // Debug: Writing text at ({x},{y})
                _renderingSystem.WriteText(x, y, textNode.Content, style);
            }
        }
    }

    private void RenderBox(ElementNode node)
    {
        var x = GetIntProp(node, "x", _currentRenderContext?.X ?? 0);
        var y = GetIntProp(node, "y", _currentRenderContext?.Y ?? 0);
        var width = GetIntProp(node, "width", 0);
        var height = GetIntProp(node, "height", 0);
        var borderStyle = GetBoxStyleProp(node, "border-style", BoxStyle.Single);
        var style = GetStyleProp(node, "style", Style.Default);

        _renderingSystem.DrawBox(x, y, width, height, style, borderStyle);
    }

    private void RenderTable(ElementNode node)
    {
        // Table rendering would be implemented here
        foreach (var child in node.Children)
        {
            child.Accept(this);
        }
    }

    #endregion

    #region Helper Methods

    private string GetTextContent(VirtualNode node)
    {
        if (node is TextNode textNode)
            return textNode.Content;

        if (node is ElementNode elementNode)
        {
            var textParts = new List<string>();
            foreach (var child in elementNode.Children)
            {
                var childText = GetTextContent(child);
                if (!string.IsNullOrEmpty(childText))
                    textParts.Add(childText);
            }
            return string.Join("", textParts);
        }

        return string.Empty;
    }

    private int GetIntProp(VirtualNode node, string key, int defaultValue)
    {
        return node.Props.TryGetValue(key, out var value) && value is int intValue
            ? intValue : defaultValue;
    }

    private Color? GetColorProp(VirtualNode node, string key)
    {
        return node.Props.TryGetValue(key, out var value) && value is Color color
            ? color : null;
    }

    private Style GetStyleProp(VirtualNode node, string key, Style defaultStyle)
    {
        return node.Props.TryGetValue(key, out var value) && value is Style style
            ? style : defaultStyle;
    }

    private BoxStyle GetBoxStyleProp(VirtualNode node, string key, BoxStyle defaultStyle)
    {
        return node.Props.TryGetValue(key, out var value) && value is BoxStyle boxStyle
            ? boxStyle : defaultStyle;
    }


    #endregion

    /// <summary>
    /// Comparer for int[] paths used as dictionary keys.
    /// </summary>
    private class PathComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[]? x, int[]? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i]) return false;
            }

            return true;
        }

        public int GetHashCode(int[] obj)
        {
            if (obj == null) return 0;

            int hash = 17;
            foreach (var item in obj)
            {
                hash = hash * 31 + item.GetHashCode();
            }
            return hash;
        }
    }
}