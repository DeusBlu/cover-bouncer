using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;
using CoverBouncer.Core.Models;
using CoverBouncer.Coverlet;

namespace CoverBouncer.MSBuild;

/// <summary>
/// MSBuild task that verifies code coverage against policy.
/// </summary>
public sealed class VerifyCoverageTask : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Path to the Coverlet JSON coverage report.
    /// </summary>
    [Required]
    public string CoverageReportPath { get; set; } = null!;

    /// <summary>
    /// Path to the coverbouncer.json configuration file.
    /// Default: coverbouncer.json
    /// </summary>
    public string ConfigPath { get; set; } = "coverbouncer.json";

    /// <summary>
    /// Project directory to search for config file.
    /// </summary>
    public string ProjectDirectory { get; set; } = null!;

    /// <summary>
    /// Whether to fail the build on coverage violations.
    /// Default: true
    /// </summary>
    public bool FailOnViolations { get; set; } = true;

    public override bool Execute()
    {
        try
        {
            Log.LogMessage(MessageImportance.High, "CoverBouncer: Verifying coverage policy...");

            // Check if coverage report exists
            if (!File.Exists(CoverageReportPath))
            {
                Log.LogWarning($"CoverBouncer: Coverage report not found: {CoverageReportPath}");
                Log.LogWarning("CoverBouncer: Skipping coverage verification. Run tests with coverage collection enabled.");
                return true; // Don't fail if coverage wasn't collected
            }

            // Load configuration
            PolicyConfiguration config;
            try
            {
                config = ConfigurationLoader.LoadFromFileOrParent(ProjectDirectory, ConfigPath);
                Log.LogMessage(MessageImportance.Normal, $"CoverBouncer: Loaded configuration from {ConfigPath}");
            }
            catch (FileNotFoundException)
            {
                Log.LogWarning($"CoverBouncer: Configuration file '{ConfigPath}' not found. Run 'coverbouncer init' to create one.");
                Log.LogWarning("CoverBouncer: Skipping coverage verification.");
                return true; // Don't fail if config doesn't exist
            }

            // Parse coverage report
            var parser = new CoverletReportParser();
            var coverageReport = parser.ParseFile(CoverageReportPath);
            Log.LogMessage(MessageImportance.Normal, $"CoverBouncer: Parsed coverage for {coverageReport.Files.Count} files");

            // Read profile tags from source files
            var tagReader = new FileTagReader();
            foreach (var (filePath, coverage) in coverageReport.Files)
            {
                coverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
            }

            // Validate coverage
            var engine = new PolicyEngine();
            var result = engine.Validate(config, coverageReport);

            // Report results
            if (result.Success)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"✅ CoverBouncer: All {result.TotalFilesChecked} files passed coverage requirements");
                return true;
            }
            else
            {
                Log.LogError($"❌ CoverBouncer: Coverage policy violations found ({result.Violations.Count} of {result.TotalFilesChecked} files)");
                
                // Group violations by profile
                var byProfile = result.Violations.GroupBy(v => v.ProfileName).OrderBy(g => g.Key);
                
                foreach (var group in byProfile)
                {
                    Log.LogError($"CoverBouncer:   Profile: {group.Key}");
                    foreach (var violation in group.OrderBy(v => v.FilePath))
                    {
                        Log.LogError($"CoverBouncer:     {violation.FilePath}: Required {violation.RequiredCoverage:P0}, Actual {violation.ActualCoverage:P1}");
                    }
                }

                Log.LogError($"CoverBouncer:   Summary: {result.FilesPassed} passed, {result.Violations.Count} failed");

                return !FailOnViolations; // Fail build if FailOnViolations is true
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"CoverBouncer: Verification failed: {ex.Message}");
            if (ex.InnerException != null)
            {
                Log.LogError($"CoverBouncer:   {ex.InnerException.Message}");
            }
            return false;
        }
    }
}
