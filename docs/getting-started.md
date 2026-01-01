# Getting Started with Cover-Bouncer

This guide will help you set up Cover-Bouncer in your .NET project in just a few minutes.

## Prerequisites

- .NET 8.0 SDK or later
- A .NET project with tests
- Coverlet for coverage collection (or willingness to add it)

## Installation

### Step 1: Install the NuGet Package

In your test project, run:

```bash
dotnet add package CoverBouncer.MSBuild
```

This is the only package you need! It includes everything required for coverage policy enforcement.

### Step 2: Initialize Configuration

Generate a default configuration file:

```bash
dotnet coverbouncer init
```

This creates `coverbouncer.json` in your solution root with sensible defaults:

```json
{
  "coverageReportPath": "TestResults/coverage.json",
  "defaultProfile": "NoCoverage",
  "profiles": {
    "Critical": { "minLine": 1.00 },
    "BusinessLogic": { "minLine": 0.80 },
    "Standard": { "minLine": 0.60 },
    "Dto": { "minLine": 0.00 },
    "NoCoverage": { "minLine": 0.00 }
  }
}
```

> **Note:** The default is `NoCoverage` (0%) so your build won't fail on first install. Tag files with stricter profiles to opt-in to coverage enforcement.

**That's all the configuration you need!** No sprawl, no complexity.

## 🎨 Important: Profiles Are YOUR Choice!

**You are NOT limited to these profile names or percentages!** The templates above are just suggestions.

### Customize to Match Your Reality

Feel free to change profile names and thresholds to match your team's current state:

```json
{
  "defaultProfile": "WorkInProgress",
  "profiles": {
    "MustHaveTests": { "minLine": 0.45 },      // Start where you are!
    "TryingHarder": { "minLine": 0.30 },
    "SecurityStuff": { "minLine": 0.75 },      // Your own names!
    "LegacyCode": { "minLine": 0.10 },         // Be honest
    "NewCode": { "minLine": 0.80 },            // Higher bar for new work
    "EventuallyTest": { "minLine": 0.0 }
  }
}
```

### Start Where YOU Are:

- **Have 30% coverage now?** Start with 35% threshold
- **Have 60% coverage?** Maybe aim for 65-70%
- **Have 5% coverage?** Start at 10% and celebrate progress!
- **Inherited legacy code?** Create a "Legacy" profile with realistic goals

**The goal is IMPROVEMENT, not perfection!** 📈

Use profiles to:
- Set achievable goals that match your current coverage
- Gradually increase thresholds over time
- Treat new code differently than legacy code
- Reflect your team's priorities (security, business logic, etc.)

You can customize these thresholds based on your team's standards.

### Step 3: Enable Coverage Collection

Add the following to your test project's `.csproj` file or create a `Directory.Build.props` in your test folder:

```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
  <CoverletOutputFormat>json</CoverletOutputFormat>
  <EnableCoverBouncer>true</EnableCoverBouncer>
</PropertyGroup>
```

**Note:** If you don't have Coverlet installed yet, add it to your test project:

```bash
dotnet add package coverlet.msbuild
```

### Step 4: Tag Your Source Files (Optional!)

> **Untagged files use your `defaultProfile` automatically.** You can run CoverBouncer right now without any tags - all files will be checked against your default threshold. Add tags later only for files that need different thresholds.

You can tag files manually or use the automated CLI tools.

#### Option A: Manual Tagging

Add profile tags to your source files using comments at the top:

**Critical business logic:**
```csharp
// [CoverageProfile("Critical")]
namespace MyApp.PaymentProcessing
{
    public class PaymentService
    {
        // This class requires 100% coverage
    }
}
```

**DTOs that don't need testing:**
```csharp
// [CoverageProfile("Dto")]
namespace MyApp.Models
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
```

**Standard business logic:**
```csharp
// [CoverageProfile("BusinessLogic")]
namespace MyApp.Services
{
    public class CustomerService
    {
        // This class requires 90% coverage
    }
}
```

Files without tags will use the `defaultProfile` (Standard).

#### Option B: Automated Tagging with CLI

For bulk tagging, use the CLI tool. This is **much faster** for large projects!

**1. Interactive Mode** (Recommended for beginners):
```bash
dotnet coverbouncer tag --interactive

# Prompts you to:
# 1. Enter file pattern (e.g., **/*Service.cs)
# 2. Choose profile from list
# 3. Preview matching files
# 4. Confirm changes
```

**2. Auto-Suggest Mode** (Smart pattern detection):
```bash
dotnet coverbouncer tag --auto-suggest

# Automatically suggests profiles based on:
# - *Controller.cs → Integration
# - *Service.cs → BusinessLogic
# - Security/* → Critical
# - Models/* → Dto
# Then asks for confirmation
```

**3. Batch Mode** (For scripts and automation):
```bash
# Tag all service files
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic

# Tag entire directory
dotnet coverbouncer tag --path "./Security" --profile Critical

# Tag from file list
dotnet coverbouncer tag --files services.txt --profile Standard

# Preview without changing files
dotnet coverbouncer tag --pattern "**/*.cs" --profile Standard --dry-run

# Create backups before tagging
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile Standard --backup
```

**4. Example Workflow:**
```bash
# Start with auto-suggest to tag most files
dotnet coverbouncer tag --auto-suggest

# Then manually adjust specific critical areas
dotnet coverbouncer tag --path "./Security" --profile Critical

# Preview what would change
dotnet coverbouncer tag --pattern "**/*Controller.cs" --profile Integration --dry-run

# Apply it
dotnet coverbouncer tag --pattern "**/*Controller.cs" --profile Integration
```

### Step 5: Run Your Tests

That's it! Just run your tests as normal:

```bash
dotnet test
```

Cover-Bouncer will automatically:
1. Collect coverage with Coverlet
2. Validate coverage against your policies
3. Fail the build if any files violate their thresholds

## Example Output

### Success (No Violations)
```
✓ All files meet coverage requirements
  - PaymentService.cs (Critical): 100% line coverage ✓
  - CustomerService.cs (BusinessLogic): 92% line coverage ✓
  - OrderService.cs (Standard): 75% line coverage ✓
```

### Failure (Violations Found)
```
✗ Coverage policy violations detected:

  PaymentService.cs (Critical)
    Required: 100% line coverage
    Actual:   85% line coverage
    Missing:  15 lines not covered

  CustomerService.cs (BusinessLogic)
    Required: 90% line coverage
    Actual:   78% line coverage
    Missing:  22 lines not covered

Build FAILED.
```

## CI/CD Integration

### GitHub Actions

Your existing test workflow already works! Just ensure the MSBuild properties are set:

```yaml
name: CI

on:
  pull_request:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      
      - name: Run tests
        run: dotnet test
        # Coverage policy automatically enforced!
```

**Set as Required Check:**
1. Go to repository Settings → Branches
2. Add branch protection rule for `main`
3. Require "test" job to pass before merging

Now PRs with coverage violations can't be merged! 🎉

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Run tests with coverage'
  inputs:
    command: 'test'
```

Add this job as a required policy in branch policies.

### GitLab CI

```yaml
test:
  stage: test
  script:
    - dotnet test
  rules:
    - if: '$CI_PIPELINE_SOURCE == "merge_request_event"'
```

## Advanced Configuration

### Custom Report Paths

If your coverage report is in a different location:

```json
{
  "coverageReportPath": "coverage/results/coverage.json",
  "profiles": { ... }
}
```

### Disable in Local Development

Add to your `.csproj` or user-specific settings:

```xml
<PropertyGroup>
  <EnableCoverBouncer Condition="'$(CI)' != 'true'">false</EnableCoverBouncer>
</PropertyGroup>
```

This disables enforcement locally but keeps it enabled in CI.

### Multiple Profiles

You can define as many profiles as you need:

```json
{
  "profiles": {
    "Critical": { "minLine": 1.00 },
    "High": { "minLine": 0.90 },
    "Medium": { "minLine": 0.70 },
    "Low": { "minLine": 0.50 },
    "Experimental": { "minLine": 0.00 },
    "Dto": { "minLine": 0.00 }
  }
}
```

## Troubleshooting

### "Coverage report not found"

Make sure:
1. Coverlet is installed: `dotnet add package coverlet.msbuild`
2. Coverage collection is enabled in your `.csproj`
3. The `CoverletOutput` path matches `coverageReportPath` in config

### "No profile tag found"

Files without tags use the `defaultProfile`. This is normal! Only tag files that need special thresholds.

### "Build passes locally but fails in CI"

Check that:
1. `coverbouncer.json` is committed to source control
2. MSBuild properties are in a committed file (not user-specific `.user` files)
3. Coverlet is properly configured

### Coverlet Warnings about CoverBouncer Assemblies

**This is fixed automatically!** CoverBouncer v1.0.0-preview.1 and later automatically exclude CoverBouncer assemblies from Coverlet instrumentation.

If you're seeing warnings like:
```
Instrumentation of assembly 'CoverBouncer.Core' failed
Unable to find module or source files
```

**Solution:** Update to the latest version of CoverBouncer.MSBuild. The exclusions are applied automatically via `buildTransitive` targets.

**Manual workaround** (if needed):
```xml
<PropertyGroup>
  <Exclude>$(Exclude);[CoverBouncer.*]*</Exclude>
</PropertyGroup>
```

## Coverlet Best Practices

### Automatic Test Framework Exclusions

When you run `dotnet coverbouncer init`, you'll be prompted to exclude common test frameworks from coverage. This is **recommended** for:

- **Better performance**: Faster test execution
- **Cleaner reports**: Focus on your actual code
- **No warnings**: Eliminates missing symbols warnings

The init command will offer to add:
```xml
<PropertyGroup>
  <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
</PropertyGroup>
```

### Include Only Production Code

For maximum control, you can explicitly include only your production assemblies:

```xml
<PropertyGroup>
  <!-- Only collect coverage on MyApp assemblies -->
  <Include>[MyApp]*;[MyApp.Core]*;[MyApp.Services]*</Include>
</PropertyGroup>
```

This approach:
- ✅ Ensures test code is never instrumented
- ✅ Improves performance significantly
- ✅ Makes coverage reports crystal clear

### Recommended Configuration

For most projects, use this pattern in `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup Condition="'$(IsTestProject)' == 'true'">
    <!-- Enable Coverlet -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
    <CoverletOutputFormat>json</CoverletOutputFormat>
    
    <!-- Enable CoverBouncer -->
    <EnableCoverBouncer>true</EnableCoverBouncer>
    
    <!-- Exclude test frameworks (CoverBouncer assemblies excluded automatically) -->
    <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
  </PropertyGroup>
</Project>
```

**Note:** CoverBouncer automatically adds `[CoverBouncer.*]*` to exclusions, so you don't need to include it manually.

## Next Steps

- Read the [Configuration Reference](./configuration.md) for all options
- Check out [Profile Tagging Best Practices](./profile-tagging.md)
- See [CI/CD Integration Examples](./ci-cd-integration.md) for more platforms

## Questions?

Open an issue on GitHub or check existing documentation!

---

**Happy testing!** 🚀
