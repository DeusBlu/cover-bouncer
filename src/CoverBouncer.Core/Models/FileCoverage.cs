namespace CoverBouncer.Core.Models;

/// <summary>
/// Coverage information for a single file.
/// </summary>
public sealed class FileCoverage
{
    /// <summary>
    /// Full path to the source file.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Line coverage rate (0.0 to 1.0).
    /// </summary>
    public decimal LineRate { get; init; }

    /// <summary>
    /// Branch coverage rate (0.0 to 1.0). May be null if branch coverage not available.
    /// </summary>
    public decimal? BranchRate { get; init; }

    /// <summary>
    /// Total number of coverable lines.
    /// </summary>
    public int TotalLines { get; init; }

    /// <summary>
    /// Number of covered lines.
    /// </summary>
    public int CoveredLines { get; init; }

    /// <summary>
    /// Total number of coverable branches. May be 0 if no branches.
    /// </summary>
    public int TotalBranches { get; init; }

    /// <summary>
    /// Number of covered branches. May be 0 if no branches.
    /// </summary>
    public int CoveredBranches { get; init; }

    /// <summary>
    /// Coverage profile assigned to this file (from tag or default).
    /// </summary>
    public string? AssignedProfile { get; set; }
}
