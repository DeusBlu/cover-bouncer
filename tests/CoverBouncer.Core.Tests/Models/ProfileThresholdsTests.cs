using CoverBouncer.Core.Models;

namespace CoverBouncer.Core.Tests.Models;

public class ProfileThresholdsTests
{
    [Fact]
    public void Validate_WithValidLineThreshold_DoesNotThrow()
    {
        var thresholds = new ProfileThresholds { MinLine = 0.70m };
        
        var exception = Record.Exception(() => thresholds.Validate("TestProfile"));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithValidBranchThreshold_DoesNotThrow()
    {
        var thresholds = new ProfileThresholds { MinBranch = 0.80m };
        
        var exception = Record.Exception(() => thresholds.Validate("TestProfile"));
        
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithBothThresholds_DoesNotThrow()
    {
        var thresholds = new ProfileThresholds 
        { 
            MinLine = 0.70m,
            MinBranch = 0.60m 
        };
        
        var exception = Record.Exception(() => thresholds.Validate("TestProfile"));
        
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void Validate_WithInvalidLineThreshold_ThrowsArgumentException(decimal invalidValue)
    {
        var thresholds = new ProfileThresholds { MinLine = invalidValue };
        
        var exception = Assert.Throws<ArgumentException>(() => thresholds.Validate("TestProfile"));
        
        Assert.Contains("MinLine must be between 0.0 and 1.0", exception.Message);
        Assert.Contains("TestProfile", exception.Message);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Validate_WithInvalidBranchThreshold_ThrowsArgumentException(decimal invalidValue)
    {
        var thresholds = new ProfileThresholds { MinBranch = invalidValue };
        
        var exception = Assert.Throws<ArgumentException>(() => thresholds.Validate("TestProfile"));
        
        Assert.Contains("MinBranch must be between 0.0 and 1.0", exception.Message);
        Assert.Contains("TestProfile", exception.Message);
    }

    [Fact]
    public void Validate_WithNoThresholds_ThrowsArgumentException()
    {
        var thresholds = new ProfileThresholds();
        
        var exception = Assert.Throws<ArgumentException>(() => thresholds.Validate("TestProfile"));
        
        Assert.Contains("At least one threshold", exception.Message);
        Assert.Contains("TestProfile", exception.Message);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Validate_WithBoundaryValues_DoesNotThrow(decimal value)
    {
        var thresholds = new ProfileThresholds { MinLine = value };
        
        var exception = Record.Exception(() => thresholds.Validate("TestProfile"));
        
        Assert.Null(exception);
    }
}
