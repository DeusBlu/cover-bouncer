using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using CoverBouncer.Core;
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

    /// <summary>
    /// The test case filter expression from dotnet test --filter.
    /// When non-empty, files with zero covered lines are skipped
    /// (they were instrumented but not targeted by the filtered tests).
    /// </summary>
    public string TestCaseFilter { get; set; } = "";

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
            var isFilteredRun = !string.IsNullOrWhiteSpace(TestCaseFilter);
            if (isFilteredRun)
            {
                Log.LogMessage(MessageImportance.Normal, 
                    $"CoverBouncer: Filtered test run detected (filter: {TestCaseFilter})");
                Log.LogMessage(MessageImportance.Normal, 
                    "CoverBouncer: Files with zero coverage will be skipped (not targeted by filtered tests)");
            }
            var result = engine.Validate(config, coverageReport, isFilteredRun);

            // Build profile summary (exclude skipped files from counts)
            var skippedFilePaths = isFilteredRun
                ? coverageReport.Files
                    .Where(f => f.Value.CoveredLines == 0)
                    .Select(f => f.Key)
                    .ToHashSet()
                : new HashSet<string>();
            
            var profileSummary = new Dictionary<string, (int passed, int failed, decimal threshold)>();
            foreach (var (profileName, files) in taggedFiles)
            {
                var threshold = config.Profiles.TryGetValue(profileName, out var t) ? t.MinLine : 0;
                var violations = result.Violations.Where(v => v.ProfileName == profileName).Select(v => v.FilePath).ToHashSet();
                var validatedFiles = files.Where(f => !skippedFilePaths.Contains(f.FilePath)).ToList();
                var passed = validatedFiles.Count(f => !violations.Contains(f.FilePath));
                var failed = validatedFiles.Count - passed;
                profileSummary[profileName] = (passed, failed, threshold);
            }

            // Report results
            Log.LogMessage(MessageImportance.High, "");
            Log.LogMessage(MessageImportance.High, $"{Ansi.Heading}CoverBouncer: Coverage Summary by Profile{Ansi.Reset}");
            Log.LogMessage(MessageImportance.High, $"{Ansi.Muted}─────────────────────────────────────────{Ansi.Reset}");
            
            foreach (var (profile, (passed, failed, threshold)) in profileSummary.Where(p => p.Value.passed + p.Value.failed > 0).OrderBy(p => p.Key))
            {
                var status = failed == 0 ? $"{Ansi.Pass}✅" : $"{Ansi.Fail}❌";
                var thresholdStr = threshold == 0 ? "exempt" : $"{threshold:P0} required";
                var isDefault = profile == config.DefaultProfile;
                var defaultMarker = isDefault ? $" {Ansi.Muted}(default){Ansi.Reset}" : "";
                
                Log.LogMessage(MessageImportance.High, 
                    $"  {status} {Ansi.Info}{profile}{Ansi.Reset}{defaultMarker}: {passed} passed, {failed} failed {Ansi.Muted}({thresholdStr}){Ansi.Reset}");
            }
            
            // Report untagged files
            Log.LogMessage(MessageImportance.High, "");
            if (untaggedFiles.Count > 0)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"  {Ansi.Warn}ℹ️  {untaggedFiles.Count} file(s) untagged → using '{config.DefaultProfile}' profile{Ansi.Reset}");
                Log.LogMessage(MessageImportance.Normal, 
                    $"     {Ansi.Muted}Tip: Tag files with // [CoverageProfile(\"ProfileName\")] for explicit control{Ansi.Reset}");
            }
            else
            {
                Log.LogMessage(MessageImportance.High, $"  {Ansi.Pass}✅ All files explicitly tagged{Ansi.Reset}");
            }
            
            // Report skipped files (filtered test runs)
            if (result.SkippedFiles > 0)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"  {Ansi.Warn}⏭️  {result.SkippedFiles} file(s) skipped (no coverage data in filtered test run){Ansi.Reset}");
            }
            
            Log.LogMessage(MessageImportance.High, $"{Ansi.Muted}─────────────────────────────────────────{Ansi.Reset}");

            if (result.Success)
            {
                Log.LogMessage(MessageImportance.High, 
                    $"{Ansi.Pass}{Ansi.Bold}✅ CoverBouncer: All {result.TotalFilesChecked} files passed coverage requirements{Ansi.Reset}");
                return true;
            }
            else
            {
                // Detailed breakdown via LogMessage (no project path suffix from MSBuild)
                Log.LogMessage(MessageImportance.High, "");
                Log.LogMessage(MessageImportance.High, $"{Ansi.Fail}{Ansi.Bold}❌ CoverBouncer: {result.Violations.Count} coverage violation(s) found{Ansi.Reset}");
                Log.LogMessage(MessageImportance.High, "");

                var byProfile = result.Violations.GroupBy(v => v.ProfileName).OrderBy(g => g.Key);
                foreach (var group in byProfile)
                {
                    var threshold = config.Profiles.TryGetValue(group.Key, out var t) ? t.MinLine : 0;
                    Log.LogMessage(MessageImportance.High, $"  {Ansi.Info}Profile: {group.Key}{Ansi.Reset} {Ansi.Muted}(requires {threshold:P0} line coverage){Ansi.Reset}");
                    
                    foreach (var violation in group.OrderBy(v => v.FilePath))
                    {
                        var fileName = GetRelativePath(violation.FilePath);
                        Log.LogMessage(MessageImportance.High, $"    {Ansi.Fail}❌ {Ansi.File}{fileName}{Ansi.Reset}: {Ansi.Fail}{violation.ActualCoverage:P1}{Ansi.Reset}{Ansi.Muted}/{Ansi.Reset}{Ansi.Threshold}{violation.RequiredCoverage:P0}{Ansi.Reset}");
                        
                        if (violation.UncoveredLines.Count > 0)
                        {
                            var ranges = CoverageViolation.FormatLineRanges(violation.UncoveredLines);
                            Log.LogMessage(MessageImportance.High, $"       {Ansi.Muted}Uncovered lines: {ranges}{Ansi.Reset}");
                        }
                    }
                    Log.LogMessage(MessageImportance.High, "");
                }

                // Actionable suggestions
                Log.LogMessage(MessageImportance.High, $"{Ansi.Warn}💡 How to fix:{Ansi.Reset}");
                Log.LogMessage(MessageImportance.High, $"   {Ansi.Muted}• Add tests to increase coverage for failing files{Ansi.Reset}");
                Log.LogMessage(MessageImportance.High, $"   {Ansi.Muted}• Or lower the threshold by tagging with a less strict profile:{Ansi.Reset}");
                Log.LogMessage(MessageImportance.High, $"     {Ansi.Muted}// [CoverageProfile(\"Standard\")]  // or \"Dto\" for 0% requirement{Ansi.Reset}");
                Log.LogMessage(MessageImportance.High, "");

                // Clean error summary — just "Errors:" header with compact file lines
                // MSBuild appends [project.csproj] to LogError, so keep it minimal
                var errorLines = new List<string> { "Errors:" };
                foreach (var violation in result.Violations.OrderBy(v => v.FilePath))
                {
                    var fileName = GetRelativePath(violation.FilePath);
                    errorLines.Add($"  {fileName}: {violation.ActualCoverage:P1}/{violation.RequiredCoverage:P0}");
                }
                Log.LogError(string.Join(Environment.NewLine, errorLines));

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

    /// <summary>
    /// Gets a relative path from ProjectDirectory, falling back to filename only.
    /// </summary>
    private string GetRelativePath(string filePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(ProjectDirectory) && filePath.StartsWith(ProjectDirectory, StringComparison.OrdinalIgnoreCase))
            {
                var relative = Path.GetRelativePath(ProjectDirectory, filePath);
                if (!string.IsNullOrEmpty(relative) && relative != ".")
                    return relative;
            }
            
            // Try going up one level (solution root is often one level above test project)
            var parentDir = Path.GetDirectoryName(ProjectDirectory);
            if (!string.IsNullOrEmpty(parentDir) && filePath.StartsWith(parentDir, StringComparison.OrdinalIgnoreCase))
            {
                var relative = Path.GetRelativePath(parentDir, filePath);
                if (!string.IsNullOrEmpty(relative) && relative != ".")
                    return relative;
            }
        }
        catch
        {
            // Fall through to filename
        }
        
        return Path.GetFileName(filePath);
    }
}
