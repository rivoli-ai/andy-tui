using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class StyleTests
{
    [Fact]
    public void Default_HasNoFormatting()
    {
        var style = Style.Default;
        
        Assert.Equal(Color.None, style.Foreground);
        Assert.Equal(Color.None, style.Background);
        Assert.False(style.Bold);
        Assert.False(style.Italic);
        Assert.False(style.Underline);
        Assert.False(style.Strikethrough);
        Assert.False(style.Dim);
        Assert.False(style.Inverse);
        Assert.False(style.Blink);
    }
    
    [Fact]
    public void WithForeground_CreatesStyleWithForegroundColor()
    {
        var style = Style.WithForeground(Color.Red);
        
        Assert.Equal(Color.Red, style.Foreground);
        Assert.Equal(Color.None, style.Background);
        Assert.False(style.Bold);
    }
    
    [Fact]
    public void WithBackground_CreatesStyleWithBackgroundColor()
    {
        var style = Style.WithBackground(Color.Blue);
        
        Assert.Equal(Color.None, style.Foreground);
        Assert.Equal(Color.Blue, style.Background);
        Assert.False(style.Bold);
    }
    
    [Fact]
    public void WithBold_CreatesStyleWithBold()
    {
        var style = Style.WithBold();
        
        Assert.True(style.Bold);
        Assert.False(style.Italic);
    }
    
    [Fact]
    public void WithItalic_CreatesStyleWithItalic()
    {
        var style = Style.WithItalic();
        
        Assert.False(style.Bold);
        Assert.True(style.Italic);
    }
    
    [Fact]
    public void WithUnderline_CreatesStyleWithUnderline()
    {
        var style = Style.WithUnderline();
        
        Assert.True(style.Underline);
    }
    
    [Fact]
    public void Merge_CombinesStyles()
    {
        var style1 = new Style
        {
            Foreground = Color.Red,
            Bold = true
        };
        
        var style2 = new Style
        {
            Background = Color.Blue,
            Italic = true
        };
        
        var merged = style1.Merge(style2);
        
        // style2 takes precedence where both have values
        Assert.Equal(Color.Red, merged.Foreground); // style2 has None, so style1's value is kept
        Assert.Equal(Color.Blue, merged.Background);
        Assert.True(merged.Bold);
        Assert.True(merged.Italic);
    }
    
    [Fact]
    public void Merge_OverridesColors()
    {
        var style1 = new Style { Foreground = Color.Red, Background = Color.Green };
        var style2 = new Style { Foreground = Color.Blue, Background = Color.Yellow };
        
        var merged = style1.Merge(style2);
        
        Assert.Equal(Color.Blue, merged.Foreground);
        Assert.Equal(Color.Yellow, merged.Background);
    }
    
    [Fact]
    public void FluentMethods_WorkCorrectly()
    {
        var style = Style.Default
            .WithForegroundColor(Color.Red)
            .WithBackgroundColor(Color.Blue)
            .WithBold()
            .WithItalic()
            .WithUnderline()
            .WithStrikethrough()
            .WithDim()
            .WithInverse()
            .WithBlink();
            
        Assert.Equal(Color.Red, style.Foreground);
        Assert.Equal(Color.Blue, style.Background);
        Assert.True(style.Bold);
        Assert.True(style.Italic);
        Assert.True(style.Underline);
        Assert.True(style.Strikethrough);
        Assert.True(style.Dim);
        Assert.True(style.Inverse);
        Assert.True(style.Blink);
    }
    
    [Fact]
    public void FluentMethods_CanDisableAttributes()
    {
        var style = Style.WithBold()
            .WithItalic()
            .WithBold(false);
            
        Assert.False(style.Bold);
        Assert.True(style.Italic);
    }
    
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var style1 = new Style
        {
            Foreground = Color.Red,
            Background = Color.Blue,
            Bold = true,
            Italic = true
        };
        
        var style2 = new Style
        {
            Foreground = Color.Red,
            Background = Color.Blue,
            Bold = true,
            Italic = true
        };
        
        Assert.True(style1.Equals(style2));
        Assert.True(style1 == style2);
        Assert.False(style1 != style2);
    }
    
    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var style1 = new Style { Bold = true };
        var style2 = new Style { Bold = false };
        
        Assert.False(style1.Equals(style2));
        Assert.False(style1 == style2);
        Assert.True(style1 != style2);
    }
    
    [Fact]
    public void Equals_WithBoxedValue_Works()
    {
        var style1 = new Style { Bold = true };
        object style2 = new Style { Bold = true };
        object style3 = new Style { Italic = true };
        
        Assert.True(style1.Equals(style2));
        Assert.False(style1.Equals(style3));
        Assert.False(style1.Equals(null));
        Assert.False(style1.Equals("not a style"));
    }
    
    [Fact]
    public void GetHashCode_SameValues_ProduceSameHash()
    {
        var style1 = new Style { Foreground = Color.Red, Bold = true };
        var style2 = new Style { Foreground = Color.Red, Bold = true };
        
        Assert.Equal(style1.GetHashCode(), style2.GetHashCode());
    }
    
    [Fact]
    public void ToString_Empty_ReturnsEmptyStyle()
    {
        var style = Style.Default;
        
        Assert.Equal("Style()", style.ToString());
    }
    
    [Fact]
    public void ToString_WithAttributes_ReturnsFormattedString()
    {
        var style = new Style
        {
            Foreground = Color.Red,
            Background = Color.Blue,
            Bold = true,
            Italic = true,
            Underline = true
        };
        
        var str = style.ToString();
        
        Assert.Contains("fg=Red", str);
        Assert.Contains("bg=Blue", str);
        Assert.Contains("bold", str);
        Assert.Contains("italic", str);
        Assert.Contains("underline", str);
    }
    
    [Fact]
    public void ToString_AllAttributes_IncludesAll()
    {
        var style = new Style
        {
            Foreground = Color.White,
            Background = Color.Black,
            Bold = true,
            Italic = true,
            Underline = true,
            Strikethrough = true,
            Dim = true,
            Inverse = true,
            Blink = true
        };
        
        var str = style.ToString();
        
        Assert.Contains("fg=White", str);
        Assert.Contains("bg=Black", str);
        Assert.Contains("bold", str);
        Assert.Contains("italic", str);
        Assert.Contains("underline", str);
        Assert.Contains("strikethrough", str);
        Assert.Contains("dim", str);
        Assert.Contains("inverse", str);
        Assert.Contains("blink", str);
    }
}