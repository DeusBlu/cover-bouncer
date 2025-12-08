using System.Text.Json;
using CoverBouncer.Core.Models;

namespace CoverBouncer.Core.Configuration;

/// <summary>
/// Loads and parses policy configuration from JSON files.
/// </summary>
public sealed class ConfigurationLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Loads configuration from a JSON file.
    /// </summary>
    /// <param name="configFilePath">Path to the configuration file.</param>
    /// <returns>Validated policy configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when config file doesn't exist.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public static PolicyConfiguration LoadFromFile(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException(
                $"Configuration file not found: {configFilePath}",
                configFilePath);
        }

        var json = File.ReadAllText(configFilePath);
        return LoadFromJson(json, configFilePath);
    }

    /// <summary>
    /// Loads configuration from a JSON string.
    /// </summary>
    /// <param name="json">JSON configuration content.</param>
    /// <param name="source">Source identifier for error messages.</param>
    /// <returns>Validated policy configuration.</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public static PolicyConfiguration LoadFromJson(string json, string source = "JSON")
    {
        PolicyConfiguration? config;
        
        try
        {
            config = JsonSerializer.Deserialize<PolicyConfiguration>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException(
                $"Failed to parse configuration from {source}: {ex.Message}",
                ex);
        }

        if (config == null)
        {
            throw new JsonException($"Configuration from {source} deserialized to null");
        }

        // Validate the configuration
        try
        {
            config.Validate();
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(
                $"Invalid configuration in {source}: {ex.Message}",
                ex);
        }

        return config;
    }

    /// <summary>
    /// Tries to find a configuration file in the current or parent directories.
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from.</param>
    /// <param name="configFileName">Configuration file name to look for.</param>
    /// <returns>Full path to config file, or null if not found.</returns>
    public static string? FindConfigFile(string? startDirectory = null, string configFileName = "coverbouncer.json")
    {
        startDirectory ??= Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            var configPath = Path.Combine(directory.FullName, configFileName);
            if (File.Exists(configPath))
            {
                return configPath;
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Loads configuration from file, searching parent directories if not found in current directory.
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from.</param>
    /// <param name="configFileName">Configuration file name to look for.</param>
    /// <returns>Validated policy configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when config file not found.</exception>
    public static PolicyConfiguration LoadFromFileOrParent(string? startDirectory = null, string configFileName = "coverbouncer.json")
    {
        var configPath = FindConfigFile(startDirectory, configFileName);
        
        if (configPath == null)
        {
            var searchStart = startDirectory ?? Directory.GetCurrentDirectory();
            throw new FileNotFoundException(
                $"Configuration file '{configFileName}' not found in '{searchStart}' or any parent directory");
        }

        return LoadFromFile(configPath);
    }

    /// <summary>
    /// Smart loader that handles both file paths and config names.
    /// If the path exists as a file, loads it directly.
    /// Otherwise, searches for the config name in current and parent directories.
    /// </summary>
    /// <param name="pathOrName">Either a file path or a config file name.</param>
    /// <returns>Validated policy configuration.</returns>
    /// <exception cref="FileNotFoundException">Thrown when config file not found.</exception>
    public static PolicyConfiguration LoadSmart(string pathOrName)
    {
        // If it's an existing file path, load it directly
        if (File.Exists(pathOrName))
        {
            return LoadFromFile(pathOrName);
        }

        // Otherwise, treat it as a config name and search from current directory
        return LoadFromFileOrParent(
            startDirectory: Directory.GetCurrentDirectory(),
            configFileName: pathOrName);
    }
}
