using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Models;
using System.Text.Json;

namespace CoverBouncer.Core.Tests.Configuration;

public class ConfigurationLoaderTests
{
    [Fact]
    public void LoadFromJson_WithValidJson_ReturnsConfiguration()
    {
        var json = """
        {
            "coverageReportPath": "TestResults/coverage.json",
            "defaultProfile": "Standard",
            "profiles": {
                "Standard": {
                    "minLine": 0.70
                }
            }
        }
        """;

        var config = ConfigurationLoader.LoadFromJson(json);

        Assert.NotNull(config);
        Assert.Equal("Standard", config.DefaultProfile);
        Assert.Single(config.Profiles);
        Assert.Equal(0.70m, config.Profiles["Standard"].MinLine);
    }

    [Fact]
    public void LoadFromJson_WithCommentsAndTrailingCommas_ReturnsConfiguration()
    {
        var json = """
        {
            // This is a comment
            "defaultProfile": "Standard",
            "profiles": {
                "Standard": {
                    "minLine": 0.70,
                },
            },
        }
        """;

        var config = ConfigurationLoader.LoadFromJson(json);

        Assert.NotNull(config);
        Assert.Equal("Standard", config.DefaultProfile);
    }

    [Fact]
    public void LoadFromJson_WithInvalidJson_ThrowsJsonException()
    {
        var json = "{ invalid json }";

        Assert.Throws<JsonException>(() => ConfigurationLoader.LoadFromJson(json));
    }

    [Fact]
    public void LoadFromJson_WithInvalidConfiguration_ThrowsArgumentException()
    {
        var json = """
        {
            "defaultProfile": "NonExistent",
            "profiles": {
                "Standard": {
                    "minLine": 0.70
                }
            }
        }
        """;

        var exception = Assert.Throws<ArgumentException>(() => ConfigurationLoader.LoadFromJson(json));
        Assert.Contains("NonExistent", exception.Message);
    }

    [Fact]
    public void LoadFromJson_WithCaseInsensitiveProperties_ReturnsConfiguration()
    {
        var json = """
        {
            "DEFAULTPROFILE": "Standard",
            "PROFILES": {
                "Standard": {
                    "MINLINE": 0.70
                }
            }
        }
        """;

        var config = ConfigurationLoader.LoadFromJson(json);

        Assert.NotNull(config);
        Assert.Equal("Standard", config.DefaultProfile);
        Assert.Equal(0.70m, config.Profiles["Standard"].MinLine);
    }

    [Fact]
    public void LoadFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        var exception = Assert.Throws<FileNotFoundException>(
            () => ConfigurationLoader.LoadFromFile("nonexistent.json"));

        Assert.Contains("nonexistent.json", exception.Message);
    }

    [Fact]
    public void LoadFromFile_WithValidFile_ReturnsConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
            {
                "defaultProfile": "Standard",
                "profiles": {
                    "Standard": { "minLine": 0.70 }
                }
            }
            """;
            File.WriteAllText(tempFile, json);

            var config = ConfigurationLoader.LoadFromFile(tempFile);

            Assert.NotNull(config);
            Assert.Equal("Standard", config.DefaultProfile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindConfigFile_WithFileInCurrentDirectory_ReturnsPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var configPath = Path.Combine(tempDir, "coverbouncer.json");
            File.WriteAllText(configPath, "{}");

            var found = ConfigurationLoader.FindConfigFile(tempDir);

            Assert.Equal(configPath, found);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindConfigFile_WithFileInParentDirectory_ReturnsPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var subDir = Path.Combine(tempDir, "sub");
        Directory.CreateDirectory(subDir);
        try
        {
            var configPath = Path.Combine(tempDir, "coverbouncer.json");
            File.WriteAllText(configPath, "{}");

            var found = ConfigurationLoader.FindConfigFile(subDir);

            Assert.Equal(configPath, found);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindConfigFile_WithNoFile_ReturnsNull()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var found = ConfigurationLoader.FindConfigFile(tempDir);

            Assert.Null(found);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
