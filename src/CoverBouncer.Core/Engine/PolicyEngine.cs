namespace CoverBouncer.Core.Engine;

using CoverBouncer.Core.Models;

/// <summary>
/// Core validation engine for coverage policy enforcement.
/// </summary>
public sealed class PolicyEngine
{
    /// <summary>
    /// Validates coverage report against policy configuration.
    /// </summary>
    /// <param name="config">The policy configuration.</param>
    /// <param name="report">The coverage report to validate.</param>
    /// <returns>Validation result with any violations found.</returns>
    public ValidationResult Validate(PolicyConfiguration config, CoverageReport report)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(report);

        var violations = new List<CoverageViolation>();
        var totalFiles = report.Files.Count;

        foreach (var (filePath, coverage) in report.Files)
        {
            // Determine profile for this file
            var profileName = coverage.AssignedProfile ?? config.DefaultProfile;
            
            if (!config.Profiles.TryGetValue(profileName, out var thresholds))
            {
                throw new InvalidOperationException(
                    $"File '{filePath}' references profile '{profileName}' which does not exist in configuration");
            }

            // Check line coverage
            if (coverage.LineRate < thresholds.MinLine)
            {
                violations.Add(new CoverageViolation
                {
                    FilePath = filePath,
                    ProfileName = profileName,
                    ViolationType = ViolationType.LineCoverageTooLow,
                    RequiredCoverage = thresholds.MinLine,
                    ActualCoverage = coverage.LineRate
                });
            }
        }

        return new ValidationResult
        {
            Violations = violations,
            TotalFilesChecked = totalFiles
        };
    }
}
