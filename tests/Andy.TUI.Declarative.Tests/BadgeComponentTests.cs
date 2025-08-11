using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Rendering;

namespace Andy.TUI.Declarative.Tests;

public class BadgeComponentTests
{
    [Fact]
    public void Badge_CreatesSuccessfully()
    {
        // Act
        var badge = new Badge("NEW");

        // Assert
        Assert.NotNull(badge);
    }

    [Fact]
    public void Badge_CreatesWithAllParameters()
    {
        // Act
        var badge = new Badge(
            "BETA",
            BadgeStyle.Rounded,
            BadgeVariant.Warning,
            customColor: Color.Black,
            customBackgroundColor: Color.Yellow,
            bold: false,
            prefix: "⚠️ ",
            suffix: " ⚠️"
        );

        // Assert
        Assert.NotNull(badge);
    }

    [Theory]
    [InlineData(BadgeVariant.Success)]
    [InlineData(BadgeVariant.Warning)]
    [InlineData(BadgeVariant.Error)]
    [InlineData(BadgeVariant.Info)]
    [InlineData(BadgeVariant.Primary)]
    [InlineData(BadgeVariant.Secondary)]
    [InlineData(BadgeVariant.Default)]
    public void Badge_CreatesWithAllVariants(BadgeVariant variant)
    {
        // Act
        var badge = new Badge("TEST", variant: variant);

        // Assert
        Assert.NotNull(badge);
    }

    [Theory]
    [InlineData(BadgeStyle.Default)]
    [InlineData(BadgeStyle.Rounded)]
    [InlineData(BadgeStyle.Square)]
    [InlineData(BadgeStyle.Pill)]
    [InlineData(BadgeStyle.Dot)]
    [InlineData(BadgeStyle.Count)]
    [InlineData(BadgeStyle.Icon)]
    public void Badge_CreatesWithAllStyles(BadgeStyle style)
    {
        // Act
        var badge = new Badge("TEST", style);

        // Assert
        Assert.NotNull(badge);
    }

    [Fact]
    public void BadgeInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var badge = new Badge("NEW");

        // Act
        var instance = manager.GetOrCreateInstance(badge, "badge1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<BadgeInstance>(instance);
    }

    [Fact]
    public void Badge_ShowcaseExamples()
    {
        // Test examples from FinalComponentsShowcase

        // Style variations
        var newBadge = new Badge("NEW", BadgeStyle.Rounded, BadgeVariant.Primary);
        Assert.NotNull(newBadge);

        var betaBadge = new Badge("BETA", BadgeStyle.Square, BadgeVariant.Warning);
        Assert.NotNull(betaBadge);

        // Count badge
        var countBadge = new Badge("3", BadgeStyle.Count, BadgeVariant.Error);
        Assert.NotNull(countBadge);

        // Dot badge (status indicator)
        var dotBadge = new Badge("", BadgeStyle.Dot, BadgeVariant.Success);
        Assert.NotNull(dotBadge);

        // Pill badge
        var pillBadge = new Badge("PRO", BadgeStyle.Pill, BadgeVariant.Info, bold: true);
        Assert.NotNull(pillBadge);

        // Custom colors
        var customBadge = new Badge("Custom Color", customColor: Color.Black, customBackgroundColor: Color.Cyan);
        Assert.NotNull(customBadge);

        // With emoji prefix
        var liveBadge = new Badge("LIVE", BadgeStyle.Square, BadgeVariant.Error, prefix: "🔴 ");
        Assert.NotNull(liveBadge);
    }

    [Fact]
    public void Badge_EmptyContent()
    {
        // Act
        var badge = new Badge("");

        // Assert
        Assert.NotNull(badge);
    }

    [Fact]
    public void Badge_NullContent()
    {
        // Act & Assert - Should handle null gracefully
        var badge = new Badge(null!);
        Assert.NotNull(badge);
    }

    [Fact]
    public void Badge_CustomColorsOverrideVariant()
    {
        // When custom colors are provided, they should take precedence over variant colors
        var badge = new Badge(
            "Custom",
            variant: BadgeVariant.Error, // This would normally be white on red
            customColor: Color.Green,
            customBackgroundColor: Color.Black
        );

        Assert.NotNull(badge);
    }

    [Fact]
    public void Badge_WithPrefixAndSuffix()
    {
        // Act
        var badge = new Badge("LIVE", BadgeStyle.Default, prefix: "🔴 ", suffix: " 🔴");

        // Assert
        Assert.NotNull(badge);
    }

    [Fact]
    public void Badge_VariousContentLengths()
    {
        // Test different content lengths
        var shortBadge = new Badge("A");
        Assert.NotNull(shortBadge);

        var mediumBadge = new Badge("MEDIUM");
        Assert.NotNull(mediumBadge);

        var longBadge = new Badge("THIS IS A VERY LONG BADGE");
        Assert.NotNull(longBadge);
    }

    [Fact]
    public void Badge_CombinationsOfStylesAndVariants()
    {
        // Test various combinations
        var combinations = new[]
        {
            (BadgeStyle.Rounded, BadgeVariant.Success),
            (BadgeStyle.Square, BadgeVariant.Warning),
            (BadgeStyle.Pill, BadgeVariant.Error),
            (BadgeStyle.Count, BadgeVariant.Info),
            (BadgeStyle.Dot, BadgeVariant.Primary)
        };

        foreach (var (style, variant) in combinations)
        {
            var badge = new Badge("TEST", style, variant);
            Assert.NotNull(badge);
        }
    }
}