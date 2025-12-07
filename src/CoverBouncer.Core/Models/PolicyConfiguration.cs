using System.Text.Json.Serialization;

namespace CoverBouncer.Core.Models;

/// <summary>
/// Configuration for coverage policy enforcement.
/// </summary>
public sealed class PolicyConfiguration
{
    /// <summary>
    /// Path to the Coverlet JSON coverage report.
    /// </summary>
    [JsonPropertyName("coverageReportPath")]
    public string CoverageReportPath { get; set; } = "TestResults/coverage.json";

    /// <summary>
    /// Default profile applied to files without explicit tags.
    /// </summary>
    [JsonPropertyName("defaultProfile")]
    public string DefaultProfile { get; set; } = null!;

    /// <summary>
    /// Dictionary of profile names to their threshold requirements.
    /// </summary>
    [JsonPropertyName("profiles")]
    public Dictionary<string, ProfileThresholds> Profiles { get; set; } = new();

    /// <summary>
    /// Validates the configuration for correctness.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        // DefaultProfile is required
        if (string.IsNullOrWhiteSpace(DefaultProfile))
        {
            throw new ArgumentException(
                "Configuration error: 'defaultProfile' is required",
                nameof(DefaultProfile));
        }

        // Profiles dictionary is required
        if (Profiles == null || Profiles.Count == 0)
        {
            throw new ArgumentException(
                "Configuration error: 'profiles' must contain at least one profile",
                nameof(Profiles));
        }

        // DefaultProfile must exist in Profiles
        if (!Profiles.ContainsKey(DefaultProfile))
        {
            throw new ArgumentException(
                $"Configuration error: defaultProfile '{DefaultProfile}' does not exist in profiles. " +
                $"Available profiles: {string.Join(", ", Profiles.Keys)}",
                nameof(DefaultProfile));
        }

        // Validate each profile's thresholds
        foreach (var (profileName, thresholds) in Profiles)
        {
            if (thresholds == null)
            {
                throw new ArgumentException(
                    $"Configuration error: Profile '{profileName}' has null thresholds",
                    nameof(Profiles));
            }

            thresholds.Validate(profileName);
        }

        // Validate coverage report path is not empty
        if (string.IsNullOrWhiteSpace(CoverageReportPath))
        {
            throw new ArgumentException(
                "Configuration error: 'coverageReportPath' cannot be empty",
                nameof(CoverageReportPath));
        }
    }
}
