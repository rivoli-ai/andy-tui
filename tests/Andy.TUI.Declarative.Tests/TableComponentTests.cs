using System;
using System.Collections.Generic;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests;

public class TableComponentTests
{
    [Fact]
    public void Table_CreatesInstanceWithCorrectProperties()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<TestItem>.None;
        var binding = new Binding<Optional<TestItem>>(() => selectedItem, v => selectedItem = v);

        var items = new[]
        {
            new TestItem { Id = 1, Name = "Item 1", Value = 10 },
            new TestItem { Id = 2, Name = "Item 2", Value = 20 }
        };

        var columns = new[]
        {
            new TableColumn<TestItem>("ID", item => item.Id.ToString(), width: 5),
            new TableColumn<TestItem>("Name", item => item.Name),
            new TableColumn<TestItem>("Value", item => item.Value.ToString())
        };

        var table = new Table<TestItem>(items, columns, binding)
            .VisibleRows(5);

        // Act
        var instance = manager.GetOrCreateInstance(table, "table1") as TableInstance<TestItem>;
        Assert.NotNull(instance);

        // Assert
        Assert.IsType<TableInstance<TestItem>>(instance);
        Assert.True(instance.CanFocus);
    }

    [Fact]
    public void Table_CalculatesLayoutCorrectly()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { new TestItem { Id = 1, Name = "Test", Value = 100 } };

        var columns = new[]
        {
            new TableColumn<TestItem>("ID", item => item.Id.ToString(), width: 5),
            new TableColumn<TestItem>("Name", item => item.Name, width: 10),
            new TableColumn<TestItem>("Value", item => item.Value.ToString(), width: 8)
        };

        var table = new Table<TestItem>(items, columns, visibleRows: 3);
        var instance = manager.GetOrCreateInstance(table, "table1") as TableInstance<TestItem>;
        Assert.NotNull(instance);

        // Act
        instance.CalculateLayout(LayoutConstraints.Loose(100, 50));

        // Assert
        // Width: 5 + 10 + 8 + (2 * 3) for separators + 4 for borders = 33
        // Height: 3 rows + 2 header lines + 2 borders = 7
        Assert.True(instance.Layout.Width >= 33);
        Assert.Equal(7, instance.Layout.Height);
    }

    [Fact]
    public void Table_HandlesSortableColumns()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<TestItem>.None;
        var binding = new Binding<Optional<TestItem>>(() => selectedItem, v => selectedItem = v);

        var items = new[]
        {
            new TestItem { Id = 3, Name = "Charlie", Value = 30 },
            new TestItem { Id = 1, Name = "Alice", Value = 10 },
            new TestItem { Id = 2, Name = "Bob", Value = 20 }
        };

        var columns = new[]
        {
            new TableColumn<TestItem>("ID", item => item.Id.ToString(), sortable: true,
                comparer: (a, b) => a.Id.CompareTo(b.Id)),
            new TableColumn<TestItem>("Name", item => item.Name, sortable: true,
                comparer: (a, b) => string.Compare(a.Name, b.Name))
        };

        var table = new Table<TestItem>(items, columns, binding);

        // Act
        var instance = manager.GetOrCreateInstance(table, "table1") as TableInstance<TestItem>;
        Assert.NotNull(instance);

        // Assert - instance created successfully with sortable columns
        Assert.IsType<TableInstance<TestItem>>(instance);
    }

    [Fact]
    public void Table_HandlesKeyboardNavigation()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var selectedItem = Optional<TestItem>.None;
        var binding = new Binding<Optional<TestItem>>(() => selectedItem, v => selectedItem = v);

        var items = new[]
        {
            new TestItem { Id = 1, Name = "A", Value = 1 },
            new TestItem { Id = 2, Name = "B", Value = 2 },
            new TestItem { Id = 3, Name = "C", Value = 3 }
        };

        var columns = new[]
        {
            new TableColumn<TestItem>("Name", item => item.Name)
        };

        var table = new Table<TestItem>(items, columns, binding);
        var instance = manager.GetOrCreateInstance(table, "table1") as TableInstance<TestItem>;
        Assert.NotNull(instance);

        instance.Update(table);
        instance.OnGotFocus();

        // Act - Navigate down
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.DownArrow, false, false, false));
        Assert.True(handled);

        // Select item
        handled = instance.HandleKeyPress(new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false));
        Assert.True(handled);

        // Assert
        Assert.True(binding.Value.TryGetValue(out var selected));
        Assert.Equal("B", selected.Name);
    }

    [Fact]
    public void Table_SupportsHidingBorderAndHeader()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var items = new[] { new TestItem { Id = 1, Name = "Test", Value = 100 } };
        var columns = new[] { new TableColumn<TestItem>("Name", item => item.Name) };

        var table = new Table<TestItem>(items, columns)
            .HideBorder()
            .HideHeader();

        // Act
        var instance = manager.GetOrCreateInstance(table, "table1") as TableInstance<TestItem>;
        Assert.NotNull(instance);

        instance.CalculateLayout(LayoutConstraints.Loose(100, 50));

        // Assert - without border and header, height should be just visible rows (10 default)
        Assert.Equal(10, instance.Layout.Height); // Default visible rows without borders/header
    }

    // Test data class
    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}