using CoverBouncer.Coverlet;
using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;
using CoverBouncer.Core.Models;

namespace CoverBouncer.CLI;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return 0;
        }

        var command = args[0].ToLowerInvariant();
        
        return command switch
        {
            "init" => HandleInit(args.Skip(1).ToArray()),
            "verify" => HandleVerify(args.Skip(1).ToArray()),
            "tag" => HandleTag(args.Skip(1).ToArray()),
            "help" or "--help" or "-h" => ShowHelp(),
            _ => ShowHelp()
        };
    }

    static int ShowHelp()
    {
        Console.WriteLine("CoverBouncer - Profile-based code coverage enforcement");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  init [template]           Create a default coverbouncer.json");
        Console.WriteLine("                            Templates: basic (default), strict, relaxed");
        Console.WriteLine();
        Console.WriteLine("  verify                    Verify coverage against policy");
        Console.WriteLine("    --coverage, -c <path>   Coverage report path (default: TestResults/coverage.json)");
        Console.WriteLine("    --config, -f <path>     Config file path (default: coverbouncer.json)");
        Console.WriteLine("    --filtered              Treat as filtered test run (skip files with 0 coverage)");
        Console.WriteLine();
        Console.WriteLine("  tag                       Tag files with coverage profiles");
        Console.WriteLine("    --pattern <glob>        Tag files matching glob pattern (e.g., \"**/*Service.cs\")");
        Console.WriteLine("    --path <dir>            Tag all files in directory");
        Console.WriteLine("    --files <list>          Tag files listed in text file (one per line)");
        Console.WriteLine("    --profile <name>        Profile to apply (required)");
        Console.WriteLine("    --auto-suggest          Suggest profiles based on file patterns");
        Console.WriteLine("    --interactive           Interactive mode with prompts");
        Console.WriteLine("    --dry-run               Show what would happen without modifying files");
        Console.WriteLine("    --backup                Create backup files before tagging");
        Console.WriteLine();
        Console.WriteLine("  help                      Show this help");
        return 0;
    }

    static int HandleInit(string[] args)
    {
        try
        {
            var template = args.Length > 0 ? args[0] : "basic";
            var output = "coverbouncer.json";

            // Parse options
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--output" || args[i] == "-o")
                {
                    if (i + 1 < args.Length)
                    {
                        output = args[i + 1];
                        i++;
                    }
                }
            }

            if (!new[] { "basic", "strict", "relaxed" }.Contains(template.ToLowerInvariant()))
            {
                Console.WriteLine($"❌ Invalid template: {template}");
                Console.WriteLine("   Valid templates: basic, strict, relaxed");
                return 1;
            }

            if (File.Exists(output))
            {
                Console.WriteLine($"❌ Config file already exists: {output}");
                Console.WriteLine("   Delete it first or use a different output path.");
                return 1;
            }

            ConfigurationGenerator.GenerateTemplateFile(template, output);
            
            Console.WriteLine($"✅ Created {output} with '{template}' template");
            Console.WriteLine();
            Console.WriteLine("Built-in profiles:");
            Console.WriteLine("  • NoCoverage (0% - default, no requirements)");
            Console.WriteLine("  • Standard (60% line coverage)");
            Console.WriteLine("  • BusinessLogic (80% line coverage)");
            Console.WriteLine("  • Critical (100% line coverage)");
            Console.WriteLine("  • Dto (0% - for data objects)");
            Console.WriteLine();
            Console.WriteLine("ℹ️  Default is NoCoverage - your build won't fail until you tag files.");
            Console.WriteLine("   Tag files with: // [CoverageProfile(\"Standard\")]");
            Console.WriteLine();
            
            // Check for Coverlet and offer to configure exclusions
            OfferCoverletConfiguration();
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to create config: {ex.Message}");
            return 1;
        }
    }

    static int HandleVerify(string[] args)
    {
        try
        {
            var coverage = "TestResults/coverage.json";
            var config = "coverbouncer.json";
            var isFilteredRun = false;

            // Parse options
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--coverage" || args[i] == "-c")
                {
                    if (i + 1 < args.Length)
                    {
                        coverage = args[i + 1];
                        i++;
                    }
                }
                else if (args[i] == "--config" || args[i] == "-f")
                {
                    if (i + 1 < args.Length)
                    {
                        config = args[i + 1];
                        i++;
                    }
                }
                else if (args[i] == "--filtered")
                {
                    isFilteredRun = true;
                }
            }

            // Load configuration
            var policyConfig = ConfigurationLoader.LoadSmart(config);
            
            // Parse coverage report
            var parser = new CoverletReportParser();
            var coverageReport = parser.ParseFile(coverage);
            
            // Read profile tags from source files and track tagging stats
            var tagReader = new FileTagReader();
            var taggedFiles = new Dictionary<string, List<FileCoverage>>();
            var untaggedFiles = new List<FileCoverage>();
            
            foreach (var (filePath, fileCoverage) in coverageReport.Files)
            {
                var tag = tagReader.ReadProfileTag(filePath);
                fileCoverage.AssignedProfile = tag;
                
                var effectiveProfile = tag ?? policyConfig.DefaultProfile;
                
                if (tag == null)
                {
                    untaggedFiles.Add(fileCoverage);
                }
                
                if (!taggedFiles.ContainsKey(effectiveProfile))
                {
                    taggedFiles[effectiveProfile] = new List<FileCoverage>();
                }
                taggedFiles[effectiveProfile].Add(fileCoverage);
            }
            
            // Validate
            var engine = new PolicyEngine();
            if (isFilteredRun)
            {
                Console.WriteLine("ℹ️  Filtered test run mode: files with zero coverage will be skipped");
            }
            var result = engine.Validate(policyConfig, coverageReport, isFilteredRun);
            
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
                var threshold = policyConfig.Profiles.TryGetValue(profileName, out var t) ? t.MinLine : 0;
                var violations = result.Violations.Where(v => v.ProfileName == profileName).Select(v => v.FilePath).ToHashSet();
                var validatedFiles = files.Where(f => !skippedFilePaths.Contains(f.FilePath)).ToList();
                var passed = validatedFiles.Count(f => !violations.Contains(f.FilePath));
                var failed = validatedFiles.Count - passed;
                profileSummary[profileName] = (passed, failed, threshold);
            }

            // Output summary
            Console.WriteLine();
            Console.WriteLine("Coverage Summary by Profile");
            Console.WriteLine("─────────────────────────────────────────");
            
            foreach (var (profileName, (passed, failed, threshold)) in profileSummary.Where(p => p.Value.passed + p.Value.failed > 0).OrderBy(p => p.Key))
            {
                var status = failed == 0 ? "✅" : "❌";
                var thresholdStr = threshold == 0 ? "exempt" : $"{threshold:P0} required";
                var isDefault = profileName == policyConfig.DefaultProfile;
                var defaultMarker = isDefault ? " (default)" : "";
                
                Console.WriteLine($"  {status} {profileName}{defaultMarker}: {passed} passed, {failed} failed ({thresholdStr})");
            }
            
            Console.WriteLine();
            if (untaggedFiles.Count > 0)
            {
                Console.WriteLine($"  ℹ️  {untaggedFiles.Count} file(s) untagged → using '{policyConfig.DefaultProfile}' profile");
                Console.WriteLine($"     Tip: Tag files with // [CoverageProfile(\"ProfileName\")] for explicit control");
            }
            else
            {
                Console.WriteLine("  ✅ All files explicitly tagged");
            }
            
            // Report skipped files (filtered test runs)
            if (result.SkippedFiles > 0)
            {
                Console.WriteLine($"  ⏭️  {result.SkippedFiles} file(s) skipped (no coverage data in filtered test run)");
            }
            
            Console.WriteLine("─────────────────────────────────────────");
            Console.WriteLine();

            // Output results
            if (result.Success)
            {
                Console.WriteLine($"✅ All {result.TotalFilesChecked} files passed coverage requirements");
                return 0;
            }
            else
            {
                Console.WriteLine($"❌ {result.Violations.Count} coverage violation(s) found");
                Console.WriteLine();
                
                // Group violations by profile
                var byProfile = result.Violations.GroupBy(v => v.ProfileName);
                
                foreach (var group in byProfile.OrderBy(g => g.Key))
                {
                    var threshold = policyConfig.Profiles.TryGetValue(group.Key, out var t) ? t.MinLine : 0;
                    Console.WriteLine($"  Profile: {group.Key} (requires {threshold:P0} line coverage)");
                    
                    foreach (var violation in group.OrderBy(v => v.FilePath))
                    {
                        var fileName = Path.GetFileName(violation.FilePath);
                        var gap = violation.RequiredCoverage - violation.ActualCoverage;
                        Console.WriteLine($"    ❌ {fileName}: {violation.ActualCoverage:P1} coverage (need {gap:P1} more)");
                    }
                    Console.WriteLine();
                }

                // Actionable suggestions
                Console.WriteLine("💡 How to fix:");
                Console.WriteLine("   • Add tests to increase coverage for failing files");
                Console.WriteLine("   • Or lower the threshold by tagging with a less strict profile:");
                Console.WriteLine("     // [CoverageProfile(\"Standard\")]  // or \"Dto\" for 0% requirement");
                Console.WriteLine();
                
                return 1;
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"❌ File not found: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Verification failed: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   {ex.InnerException.Message}");
            }
            return 1;
        }
    }
    
    static int HandleTag(string[] args)
    {
        try
        {
            string? pattern = null;
            string? path = null;
            string? filesListPath = null;
            string? profile = null;
            bool autoSuggest = false;
            bool interactive = false;
            bool dryRun = false;
            bool backup = false;

            // Parse options
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--pattern":
                        if (i + 1 < args.Length) pattern = args[++i];
                        break;
                    case "--path":
                        if (i + 1 < args.Length) path = args[++i];
                        break;
                    case "--files":
                        if (i + 1 < args.Length) filesListPath = args[++i];
                        break;
                    case "--profile":
                        if (i + 1 < args.Length) profile = args[++i];
                        break;
                    case "--auto-suggest":
                        autoSuggest = true;
                        break;
                    case "--interactive":
                        interactive = true;
                        break;
                    case "--dry-run":
                        dryRun = true;
                        break;
                    case "--backup":
                        backup = true;
                        break;
                }
            }

            var taggingService = new FileTaggingService();
            var baseDirectory = Directory.GetCurrentDirectory();

            // Interactive mode
            if (interactive)
            {
                return HandleInteractiveTagging(taggingService, baseDirectory);
            }

            // Auto-suggest mode
            if (autoSuggest)
            {
                return HandleAutoSuggest(taggingService, baseDirectory, dryRun, backup);
            }

            // Validate that profile is specified for non-interactive modes
            if (string.IsNullOrEmpty(profile))
            {
                Console.WriteLine("❌ --profile is required (or use --interactive or --auto-suggest)");
                return 1;
            }

            // Load configuration to validate profile exists
            var config = TryLoadConfiguration();
            if (config != null && !config.Profiles.ContainsKey(profile))
            {
                Console.WriteLine($"⚠️  Warning: Profile '{profile}' not found in coverbouncer.json");
                Console.WriteLine("   Available profiles: " + string.Join(", ", config.Profiles.Keys));
                Console.WriteLine();
            }

            FileTaggingService.TaggingResult result;

            // Execute tagging based on mode
            if (!string.IsNullOrEmpty(pattern))
            {
                Console.WriteLine($"🏷️  Tagging files matching pattern: {pattern}");
                Console.WriteLine($"   Profile: {profile}");
                if (dryRun) Console.WriteLine("   [DRY RUN - No files will be modified]");
                Console.WriteLine();

                result = taggingService.TagByPattern(baseDirectory, pattern, profile, backup, dryRun);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine($"🏷️  Tagging files in directory: {path}");
                Console.WriteLine($"   Profile: {profile}");
                if (dryRun) Console.WriteLine("   [DRY RUN - No files will be modified]");
                Console.WriteLine();

                result = taggingService.TagByDirectory(path, profile, recursive: true, backup, dryRun);
            }
            else if (!string.IsNullOrEmpty(filesListPath))
            {
                Console.WriteLine($"🏷️  Tagging files from list: {filesListPath}");
                Console.WriteLine($"   Profile: {profile}");
                if (dryRun) Console.WriteLine("   [DRY RUN - No files will be modified]");
                Console.WriteLine();

                var files = FileTaggingService.ReadFileList(filesListPath);
                result = taggingService.TagFiles(files, profile, backup, dryRun);
            }
            else
            {
                Console.WriteLine("❌ Please specify --pattern, --path, --files, --interactive, or --auto-suggest");
                return 1;
            }

            // Display results
            DisplayTaggingResults(result, dryRun);

            return result.FilesErrored > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Tagging failed: {ex.Message}");
            return 1;
        }
    }

    static int HandleInteractiveTagging(FileTaggingService taggingService, string baseDirectory)
    {
        Console.WriteLine("🎨 Interactive Tagging Mode");
        Console.WriteLine();

        // Step 1: Select pattern
        Console.WriteLine("Enter file pattern to tag (e.g., **/*Service.cs, ./Security/**/*.cs):");
        Console.Write("> ");
        var pattern = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(pattern))
        {
            Console.WriteLine("❌ Pattern is required");
            return 1;
        }

        // Preview matching files
        var matcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher();
        matcher.AddInclude(pattern);
        var matchResult = matcher.Execute(
            new Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper(
                new DirectoryInfo(baseDirectory)));
        
        var matchedFiles = matchResult.Files
            .Select(f => Path.Combine(baseDirectory, f.Path))
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Found {matchedFiles.Count} files matching pattern:");
        foreach (var file in matchedFiles.Take(10))
        {
            Console.WriteLine($"  • {Path.GetRelativePath(baseDirectory, file)}");
        }
        if (matchedFiles.Count > 10)
        {
            Console.WriteLine($"  ... and {matchedFiles.Count - 10} more");
        }
        Console.WriteLine();

        // Step 2: Select profile
        var config = TryLoadConfiguration();
        var profiles = config?.Profiles.Keys.ToList() ?? new List<string> { "Standard", "Critical", "BusinessLogic", "Dto" };

        Console.WriteLine("Available profiles:");
        for (int i = 0; i < profiles.Count; i++)
        {
            var profileName = profiles[i];
            var threshold = config?.Profiles.GetValueOrDefault(profileName)?.MinLine;
            var thresholdStr = threshold.HasValue ? $"({threshold.Value:P0} line coverage)" : "";
            Console.WriteLine($"  {i + 1}. {profileName} {thresholdStr}");
        }
        Console.WriteLine();
        Console.Write("Select profile (number or name): ");
        var profileInput = Console.ReadLine()?.Trim();

        string? selectedProfile = null;
        if (int.TryParse(profileInput, out var profileIndex) && profileIndex > 0 && profileIndex <= profiles.Count)
        {
            selectedProfile = profiles[profileIndex - 1];
        }
        else if (!string.IsNullOrEmpty(profileInput))
        {
            selectedProfile = profileInput;
        }

        if (string.IsNullOrEmpty(selectedProfile))
        {
            Console.WriteLine("❌ Profile is required");
            return 1;
        }

        Console.WriteLine();
        Console.WriteLine($"Selected profile: {selectedProfile}");
        Console.WriteLine();

        // Step 3: Confirm
        Console.Write("Apply changes? (Y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm == "n" || confirm == "no")
        {
            Console.WriteLine("Cancelled.");
            return 0;
        }

        // Execute tagging
        var result = taggingService.TagFiles(matchedFiles, selectedProfile, createBackup: false, dryRun: false);
        DisplayTaggingResults(result, dryRun: false);

        return result.FilesErrored > 0 ? 1 : 0;
    }

    static int HandleAutoSuggest(FileTaggingService taggingService, string baseDirectory, bool dryRun, bool backup)
    {
        Console.WriteLine("🤖 Auto-Suggest Mode");
        Console.WriteLine();
        Console.WriteLine("Analyzing project files...");
        Console.WriteLine();

        // Find all .cs files
        var allFiles = Directory.GetFiles(baseDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
            .ToList();

        var suggestions = taggingService.SuggestProfiles(allFiles);
        var grouped = suggestions.GroupBy(kvp => kvp.Value);

        Console.WriteLine("📋 Suggested Profile Assignments:");
        Console.WriteLine();

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            Console.WriteLine($"Profile: {group.Key} ({group.Count()} files)");
            foreach (var file in group.Take(5))
            {
                Console.WriteLine($"  • {Path.GetRelativePath(baseDirectory, file.Key)}");
            }
            if (group.Count() > 5)
            {
                Console.WriteLine($"  ... and {group.Count() - 5} more");
            }
            Console.WriteLine();
        }

        Console.Write("Apply these suggestions? (Y/n): ");
        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm == "n" || confirm == "no")
        {
            Console.WriteLine("Cancelled.");
            return 0;
        }

        // Apply suggestions by profile
        var totalTagged = 0;
        var totalSkipped = 0;
        var totalErrors = 0;

        foreach (var group in grouped)
        {
            var files = group.Select(kvp => kvp.Key).ToList();
            var result = taggingService.TagFiles(files, group.Key, backup, dryRun);
            
            totalTagged += result.FilesTagged;
            totalSkipped += result.FilesSkipped;
            totalErrors += result.FilesErrored;
        }

        Console.WriteLine();
        if (dryRun)
        {
            Console.WriteLine("✅ [DRY RUN] Would tag files:");
        }
        else
        {
            Console.WriteLine("✅ Tagging complete:");
        }
        Console.WriteLine($"   Tagged: {totalTagged}");
        Console.WriteLine($"   Skipped: {totalSkipped} (already tagged)");
        if (totalErrors > 0)
        {
            Console.WriteLine($"   Errors: {totalErrors}");
        }

        return totalErrors > 0 ? 1 : 0;
    }

    static void DisplayTaggingResults(FileTaggingService.TaggingResult result, bool dryRun)
    {
        if (result.FilesTagged > 0)
        {
            if (dryRun)
            {
                Console.WriteLine($"✅ [DRY RUN] Would tag {result.FilesTagged} file(s):");
            }
            else
            {
                Console.WriteLine($"✅ Tagged {result.FilesTagged} file(s):");
            }
            
            foreach (var file in result.TaggedFiles.Take(10))
            {
                Console.WriteLine($"   • {Path.GetFileName(file)}");
            }
            if (result.TaggedFiles.Count > 10)
            {
                Console.WriteLine($"   ... and {result.TaggedFiles.Count - 10} more");
            }
            Console.WriteLine();
        }

        if (result.FilesSkipped > 0)
        {
            Console.WriteLine($"ℹ️  Skipped {result.FilesSkipped} file(s) (already tagged with same profile)");
            Console.WriteLine();
        }

        if (result.FilesErrored > 0)
        {
            Console.WriteLine($"❌ Failed to tag {result.FilesErrored} file(s):");
            foreach (var (file, error) in result.Errors)
            {
                Console.WriteLine($"   • {Path.GetFileName(file)}: {error}");
            }
            Console.WriteLine();
        }

        if (result.FilesMatched > 0)
        {
            Console.WriteLine($"Summary: {result.FilesTagged} tagged, {result.FilesSkipped} skipped, {result.FilesErrored} errors");
        }
    }

    static PolicyConfiguration? TryLoadConfiguration()
    {
        try
        {
            if (File.Exists("coverbouncer.json"))
            {
                return ConfigurationLoader.LoadSmart("coverbouncer.json");
            }
        }
        catch
        {
            // Ignore errors, return null
        }
        return null;
    }
    
    /// <summary>
    /// Checks for Coverlet usage and offers to configure exclusions
    /// </summary>
    static void OfferCoverletConfiguration()
    {
        // Check if Coverlet is being used
        if (!IsCoverletDetected())
        {
            return;
        }
        
        Console.WriteLine("ℹ️  Coverlet detected!");
        Console.WriteLine();
        Console.WriteLine("💡 Recommendation: Exclude test frameworks from coverage to improve performance");
        Console.WriteLine("   and reduce noise. CoverBouncer assemblies are already excluded automatically.");
        Console.WriteLine();
        Console.WriteLine("Would you like to add recommended exclusions to Directory.Build.props? (Y/n): ");
        
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        
        if (response != "y" && response != "yes" && response != "")
        {
            Console.WriteLine("Skipped. You can manually configure exclusions later if needed.");
            return;
        }
        
        try
        {
            AddCoverletExclusions();
            Console.WriteLine("✅ Added Coverlet exclusions to Directory.Build.props");
            Console.WriteLine();
            Console.WriteLine("Excluded assemblies:");
            Console.WriteLine("  • [xunit.*]* (xUnit test framework)");
            Console.WriteLine("  • [FluentAssertions]*");
            Console.WriteLine("  • [Moq]*");
            Console.WriteLine("  • [NSubstitute]*");
            Console.WriteLine();
            Console.WriteLine("Note: CoverBouncer automatically excludes [CoverBouncer.*]* via MSBuild targets.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Could not update Directory.Build.props: {ex.Message}");
            Console.WriteLine("   You can manually add exclusions if needed.");
        }
    }
    
    /// <summary>
    /// Detects if Coverlet is being used in the current project
    /// </summary>
    static bool IsCoverletDetected()
    {
        // Look for .csproj files in current directory
        var csprojFiles = Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);
        
        foreach (var csprojFile in csprojFiles)
        {
            try
            {
                var content = File.ReadAllText(csprojFile);
                
                // Check for Coverlet package references or CollectCoverage property
                if (content.Contains("coverlet.msbuild", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("coverlet.collector", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("<CollectCoverage>true</CollectCoverage>", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch
            {
                // Ignore file read errors
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Adds recommended Coverlet exclusions to Directory.Build.props
    /// </summary>
    static void AddCoverletExclusions()
    {
        const string directoryBuildPropsPath = "Directory.Build.props";
        
        // Create Directory.Build.props if it doesn't exist
        if (!File.Exists(directoryBuildPropsPath))
        {
            var newContent = @"<Project>
  <PropertyGroup>
    <!-- Coverlet exclusions: Exclude test frameworks and mocking libraries from coverage -->
    <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
  </PropertyGroup>
</Project>";
            File.WriteAllText(directoryBuildPropsPath, newContent);
            return;
        }
        
        // Update existing file
        var content = File.ReadAllText(directoryBuildPropsPath);
        
        // Check if exclusions already exist
        if (content.Contains("[xunit.*]*", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("<Exclude>", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("ℹ️  Exclusions already configured in Directory.Build.props");
            return;
        }
        
        // Add exclusions
        var exclusionsBlock = @"
  <PropertyGroup>
    <!-- Coverlet exclusions: Exclude test frameworks and mocking libraries from coverage -->
    <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
  </PropertyGroup>";
        
        // Insert before closing </Project> tag
        if (content.Contains("</Project>", StringComparison.OrdinalIgnoreCase))
        {
            var lastIndex = content.LastIndexOf("</Project>", StringComparison.OrdinalIgnoreCase);
            content = content.Insert(lastIndex, exclusionsBlock + "\n");
            File.WriteAllText(directoryBuildPropsPath, content);
        }
        else
        {
            throw new InvalidOperationException("Invalid Directory.Build.props format");
        }
    }
}
