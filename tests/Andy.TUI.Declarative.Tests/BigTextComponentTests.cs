using System;
using System.Collections.Generic;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class BigTextComponentTests
{
    [Fact]
    public void BigText_CreatesSuccessfully()
    {
        // Act
        var bigText = new BigText("HELLO");

        // Assert
        Assert.NotNull(bigText);
    }

    [Fact]
    public void BigText_CreatesWithAllParameters()
    {
        // Act
        var bigText = new BigText(
            "Test",
            BigTextFont.Slim,
            color: Color.Cyan,
            fillChar: '▓',
            spacing: 2
        );

        // Assert
        Assert.NotNull(bigText);
    }

    [Theory]
    [InlineData(BigTextFont.Block)]
    [InlineData(BigTextFont.Slim)]
    [InlineData(BigTextFont.Mini)]
    public void BigText_SupportsAllFonts(BigTextFont font)
    {
        // Act
        var bigText = new BigText("ABC", font);

        // Assert
        Assert.NotNull(bigText);
    }

    [Theory]
    [InlineData('A')]
    [InlineData('B')]
    [InlineData('Z')]
    [InlineData('0')]
    [InlineData('1')]
    [InlineData('9')]
    [InlineData(' ')]
    [InlineData('!')]
    [InlineData('?')]
    [InlineData('.')]
    [InlineData(',')]
    [InlineData('-')]
    public void GetCharacterPattern_ReturnsPatternForSupportedCharacters(char character)
    {
        // Act
        var pattern = BigText.GetCharacterPattern(character, BigTextFont.Block);

        // Assert
        Assert.NotNull(pattern);
        Assert.NotEmpty(pattern);
        Assert.True(pattern.Length > 0);
    }

    [Fact]
    public void GetCharacterPattern_ReturnsNullForUnsupportedCharacters()
    {
        // Act
        var pattern = BigText.GetCharacterPattern('@', BigTextFont.Block);

        // Assert
        Assert.Null(pattern);
    }

    [Fact]
    public void GetCharacterPattern_FontDimensions()
    {
        // Test Block font
        var blockPattern = BigText.GetCharacterPattern('A', BigTextFont.Block);
        Assert.NotNull(blockPattern);
        Assert.Equal(5, blockPattern.Length); // Block font is 5 lines tall

        // Test Slim font
        var slimPattern = BigText.GetCharacterPattern('A', BigTextFont.Slim);
        Assert.NotNull(slimPattern);
        Assert.Equal(4, slimPattern.Length); // Slim font is 4 lines tall

        // Test Mini font
        var miniPattern = BigText.GetCharacterPattern('A', BigTextFont.Mini);
        Assert.NotNull(miniPattern);
        Assert.Equal(3, miniPattern.Length); // Mini font is 3 lines tall
    }

    [Fact]
    public void BigText_CustomFont()
    {
        // Arrange
        var customFont = new Dictionary<char, string[]>
        {
            ['X'] = new[] { "X X", " X ", "X X" },
            ['O'] = new[] { "OOO", "O O", "OOO" }
        };

        // Act
        var bigText = new BigText("XO", BigTextFont.Custom, customFont);

        // Assert
        Assert.NotNull(bigText);
    }

    [Fact]
    public void BigTextInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var bigText = new BigText("HI");

        // Act
        var instance = manager.GetOrCreateInstance(bigText, "bigtext1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<BigTextInstance>(instance);
    }

    [Fact]
    public void BigText_ShowcaseExamples()
    {
        // Test examples from FinalComponentsShowcase
        
        // Block font title
        var blockText = new BigText("FINAL", BigTextFont.Block, color: Color.Cyan);
        Assert.NotNull(blockText);

        // Slim font subtitle
        var slimText = new BigText("SHOWCASE", BigTextFont.Slim, color: Color.Magenta);
        Assert.NotNull(slimText);

        // Mini font status
        var miniText = new BigText("STATUS", BigTextFont.Mini, color: Color.Green);
        Assert.NotNull(miniText);
    }

    [Fact]
    public void BigText_HandlesEmptyText()
    {
        // Act
        var bigText = new BigText("");

        // Assert
        Assert.NotNull(bigText);
    }

    [Fact]
    public void BigText_SpecialCharacters()
    {
        // Arrange - Test all special characters that have patterns
        var specialChars = "!?.,- ";

        // Act & Assert
        foreach (char c in specialChars)
        {
            var pattern = BigText.GetCharacterPattern(c, BigTextFont.Block);
            Assert.NotNull(pattern);
        }
    }

    [Fact]
    public void BigText_AllFontsHaveConsistentCharacterSets()
    {
        // Test that common characters exist in all fonts
        var commonChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
        var fonts = new[] { BigTextFont.Block, BigTextFont.Slim, BigTextFont.Mini };

        foreach (var font in fonts)
        {
            foreach (char c in commonChars)
            {
                var pattern = BigText.GetCharacterPattern(c, font);
                Assert.NotNull(pattern);
                Assert.NotEmpty(pattern);
            }
        }
    }
}