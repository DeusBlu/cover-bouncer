namespace CoverBouncer.Core.Models;

/// <summary>
/// Coverage information for a single file.
/// MVP: Line coverage only.
/// </summary>
public sealed class FileCoverage
{
    /// <summary>
    /// Full path to the source file.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Line coverage rate (0.0 to 1.0).
    /// </summary>
    public decimal LineRate { get; set; }

    /// <summary>
    /// Total number of coverable lines.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Number of covered lines.
    /// </summary>
    public int CoveredLines { get; set; }

    /// <summary>
    /// Coverage profile assigned to this file (from tag or default).
    /// </summary>
    public string? AssignedProfile { get; set; }
}
