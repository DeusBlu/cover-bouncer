# 🚪 Cover-Bouncer

**The Coverage Doorman for .NET Projects**

Cover-Bouncer enforces profile-based code coverage policies in your .NET projects. Tag your files, set thresholds, and let the bouncer keep your coverage standards high.

## Why Cover-Bouncer?

Standard coverage tools give you one number: "X% coverage." But not all code is equal:
- Your critical business logic should have 90%+ coverage
- Your DTOs might not need any tests
- Your integration adapters need moderate coverage

**Cover-Bouncer lets you enforce different coverage requirements for different parts of your codebase.**

## 🎨 Profiles Are Completely Customizable!

**You are NOT limited to any specific profile names or percentages:**

❌ **Common Misconception:**
   "I must use 'Critical' (90%), 'Standard' (80%), etc."

✅ **Reality:**
   "I can use ANY names with ANY thresholds!"

### Examples of Valid Profiles:

```json
{
  "profiles": {
    "MustHaveTests": { "minLine": 0.45 },      // Start achievable!
    "NiceToHave": { "minLine": 0.25 },
    "SecurityStuff": { "minLine": 0.75 },      // Your own names!
    "LegacyCode": { "minLine": 0.10 },         // Be honest about reality
    "WillFixLater": { "minLine": 0.0 }
  }
}
```

### Start Where YOU Are:

- Have 30% coverage now? **Start with 35% threshold**
- Have 60% coverage? **Maybe aim for 65-70%**
- Have 5% coverage? **Start at 10% and celebrate progress!**

**The goal is IMPROVEMENT, not perfection!** 📈

The built-in templates (Basic, Strict, Relaxed) are just **suggestions** to get you started. Feel free to customize them to match your team's current reality and goals.

## 🚀 Adoption Path (Recommended)

Don't try to tag everything at once! CoverBouncer defaults to **no coverage enforcement** so you can adopt gradually.

### Step 1: Install and Initialize
```bash
dotnet add MyApp.Tests package CoverBouncer.MSBuild
dotnet coverbouncer init
```
Your build will pass immediately - untagged files default to `NoCoverage` (0%).

### Step 2: Tag ONE Critical File
Pick your most important file (payments, auth, etc.) and tag it:
```csharp
// [CoverageProfile("Critical")]
public class PaymentProcessor { }
```

### Step 3: Customize Profiles (Optional)
Adjust thresholds to match your codebase:
```json
{
  "defaultProfile": "NoCoverage",
  "profiles": {
    "Critical": { "minLine": 0.90 },      // Payments, Auth, Security
    "Standard": { "minLine": 0.60 },      // Code you want tested
    "NoCoverage": { "minLine": 0.0 }      // Default - no enforcement
  }
}
```

### Step 4: Expand Gradually
Once Critical is stable, expand to more areas:
```bash
# Week 2: Tag your core business logic
dotnet coverbouncer tag --path "./Core" --profile Standard

# Week 3: Tag standard application code  
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile Standard

# Week 4: Tag DTOs to exclude from coverage requirements
dotnet coverbouncer tag --pattern "**/*Dto.cs" --profile Dto
```

**Pro tip:** Use `--dry-run` to preview changes before applying them!

## Quick Start

### 1. Install
```bash
# Install CoverBouncer
dotnet add MyApp.Tests package CoverBouncer.MSBuild

# Install Coverlet for coverage collection
dotnet add MyApp.Tests package coverlet.msbuild
```

### 2. Initialize
```bash
dotnet coverbouncer init
```

### 3. Configure Coverage (one-time)
Add to `Directory.Build.props`:
```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
  <CoverletOutputFormat>json</CoverletOutputFormat>
  <EnableCoverBouncer>true</EnableCoverBouncer>
</PropertyGroup>
```

### 4. Tag Your Files (Optional!)

> **Note:** Untagged files automatically use your `defaultProfile`. You can start with zero tags and add them later for files needing different thresholds.

You can tag files manually or use the CLI tagging features:

#### Manual Tagging
Add a comment with the profile attribute:
```csharp
// [CoverageProfile("Critical")]
namespace MyApp.PaymentProcessing
{
    public class PaymentService { }
}
```

#### Automated Tagging

**Interactive Mode** (Recommended for beginners):
```bash
dotnet coverbouncer tag --interactive
```

**Batch Mode** - Tag by pattern:
```bash
# Tag all service files with "Standard" profile
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile Standard

# Tag entire folder with "Critical" profile
dotnet coverbouncer tag --path "./Security" --profile Critical

# Tag files from a list
dotnet coverbouncer tag --files services.txt --profile Standard
```

**Smart Detection** - Auto-suggest profiles:
```bash
# Automatically suggest profiles based on file patterns
dotnet coverbouncer tag --auto-suggest

# Preview changes without modifying files
dotnet coverbouncer tag --pattern "**/*.cs" --profile Standard --dry-run
```

The CLI automatically detects patterns like:
- `*Controller.cs` → Integration
- `*Service.cs` → BusinessLogic
- `Security/*` → Critical
- `Models/*` → Dto

### 5. Run Tests
```bash
dotnet test
```

Coverage policy automatically enforced! ✅

## Features

- 🎯 **Profile-Based Coverage** - Different thresholds for different code types
- 🤖 **Smart Tagging** - Interactive, batch, and auto-suggest modes for tagging files
- 🔌 **Drop-In Integration** - Works with your existing `dotnet test` workflow
- 🔍 **Filtered Run Support** - `dotnet test --filter` works without false failures
- 🚫 **CI/CD Ready** - Blocks merges when coverage drops below thresholds
- 📦 **NuGet Packaged** - Easy to install and distribute
- 🏷️ **File-Level Tags** - Simple attribute-based profile assignment
- ⚙️ **Single Config File** - No configuration sprawl
- 🔧 **Auto-Configuration** - Automatically excludes CoverBouncer from coverage
- 🎨 **Fully Customizable** - Use any profile names and thresholds you want

## 🔍 Filtered Test Runs (`--filter`)

Running `dotnet test --filter "Category=Unit"` or any `--filter` expression? **CoverBouncer handles it automatically.**

### The Problem

When you use `--filter`, Coverlet still instruments the **entire assembly**. Files not targeted by the filtered tests appear with 0% coverage, even though no tests were supposed to run against them. Without awareness, CoverBouncer would report false failures for every untargeted file.

### How CoverBouncer Solves It

CoverBouncer automatically detects filtered test runs via the `$(VSTestTestCaseFilter)` MSBuild property. When a filter is active, files with zero covered lines are **skipped** — they were instrumented but not targeted.

**No configuration needed** — it just works.

### Example: Same Coverage Data, Different Modes

**Filtered run** (`dotnet test --filter "Category=OrderTests"`):
```
ℹ️  Filtered test run mode: files with zero coverage will be skipped
Coverage Summary by Profile
─────────────────────────────────────────
  ✅ BusinessLogic: 1 passed, 0 failed (80% required)
  ✅ Critical: 1 passed, 0 failed (100% required)
  ⏭️  4 file(s) skipped (no coverage data in filtered test run)
─────────────────────────────────────────
✅ All 2 files passed coverage requirements
```

**Full run** (`dotnet test`, same data):
```
Coverage Summary by Profile
─────────────────────────────────────────
  ❌ BusinessLogic: 1 passed, 1 failed (80% required)
  ❌ Critical: 1 passed, 1 failed (100% required)
  ✅ Dto: 1 passed, 0 failed (exempt)
  ❌ Standard (default): 0 passed, 1 failed (60% required)
─────────────────────────────────────────
❌ 3 coverage violation(s) found
```

### ⚠️ Best Practice: Use Full Runs as Your CI Gate

Filtered runs are great for **fast local feedback** — validate just the files you're working on. But only a **full run** validates coverage across your entire codebase.

**Recommended CI setup:**
```yaml
jobs:
  quick-check:
    # Fast feedback on PRs — only validates targeted files
    run: dotnet test --filter "Category=Unit"

  coverage-gate:
    # Final gate — validates ALL coverage thresholds
    run: dotnet test
```

### How It Works Under the Hood

| Situation | CoveredLines | Filtered Run | Full Run |
|-----------|-------------|--------------|----------|
| File targeted by tests, meets threshold | > 0 | ✅ Pass | ✅ Pass |
| File targeted by tests, below threshold | > 0 | ❌ Fail | ❌ Fail |
| File NOT targeted (not in filter) | 0 | ⏭️ Skip | ❌ Fail |
| File with Dto profile (0% allowed) | 0 | ⏭️ Skip | ✅ Pass |

**Key insight:** On a filtered run, there's no way to distinguish "file not in the filter" from "file genuinely has no tests." That's why the full run is essential as your final gate — it catches files that truly need tests.

### CLI Usage

For CLI usage (outside MSBuild), pass the `--filtered` flag:
```bash
coverbouncer verify --coverage coverage.json --config coverbouncer.json --filtered
```

## Coverlet Integration

CoverBouncer works seamlessly with [Coverlet](https://github.com/coverlet-coverage/coverlet), the popular .NET code coverage library.

### Automatic Exclusions

When you add CoverBouncer.MSBuild as a NuGet package, it **automatically excludes** all CoverBouncer assemblies from Coverlet instrumentation via `buildTransitive` targets. This prevents warnings about missing debug symbols and improves performance.

**What's excluded automatically:**
- `[CoverBouncer.Core]*`
- `[CoverBouncer.Coverlet]*`
- `[CoverBouncer.MSBuild]*`

No manual configuration needed! 🎉

### Optional: Exclude Test Frameworks

During `dotnet coverbouncer init`, you'll be prompted to exclude common test frameworks and mocking libraries for better performance. This is **optional but recommended**:

```xml
<!-- Directory.Build.props -->
<PropertyGroup>
  <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
</PropertyGroup>
```

### Why Exclude These?

- **Performance**: Faster test runs by skipping unnecessary instrumentation
- **Noise Reduction**: Focus coverage reports on your actual code
- **No Warnings**: Prevents "missing symbols" warnings for NuGet packages

### Manual Configuration

If you need more control over Coverlet exclusions:

```xml
<PropertyGroup>
  <!-- Include only your production code -->
  <Include>[MyApp]*</Include>
  
  <!-- Or exclude specific assemblies -->
  <Exclude>$(Exclude);[MyApp.Tests]*;[ThirdParty.*]*</Exclude>
</PropertyGroup>
```

See [Coverlet documentation](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md) for more options.

## How It Works

1. **Tag files** with coverage profiles using file-level attributes
2. **Set thresholds** in `coverbouncer.json`
3. **Run tests** - Cover-Bouncer validates coverage automatically
4. **Build fails** if any file violates its profile's threshold

## Project Structure

- `CoverBouncer.Core` - Core coverage policy engine
- `CoverBouncer.Coverlet` - Coverlet adapter for coverage data
- `CoverBouncer.MSBuild` - MSBuild integration (main user package)
- `CoverBouncer.Analyzers` - Roslyn analyzers (coming soon)

## Example Configuration

```json
{
  "coverageReportPath": "TestResults/coverage.json",
  "defaultProfile": "Standard",
  "profiles": {
    "Critical": {
      "minLine": 0.90
    },
    "Standard": {
      "minLine": 0.60
    },
    "Integration": {
      "minLine": 0.40
    },
    "Dto": {
      "minLine": 0.00
    }
  }
}
```

> **Note:** Values are decimals (0.0-1.0), not percentages. So 0.90 = 90% line coverage.

## Installation & Usage

### MSBuild Task (Recommended)
Automatically validates coverage after `dotnet test`:

```bash
dotnet add package CoverBouncer.MSBuild
```

Add to your test project's `.csproj`:
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  
  <!-- Optional: Customize coverage report path -->
  <!-- <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/custom/coverage.json</CoverBouncerCoverageReport> -->
</PropertyGroup>
```

**See [MSBuild Configuration](./docs/msbuild-configuration.md) for all available properties.**

### CLI Tool
For manual validation or CI/CD scripts:

```bash
dotnet tool install -g CoverBouncer.CLI
coverbouncer verify --coverage coverage.json --config coverbouncer.json
```

## Validation Tests

This project includes comprehensive validation tests:
- ✅ All files pass thresholds
- ✅ Mixed pass/fail scenarios
- ✅ Critical violations
- ✅ Untagged file handling
- ✅ Multiple profile scenarios
- ✅ Edge cases (exact thresholds, zero coverage)
- ✅ Real-world project simulation
- ✅ **Filtered test run** (untargeted files skipped)
- ✅ **Full run with same data** (contrast — same data fails without filter awareness)

Run validation tests: `dotnet test tests/CoverBouncer.ValidationTests`

## Building from Source

```bash
# Build all projects
./build.sh build

# Run all tests
./build.sh test

# Create NuGet packages
./build.sh pack

# Install CLI tool locally
./build.sh install-cli
```

## Documentation

- [Getting Started Guide](./docs/getting-started.md) - Complete setup walkthrough
- [File Tagging Guide](./docs/tagging-guide.md) - Learn all the ways to tag files (manual & automated)
- [Configuration Reference](./docs/configuration.md) - Detailed `coverbouncer.json` options
- [MSBuild Configuration](./docs/msbuild-configuration.md) - MSBuild properties & customization
- [Coverlet Integration](./docs/coverlet-integration.md) - Best practices for Coverlet setup

## Contributing

We welcome contributions! See [CONTRIBUTING.md](./CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](./LICENSE) for details.

---

Built with ❤️ for the .NET community
