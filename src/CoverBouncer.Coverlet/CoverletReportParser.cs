namespace CoverBouncer.Coverlet;

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

        var fileData = new Dictionary<string, (int total, int covered)>();

        // Coverlet format: { "Module.dll": { "FilePath": { "ClassName": { "Method": { "Lines": {...} } } } } }
        foreach (var moduleProperty in doc.RootElement.EnumerateObject())
        {
            // Each module contains files
            foreach (var fileProperty in moduleProperty.Value.EnumerateObject())
            {
                var filePath = fileProperty.Name;
                
                // Each file contains classes
                foreach (var classProperty in fileProperty.Value.EnumerateObject())
                {
                    // Each class contains methods
                    foreach (var methodProperty in classProperty.Value.EnumerateObject())
                    {
                        if (!methodProperty.Value.TryGetProperty("Lines", out var lines))
                        {
                            continue;
                        }

                        // Aggregate line coverage at file level
                        if (!fileData.ContainsKey(filePath))
                        {
                            fileData[filePath] = (0, 0);
                        }

                        var (totalLines, coveredLines) = fileData[filePath];

                        foreach (var line in lines.EnumerateObject())
                        {
                            totalLines++;
                            var hitCount = line.Value.GetInt32();
                            if (hitCount > 0)
                            {
                                coveredLines++;
                            }
                        }

                        fileData[filePath] = (totalLines, coveredLines);
                    }
                }
            }
        }

        // Convert to FileCoverage objects
        var files = new Dictionary<string, FileCoverage>();
        foreach (var (filePath, (totalLines, coveredLines)) in fileData)
        {
            var lineRate = totalLines > 0 ? (decimal)coveredLines / totalLines : 0m;

            files[filePath] = new FileCoverage
            {
                FilePath = filePath,
                LineRate = lineRate,
                TotalLines = totalLines,
                CoveredLines = coveredLines
            };
        }

        return new CoverageReport
        {
            Files = files,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
