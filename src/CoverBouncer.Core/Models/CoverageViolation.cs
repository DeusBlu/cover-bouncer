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
    /// Line numbers that were not covered.
    /// </summary>
    public List<int> UncoveredLines { get; set; } = new();

    /// <summary>
    /// Gets a formatted message describing the violation.
    /// </summary>
    public string GetMessage()
    {
        var msg = $"{FilePath} ({ProfileName}): " +
               $"line coverage is {ActualCoverage:P1}, required {RequiredCoverage:P0}";
        var ranges = FormatLineRanges(UncoveredLines);
        if (!string.IsNullOrEmpty(ranges))
        {
            msg += $" [uncovered: {ranges}]";
        }
        return msg;
    }

    /// <summary>
    /// Gets a short summary of the violation.
    /// </summary>
    public string GetShortMessage()
    {
        return $"{ActualCoverage:P1} < {RequiredCoverage:P0}";
    }

    /// <summary>
    /// Formats a sorted list of line numbers into compact ranges.
    /// Example: [1, 2, 3, 7, 9, 10, 11] → "1-3, 7, 9-11"
    /// </summary>
    public static string FormatLineRanges(List<int> lines)
    {
        if (lines == null || lines.Count == 0)
            return "";

        var sorted = lines.OrderBy(n => n).ToList();
        var ranges = new List<string>();
        var start = sorted[0];
        var end = sorted[0];

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] == end + 1)
            {
                end = sorted[i];
            }
            else
            {
                ranges.Add(start == end ? $"{start}" : $"{start}-{end}");
                start = sorted[i];
                end = sorted[i];
            }
        }

        ranges.Add(start == end ? $"{start}" : $"{start}-{end}");
        return string.Join(", ", ranges);
    }
}
