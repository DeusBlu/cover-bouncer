using CoverBouncer.Coverlet;
using CoverBouncer.Core.Configuration;
using CoverBouncer.Core.Engine;

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
            Console.WriteLine("  • Critical (100% line coverage)");
            Console.WriteLine("  • BusinessLogic (80% line coverage)");
            Console.WriteLine("  • Standard (60% line coverage)");
            Console.WriteLine("  • Dto (0% - no requirements)");
            Console.WriteLine();
            Console.WriteLine("Tag files with: [CoverageProfile(\"ProfileName\")]");
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
            }

            // Load configuration
            var policyConfig = ConfigurationLoader.LoadSmart(config);
            
            // Parse coverage report
            var parser = new CoverletReportParser();
            var coverageReport = parser.ParseFile(coverage);
            
            // Read profile tags from source files
            var tagReader = new FileTagReader();
            foreach (var (filePath, fileCoverage) in coverageReport.Files)
            {
                fileCoverage.AssignedProfile = tagReader.ReadProfileTag(filePath);
            }
            
            // Validate
            var engine = new PolicyEngine();
            var result = engine.Validate(policyConfig, coverageReport);
            
            // Output results
            if (result.Success)
            {
                Console.WriteLine($"✅ All {result.TotalFilesChecked} files passed coverage requirements");
                return 0;
            }
            else
            {
                Console.WriteLine($"❌ Coverage policy violations found ({result.Violations.Count} of {result.TotalFilesChecked} files)");
                Console.WriteLine();
                
                // Group violations by profile
                var byProfile = result.Violations.GroupBy(v => v.ProfileName);
                
                foreach (var group in byProfile.OrderBy(g => g.Key))
                {
                    Console.WriteLine($"Profile: {group.Key}");
                    foreach (var violation in group.OrderBy(v => v.FilePath))
                    {
                        Console.WriteLine($"  {violation.FilePath}");
                        Console.WriteLine($"    Required: {violation.RequiredCoverage:P0}, Actual: {violation.ActualCoverage:P1}");
                    }
                    Console.WriteLine();
                }
                
                Console.WriteLine($"Summary: {result.FilesPassed} passed, {result.Violations.Count} failed");
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
