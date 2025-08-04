using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for MultiSelectInput component.
/// </summary>
public class MultiSelectInputInstance<T> : ViewInstance, IFocusable
{
    private IReadOnlyList<T> _items = Array.Empty<T>();
    private Binding<ISet<T>>? _selectedItemsBinding;
    private Func<T, string> _itemRenderer = _ => "";
    private string _checkedMark = "[×]";
    private string _uncheckedMark = "[ ]";
    private int _currentIndex = 0;
    
    public MultiSelectInputInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not MultiSelectInput<T> multiSelect)
            throw new InvalidOperationException($"Expected MultiSelectInput<{typeof(T).Name}>, got {viewDeclaration.GetType()}");
        
        // Check if items have changed
        var itemsChanged = !ReferenceEquals(_items, multiSelect.GetItems());
        
        _items = multiSelect.GetItems();
        _selectedItemsBinding = multiSelect.GetSelectedItemsBinding();
        _itemRenderer = multiSelect.GetItemRenderer();
        _checkedMark = multiSelect.GetCheckedMark();
        _uncheckedMark = multiSelect.GetUncheckedMark();
        
        // If items changed, validate current index
        if (itemsChanged && _currentIndex >= _items.Count)
        {
            _currentIndex = Math.Max(0, _items.Count - 1);
        }
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        if (_items.Count == 0)
        {
            return new LayoutBox { Width = constraints.MaxWidth, Height = 1 };
        }
        
        // Calculate the maximum width needed
        var markLength = Math.Max(_checkedMark.Length, _uncheckedMark.Length);
        var maxItemWidth = _items.Max(item => 
            _itemRenderer(item).Length + markLength + 1);
        
        var width = Math.Min(maxItemWidth, constraints.MaxWidth);
        var height = Math.Min(_items.Count, constraints.MaxHeight);
        
        return new LayoutBox { Width = width, Height = height };
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_items.Count == 0)
        {
            return Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode("(No items)"))
                .Build();
        }
        
        var selectedItems = _selectedItemsBinding?.Value ?? new HashSet<T>();
        var children = new List<VirtualNode>();
        var visibleCount = Math.Min(_items.Count, (int)layout.Height);
        
        // Calculate scroll offset to keep current item visible
        int scrollOffset = 0;
        if (_currentIndex >= visibleCount)
        {
            scrollOffset = _currentIndex - visibleCount + 1;
        }
        
        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = i + scrollOffset;
            if (itemIndex >= _items.Count) break;
            
            var item = _items[itemIndex];
            var isSelected = selectedItems.Contains(item);
            var isFocused = itemIndex == _currentIndex && IsFocused;
            
            var mark = isSelected ? _checkedMark : _uncheckedMark;
            var text = $"{mark} {_itemRenderer(item)}";
            var paddedText = text.PadRight((int)layout.Width);
            
            var textColor = isFocused ? Color.Black : Color.Gray;
            var bgColor = isFocused ? Color.Cyan : Color.Black;
            
            var style = Style.Default
                .WithForegroundColor(textColor)
                .WithBackgroundColor(bgColor);
            
            var textNode = Element("text")
                .WithProp("style", style)
                .WithProp("x", (int)layout.AbsoluteX)
                .WithProp("y", (int)(layout.AbsoluteY + i))
                .WithChild(new TextNode(paddedText))
                .Build();
            
            children.Add(textNode);
        }
        
        // Add scroll indicators if needed
        if (scrollOffset > 0)
        {
            var upIndicator = Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)(layout.AbsoluteX + layout.Width - 1))
                .WithProp("y", (int)layout.AbsoluteY)
                .WithChild(new TextNode("↑"))
                .Build();
            children.Add(upIndicator);
        }
        
        if (scrollOffset + visibleCount < _items.Count)
        {
            var downIndicator = Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.DarkGray))
                .WithProp("x", (int)(layout.AbsoluteX + layout.Width - 1))
                .WithProp("y", (int)(layout.AbsoluteY + visibleCount - 1))
                .WithChild(new TextNode("↓"))
                .Build();
            children.Add(downIndicator);
        }
        
        return Element("container")
            .WithChildren(children.ToArray())
            .Build();
    }
    
    public bool IsFocused { get; private set; }
    public bool CanFocus => true;
    
    public void OnGotFocus()
    {
        IsFocused = true;
        InvalidateView();
    }
    
    public void OnLostFocus()
    {
        IsFocused = false;
        InvalidateView();
    }
    
    public bool HandleKeyPress(ConsoleKeyInfo key)
    {
        if (_items.Count == 0) return false;
        
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (_currentIndex > 0)
                {
                    _currentIndex--;
                    InvalidateView();
                }
                return true;
                
            case ConsoleKey.DownArrow:
                if (_currentIndex < _items.Count - 1)
                {
                    _currentIndex++;
                    InvalidateView();
                }
                return true;
                
            case ConsoleKey.Home:
                _currentIndex = 0;
                InvalidateView();
                return true;
                
            case ConsoleKey.End:
                _currentIndex = _items.Count - 1;
                InvalidateView();
                return true;
                
            case ConsoleKey.PageUp:
                _currentIndex = Math.Max(0, _currentIndex - 5);
                InvalidateView();
                return true;
                
            case ConsoleKey.PageDown:
                _currentIndex = Math.Min(_items.Count - 1, _currentIndex + 5);
                InvalidateView();
                return true;
                
            case ConsoleKey.Spacebar:
            case ConsoleKey.Enter:
                if (_selectedItemsBinding != null && _currentIndex < _items.Count)
                {
                    var currentItem = _items[_currentIndex];
                    var selectedItems = new HashSet<T>(_selectedItemsBinding.Value ?? new HashSet<T>());
                    
                    if (selectedItems.Contains(currentItem))
                    {
                        selectedItems.Remove(currentItem);
                    }
                    else
                    {
                        selectedItems.Add(currentItem);
                    }
                    
                    _selectedItemsBinding.Value = selectedItems;
                    InvalidateView();
                }
                return true;
                
            default:
                return false;
        }
    }
}