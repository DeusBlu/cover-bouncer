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

            // Read profile tags from source files and track tagging stats
            var tagReader = new FileTagReader();
            var taggedFiles = new Dictionary<string, List<FileCoverage>>();
            var untaggedFiles = new List<FileCoverage>();
            
            foreach (var (filePath, coverage) in coverageReport.Files)
            {
                var tag = tagReader.ReadProfileTag(filePath);
                coverage.AssignedProfile = tag;
                
                var effectiveProfile = tag ?? config.DefaultProfile;
                
                if (tag == null)
                {
                    untaggedFiles.Add(coverage);
                }
                
                if (!taggedFiles.ContainsKey(effectiveProfile))
                {
                    taggedFiles[effectiveProfile] = new List<FileCoverage>();
                }
                taggedFiles[effectiveProfile].Add(coverage);
            }

            // Validate coverage
            var engine = new PolicyEngine();
            var result = engine.Validate(config, coverageReport);

            // Build profile summary
            var profileSummary = new Dictionary<string, (int passed, int failed, decimal threshold)>();
            foreach (var (profileName, files) in taggedFiles)
            {
                var threshold = config.Profiles.TryGetValue(profileName, out var t) ? t.MinLine : 0;
                var violations = result.Violations.Where(v => v.ProfileName == profileName).Select(v => v.FilePath).ToHashSet();
                var passed = files.Count(f => !violations.Contains(f.FilePath));
                var failed = files.Count - passed;
                profileSummary[profileName] = (passed, failed, threshold);
            }

            // Report results
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, "CoverBouncer: Coverage Summary by Profile");
            Log.LogMessage(MessageImportance.High, "─────────────────────────────────────────");
            
            foreach (var (profile, (passed, failed, threshold)) in profileSummary.OrderBy(p => p.Key))
            {
                var status = failed == 0 ? "✅" : "❌";
                var thresholdStr = threshold == 0 ? "exempt" : $"{threshold:P0} required";
                var isDefault = profile == config.DefaultProfile;
                var defaultMarker = isDefault ? " (default)" : "";
                
                Log.LogMessage(MessageImportance.High, 
                    $"  {status} {profile}{defaultMarker}: {passed} passed, {failed} failed ({thresholdStr})");
            }
            
            // Report untagged files
            Log.LogMessage(MessageImportance.High, "");
            if (untaggedFiles.Count > 0)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"  ℹ️  {untaggedFiles.Count} file(s) untagged → using '{config.DefaultProfile}' profile");
                Log.LogMessage(MessageImportance.Normal, 
                    $"     Tip: Tag files with // [CoverageProfile(\"ProfileName\")] for explicit control");
            }
            else
            {
                Log.LogMessage(MessageImportance.High, "  ✅ All files explicitly tagged");
            }
            
            Log.LogMessage(MessageImportance.High, "─────────────────────────────────────────");

            if (result.Success)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"✅ CoverBouncer: All {result.TotalFilesChecked} files passed coverage requirements");
                return true;
            }
            else
            {
                Log.LogError($"❌ CoverBouncer: {result.Violations.Count} coverage violation(s) found");
                Log.LogError("");
                
                // Group violations by profile
                var byProfile = result.Violations.GroupBy(v => v.ProfileName).OrderBy(g => g.Key);
                
                foreach (var group in byProfile)
                {
                    var threshold = config.Profiles.TryGetValue(group.Key, out var t) ? t.MinLine : 0;
                    Log.LogError($"  Profile: {group.Key} (requires {threshold:P0} line coverage)");
                    
                    foreach (var violation in group.OrderBy(v => v.FilePath))
                    {
                        var fileName = Path.GetFileName(violation.FilePath);
                        var gap = violation.RequiredCoverage - violation.ActualCoverage;
                        Log.LogError($"    ❌ {fileName}: {violation.ActualCoverage:P1} coverage (need {gap:P1} more)");
                    }
                    Log.LogError("");
                }

                // Actionable suggestions
                Log.LogMessage(MessageImportance.High, "💡 How to fix:");
                Log.LogMessage(MessageImportance.High, "   • Add tests to increase coverage for failing files");
                Log.LogMessage(MessageImportance.High, "   • Or lower the threshold by tagging with a less strict profile:");
                Log.LogMessage(MessageImportance.High, "     // [CoverageProfile(\"Standard\")]  // or \"Dto\" for 0% requirement");
                Log.LogMessage(MessageImportance.High, "");

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
