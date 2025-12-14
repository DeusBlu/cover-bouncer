namespace CoverBouncer.Core.Engine;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Writes coverage profile tags to source files.
/// Supports adding [CoverageProfile("Name")] attributes to files.
/// </summary>
public sealed class FileTagWriter
{
    // Matches: [CoverageProfile("ProfileName")]
    private static readonly Regex ProfileAttributeRegex = new(
        @"\[CoverageProfile\s*\(\s*""([^""]+)""\s*\)\]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Adds or updates a coverage profile tag in a source file.
    /// </summary>
    /// <param name="filePath">Path to the source file.</param>
    /// <param name="profileName">The profile name to tag the file with.</param>
    /// <param name="createBackup">Whether to create a backup of the original file.</param>
    /// <returns>True if the file was modified; false if it already had the same tag.</returns>
    public bool WriteProfileTag(string filePath, string profileName, bool createBackup = false)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Source file not found: {filePath}");
        }

        var content = File.ReadAllText(filePath);
        var existingProfile = ExtractProfileFromContent(content);

        // Already has the same tag
        if (existingProfile == profileName)
        {
            return false;
        }

        // Create backup if requested
        if (createBackup)
        {
            File.Copy(filePath, $"{filePath}.backup", overwrite: true);
        }

        string newContent;
        if (existingProfile != null)
        {
            // Replace existing tag
            newContent = ProfileAttributeRegex.Replace(
                content, 
                $"[CoverageProfile(\"{profileName}\")]",
                1); // Only replace the first occurrence
        }
        else
        {
            // Add new tag
            newContent = AddProfileTag(content, profileName);
        }

        File.WriteAllText(filePath, newContent);
        return true;
    }

    /// <summary>
    /// Removes a coverage profile tag from a source file.
    /// </summary>
    /// <param name="filePath">Path to the source file.</param>
    /// <returns>True if a tag was removed; false if no tag was found.</returns>
    public bool RemoveProfileTag(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Source file not found: {filePath}");
        }

        var content = File.ReadAllText(filePath);
        var existingProfile = ExtractProfileFromContent(content);

        if (existingProfile == null)
        {
            return false;
        }

        // Remove the tag and any surrounding whitespace
        var newContent = ProfileAttributeRegex.Replace(content, "", 1);
        
        // Clean up extra blank lines that might be left
        newContent = Regex.Replace(newContent, @"\n\s*\n\s*\n", "\n\n");

        File.WriteAllText(filePath, newContent);
        return true;
    }

    /// <summary>
    /// Extracts the profile name from file content.
    /// </summary>
    private string? ExtractProfileFromContent(string content)
    {
        var match = ProfileAttributeRegex.Match(content);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Adds a profile tag to the file content.
    /// Inserts before the namespace declaration or at the beginning of the file.
    /// </summary>
    private string AddProfileTag(string content, string profileName)
    {
        var tag = $"// [CoverageProfile(\"{profileName}\")]";
        
        // Try to find namespace declaration (handles both namespace Foo; and namespace Foo { })
        var namespaceMatch = Regex.Match(content, @"^\s*namespace\s+", RegexOptions.Multiline);
        
        if (namespaceMatch.Success)
        {
            // Insert before namespace
            var insertIndex = namespaceMatch.Index;
            
            // Check if there's already a comment section before namespace
            var beforeNamespace = content.Substring(0, insertIndex).TrimEnd();
            
            // If there's existing content, add a blank line before the tag
            if (beforeNamespace.Length > 0 && !beforeNamespace.EndsWith("\n\n"))
            {
                tag = "\n" + tag;
            }
            
            return content.Insert(insertIndex, tag + "\n");
        }
        
        // Try to find class/interface/enum/record declaration
        var typeMatch = Regex.Match(
            content, 
            @"^\s*(public|internal|private|protected)?\s*(static|abstract|sealed|partial)?\s*(class|interface|enum|record|struct)\s+",
            RegexOptions.Multiline);
        
        if (typeMatch.Success)
        {
            // Insert before type declaration
            var insertIndex = typeMatch.Index;
            var beforeType = content.Substring(0, insertIndex).TrimEnd();
            
            if (beforeType.Length > 0 && !beforeType.EndsWith("\n\n"))
            {
                tag = "\n" + tag;
            }
            
            return content.Insert(insertIndex, tag + "\n");
        }
        
        // Fallback: Add at the beginning of the file (after usings if present)
        var lastUsingMatch = Regex.Matches(content, @"^\s*using\s+.*;", RegexOptions.Multiline)
            .Cast<Match>()
            .LastOrDefault();
        
        if (lastUsingMatch != null)
        {
            var insertIndex = lastUsingMatch.Index + lastUsingMatch.Length;
            return content.Insert(insertIndex, "\n\n" + tag);
        }
        
        // Absolute fallback: beginning of file
        return tag + "\n\n" + content;
    }
}
