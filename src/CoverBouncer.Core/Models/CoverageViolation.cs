namespace CoverBouncer.Core.Models;

/// <summary>
/// Represents a coverage policy violation for a single file.
/// </summary>
public sealed class CoverageViolation
{
    /// <summary>
    /// Path to the file that violated the policy.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Profile that was violated.
    /// </summary>
    public required string ProfileName { get; init; }

    /// <summary>
    /// Type of violation (line or branch coverage).
    /// </summary>
    public required ViolationType ViolationType { get; init; }

    /// <summary>
    /// Required coverage threshold (0.0 to 1.0).
    /// </summary>
    public required decimal RequiredCoverage { get; init; }

    /// <summary>
    /// Actual coverage achieved (0.0 to 1.0).
    /// </summary>
    public required decimal ActualCoverage { get; init; }

    /// <summary>
    /// Total number of coverable items (lines or branches).
    /// </summary>
    public required int TotalItems { get; init; }

    /// <summary>
    /// Number of covered items (lines or branches).
    /// </summary>
    public required int CoveredItems { get; init; }

    /// <summary>
    /// Number of items missing coverage.
    /// </summary>
    public int MissingItems => TotalItems - CoveredItems;

    /// <summary>
    /// Gets a formatted message describing the violation.
    /// </summary>
    public string GetMessage()
    {
        var coverageType = ViolationType == ViolationType.LineCoverageTooLow ? "line" : "branch";
        var actualPercent = (ActualCoverage * 100).ToString("F1");
        var requiredPercent = (RequiredCoverage * 100).ToString("F1");
        
        return $"{FilePath} ({ProfileName}): " +
               $"{coverageType} coverage is {actualPercent}%, required {requiredPercent}% " +
               $"({CoveredItems}/{TotalItems} {coverageType}s covered, {MissingItems} missing)";
    }

    /// <summary>
    /// Gets a short summary of the violation.
    /// </summary>
    public string GetShortMessage()
    {
        var coverageType = ViolationType == ViolationType.LineCoverageTooLow ? "line" : "branch";
        var actualPercent = (ActualCoverage * 100).ToString("F1");
        var requiredPercent = (RequiredCoverage * 100).ToString("F1");
        
        return $"{FilePath}: {coverageType} {actualPercent}% < {requiredPercent}%";
    }
}
