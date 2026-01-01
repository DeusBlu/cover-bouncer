# Configuration Reference

Cover-Bouncer uses a **single configuration file** with minimal required settings. Everything else is handled by Coverlet.

## Philosophy

- ✅ **One file, one source of truth:** `coverbouncer.json`
- ✅ **Piggyback on Coverlet:** We read their output, no duplicate config
- ✅ **Profile tags over exclusions:** Tag files you don't want tested with `Dto` (0.00 threshold)
- ✅ **Sensible defaults:** Most users only need to define profiles

---

## Complete Schema

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

> **Note:** The default profile is `NoCoverage` (0%) so your build won't fail on first install. This allows you to adopt coverage enforcement gradually by tagging files with stricter profiles.
```

---

## Property Reference

### `coverageReportPath`
**Type:** `string`  
**Default:** `"TestResults/coverage.json"`  
**Required:** No

Path to the Coverlet JSON coverage report output.

**When to change:**
- You have a custom Coverlet output path configured
- You're using a non-standard test project structure

**Example:**
```json
{
  "coverageReportPath": "coverage/results/report.json"
}
```

---

### `defaultProfile`
**Type:** `string`  
**Required:** Yes

The profile name applied to **all files that don't have an explicit `[CoverageProfile("...")]` tag**.

> **Important:** This is how CoverBouncer handles untagged files. If you don't tag a file, it automatically uses the `defaultProfile` threshold. This means you can start using CoverBouncer immediately without tagging any files - all files will be evaluated against your default profile.

**Must match a key in the `profiles` object.**

**Example:**
```json
{
  "defaultProfile": "Standard"
}
```

---

### `profiles`
**Type:** `object`  
**Required:** Yes

Dictionary of profile name → coverage thresholds.

**Each profile can specify:**
- `minLine` (decimal 0.0-1.0) - Minimum line coverage required

> **Note:** Branch and method coverage support is planned for a future release (MVP currently supports line coverage only).

**Threshold values:**
- `0.00` = 0% (no coverage required)
- `0.50` = 50%
- `0.80` = 80%
- `1.00` = 100%

**Example:**
```json
{
  "profiles": {
    "Standard": {
      "minLine": 0.70
    },
    "Critical": {
      "minLine": 1.00
    },
    "Dto": {
      "minLine": 0.00
    }
  }
}
```

---

## Minimal Example

The absolute minimum valid configuration:

```json
{
  "defaultProfile": "NoCoverage",
  "profiles": {
    "NoCoverage": { "minLine": 0.00 }
  }
}
```

This assumes:
- Coverlet outputs to `TestResults/coverage.json`
- You only care about line coverage
- **All untagged files have no coverage requirement (0%)**

> **Note:** With this config you're effectively in "audit mode" - CoverBouncer will report coverage but won't fail builds. Tag files with profiles like `Standard` or `Critical` to start enforcing thresholds.

---

## Common Configurations

### Strict Project
```json
{
  "defaultProfile": "High",
  "profiles": {
    "High": {
      "minLine": 0.90
    },
    "Critical": {
      "minLine": 1.00
    }
  }
}
```

### Relaxed Project
```json
{
  "defaultProfile": "Low",
  "profiles": {
    "Low": {
      "minLine": 0.50
    },
    "Important": {
      "minLine": 0.80
    }
  }
}
```

### Balanced (Recommended)
```json
{
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": {
      "minLine": 0.70
    },
    "BusinessLogic": {
      "minLine": 0.90
    },
    "Critical": {
      "minLine": 1.00
    },
    "Dto": {
      "minLine": 0.00
    },
    "Generated": {
      "minLine": 0.00
    }
  }
}
```

---

## What About Exclusions?

**You don't need them!** Use profile tags instead.

### Old Way (Other Tools)
```xml
<Exclude>**/*.Designer.cs</Exclude>
<Exclude>**/Migrations/**</Exclude>
<Exclude>**/DTOs/**</Exclude>
```

### Cover-Bouncer Way
```csharp
// In your DTO files
// [CoverageProfile("Dto")]
namespace MyApp.Models
{
    public class CustomerDto { }
}

// In generated files
// [CoverageProfile("Generated")]
namespace MyApp.Migrations
{
    public partial class InitialCreate { }
}
```

**Benefits:**
- ✅ Explicit and visible in source code
- ✅ No complex glob patterns
- ✅ Easy to see why a file isn't tested
- ✅ Can still track that coverage is 0% (vs completely hidden)

---

## What About Coverage Format?

**Handled by Coverlet!** We just read whatever Coverlet outputs.

In your test project `.csproj` or `Directory.Build.props`:
```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
  <CoverletOutputFormat>json</CoverletOutputFormat>
</PropertyGroup>
```

Cover-Bouncer reads the JSON output. That's it!

---

## MSBuild Integration

Most users won't need MSBuild properties, but they're available:

```xml
<PropertyGroup>
  <!-- Enable/disable (default: true when package installed) -->
  <EnableCoverBouncer>true</EnableCoverBouncer>
  
  <!-- Config file location (default: coverbouncer.json at solution root) -->
  <CoverBouncerConfigFile>$(SolutionDir)coverbouncer.json</CoverBouncerConfigFile>
</PropertyGroup>
```

---

## CLI Overrides

When using the CLI explicitly, you can override the config path:

```bash
dotnet coverbouncer verify \
  --coverage custom/path/coverage.json \
  --config custom/path/coverbouncer.json
```

---

## FAQ

### Why not use `[ExcludeFromCodeCoverage]`?
Because that removes files from the Coverlet report entirely. We want to:
- Track that coverage exists (even if it's 0%)
- Be explicit about why files aren't tested
- Keep the decision in source code, not config

### Can I have multiple config files?
No. One project = one `coverbouncer.json`. Simplicity wins.

### Can profiles inherit from each other?
Not in v1. Each profile is independent. This keeps the config simple and explicit.

### Can one file use multiple profiles?
Not in v1. One file = one profile. Pick the strictest requirement for that file.

### What if Coverlet isn't installed?
The MSBuild package will guide you to install it. Cover-Bouncer requires Coverlet for coverage collection.

---

## Validation

Cover-Bouncer validates your config on load:

**Errors:**
- Missing `defaultProfile`
- Missing `profiles`
- `defaultProfile` doesn't match any profile key
- Invalid threshold values (not between 0.0 and 1.0)
- Invalid JSON syntax

**Warnings:**
- `coverageReportPath` points to non-existent file (at verify time)
- Profile has no thresholds defined

---

## Schema File

For IDE autocomplete and validation, we'll provide a JSON schema:

```json
{
  "$schema": "https://coverbouncer.dev/schema/v1/config.json"
}
```

(Coming soon!)

---

**Questions?** Open an issue on GitHub!
