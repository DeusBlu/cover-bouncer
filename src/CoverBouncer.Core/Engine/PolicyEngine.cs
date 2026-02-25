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
    /// <param name="isFilteredTestRun">
    /// When true, files with zero covered lines are skipped (assumed not targeted by filtered tests).
    /// When false (full run), all files in the report are validated including those with 0% coverage.
    /// </param>
    /// <returns>Validation result with any violations found.</returns>
    public ValidationResult Validate(PolicyConfiguration config, CoverageReport report, bool isFilteredTestRun = false)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(report);

        var violations = new List<CoverageViolation>();
        var skippedFiles = 0;

        foreach (var (filePath, coverage) in report.Files)
        {
            // In filtered test runs, skip files that were instrumented but never executed.
            // These files appear in the Coverlet report because they are part of the
            // instrumented assembly, but no tests in the filtered set targeted them.
            // They would show 0% coverage regardless, producing false failures.
            if (isFilteredTestRun && coverage.CoveredLines == 0)
            {
                skippedFiles++;
                continue;
            }

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

        var totalFilesChecked = report.Files.Count - skippedFiles;

        return new ValidationResult
        {
            Violations = violations,
            TotalFilesChecked = totalFilesChecked,
            SkippedFiles = skippedFiles
        };
    }
}
