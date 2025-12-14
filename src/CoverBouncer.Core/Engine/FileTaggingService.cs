namespace CoverBouncer.Core.Engine;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

/// <summary>
/// Service for batch tagging files with coverage profiles.
/// </summary>
public sealed class FileTaggingService
{
    private readonly FileTagWriter _writer;
    private readonly FileTagReader _reader;

    public FileTaggingService()
    {
        _writer = new FileTagWriter();
        _reader = new FileTagReader();
    }

    /// <summary>
    /// Result of a tagging operation.
    /// </summary>
    public record TaggingResult(
        int FilesMatched,
        int FilesTagged,
        int FilesSkipped,
        int FilesErrored,
        List<string> TaggedFiles,
        List<string> SkippedFiles,
        List<(string File, string Error)> Errors);

    /// <summary>
    /// Tags files matching a glob pattern with the specified profile.
    /// </summary>
    /// <param name="baseDirectory">Base directory to search from.</param>
    /// <param name="pattern">Glob pattern to match files (e.g., "**/*Service.cs").</param>
    /// <param name="profileName">Profile name to apply.</param>
    /// <param name="createBackup">Whether to create backups before modifying.</param>
    /// <param name="dryRun">If true, only simulate the operation without modifying files.</param>
    public TaggingResult TagByPattern(
        string baseDirectory,
        string pattern,
        string profileName,
        bool createBackup = false,
        bool dryRun = false)
    {
        var matcher = new Matcher();
        matcher.AddInclude(pattern);

        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(baseDirectory)));
        var matchedFiles = result.Files
            .Select(f => Path.Combine(baseDirectory, f.Path))
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return TagFiles(matchedFiles, profileName, createBackup, dryRun);
    }

    /// <summary>
    /// Tags all files in a directory (and optionally subdirectories) with the specified profile.
    /// </summary>
    public TaggingResult TagByDirectory(
        string directoryPath,
        string profileName,
        bool recursive = true,
        bool createBackup = false,
        bool dryRun = false)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = Directory.GetFiles(directoryPath, "*.cs", searchOption).ToList();

        return TagFiles(files, profileName, createBackup, dryRun);
    }

    /// <summary>
    /// Tags specific files with the specified profile.
    /// </summary>
    public TaggingResult TagFiles(
        IEnumerable<string> filePaths,
        string profileName,
        bool createBackup = false,
        bool dryRun = false)
    {
        var tagged = new List<string>();
        var skipped = new List<string>();
        var errors = new List<(string, string)>();

        foreach (var filePath in filePaths)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    errors.Add((filePath, "File not found"));
                    continue;
                }

                // Check if file already has the same tag
                var existingProfile = _reader.ReadProfileTag(filePath);
                if (existingProfile == profileName)
                {
                    skipped.Add(filePath);
                    continue;
                }

                if (!dryRun)
                {
                    _writer.WriteProfileTag(filePath, profileName, createBackup);
                }

                tagged.Add(filePath);
            }
            catch (Exception ex)
            {
                errors.Add((filePath, ex.Message));
            }
        }

        return new TaggingResult(
            FilesMatched: filePaths.Count(),
            FilesTagged: tagged.Count,
            FilesSkipped: skipped.Count,
            FilesErrored: errors.Count,
            TaggedFiles: tagged,
            SkippedFiles: skipped,
            Errors: errors);
    }

    /// <summary>
    /// Suggests profiles for files based on naming patterns.
    /// </summary>
    public Dictionary<string, string> SuggestProfiles(IEnumerable<string> filePaths)
    {
        var suggestions = new Dictionary<string, string>();

        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileName(filePath);
            var directory = Path.GetFileName(Path.GetDirectoryName(filePath) ?? "");

            var profile = SuggestProfileForFile(fileName, directory);
            if (profile != null)
            {
                suggestions[filePath] = profile;
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Suggests a profile based on file name and directory patterns.
    /// </summary>
    private string? SuggestProfileForFile(string fileName, string directory)
    {
        // Critical patterns
        if (directory.Contains("Security", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Auth", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("Security", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("Payment", StringComparison.OrdinalIgnoreCase))
        {
            return "Critical";
        }

        // Integration patterns
        if (fileName.EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("Adapter.cs", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Controllers", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Adapters", StringComparison.OrdinalIgnoreCase))
        {
            return "Integration";
        }

        // Business Logic patterns
        if (fileName.EndsWith("Service.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("Manager.cs", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Services", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Business", StringComparison.OrdinalIgnoreCase))
        {
            return "BusinessLogic";
        }

        // DTO patterns
        if (fileName.EndsWith("Dto.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("ViewModel.cs", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("Model.cs", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("Models", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("DTOs", StringComparison.OrdinalIgnoreCase) ||
            directory.Contains("ViewModels", StringComparison.OrdinalIgnoreCase))
        {
            return "Dto";
        }

        // Default: Standard
        return "Standard";
    }

    /// <summary>
    /// Reads files from a text file (one file path per line).
    /// </summary>
    public static List<string> ReadFileList(string listFilePath)
    {
        if (!File.Exists(listFilePath))
        {
            throw new FileNotFoundException($"File list not found: {listFilePath}");
        }

        return File.ReadAllLines(listFilePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.TrimStart().StartsWith("#")) // Skip comments
            .Select(line => line.Trim())
            .ToList();
    }
}
