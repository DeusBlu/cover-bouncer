namespace CoverBouncer.Core.Models;

/// <summary>
/// Result of validating coverage against policy.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// List of violations found.
    /// </summary>
    public List<CoverageViolation> Violations { get; init; } = new();

    /// <summary>
    /// Total number of files checked.
    /// </summary>
    public int TotalFilesChecked { get; init; }

    /// <summary>
    /// Number of files skipped (e.g., uninstrumented files during filtered test runs).
    /// </summary>
    public int SkippedFiles { get; init; }

    /// <summary>
    /// Number of files that passed policy requirements.
    /// </summary>
    public int FilesPassed => TotalFilesChecked - FilesFailed;

    /// <summary>
    /// Number of files that failed policy requirements.
    /// </summary>
    public int FilesFailed => Violations.Select(v => v.FilePath).Distinct().Count();

    /// <summary>
    /// Whether all files passed policy requirements.
    /// </summary>
    public bool Success => Violations.Count == 0;

    /// <summary>
    /// When the validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a summary message of the validation result.
    /// </summary>
    public string GetSummary()
    {
        var skippedMsg = SkippedFiles > 0 ? $", {SkippedFiles} skipped (filtered run)" : "";

        if (Success)
        {
            return $"✓ All {TotalFilesChecked} files meet coverage requirements{skippedMsg}";
        }

        var distinctFiles = FilesFailed;
        var totalViolations = Violations.Count;
        
        return $"✗ {distinctFiles} file(s) with {totalViolations} violation(s) " +
               $"(checked {TotalFilesChecked} files, {FilesPassed} passed{skippedMsg})";
    }
}
