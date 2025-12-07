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
    /// </summary>
    public static PolicyConfiguration GenerateBasic()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "Standard",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Standard"] = new() { MinLine = 0.70m, MinBranch = 0.60m },
                ["BusinessLogic"] = new() { MinLine = 0.90m, MinBranch = 0.80m },
                ["Critical"] = new() { MinLine = 1.00m, MinBranch = 1.00m },
                ["Dto"] = new() { MinLine = 0.00m, MinBranch = 0.00m }
            }
        };
    }

    /// <summary>
    /// Generates a strict configuration with high coverage requirements.
    /// </summary>
    public static PolicyConfiguration GenerateStrict()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "High",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["High"] = new() { MinLine = 0.90m, MinBranch = 0.85m },
                ["Critical"] = new() { MinLine = 1.00m, MinBranch = 1.00m },
                ["Moderate"] = new() { MinLine = 0.75m, MinBranch = 0.70m },
                ["Low"] = new() { MinLine = 0.50m, MinBranch = 0.40m }
            }
        };
    }

    /// <summary>
    /// Generates a relaxed configuration with lower coverage requirements.
    /// </summary>
    public static PolicyConfiguration GenerateRelaxed()
    {
        return new PolicyConfiguration
        {
            CoverageReportPath = "TestResults/coverage.json",
            DefaultProfile = "Low",
            Profiles = new Dictionary<string, ProfileThresholds>
            {
                ["Low"] = new() { MinLine = 0.50m },
                ["Moderate"] = new() { MinLine = 0.70m },
                ["Important"] = new() { MinLine = 0.80m, MinBranch = 0.70m },
                ["Critical"] = new() { MinLine = 1.00m, MinBranch = 1.00m }
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
