using Xunit;
using Andy.TUI;

namespace Andy.TUI.Tests;

public class CalculatorTests
{
    private readonly Calculator _calculator = new();

    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        var result = _calculator.Add(5, 3);
        Assert.Equal(8, result);
    }

    [Fact]
    public void Subtract_ShouldReturnCorrectDifference()
    {
        var result = _calculator.Subtract(10, 4);
        Assert.Equal(6, result);
    }

    [Fact]
    public void Multiply_ShouldReturnCorrectProduct()
    {
        var result = _calculator.Multiply(6, 7);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Divide_ShouldReturnCorrectQuotient()
    {
        var result = _calculator.Divide(10, 2);
        Assert.Equal(5.0, result);
    }

    [Fact]
    public void Divide_ByZero_ShouldThrowDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() => _calculator.Divide(10, 0));
    }
}