using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;
using CoverBouncer.Coverlet;
using Xunit;
using Xunit.Abstractions;

namespace CoverBouncer.ValidationTests;

public class DebugCoverageTest
{
    private readonly ITestOutputHelper _output;

    public DebugCoverageTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("5-MultipleProfiles")]
    [InlineData("6-EdgeCases")]
    [InlineData("7-RealWorld")]
    public void Debug_Scenario_Coverage(string scenarioName)
    {
        // Arrange
        var assemblyLocation = Path.GetDirectoryName(typeof(DebugCoverageTest).Assembly.Location)!;
        var scenarioPath = Path.Combine(assemblyLocation, "TestProjects", scenarioName);
        var configPath = Path.Combine(scenarioPath, "coverbouncer.json");
        var coveragePath = Path.Combine(scenarioPath, "coverage.json");

        // Act
        var config = ConfigurationLoader.LoadFromFile(configPath);
        var parser = new CoverletReportParser();
        var coverageReport = parser.ParseFile(coveragePath);

        _output.WriteLine($"=== Scenario: {scenarioName} ===");
        _output.WriteLine($"Total files in report: {coverageReport.Files.Count}");
        _output.WriteLine("");

        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            _output.WriteLine($"File: {Path.GetFileName(filePath)}");
            _output.WriteLine($"  Total Lines: {coverage.TotalLines}");
            _output.WriteLine($"  Covered Lines: {coverage.CoveredLines}");
            _output.WriteLine($"  Line Rate: {coverage.LineRate:P2} ({coverage.LineRate:F4})");
            _output.WriteLine("");
        }

        // Read profile tags
        var tagReader = new FileTagReader();
        foreach (var (filePath, coverage) in coverageReport.Files)
        {
            coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
            _output.WriteLine($"{Path.GetFileName(filePath)} -> Profile: {coverage.AssignedProfile ?? config.DefaultProfile}");
        }
        _output.WriteLine("");

        // Validate
        var engine = new PolicyEngine();
        var result = engine.Validate(config, coverageReport);

        _output.WriteLine($"Validation Success: {result.Success}");
        _output.WriteLine($"Total Violations: {result.Violations.Count}");
        _output.WriteLine($"Files Passed: {result.FilesPassed}");
        _output.WriteLine("");

        foreach (var violation in result.Violations)
        {
            _output.WriteLine($"VIOLATION:");
            _output.WriteLine($"  File: {Path.GetFileName(violation.FilePath)}");
            _output.WriteLine($"  Profile: {violation.ProfileName}");
            _output.WriteLine($"  Required: {violation.RequiredCoverage:P0}");
            _output.WriteLine($"  Actual: {violation.ActualCoverage:P2}");
            _output.WriteLine("");
        }
    }
}
