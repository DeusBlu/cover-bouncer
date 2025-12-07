namespace CoverBouncer.Core.Adapters;

using System.Text.Json;
using CoverBouncer.Core.Models;

/// <summary>
/// Parses Coverlet JSON coverage reports into normalized CoverageReport model.
/// </summary>
public sealed class CoverletReportParser
{
    /// <summary>
    /// Parses a Coverlet JSON file into a normalized coverage report.
    /// </summary>
    /// <param name="jsonPath">Path to the Coverlet JSON file.</param>
    /// <returns>Normalized coverage report.</returns>
    public CoverageReport ParseFile(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException($"Coverage report not found: {jsonPath}");
        }

        var json = File.ReadAllText(jsonPath);
        return ParseJson(json);
    }

    /// <summary>
    /// Parses Coverlet JSON content into a normalized coverage report.
    /// </summary>
    /// <param name="json">JSON content from Coverlet.</param>
    /// <returns>Normalized coverage report.</returns>
    public CoverageReport ParseJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        var files = new Dictionary<string, FileCoverage>();

        // Coverlet format: root object has module keys
        foreach (var moduleProperty in doc.RootElement.EnumerateObject())
        {
            if (!moduleProperty.Value.TryGetProperty("Documents", out var documents))
            {
                continue;
            }

            // Each document is a file
            foreach (var docProperty in documents.EnumerateObject())
            {
                var filePath = docProperty.Name;
                var docData = docProperty.Value;

                var totalLines = 0;
                var coveredLines = 0;

                // Lines object contains line numbers and hit counts
                if (docData.TryGetProperty("Lines", out var lines))
                {
                    foreach (var line in lines.EnumerateObject())
                    {
                        totalLines++;
                        var hitCount = line.Value.GetInt32();
                        if (hitCount > 0)
                        {
                            coveredLines++;
                        }
                    }
                }

                // Calculate line rate
                var lineRate = totalLines > 0 
                    ? (decimal)coveredLines / totalLines 
                    : 0m;

                files[filePath] = new FileCoverage
                {
                    FilePath = filePath,
                    LineRate = lineRate,
                    TotalLines = totalLines,
                    CoveredLines = coveredLines
                };
            }
        }

        return new CoverageReport
        {
            Files = files,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
