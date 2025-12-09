using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;
using CoverBouncer.Coverlet;
using System.Text.Json;
using Xunit;

namespace CoverBouncer.ValidationTests;

/// <summary>
/// End-to-end validation tests for CoverBouncer with realistic scenarios.
/// Tests expected pass/fail cases to ensure policy enforcement works correctly.
/// </summary>
public class ValidationScenariosTests
{
    private readonly string _testProjectsPath;

    public ValidationScenariosTests()
    {
        // Get the path to TestProjects folder
        var assemblyLocation = Path.GetDirectoryName(typeof(ValidationScenariosTests).Assembly.Location)!;
        _testProjectsPath = Path.Combine(assemblyLocation, "TestProjects");
    }

    [Theory]
    [InlineData("1-AllPass", true, 0)]
    [InlineData("2-MixedResults", false, 3)]  // 3 violations: AuthService (85%<100%), EmailSender (50%<60%), Logger (25%<60%)
    [InlineData("3-CriticalViolation", false, 1)]
    [InlineData("4-UntaggedFiles", false, 1)]
    public void ValidateScenario_ReturnsExpectedResult(
        string scenarioName, 
        bool shouldPass, 
        int expectedViolations)
    {
        // Arrange
        var scenarioPath = Path.Combine(_testProjectsPath, scenarioName);
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        Assert.True(Directory.Exists(scenarioPath), 
            $"Scenario directory not found: {scenarioPath}");
        Assert.True(File.Exists(configPath), 
            $"Config file not found: {configPath}");
        Assert.True(File.Exists(coveragePath), 
            $"Coverage file not found: {coveragePath}");

        // Act
        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        // Read profile tags from source files
        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
        }

        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport);

        // Assert
        Assert.Equal(shouldPass, result.Success);
        Assert.Equal(expectedViolations, result.Violations.Count);

        // Additional validations
        if (!shouldPass)
        {
            Assert.True(result.Violations.Count > 0, "Failed result should have violations");
            foreach (var violation in result.Violations)
            {
                Assert.NotNull(violation.FilePath);
                Assert.NotNull(violation.ProfileName);
                Assert.True(violation.ActualCoverage < violation.RequiredCoverage,
                    $"Violation should have actual < required: {violation.FilePath}");
            }
        }
    }

    [Fact]
    public void Scenario5_MultipleProfiles_ValidatesCorrectly()
    {
        // Arrange
        var scenarioPath = Path.Combine(_testProjectsPath, "5-MultipleProfiles");
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        // Act
        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
        }

        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport);

        // Assert - 4 violations: Logic1, Logic2 (BusinessLogic 50%<80%), Standard1, Standard2 (Standard 50%<60%)
        Assert.False(result.Success);
        Assert.Equal(4, result.Violations.Count);
        
        Assert.True(result.TotalFilesChecked >= 8, "Should check multiple files");
        
        // Verify each profile group
        var profileGroups = result.Violations.GroupBy(v => v.ProfileName).ToList();
        Assert.True(profileGroups.Count >= 2, "Should have violations in multiple profiles");
        
        foreach (var group in profileGroups)
        {
            Assert.NotEmpty(group.Key);
            Assert.All(group, v => Assert.Equal(group.Key, v.ProfileName));
        }
    }

    [Fact]
    public void Scenario6_EdgeCases_HandlesCorrectly()
    {
        // Arrange
        var scenarioPath = Path.Combine(_testProjectsPath, "6-EdgeCases");
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        // Act
        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
        }

        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport);

        // Assert - All edge cases pass: ExactlyAtThreshold (80%=80%), JustBelowThreshold (83%>80%), ZeroCoverageDto (0% allowed), PerfectCritical (100%=100%)
        Assert.True(result.Success, "All edge case files should pass their thresholds");
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void Scenario7_RealWorld_SimulatesProductionUsage()
    {
        // Arrange
        var scenarioPath = Path.Combine(_testProjectsPath, "7-RealWorld");
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        // Act
        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
        }

        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport);

        // Assert - Real-world application structure
        // 4 violations: OrderService, InventoryService (BusinessLogic 50%<80%), Logger, EmailService (Standard 50%<60%)
        Assert.False(result.Success);
        Assert.Equal(4, result.Violations.Count);
        Assert.True(result.TotalFilesChecked >= 8, 
            "Real-world scenario should have multiple files");

        // Verify we have files from different layers
        var filesByProfile = coverageReport.Files
            .GroupBy(f => f.Value.AssignedProfile ?? config.DefaultProfile)
            .ToDictionary(g => g.Key, g => g.Count());

        Assert.True(filesByProfile.Count >= 3, 
            "Should have multiple profiles in use");
    }
}
