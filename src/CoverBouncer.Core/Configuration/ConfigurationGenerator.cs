using System.Text.Json;
using CoverBouncer.Core.Models;

namespace CoverBouncer.Core.Configuration;

/// <summary>
/// Generates default configuration files.
/// </summary>
public static class ConfigurationGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Generates a basic default configuration.
    /// MVP: Line coverage only with built-in profiles.
    /// Defaults to NoCoverage (0%) so users can adopt gradually.
    /// </summary>
    public static PolicyConfiguration GenerateBasic()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "NoCoverage",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Critical"] = new() { MinLine = 1.0m },
                ["BusinessLogic"] = new() { MinLine = 0.8m },
                ["Standard"] = new() { MinLine = 0.6m },
                ["Dto"] = new() { MinLine = 0.0m },
                ["NoCoverage"] = new() { MinLine = 0.0m }
            }
        };
    }

    /// <summary>
    /// Generates a strict configuration with high coverage requirements.
    /// Defaults to NoCoverage (0%) so users can adopt gradually.
    /// </summary>
    public static PolicyConfiguration GenerateStrict()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "NoCoverage",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Critical"] = new() { MinLine = 1.0m },
                ["BusinessLogic"] = new() { MinLine = 0.9m },
                ["Standard"] = new() { MinLine = 0.8m },
                ["Dto"] = new() { MinLine = 0.0m },
                ["NoCoverage"] = new() { MinLine = 0.0m }
            }
        };
    }

    /// <summary>
    /// Generates a relaxed configuration with lower coverage requirements.
    /// Defaults to NoCoverage (0%) so users can adopt gradually.
    /// </summary>
    public static PolicyConfiguration GenerateRelaxed()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "NoCoverage",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Critical"] = new() { MinLine = 0.8m },
                ["BusinessLogic"] = new() { MinLine = 0.6m },
                ["Standard"] = new() { MinLine = 0.4m },
                ["Dto"] = new() { MinLine = 0.0m },
                ["NoCoverage"] = new() { MinLine = 0.0m }
            }
        };
    }

    /// <summary>
    /// Gets a configuration by template name.
    /// </summary>
    public static PolicyConfiguration GetTemplate(string templateName)
    {
        return templateName.ToLowerInvariant() switch
        {
            "basic" => GenerateBasic(),
            "strict" => GenerateStrict(),
            "relaxed" => GenerateRelaxed(),
            _ => throw new ArgumentException(
                $"Unknown template '{templateName}'. Valid templates: basic, strict, relaxed",
                nameof(templateName))
        };
    }

    /// <summary>
    /// Generates configuration JSON string.
    /// </summary>
    public static string ToJson(PolicyConfiguration config)
    {
        return JsonSerializer.Serialize(config, JsonOptions);
    }

    /// <summary>
    /// Writes configuration to a file.
    /// </summary>
    public static void WriteToFile(PolicyConfiguration config, string filePath)
    {
        var json = ToJson(config);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Generates and writes a template configuration to a file.
    /// </summary>
    public static void GenerateTemplateFile(string templateName, string outputPath)
    {
        var config = GetTemplate(templateName);
        WriteToFile(config, outputPath);
    }
}
