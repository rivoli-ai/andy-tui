using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal.Rendering;
using Moq;
using Xunit;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Terminal.Tests.Rendering;

public class VirtualDomRendererTests
{
    private readonly Mock<IRenderingSystem> _mockRenderingSystem;
    private readonly VirtualDomRenderer _renderer;

    public VirtualDomRendererTests()
    {
        _mockRenderingSystem = new Mock<IRenderingSystem>();
        _renderer = new VirtualDomRenderer(_mockRenderingSystem.Object);
    }

    [Fact]
    public void Render_SimpleTextNode_RendersCorrectly()
    {
        // Arrange
        var tree = Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            .WithProp("style", Style.Default.WithForegroundColor(Color.Green))
            .WithChild(Text("Hello World"))
            .Build();

        // Act
        _renderer.Render(tree);

        // Assert
        _mockRenderingSystem.Verify(r => r.WriteText(10, 5, "Hello World",
            It.Is<Style>(s => s.Foreground == Color.Green)), Times.Once);
    }

    [Fact]
    public void Render_RectElement_FillsArea()
    {
        // Arrange
        var tree = Element("rect")
            .WithProp("x", 0)
            .WithProp("y", 0)
            .WithProp("width", 20)
            .WithProp("height", 10)
            .WithProp("fill", Color.Blue)
            .Build();

        // Act
        _renderer.Render(tree);

        // Assert
        _mockRenderingSystem.Verify(r => r.FillRect(0, 0, 20, 10, ' ',
            It.Is<Style>(s => s.Background == Color.Blue)), Times.Once);
    }

    [Fact]
    public void Render_BoxElement_DrawsBox()
    {
        // Arrange
        var tree = Element("box")
            .WithProp("x", 5)
            .WithProp("y", 5)
            .WithProp("width", 30)
            .WithProp("height", 10)
            .WithProp("border-style", BoxStyle.Double)
            .WithProp("style", Style.Default.WithForegroundColor(Color.Cyan))
            .Build();

        // Act
        _renderer.Render(tree);

        // Assert
        _mockRenderingSystem.Verify(r => r.DrawBox(5, 5, 30, 10,
            It.Is<Style>(s => s.Foreground == Color.Cyan), BoxStyle.Double), Times.Once);
    }

    [Fact]
    public void Render_NestedElements_RendersInCorrectOrder()
    {
        // Arrange
        var tree = Element("box")
            .WithProp("x", 0)
            .WithProp("y", 0)
            .WithProp("width", 40)
            .WithProp("height", 20)
            .WithChildren(
                Element("rect")
                    .WithProp("x", 5)
                    .WithProp("y", 5)
                    .WithProp("width", 30)
                    .WithProp("height", 10)
                    .WithProp("fill", Color.DarkBlue),
                Element("text")
                    .WithProp("x", 10)
                    .WithProp("y", 10)
                    .WithChild(Text("Overlay Text"))
            )
            .Build();

        // Act
        _renderer.Render(tree);

        // Assert
        var sequence = new MockSequence();
        _mockRenderingSystem.InSequence(sequence).Setup(r => r.DrawBox(0, 0, 40, 20, It.IsAny<Style>(), It.IsAny<BoxStyle>()));
        _mockRenderingSystem.InSequence(sequence).Setup(r => r.FillRect(5, 5, 30, 10, ' ', It.IsAny<Style>()));
        _mockRenderingSystem.InSequence(sequence).Setup(r => r.WriteText(10, 10, "Overlay Text", It.IsAny<Style>()));
    }

    [Fact]
    public void Render_WithZIndex_RendersInCorrectOrder()
    {
        // Arrange
        var tree = Fragment(
            Element("text")
                .WithProp("x", 10)
                .WithProp("y", 10)
                .WithProp("z-index", 2)
                .WithChild(Text("Top")),
            Element("rect")
                .WithProp("x", 8)
                .WithProp("y", 9)
                .WithProp("width", 10)
                .WithProp("height", 3)
                .WithProp("fill", Color.Red)
                .WithProp("z-index", 1),
            Element("text")
                .WithProp("x", 5)
                .WithProp("y", 10)
                .WithProp("z-index", 0)
                .WithChild(Text("Bottom"))
        );

        // Assert - Elements should be rendered in z-index order (0, 1, 2)
        var callOrder = new List<string>();
        _mockRenderingSystem.Setup(r => r.WriteText(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Style>()))
            .Callback<int, int, string, Style>((x, y, text, style) => callOrder.Add($"text:{text}"));
        _mockRenderingSystem.Setup(r => r.FillRect(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<char>(), It.IsAny<Style>()))
            .Callback<int, int, int, int, char, Style>((x, y, w, h, c, s) => callOrder.Add("rect"));

        // Act
        _renderer.Render(tree);

        Assert.Equal(new[] { "text:Bottom", "rect", "text:Top" }, callOrder);
    }

    [Fact]
    public void ApplyPatches_UpdateText_OnlyRedrawsAffectedRegion()
    {
        // Arrange
        var oldTree = Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            .WithProp("width", 10)
            .WithProp("height", 1)
            .WithChild(Text("Old Text"))
            .Build();

        _renderer.Render(oldTree);
        _mockRenderingSystem.Invocations.Clear();

        var patches = new List<Patch>
        {
            new UpdateTextPatch(new[] { 0 }, "New Text")
        };

        // Act
        _renderer.ApplyPatches(patches);

        // Assert - Should clear the dirty region and re-render
        _mockRenderingSystem.Verify(r => r.FillRect(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), 1, ' ', Style.Default), Times.AtLeastOnce);
        _mockRenderingSystem.Verify(r => r.WriteText(10, 5, "New Text", It.IsAny<Style>()), Times.Once);
    }

    [Fact]
    public void DirtyRegionTracker_MergesAdjacentRegions()
    {
        // Arrange
        var tracker = new DirtyRegionTracker();

        // Act
        tracker.MarkDirty(new Rectangle(0, 0, 10, 10));
        tracker.MarkDirty(new Rectangle(10, 0, 10, 10)); // Adjacent horizontally
        tracker.MarkDirty(new Rectangle(0, 10, 20, 10)); // Adjacent vertically

        // Assert
        var regions = tracker.GetDirtyRegions();
        Assert.Single(regions);
        Assert.Equal(new Rectangle(0, 0, 20, 20), regions[0]);
    }

    [Fact]
    public void DirtyRegionTracker_MergesOverlappingRegions()
    {
        // Arrange
        var tracker = new DirtyRegionTracker();

        // Act
        tracker.MarkDirty(new Rectangle(0, 0, 20, 20));
        tracker.MarkDirty(new Rectangle(10, 10, 20, 20)); // Overlapping

        // Assert
        var regions = tracker.GetDirtyRegions();
        Assert.Single(regions);
        Assert.Equal(new Rectangle(0, 0, 30, 30), regions[0]);
    }

    [Fact]
    public void ApplyPatches_UpdatePropsWithStyleChange_MarksCorrectDirtyRegion()
    {
        // Arrange
        var oldTree = Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            .WithProp("style", Style.Default.WithForegroundColor(Color.White))
            .WithChild(Text("Button"))
            .Build();

        _renderer.Render(oldTree);
        _mockRenderingSystem.Invocations.Clear();

        // Create patch to update style
        var patches = new List<Patch>
        {
            new UpdatePropsPatch(
                new[] { 0 },
                new Dictionary<string, object?> { { "style", Style.Default.WithForegroundColor(Color.Yellow) } },
                new HashSet<string>())
        };

        // Act
        _renderer.ApplyPatches(patches);

        // Assert - Should mark dirty and re-render with new style
        _mockRenderingSystem.Verify(r => r.FillRect(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), ' ', Style.Default), Times.AtLeastOnce);
        _mockRenderingSystem.Verify(r => r.WriteText(10, 5, "Button", It.Is<Style>(s => s.Foreground == Color.Yellow)), Times.Once);
    }

    [Fact]
    public void ApplyPatches_UpdateTextInNestedElement_FindsCorrectParent()
    {
        // Arrange
        var oldTree = Element("container")
            .WithProp("x", 0)
            .WithProp("y", 0)
            .WithChildren(
                Element("text")
                    .WithProp("x", 10)
                    .WithProp("y", 5)
                    .WithChild(Text("Old"))
            )
            .Build();

        _renderer.Render(oldTree);
        _mockRenderingSystem.Invocations.Clear();

        // Patch to update nested text
        var patches = new List<Patch>
        {
            new UpdateTextPatch(new[] { 0, 0, 0 }, "New")
        };

        // Act
        _renderer.ApplyPatches(patches);

        // Assert - Should find parent and re-render
        _mockRenderingSystem.Verify(r => r.WriteText(10, 5, "New", It.IsAny<Style>()), Times.Once);
    }

    [Fact]
    public void TextElement_WithoutExplicitDimensions_ComputesSizeFromContent()
    {
        // This test verifies the fix for 0x0 dirty regions
        // Arrange
        var tree = Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            // Note: No width/height props
            .WithChild(Text("Hello World"))
            .Build();

        _renderer.Render(tree);
        _mockRenderingSystem.Invocations.Clear();

        // Create patch to update the text
        var patches = new List<Patch>
        {
            new UpdateTextPatch(new[] { 0, 0 }, "Updated Text")
        };

        // Act
        _renderer.ApplyPatches(patches);

        // Assert - Should successfully mark dirty and re-render
        // even though the text element has no explicit width/height
        _mockRenderingSystem.Verify(r => r.FillRect(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), ' ', Style.Default), Times.AtLeastOnce);
        _mockRenderingSystem.Verify(r => r.WriteText(10, 5, "Updated Text", It.IsAny<Style>()), Times.Once);
    }
}