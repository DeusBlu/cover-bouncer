using CoverBouncer.Core.Adapters;
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
            var policyConfig = ConfigurationLoader.LoadFromFileOrParent(config);
            
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
}
