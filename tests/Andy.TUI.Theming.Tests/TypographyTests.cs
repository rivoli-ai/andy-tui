using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class TypographyTests
{
    [Fact]
    public void Typography_DefaultConstructor_AllPropertiesFalse()
    {
        var typography = new Typography();

        Assert.False(typography.Bold);
        Assert.False(typography.Italic);
        Assert.False(typography.Underline);
        Assert.False(typography.Strikethrough);
        Assert.False(typography.Dim);
        Assert.False(typography.Blink);
    }

    [Fact]
    public void Typography_ConstructorWithParameters_SetsProperties()
    {
        var typography = new Typography(
            Bold: true,
            Italic: true,
            Underline: false,
            Strikethrough: true,
            Dim: false,
            Blink: true);

        Assert.True(typography.Bold);
        Assert.True(typography.Italic);
        Assert.False(typography.Underline);
        Assert.True(typography.Strikethrough);
        Assert.False(typography.Dim);
        Assert.True(typography.Blink);
    }

    [Fact]
    public void Typography_PartialConstructor_SetsOnlySpecifiedProperties()
    {
        var boldOnly = new Typography(Bold: true);

        Assert.True(boldOnly.Bold);
        Assert.False(boldOnly.Italic);
        Assert.False(boldOnly.Underline);

        var italicUnderline = new Typography(Italic: true, Underline: true);

        Assert.False(italicUnderline.Bold);
        Assert.True(italicUnderline.Italic);
        Assert.True(italicUnderline.Underline);
    }

    [Fact]
    public void Typography_RecordEquality()
    {
        var typo1 = new Typography(Bold: true, Italic: true);
        var typo2 = new Typography(Bold: true, Italic: true);
        var typo3 = new Typography(Bold: true, Italic: false);

        Assert.Equal(typo1, typo2);
        Assert.NotEqual(typo1, typo3);
    }

    [Fact]
    public void Typography_WithModification()
    {
        var original = new Typography(Bold: true);
        var modified = original with { Italic = true };

        Assert.True(original.Bold);
        Assert.False(original.Italic);

        Assert.True(modified.Bold);
        Assert.True(modified.Italic);
    }
}