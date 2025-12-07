namespace CoverBouncer.Core.Engine;

using System.Text.RegularExpressions;

/// <summary>
/// Reads coverage profile tags from source files.
/// MVP: Supports [CoverageProfile("Name")] attribute on file-scoped types.
/// </summary>
public sealed class FileTagReader
{
    // Matches: [CoverageProfile("ProfileName")]
    private static readonly Regex ProfileAttributeRegex = new(
        @"\[CoverageProfile\s*\(\s*""([^""]+)""\s*\)\]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Reads the coverage profile tag from a source file.
    /// </summary>
    /// <param name="filePath">Path to the source file.</param>
    /// <returns>The profile name if found; otherwise null.</returns>
    public string? ReadProfileTag(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var content = File.ReadAllText(filePath);
            return ExtractProfileFromContent(content);
        }
        catch (IOException)
        {
            // File access issues - return null to use default profile
            return null;
        }
    }

    /// <summary>
    /// Extracts the profile name from file content.
    /// </summary>
    /// <param name="content">The source file content.</param>
    /// <returns>The profile name if found; otherwise null.</returns>
    public string? ExtractProfileFromContent(string content)
    {
        var match = ProfileAttributeRegex.Match(content);
        return match.Success ? match.Groups[1].Value : null;
    }
}
