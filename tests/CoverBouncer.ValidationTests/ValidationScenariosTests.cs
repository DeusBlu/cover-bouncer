using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;
using CoverBouncer.Core.Models;
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

    // ──────────────────────────────────────────────
    // Filtered Run Scenarios (8 & 9)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Loads a scenario and runs the full pipeline (parse, tag, validate).
    /// </summary>
    private (CoverBouncer.Core.Models.ValidationResult result, CoverBouncer.Core.Models.CoverageReport report, PolicyConfiguration config)
        RunScenario(string scenarioName, bool isFilteredTestRun)
    {
        var scenarioPath = Path.Combine(_testProjectsPath, scenarioName);
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        Assert.True(Directory.Exists(scenarioPath), $"Scenario directory not found: {scenarioPath}");
        Assert.True(File.Exists(configPath), $"Config file not found: {configPath}");
        Assert.True(File.Exists(coveragePath), $"Coverage file not found: {coveragePath}");

        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
        }

        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport, isFilteredTestRun);

        return (result, coverageReport, config);
    }

    [Fact]
    public void Scenario8_FilteredRun_SkipsUntargetedFiles_Passes()
    {
        // Scenario: Developer runs `dotnet test --filter "Category=OrderTests"`
        // Only PaymentService and OrderService are targeted.
        // AuthenticationService, InventoryService, Logger, UserDto all show 0% 
        // because Coverlet instrumentd them but no filtered tests touched them.
        var (result, report, config) = RunScenario("8-FilteredRun", isFilteredTestRun: true);

        // With filtered-run awareness, untargeted 0% files are skipped
        Assert.True(result.Success, 
            $"Filtered run should pass — untargeted files should be skipped. " +
            $"Got {result.Violations.Count} violation(s): " +
            string.Join(", ", result.Violations.Select(v => $"{Path.GetFileName(v.FilePath)} ({v.ActualCoverage:P0}<{v.RequiredCoverage:P0})")));
        
        Assert.Empty(result.Violations);
        Assert.Equal(2, result.TotalFilesChecked);  // PaymentService + OrderService
        Assert.Equal(4, result.SkippedFiles);         // Auth + Inventory + Logger + UserDto
        
        // Verify the report has all 6 files (Coverlet instruments everything)
        Assert.Equal(6, report.Files.Count);
    }

    [Fact]
    public void Scenario8_FilteredRun_TargetedFilesStillValidated()
    {
        // Even on a filtered run, files WITH coverage are validated normally
        var (result, report, _) = RunScenario("8-FilteredRun", isFilteredTestRun: true);

        // The 2 targeted files should have been validated (and they pass)
        var checkedFiles = report.Files
            .Where(f => f.Value.CoveredLines > 0)
            .Select(f => Path.GetFileName(f.Key))
            .OrderBy(f => f)
            .ToList();

        Assert.Equal(2, checkedFiles.Count);
        Assert.Contains("OrderService.cs", checkedFiles);
        Assert.Contains("PaymentService.cs", checkedFiles);
    }

    [Fact]
    public void Scenario9_FullRunSameData_FailsOnUntargetedFiles()
    {
        // Same coverage data as Scenario 8, but validated as a FULL run.
        // Now the 0% coverage files are NOT skipped — they are validated and fail.
        var (result, report, _) = RunScenario("9-FullRunSameData", isFilteredTestRun: false);

        Assert.False(result.Success, 
            "Full run with 0% coverage files should fail");
        
        // 3 violations: AuthenticationService (0%<100%), InventoryService (0%<80%), Logger (0%<60%)
        // UserDto passes because Dto profile allows 0%
        Assert.Equal(3, result.Violations.Count);
        Assert.Equal(6, result.TotalFilesChecked);  // All 6 files validated
        Assert.Equal(0, result.SkippedFiles);        // None skipped on full run

        // Verify which files failed
        var failedFiles = result.Violations
            .Select(v => Path.GetFileName(v.FilePath))
            .OrderBy(f => f)
            .ToList();

        Assert.Equal(new[] { "AuthenticationService.cs", "InventoryService.cs", "Logger.cs" }, failedFiles);
    }

    [Fact]
    public void Scenario9_FullRun_DtoFileWithZeroCoverage_StillPasses()
    {
        // On a full run, UserDto has 0% coverage but Dto profile allows 0% — it passes
        var (result, report, _) = RunScenario("9-FullRunSameData", isFilteredTestRun: false);

        var dtoViolation = result.Violations
            .FirstOrDefault(v => v.FilePath.Contains("UserDto"));

        Assert.Null(dtoViolation); // No violation for UserDto — 0% meets 0% threshold
    }

    [Fact]
    public void Scenario8vs9_SameData_DifferentOutcomes()
    {
        // THE KEY TEST: Same coverage data, different filter flag = different (correct) results
        var (filteredResult, _, _) = RunScenario("8-FilteredRun", isFilteredTestRun: true);
        var (fullResult, _, _) = RunScenario("9-FullRunSameData", isFilteredTestRun: false);

        // Filtered run passes, full run fails
        Assert.True(filteredResult.Success, "Filtered run should pass");
        Assert.False(fullResult.Success, "Full run should fail");

        // Filtered skips 4, full skips 0
        Assert.Equal(4, filteredResult.SkippedFiles);
        Assert.Equal(0, fullResult.SkippedFiles);

        // Same total files in both reports
        Assert.Equal(2, filteredResult.TotalFilesChecked);
        Assert.Equal(6, fullResult.TotalFilesChecked);

        // Full run found the violations the filtered run correctly skipped
        Assert.Empty(filteredResult.Violations);
        Assert.Equal(3, fullResult.Violations.Count);
    }
}
