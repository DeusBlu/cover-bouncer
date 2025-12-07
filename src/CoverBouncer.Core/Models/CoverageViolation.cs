namespace CoverBouncer.Core.Models;

/// <summary>
/// Represents a coverage policy violation for a single file.
/// MVP: Line coverage only.
/// </summary>
public sealed class CoverageViolation
{
    /// <summary>
    /// Path to the file that violated the policy.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Profile that was violated.
    /// </summary>
    public string ProfileName { get; set; } = null!;

    /// <summary>
    /// Type of violation (line coverage).
    /// </summary>
    public ViolationType ViolationType { get; set; }

    /// <summary>
    /// Required coverage threshold (0.0 to 1.0).
    /// </summary>
    public decimal RequiredCoverage { get; set; }

    /// <summary>
    /// Actual coverage achieved (0.0 to 1.0).
    /// </summary>
    public decimal ActualCoverage { get; set; }

    /// <summary>
    /// Gets a formatted message describing the violation.
    /// </summary>
    public string GetMessage()
    {
        return $"{FilePath} ({ProfileName}): " +
               $"line coverage is {ActualCoverage:P1}, required {RequiredCoverage:P0}";
    }

    /// <summary>
    /// Gets a short summary of the violation.
    /// </summary>
    public string GetShortMessage()
    {
        return $"{ActualCoverage:P1} < {RequiredCoverage:P0}";
    }
}
