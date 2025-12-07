using CoverBouncer.Core.Models;

namespace CoverBouncer.Core.Tests.Models;

public class PolicyConfigurationTests
{
    [Fact]
    public void Validate_WithValidConfiguration_DoesNotThrow()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m }
            }
        };

        var exception = Record.Exception(() => config.Validate());

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithMissingDefaultProfile_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m }
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("defaultProfile", exception.Message);
    }

    [Fact]
    public void Validate_WithEmptyProfiles_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>()
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("profiles", exception.Message);
        Assert.Contains("at least one profile", exception.Message);
    }

    [Fact]
    public void Validate_WithNonExistentDefaultProfile_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "NonExistent",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m }
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("NonExistent", exception.Message);
        Assert.Contains("does not exist in profiles", exception.Message);
        Assert.Contains("Standard", exception.Message);
    }

    [Fact]
    public void Validate_WithInvalidProfileThresholds_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 1.5m } // Invalid
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("Standard", exception.Message);
        Assert.Contains("MinLine", exception.Message);
    }

    [Fact]
    public void Validate_WithNullThresholds_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = null!
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("Standard", exception.Message);
        Assert.Contains("null thresholds", exception.Message);
    }

    [Fact]
    public void Validate_WithEmptyCoverageReportPath_ThrowsArgumentException()
    {
        var config = new PolicyConfiguration
        {
            CoverageReportPath = "",
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m }
            }
        };

        var exception = Assert.Throws<ArgumentException>(() => config.Validate());

        Assert.Contains("coverageReportPath", exception.Message);
    }

    [Fact]
    public void Validate_WithMultipleProfiles_ValidatesAll()
    {
        var config = new PolicyConfiguration
        {
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m },
                ["Critical"] = new() { MinLine = 1.00m, MinBranch = 1.00m },
                ["Dto"] = new() { MinLine = 0.00m }
            }
        };

        var exception = Record.Exception(() => config.Validate());

        Assert.Null(exception);
    }
}
