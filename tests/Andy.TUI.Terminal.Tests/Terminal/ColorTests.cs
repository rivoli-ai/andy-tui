using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class ColorTests
{
    [Fact]
    public void None_ReturnsDefaultColor()
    {
        var color = Color.None;

        Assert.Equal(ColorType.None, color.Type);
        Assert.Null(color.ConsoleColor);
        Assert.Null(color.Rgb);
        Assert.Null(color.ColorIndex);
    }

    [Fact]
    public void ConsoleColorConstructor_SetsCorrectValues()
    {
        var color = new Color(ConsoleColor.Red);

        Assert.Equal(ColorType.ConsoleColor, color.Type);
        Assert.Equal(ConsoleColor.Red, color.ConsoleColor);
        Assert.Null(color.Rgb);
        Assert.Null(color.ColorIndex);
    }

    [Fact]
    public void RgbConstructor_SetsCorrectValues()
    {
        var color = new Color(255, 128, 64);

        Assert.Equal(ColorType.Rgb, color.Type);
        Assert.Null(color.ConsoleColor);
        Assert.True(color.Rgb.HasValue);
        Assert.Equal(255, color.Rgb.Value.R);
        Assert.Equal(128, color.Rgb.Value.G);
        Assert.Equal(64, color.Rgb.Value.B);
        Assert.Null(color.ColorIndex);
    }

    [Fact]
    public void EightBitConstructor_SetsCorrectValues()
    {
        var color = new Color(196);

        Assert.Equal(ColorType.EightBit, color.Type);
        Assert.Null(color.ConsoleColor);
        Assert.Null(color.Rgb);
        Assert.True(color.ColorIndex.HasValue);
        Assert.Equal(196, color.ColorIndex.Value);
    }

    [Fact]
    public void StaticColors_HaveCorrectValues()
    {
        Assert.Equal(ConsoleColor.Black, Color.Black.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkBlue, Color.DarkBlue.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkGreen, Color.DarkGreen.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkCyan, Color.DarkCyan.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkRed, Color.DarkRed.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkMagenta, Color.DarkMagenta.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkYellow, Color.DarkYellow.ConsoleColor);
        Assert.Equal(ConsoleColor.Gray, Color.Gray.ConsoleColor);
        Assert.Equal(ConsoleColor.DarkGray, Color.DarkGray.ConsoleColor);
        Assert.Equal(ConsoleColor.Blue, Color.Blue.ConsoleColor);
        Assert.Equal(ConsoleColor.Green, Color.Green.ConsoleColor);
        Assert.Equal(ConsoleColor.Cyan, Color.Cyan.ConsoleColor);
        Assert.Equal(ConsoleColor.Red, Color.Red.ConsoleColor);
        Assert.Equal(ConsoleColor.Magenta, Color.Magenta.ConsoleColor);
        Assert.Equal(ConsoleColor.Yellow, Color.Yellow.ConsoleColor);
        Assert.Equal(ConsoleColor.White, Color.White.ConsoleColor);
    }

    [Fact]
    public void FromRgb_CreatesCorrectColor()
    {
        var color = Color.FromRgb(100, 150, 200);

        Assert.Equal(ColorType.Rgb, color.Type);
        Assert.True(color.Rgb.HasValue);
        Assert.Equal(100, color.Rgb.Value.R);
        Assert.Equal(150, color.Rgb.Value.G);
        Assert.Equal(200, color.Rgb.Value.B);
    }

    [Fact]
    public void FromHex_WithHash_ParsesCorrectly()
    {
        var color = Color.FromHex("#FF8040");

        Assert.Equal(ColorType.Rgb, color.Type);
        Assert.True(color.Rgb.HasValue);
        Assert.Equal(255, color.Rgb.Value.R);
        Assert.Equal(128, color.Rgb.Value.G);
        Assert.Equal(64, color.Rgb.Value.B);
    }

    [Fact]
    public void FromHex_WithoutHash_ParsesCorrectly()
    {
        var color = Color.FromHex("00FF00");

        Assert.Equal(ColorType.Rgb, color.Type);
        Assert.True(color.Rgb.HasValue);
        Assert.Equal(0, color.Rgb.Value.R);
        Assert.Equal(255, color.Rgb.Value.G);
        Assert.Equal(0, color.Rgb.Value.B);
    }

    [Fact]
    public void FromHex_WithNullString_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Color.FromHex(null!));
    }

    [Fact]
    public void FromHex_WithEmptyString_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Color.FromHex(""));
    }

    [Fact]
    public void FromHex_WithInvalidLength_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Color.FromHex("FF"));
        Assert.Throws<ArgumentException>(() => Color.FromHex("FFFFFFF"));
    }

    [Fact]
    public void FromEightBit_CreatesCorrectColor()
    {
        var color = Color.FromEightBit(42);

        Assert.Equal(ColorType.EightBit, color.Type);
        Assert.True(color.ColorIndex.HasValue);
        Assert.Equal(42, color.ColorIndex.Value);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var color1 = new Color(ConsoleColor.Red);
        var color2 = new Color(ConsoleColor.Red);

        Assert.True(color1.Equals(color2));
        Assert.True(color1 == color2);
        Assert.False(color1 != color2);
    }

    [Fact]
    public void Equals_WithDifferentTypes_ReturnsFalse()
    {
        var color1 = new Color(ConsoleColor.Red);
        var color2 = new Color(255, 0, 0);

        Assert.False(color1.Equals(color2));
        Assert.False(color1 == color2);
        Assert.True(color1 != color2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var color1 = new Color(ConsoleColor.Red);
        var color2 = new Color(ConsoleColor.Blue);

        Assert.False(color1.Equals(color2));
    }

    [Fact]
    public void Equals_WithBoxedValue_Works()
    {
        var color1 = new Color(ConsoleColor.Green);
        object color2 = new Color(ConsoleColor.Green);
        object color3 = new Color(ConsoleColor.Blue);

        Assert.True(color1.Equals(color2));
        Assert.False(color1.Equals(color3));
        Assert.False(color1.Equals(null));
        Assert.False(color1.Equals("not a color"));
    }

    [Fact]
    public void GetHashCode_SameValues_ProduceSameHash()
    {
        var color1 = new Color(100, 150, 200);
        var color2 = new Color(100, 150, 200);

        Assert.Equal(color1.GetHashCode(), color2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ProduceDifferentHash()
    {
        var color1 = Color.None;
        var color2 = new Color(ConsoleColor.Red);
        var color3 = new Color(255, 0, 0);
        var color4 = new Color(196);

        // While hash codes can collide, these should be different
        Assert.NotEqual(color1.GetHashCode(), color2.GetHashCode());
        Assert.NotEqual(color2.GetHashCode(), color3.GetHashCode());
        Assert.NotEqual(color3.GetHashCode(), color4.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        Assert.Equal("None", Color.None.ToString());
        Assert.Equal("Red", Color.Red.ToString());
        Assert.Equal("RGB(100, 150, 200)", Color.FromRgb(100, 150, 200).ToString());
        Assert.Equal("8-bit(42)", Color.FromEightBit(42).ToString());
    }
}