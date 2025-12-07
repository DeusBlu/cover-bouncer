namespace CoverBouncer.Core.Models;

/// <summary>
/// Normalized coverage report containing coverage information for all files.
/// </summary>
public sealed class CoverageReport
{
    /// <summary>
    /// Coverage information for each file, keyed by file path.
    /// </summary>
    public Dictionary<string, FileCoverage> Files { get; init; } = new();

    /// <summary>
    /// When the coverage was generated.
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a file's coverage by path, or null if not found.
    /// </summary>
    public FileCoverage? GetFile(string filePath)
    {
        return Files.TryGetValue(filePath, out var coverage) ? coverage : null;
    }

    /// <summary>
    /// Gets all files that have coverage data.
    /// </summary>
    public IReadOnlyCollection<FileCoverage> GetAllFiles()
    {
        return Files.Values.ToList().AsReadOnly();
    }
}
