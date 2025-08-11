using System;
using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class ListComponentTests
{
    [Fact]
    public void List_CreatesSuccessfully()
    {
        // Act
        var list = new List(new ISimpleComponent[] {
            new Text("First item"),
            new Text("Second item"),
            new Text("Third item")
        });

        // Assert
        Assert.NotNull(list);
    }

    [Fact]
    public void List_CreatesWithAllParameters()
    {
        // Act
        var list = new List(
            new ISimpleComponent[] {
                new Text("Apple"),
                new Text("Banana"),
                new Text("Cherry")
            },
            ListMarkerStyle.Number,
            customMarker: "→",
            markerColor: Color.Green,
            indent: 4,
            spacing: 2
        );

        // Assert
        Assert.NotNull(list);
    }

    [Theory]
    [InlineData(ListMarkerStyle.Bullet, "•")]
    [InlineData(ListMarkerStyle.Dash, "-")]
    [InlineData(ListMarkerStyle.Arrow, "→")]
    [InlineData(ListMarkerStyle.Star, "*")]
    [InlineData(ListMarkerStyle.Square, "■")]
    [InlineData(ListMarkerStyle.Circle, "○")]
    [InlineData(ListMarkerStyle.Diamond, "◆")]
    public void GetMarker_ReturnsCorrectMarkerForStyle(ListMarkerStyle style, string expectedMarker)
    {
        // Act
        var marker = List.GetMarker(style, 0, "");

        // Assert
        Assert.Equal(expectedMarker, marker);
    }

    [Theory]
    [InlineData(0, "1.")]
    [InlineData(1, "2.")]
    [InlineData(9, "10.")]
    [InlineData(99, "100.")]
    public void GetMarker_ReturnsCorrectNumberForNumberStyle(int index, string expectedMarker)
    {
        // Act
        var marker = List.GetMarker(ListMarkerStyle.Number, index, "");

        // Assert
        Assert.Equal(expectedMarker, marker);
    }

    [Theory]
    [InlineData(0, "a.")]
    [InlineData(1, "b.")]
    [InlineData(25, "z.")]
    [InlineData(26, "a.")] // Wraps back to 'a'
    [InlineData(27, "b.")] // Wraps to 'b'
    [InlineData(51, "z.")] // 51 % 26 = 25 = 'z'
    [InlineData(52, "a.")] // 52 % 26 = 0 = 'a'
    public void GetMarker_ReturnsCorrectLetterForLetterStyle(int index, string expectedMarker)
    {
        // Act
        var marker = List.GetMarker(ListMarkerStyle.Letter, index, "");

        // Assert
        Assert.Equal(expectedMarker, marker);
    }

    [Theory]
    [InlineData(0, "i.")]
    [InlineData(1, "ii.")]
    [InlineData(2, "iii.")]
    [InlineData(3, "iv.")]
    [InlineData(4, "v.")]
    [InlineData(8, "ix.")]
    [InlineData(9, "x.")]
    public void GetMarker_ReturnsCorrectRomanNumeralForRomanStyle(int index, string expectedMarker)
    {
        // Act
        var marker = List.GetMarker(ListMarkerStyle.Roman, index, "");

        // Assert
        Assert.Equal(expectedMarker, marker);
    }

    [Fact]
    public void GetMarker_ReturnsRomanNumeralsCorrectly()
    {
        // Test Roman numeral markers with more examples
        var testCases = new[] {
            (3, "iv."), (4, "v."), (8, "ix."), (14, "xv."), (19, "xx.")
        };

        foreach (var (index, expected) in testCases)
        {
            var result = List.GetMarker(ListMarkerStyle.Roman, index);
            Assert.Equal(expected, result);
        }
    }

    [Fact]
    public void GetMarker_ReturnsCustomMarkerForCustomStyle()
    {
        // Act
        var marker = List.GetMarker(ListMarkerStyle.Custom, 0, "➤");

        // Assert
        Assert.Equal("➤", marker);
    }

    [Fact]
    public void ListInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var list = new List(new ISimpleComponent[] {
            new Text("One"),
            new Text("Two"),
            new Text("Three")
        });

        // Act
        var instance = manager.GetOrCreateInstance(list, "list1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<ListInstance>(instance);
    }

    [Fact]
    public void List_HandlesEmptyItemsList()
    {
        // Act
        var list = new List(Array.Empty<ISimpleComponent>());

        // Assert
        Assert.NotNull(list);
    }

    [Fact]
    public void List_ShowcaseExamples()
    {
        // Test examples from UIComponentsShowcase

        // Bullet list
        var bulletList = new List(
            new ISimpleComponent[] {
                new Text("Bullet list item 1"),
                new Text("Bullet list item 2"),
                new Text("Bullet list item 3").Color(Color.Green)
            },
            ListMarkerStyle.Bullet
        );
        Assert.NotNull(bulletList);

        // Numbered list
        var numberedList = new List(
            new ISimpleComponent[] {
                new Text("First numbered item"),
                new Text("Second numbered item"),
                new Text("Third numbered item")
            },
            ListMarkerStyle.Number,
            markerColor: Color.Yellow
        );
        Assert.NotNull(numberedList);

        // Arrow list with mixed content
        var arrowList = new List(
            new ISimpleComponent[] {
                new Text("Arrow item").Bold(),
                new Text("Another arrow item"),
                new Box { new Text("Boxed item") }.WithPadding(1)
            },
            ListMarkerStyle.Arrow,
            markerColor: Color.Cyan
        );
        Assert.NotNull(arrowList);
    }

    [Fact]
    public void GetMarker_HandlesLargeIndices()
    {
        // Test that large indices don't cause issues
        var marker = List.GetMarker(ListMarkerStyle.Number, 999, "");
        Assert.Equal("1000.", marker);

        var letterMarker = List.GetMarker(ListMarkerStyle.Letter, 701, ""); // 701 % 26 = 25 = 'z'
        Assert.Equal("z.", letterMarker);
    }
}