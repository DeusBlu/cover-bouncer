namespace CoverBouncer.Core.Models;

/// <summary>
/// Type of coverage violation.
/// </summary>
public enum ViolationType
{
    /// <summary>
    /// Line coverage below threshold.
    /// </summary>
    LineCoverageTooLow,

    /// <summary>
    /// Branch coverage below threshold.
    /// </summary>
    BranchCoverageTooLow
}
