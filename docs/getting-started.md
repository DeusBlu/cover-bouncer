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
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": {
      "minLine": 0.70,
      "minBranch": 0.60
    },
    "BusinessLogic": {
      "minLine": 0.90,
      "minBranch": 0.80
    },
    "Critical": {
      "minLine": 1.00,
      "minBranch": 1.00
    },
    "Dto": {
      "minLine": 0.00,
      "minBranch": 0.00
    }
  }
}
```

**That's all the configuration you need!** No sprawl, no complexity.

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

### Step 4: Tag Your Source Files

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
  "coverletReportPath": "coverage/results/coverage.json",
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
3. The `CoverletOutput` path matches `coverletReportPath` in config

### "No profile tag found"

Files without tags use the `defaultProfile`. This is normal! Only tag files that need special thresholds.

### "Build passes locally but fails in CI"

Check that:
1. `coverbouncer.json` is committed to source control
2. MSBuild properties are in a committed file (not user-specific `.user` files)
3. Coverlet is properly configured

## Next Steps

- Read the [Configuration Reference](./configuration.md) for all options
- Check out [Profile Tagging Best Practices](./profile-tagging.md)
- See [CI/CD Integration Examples](./ci-cd-integration.md) for more platforms

## Questions?

Open an issue on GitHub or check existing documentation!

---

**Happy testing!** 🚀
