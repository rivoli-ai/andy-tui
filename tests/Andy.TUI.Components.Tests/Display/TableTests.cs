using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Components.Display;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Display;

public class TableTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }
    
    private Table<TestItem> CreateTable()
    {
        var table = new Table<TestItem>();
        var context = TestHelpers.CreateMockContext(table);
        table.Initialize(context);
        return table;
    }
    
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var table = CreateTable();
        
        Assert.Empty(table.Columns);
        Assert.Empty(table.Items);
        Assert.True(table.ShowHeader);
        Assert.True(table.ShowBorder);
        Assert.False(table.AllowSelection);
        Assert.Equal(-1, table.SelectedIndex);
        Assert.Null(table.SelectedItem);
    }
    
    [Fact]
    public void Columns_CanBeSet()
    {
        var table = CreateTable();
        var columns = new List<TableColumn<TestItem>>
        {
            new() { Header = "ID", Width = 5 },
            new() { Header = "Name", Width = 20 }
        };
        
        table.Columns = columns;
        
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal("ID", table.Columns[0].Header);
        Assert.Equal(5, table.Columns[0].Width);
    }
    
    [Fact]
    public void Items_CanBeSet()
    {
        var table = CreateTable();
        var items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1" },
            new() { Id = 2, Name = "Item 2" }
        };
        
        table.Items = items;
        
        Assert.Equal(2, table.Items.Count);
        Assert.Equal("Item 1", table.Items[0].Name);
    }
    
    [Fact]
    public void Measure_ReturnsCorrectSize()
    {
        var table = CreateTable();
        table.Columns = new List<TableColumn<TestItem>>
        {
            new() { Width = 10 },
            new() { Width = 20 }
        };
        table.Items = new List<TestItem> { new(), new(), new() };
        
        var size = table.Measure(new Size(100, 100));
        
        // Width: 1 (left border) + 10 + 1 (separator) + 20 + 1 (right border) = 33
        // Height: 1 (top) + 1 (header) + 1 (separator) + 3 (items) + 1 (bottom) = 7
        Assert.Equal(33, size.Width);
        Assert.Equal(7, size.Height);
    }
    
    [Fact]
    public void Measure_WithoutBorder_ReturnsCorrectSize()
    {
        var table = CreateTable();
        table.ShowBorder = false;
        table.Columns = new List<TableColumn<TestItem>>
        {
            new() { Width = 10 },
            new() { Width = 20 }
        };
        
        var size = table.Measure(new Size(100, 100));
        
        Assert.Equal(30, size.Width); // No borders
    }
    
    [Fact]
    public void AllowSelection_EnablesSelection()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new() };
        
        table.AllowSelection = true;
        
        Assert.Equal(0, table.SelectedIndex);
        Assert.NotNull(table.SelectedItem);
    }
    
    [Fact]
    public void AllowSelection_DisablingClearsSelection()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new() };
        table.AllowSelection = true;
        table.SelectedIndex = 1;
        
        table.AllowSelection = false;
        
        Assert.Equal(-1, table.SelectedIndex);
        Assert.Null(table.SelectedItem);
    }
    
    [Fact]
    public void SelectedIndex_CanBeSet()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new(), new() };
        table.AllowSelection = true;
        
        table.SelectedIndex = 2;
        
        Assert.Equal(2, table.SelectedIndex);
        Assert.Same(table.Items[2], table.SelectedItem);
    }
    
    [Fact]
    public void SelectedIndex_ClampedToValidRange()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new() };
        table.AllowSelection = true;
        
        table.SelectedIndex = 5;
        Assert.Equal(0, table.SelectedIndex); // Should remain at 0
        
        table.SelectedIndex = -2;
        Assert.Equal(0, table.SelectedIndex); // Should remain at 0
    }
    
    [Fact]
    public void SelectionChanged_RaisedWhenSelectionChanges()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new() };
        table.AllowSelection = true;
        
        var raised = false;
        table.SelectionChanged += (s, e) => raised = true;
        
        table.SelectedIndex = 1;
        
        Assert.True(raised);
    }
    
    [Fact]
    public void HandleKeyPress_UpArrow_MovesSelectionUp()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new(), new() };
        table.AllowSelection = true;
        table.SelectedIndex = 2;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        
        Assert.True(handled);
        Assert.Equal(1, table.SelectedIndex);
    }
    
    [Fact]
    public void HandleKeyPress_DownArrow_MovesSelectionDown()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new(), new() };
        table.AllowSelection = true;
        table.SelectedIndex = 0;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        
        Assert.True(handled);
        Assert.Equal(1, table.SelectedIndex);
    }
    
    [Fact]
    public void HandleKeyPress_Home_MovesToFirst()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new(), new() };
        table.AllowSelection = true;
        table.SelectedIndex = 2;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Home, false, false, false));
        
        Assert.True(handled);
        Assert.Equal(0, table.SelectedIndex);
    }
    
    [Fact]
    public void HandleKeyPress_End_MovesToLast()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new(), new(), new() };
        table.AllowSelection = true;
        table.SelectedIndex = 0;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.End, false, false, false));
        
        Assert.True(handled);
        Assert.Equal(2, table.SelectedIndex);
    }
    
    [Fact]
    public void HandleKeyPress_Enter_RaisesRowActivated()
    {
        var table = CreateTable();
        var item = new TestItem { Id = 1, Name = "Test" };
        table.Items = new List<TestItem> { item };
        table.AllowSelection = true;
        
        TableRowEventArgs<TestItem>? args = null;
        table.RowActivated += (s, e) => args = e;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Enter, false, false, false));
        
        Assert.True(handled);
        Assert.NotNull(args);
        Assert.Same(item, args.Item);
        Assert.Equal(0, args.Index);
    }
    
    [Fact]
    public void HandleKeyPress_WithoutSelection_ReturnsFalse()
    {
        var table = CreateTable();
        table.Items = new List<TestItem> { new() };
        table.AllowSelection = false;
        
        var handled = table.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.UpArrow, false, false, false));
        
        Assert.False(handled);
    }
    
    [Fact]
    public void TableColumn_GetValue_UsesValueGetter()
    {
        var column = new TableColumn<TestItem>
        {
            Header = "ID",
            ValueGetter = item => $"#{item.Id:D3}"
        };
        
        var item = new TestItem { Id = 42 };
        var value = column.GetValue(item);
        
        Assert.Equal("#042", value);
    }
    
    [Fact]
    public void TableColumn_GetValue_FallsBackToToString()
    {
        var column = new TableColumn<TestItem>
        {
            Header = "Item"
            // No ValueGetter set
        };
        
        var item = new TestItem { Id = 1, Name = "Test" };
        var value = column.GetValue(item);
        
        Assert.Equal(item.ToString(), value);
    }
    
    [Fact]
    public void Render_ProducesCorrectNodes()
    {
        var table = CreateTable();
        table.Columns = new List<TableColumn<TestItem>>
        {
            new() { Header = "ID", Width = 5, ValueGetter = i => i.Id.ToString() },
            new() { Header = "Name", Width = 10, ValueGetter = i => i.Name }
        };
        table.Items = new List<TestItem>
        {
            new() { Id = 1, Name = "Item 1" }
        };
        table.Arrange(new Rectangle(0, 0, 20, 10));
        
        var node = table.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        // Should have border nodes and content nodes
        Assert.Contains("ID", texts.FirstOrDefault(t => t.Contains("ID")));
        Assert.Contains("Name", texts.FirstOrDefault(t => t.Contains("Name")));
        Assert.Contains("1", texts.FirstOrDefault(t => t.Contains("1")));
        Assert.Contains("Item 1", texts.FirstOrDefault(t => t.Contains("Item 1")));
    }
    
    [Fact]
    public void Render_WithSelection_AppliesSelectionStyle()
    {
        var table = CreateTable();
        table.Columns = new List<TableColumn<TestItem>>
        {
            new() { Header = "Name", Width = 10, ValueGetter = i => i.Name }
        };
        table.Items = new List<TestItem>
        {
            new() { Name = "Selected" },
            new() { Name = "Not Selected" }
        };
        table.AllowSelection = true;
        table.SelectedIndex = 0;
        table.Arrange(new Rectangle(0, 0, 20, 10));
        
        var node = table.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        // Check that selected text is present
        Assert.Contains("Selected", texts.FirstOrDefault(t => t.Contains("Selected") && !t.Contains("Not")));
    }
    
    [Fact]
    public void Alignment_FormatsTextCorrectly()
    {
        var table = CreateTable();
        table.ShowBorder = false;
        table.ShowHeader = false;
        table.Columns = new List<TableColumn<TestItem>>
        {
            new() { Width = 10, ValueGetter = i => "Left", Alignment = TableAlignment.Left },
            new() { Width = 10, ValueGetter = i => "Right", Alignment = TableAlignment.Right },
            new() { Width = 10, ValueGetter = i => "Center", Alignment = TableAlignment.Center }
        };
        table.Items = new List<TestItem> { new() };
        table.Arrange(new Rectangle(0, 0, 30, 5));
        
        var node = table.Render();
        var texts = TestHelpers.FindTextNodes(node);
        var contentText = texts.FirstOrDefault(t => t.Contains("Left"));
        
        Assert.NotNull(contentText);
        // Left aligned: "Left      "
        // Right aligned: "     Right"
        // Center aligned: "  Center  "
        Assert.Contains("Left      ", contentText);
        Assert.Contains("     Right", contentText);
        Assert.Contains("  Center  ", contentText);
    }
}