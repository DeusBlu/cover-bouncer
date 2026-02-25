using CoverBouncer.Core.Engine;
using CoverBouncer.Core.Models;
using Xunit;

namespace CoverBouncer.Core.Tests.Engine;

/// <summary>
/// Tests for the PolicyEngine, including filtered test run behavior.
/// </summary>
public class PolicyEngineTests
{
    private readonly PolicyEngine _engine = new();

    private static PolicyConfiguration CreateConfig(
        string defaultProfile = "NoCoverage",
        Dictionary<string, ProfileThresholds>? profiles = null)
    {
        profiles ??= new Dictionary<string, ProfileThresholds>
        {
            ["Critical"] = new() { MinLine = 1.0m },
            ["Standard"] = new() { MinLine = 0.6m },
            ["Dto"] = new() { MinLine = 0.0m },
            ["NoCoverage"] = new() { MinLine = 0.0m }
        };

        return new PolicyConfiguration
        {
            DefaultProfile = defaultProfile,
            Profiles = profiles
        };
    }

    private static CoverageReport CreateReport(params FileCoverage[] files)
    {
        var report = new CoverageReport();
        foreach (var file in files)
        {
            report.Files[file.FilePath] = file;
        }
        return report;
    }

    // ──────────────────────────────────────────────
    // Full run tests (isFilteredTestRun = false)
    // ──────────────────────────────────────────────

    [Fact]
    public void FullRun_FileWithZeroCoverage_AndNonZeroThreshold_Fails()
    {
        // A file with 0% coverage and a "Standard" (60%) threshold should FAIL on full run
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/MyService.cs",
            LineRate = 0m,
            TotalLines = 10,
            CoveredLines = 0,
            AssignedProfile = "Standard"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: false);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(1, result.TotalFilesChecked);
    }

    [Fact]
    public void FullRun_FileWithZeroCoverage_AndZeroThreshold_Passes()
    {
        // A Dto file with 0% coverage and 0% threshold should PASS on full run
        var config = CreateConfig();
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/OrderDto.cs",
            LineRate = 0m,
            TotalLines = 5,
            CoveredLines = 0,
            AssignedProfile = "Dto"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: false);

        Assert.True(result.Success);
        Assert.Empty(result.Violations);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(1, result.TotalFilesChecked);
    }

    [Fact]
    public void FullRun_AllFilesValidated_NoneSkipped()
    {
        // On a full run, ALL files are validated, even those with 0 covered lines
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(
            new FileCoverage
            {
                FilePath = "/src/TestedService.cs",
                LineRate = 0.8m,
                TotalLines = 10,
                CoveredLines = 8,
                AssignedProfile = "Standard"
            },
            new FileCoverage
            {
                FilePath = "/src/UntestedService.cs",
                LineRate = 0m,
                TotalLines = 10,
                CoveredLines = 0,
                AssignedProfile = "Standard"
            });

        var result = _engine.Validate(config, report, isFilteredTestRun: false);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal("/src/UntestedService.cs", result.Violations[0].FilePath);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(2, result.TotalFilesChecked);
    }

    // ──────────────────────────────────────────────
    // Filtered run tests (isFilteredTestRun = true)
    // ──────────────────────────────────────────────

    [Fact]
    public void FilteredRun_FileWithZeroCoveredLines_IsSkipped()
    {
        // A file with 0 covered lines should be SKIPPED on filtered run
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/NotTargeted.cs",
            LineRate = 0m,
            TotalLines = 10,
            CoveredLines = 0,
            AssignedProfile = "Standard"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Empty(result.Violations);
        Assert.Equal(1, result.SkippedFiles);
        Assert.Equal(0, result.TotalFilesChecked);
    }

    [Fact]
    public void FilteredRun_FileWithCoverage_StillValidated()
    {
        // A file with actual coverage is validated normally even on filtered run
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/TestedService.cs",
            LineRate = 0.4m,
            TotalLines = 10,
            CoveredLines = 4,
            AssignedProfile = "Standard"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(1, result.TotalFilesChecked);
    }

    [Fact]
    public void FilteredRun_MixOfTargetedAndUntargeted_OnlyTargetedValidated()
    {
        // Mix: one file with coverage (validated), one without (skipped)
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(
            new FileCoverage
            {
                FilePath = "/src/TestedService.cs",
                LineRate = 0.8m,
                TotalLines = 10,
                CoveredLines = 8,
                AssignedProfile = "Standard"
            },
            new FileCoverage
            {
                FilePath = "/src/NotTargeted.cs",
                LineRate = 0m,
                TotalLines = 10,
                CoveredLines = 0,
                AssignedProfile = "Standard"
            });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Empty(result.Violations);
        Assert.Equal(1, result.SkippedFiles);
        Assert.Equal(1, result.TotalFilesChecked);
    }

    [Fact]
    public void FilteredRun_DtoFileWithZeroCoverage_IsSkipped_NotValidated()
    {
        // Even a Dto file with 0% is skipped on filtered run — it would pass anyway,
        // but it's not counted in the checked total since it wasn't targeted
        var config = CreateConfig();
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/OrderDto.cs",
            LineRate = 0m,
            TotalLines = 5,
            CoveredLines = 0,
            AssignedProfile = "Dto"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Empty(result.Violations);
        Assert.Equal(1, result.SkippedFiles);
        Assert.Equal(0, result.TotalFilesChecked);
    }

    [Fact]
    public void FilteredRun_FileWithPartialCoverage_BelowThreshold_Fails()
    {
        // A file that WAS targeted (has some coverage) but below threshold still fails
        var config = CreateConfig();
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/PaymentService.cs",
            LineRate = 0.5m,
            TotalLines = 10,
            CoveredLines = 5,
            AssignedProfile = "Critical"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal(0.5m, result.Violations[0].ActualCoverage);
        Assert.Equal(1.0m, result.Violations[0].RequiredCoverage);
    }

    [Fact]
    public void FilteredRun_MultipleUntargetedFiles_AllSkipped()
    {
        // Simulates a filtered run where most files in the assembly weren't targeted
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(
            new FileCoverage
            {
                FilePath = "/src/TargetedService.cs",
                LineRate = 0.9m,
                TotalLines = 10,
                CoveredLines = 9,
                AssignedProfile = "Standard"
            },
            new FileCoverage
            {
                FilePath = "/src/OtherService1.cs",
                LineRate = 0m,
                TotalLines = 20,
                CoveredLines = 0,
                AssignedProfile = "Standard"
            },
            new FileCoverage
            {
                FilePath = "/src/OtherService2.cs",
                LineRate = 0m,
                TotalLines = 15,
                CoveredLines = 0,
                AssignedProfile = "Critical"
            },
            new FileCoverage
            {
                FilePath = "/src/OtherDto.cs",
                LineRate = 0m,
                TotalLines = 5,
                CoveredLines = 0,
                AssignedProfile = "Dto"
            });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Empty(result.Violations);
        Assert.Equal(3, result.SkippedFiles);
        Assert.Equal(1, result.TotalFilesChecked);
    }

    // ──────────────────────────────────────────────
    // Default parameter (backward compatibility)
    // ──────────────────────────────────────────────

    [Fact]
    public void DefaultParameter_IsFullRun_ValidatesAllFiles()
    {
        // Calling Validate without isFilteredTestRun defaults to full run behavior
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/UntestedService.cs",
            LineRate = 0m,
            TotalLines = 10,
            CoveredLines = 0,
            AssignedProfile = "Standard"
        });

        // No isFilteredTestRun parameter — should default to false (full run)
        var result = _engine.Validate(config, report);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal(0, result.SkippedFiles);
    }

    // ──────────────────────────────────────────────
    // Edge cases
    // ──────────────────────────────────────────────

    [Fact]
    public void FilteredRun_FileWithZeroTotalLines_IsSkipped()
    {
        // Edge case: a file with 0 total lines and 0 covered lines
        var config = CreateConfig(defaultProfile: "Standard");
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/Empty.cs",
            LineRate = 0m,
            TotalLines = 0,
            CoveredLines = 0,
            AssignedProfile = "Standard"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Equal(1, result.SkippedFiles);
    }

    [Fact]
    public void FilteredRun_EmptyReport_Succeeds()
    {
        var config = CreateConfig();
        var report = CreateReport();

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.True(result.Success);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(0, result.TotalFilesChecked);
    }

    [Fact]
    public void FullRun_EmptyReport_Succeeds()
    {
        var config = CreateConfig();
        var report = CreateReport();

        var result = _engine.Validate(config, report, isFilteredTestRun: false);

        Assert.True(result.Success);
        Assert.Equal(0, result.SkippedFiles);
        Assert.Equal(0, result.TotalFilesChecked);
    }

    [Fact]
    public void FilteredRun_FileWithExactlyOneCoveredLine_NotSkipped()
    {
        // A file with even 1 covered line was targeted — it should be validated
        var config = CreateConfig();
        var report = CreateReport(new FileCoverage
        {
            FilePath = "/src/BarelyCovered.cs",
            LineRate = 0.1m,
            TotalLines = 10,
            CoveredLines = 1,
            AssignedProfile = "Critical"
        });

        var result = _engine.Validate(config, report, isFilteredTestRun: true);

        Assert.False(result.Success);
        Assert.Single(result.Violations);
        Assert.Equal(0, result.SkippedFiles);
    }

    [Fact]
    public void ValidationResult_SkippedFiles_DefaultsToZero()
    {
        var result = new ValidationResult();
        Assert.Equal(0, result.SkippedFiles);
    }

    [Fact]
    public void ValidationResult_GetSummary_IncludesSkippedInfo()
    {
        var result = new ValidationResult
        {
            TotalFilesChecked = 5,
            SkippedFiles = 3
        };

        var summary = result.GetSummary();
        Assert.Contains("5 files", summary);
        Assert.Contains("3 skipped", summary);
        Assert.Contains("filtered run", summary);
    }

    [Fact]
    public void ValidationResult_GetSummary_OmitsSkippedWhenZero()
    {
        var result = new ValidationResult
        {
            TotalFilesChecked = 5,
            SkippedFiles = 0
        };

        var summary = result.GetSummary();
        Assert.DoesNotContain("skipped", summary);
    }

    [Fact]
    public void ValidationResult_FilesPassed_IsComputed_FromCheckedMinusFailed()
    {
        var result = new ValidationResult
        {
            TotalFilesChecked = 5,
            Violations = new List<CoverageViolation>
            {
                new() { FilePath = "/a.cs", ProfileName = "Standard", ViolationType = ViolationType.LineCoverageTooLow, RequiredCoverage = 0.6m, ActualCoverage = 0.3m },
                new() { FilePath = "/b.cs", ProfileName = "Critical", ViolationType = ViolationType.LineCoverageTooLow, RequiredCoverage = 1.0m, ActualCoverage = 0.5m }
            }
        };

        Assert.Equal(2, result.FilesFailed);
        Assert.Equal(3, result.FilesPassed); // 5 checked - 2 failed = 3 passed
    }
}
