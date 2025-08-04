using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for List component.
/// </summary>
public class ListInstance : ViewInstance
{
    private IReadOnlyList<ISimpleComponent> _items = Array.Empty<ISimpleComponent>();
    private ListMarkerStyle _markerStyle = ListMarkerStyle.Bullet;
    private string _customMarker = "";
    private Color _markerColor = Color.Gray;
    private int _indent = 2;
    private int _spacing = 0;
    
    private readonly List<ViewInstance> _itemInstances = new();
    
    public ListInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not List list)
            throw new InvalidOperationException($"Expected List, got {viewDeclaration.GetType()}");
        
        _items = list.GetItems();
        _markerStyle = list.GetMarkerStyle();
        _customMarker = list.GetCustomMarker();
        _markerColor = list.GetMarkerColor();
        _indent = list.GetIndent();
        _spacing = list.GetSpacing();
        
        // Update item instances
        UpdateItemInstances();
    }
    
    private void UpdateItemInstances()
    {
        // Clear old instances
        _itemInstances.Clear();
        
        // Create instances for each item
        var manager = Context?.ViewInstanceManager;
        if (manager != null)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                var itemInstance = manager.GetOrCreateInstance(item, $"{Id}_item{i}");
                _itemInstances.Add(itemInstance);
            }
        }
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (_items.Count == 0)
        {
            return new LayoutBox { Width = 10, Height = 1 };
        }
        
        // Calculate marker width
        var maxMarkerWidth = 0;
        for (int i = 0; i < _items.Count; i++)
        {
            var marker = Components.List.GetMarker(_markerStyle, i, _customMarker);
            maxMarkerWidth = Math.Max(maxMarkerWidth, marker.Length);
        }
        
        var markerSpace = maxMarkerWidth + 1; // +1 for space after marker
        var contentWidth = constraints.MaxWidth - _indent - markerSpace;
        var contentConstraints = LayoutConstraints.Loose(contentWidth, constraints.MaxHeight);
        
        var totalHeight = 0;
        var maxWidth = 0;
        
        // Layout each item
        for (int i = 0; i < _itemInstances.Count; i++)
        {
            var itemInstance = _itemInstances[i];
            itemInstance.CalculateLayout(contentConstraints);
            
            var itemLayout = itemInstance.Layout;
            totalHeight += (int)itemLayout.Height;
            
            if (i < _itemInstances.Count - 1)
            {
                totalHeight += _spacing;
            }
            
            maxWidth = Math.Max(maxWidth, (int)itemLayout.Width + _indent + markerSpace);
        }
        
        return new LayoutBox 
        { 
            Width = Math.Min(maxWidth, constraints.MaxWidth),
            Height = Math.Min(totalHeight, constraints.MaxHeight)
        };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_items.Count == 0)
        {
            return Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode("(Empty list)"))
                .Build();
        }
        
        var children = new List<VirtualNode>();
        var currentY = 0;
        
        // Calculate marker width for alignment
        var maxMarkerWidth = 0;
        for (int i = 0; i < _items.Count; i++)
        {
            var marker = Components.List.GetMarker(_markerStyle, i, _customMarker);
            maxMarkerWidth = Math.Max(maxMarkerWidth, marker.Length);
        }
        
        // Render each item with its marker
        for (int i = 0; i < _itemInstances.Count; i++)
        {
            var itemInstance = _itemInstances[i];
            var marker = Components.List.GetMarker(_markerStyle, i, _customMarker);
            
            // Render marker
            children.Add(Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(_markerColor))
                .WithProp("x", (int)(layout.AbsoluteX + _indent))
                .WithProp("y", (int)(layout.AbsoluteY + currentY))
                .WithChild(new TextNode(marker.PadRight(maxMarkerWidth)))
                .Build());
            
            // Update item position and render
            var itemLayout = itemInstance.Layout;
            itemLayout.AbsoluteX = layout.AbsoluteX + _indent + maxMarkerWidth + 1;
            itemLayout.AbsoluteY = layout.AbsoluteY + currentY;
            
            children.Add(itemInstance.Render());
            
            currentY += (int)itemLayout.Height;
            if (i < _itemInstances.Count - 1)
            {
                currentY += _spacing;
            }
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
    
    public override void Dispose()
    {
        foreach (var instance in _itemInstances)
        {
            instance.Dispose();
        }
        _itemInstances.Clear();
        base.Dispose();
    }
}