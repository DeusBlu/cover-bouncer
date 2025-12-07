namespace CoverBouncer.Core.Models;

/// <summary>
/// Type of coverage violation.
/// MVP: Line coverage only.
/// </summary>
public enum ViolationType
{
    /// <summary>
    /// Line coverage is below the required threshold.
    /// </summary>
    LineCoverageTooLow
}
