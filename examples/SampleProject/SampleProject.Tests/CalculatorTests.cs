using SampleProject;

namespace SampleProject.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldReturnSum()
    {
        var calc = new Calculator();
        Assert.Equal(5, calc.Add(2, 3));
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        var calc = new Calculator();
        Assert.Equal(1, calc.Subtract(3, 2));
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
    {
        var calc = new Calculator();
        Assert.Equal(6, calc.Multiply(2, 3));
    }

    // Divide is NOT tested - brings us to ~64% coverage (passes 60% Standard profile)
}
