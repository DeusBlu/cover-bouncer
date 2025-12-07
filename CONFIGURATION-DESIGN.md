# 📋 Cover-Bouncer: Configuration Design Summary

## ✅ Final Design Decision

**ONE configuration file** with **MINIMAL required properties**

---

## The Config File: `coverbouncer.json`

```json
{
  "coverageReportPath": "TestResults/coverage.json",
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": { "minLine": 0.70 },
    "Critical": { "minLine": 1.00 },
    "Dto": { "minLine": 0.00 }
  }
}
```

**That's it! 3 properties total.**

---

## Property Breakdown

| Property | Required? | Purpose | Default |
|----------|-----------|---------|---------|
| `coverageReportPath` | ❌ Optional | Where Coverlet outputs JSON | `TestResults/coverage.json` |
| `defaultProfile` | ✅ Required | Profile for untagged files | - |
| `profiles` | ✅ Required | Threshold definitions | - |

---

## What We DON'T Need (Handled Elsewhere)

| Setting | Why Not Needed? |
|---------|-----------------|
| **Exclude patterns** | Use profile tags (`Dto` with 0.00) instead |
| **Coverage format** | Coverlet handles this in MSBuild props |
| **Coverage collection** | Coverlet handles this in MSBuild props |
| **Fail on violation** | Always fail (it's a policy enforcer!) |
| **Report format** | Console output is fine for v1 |
| **Source root** | Not needed, we read Coverlet's paths |
| **Output verbosity** | Simple console output for v1 |

---

## Philosophy

### ✅ DO
- **Piggyback on Coverlet** - Don't reinvent coverage collection
- **One source of truth** - Everything in `coverbouncer.json`
- **Explicit over implicit** - Profile tags visible in source code
- **Simple defaults** - Works out of box for 99% of users

### ❌ DON'T
- **Duplicate Coverlet config** - They already handle coverage settings
- **Create configuration sprawl** - Multiple config files = confusion
- **Magic behavior** - Be explicit about what gets checked
- **Over-engineer** - Keep it simple

---

## User Setup Flow

### 1. Install Package
```bash
dotnet add MyApp.Tests package CoverBouncer.MSBuild
```

### 2. Enable Coverlet (if not already)
```xml
<!-- In test project .csproj or Directory.Build.props -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>json</CoverletOutputFormat>
</PropertyGroup>
```

### 3. Create Config
```bash
dotnet coverbouncer init
```

### 4. Tag Files (optional)
```csharp
// [CoverageProfile("Critical")]
namespace MyApp.PaymentProcessing { }
```

### 5. Done!
```bash
dotnet test  # Automatically enforces policy
```

---

## Config Validation

Cover-Bouncer validates on load:

**✅ Valid:**
```json
{
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": { "minLine": 0.70 }
  }
}
```

**❌ Invalid:**
```json
{
  "defaultProfile": "Typo",  // ← Doesn't exist in profiles
  "profiles": {
    "Standard": { "minLine": 1.5 }  // ← Out of range
  }
}
```

**⚠️ Warning:**
```json
{
  "coverageReportPath": "wrong/path.json",  // ← File doesn't exist
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": { "minLine": 0.70 }
  }
}
```

---

## Comparison: Before vs After

### ❌ BEFORE (Complex)
```json
{
  "$schema": "https://...",
  "coverletReportPath": "...",
  "sourceRoot": "src/",
  "defaultProfile": "Standard",
  "profiles": { ... },
  "excludePatterns": ["**/obj/**", "**/bin/**", ...],
  "failOnViolation": true,
  "reportFormat": "console",
  "verbosity": "normal",
  "cacheResults": true,
  "parallelProcessing": true
}
```

**AND** MSBuild properties:
```xml
<EnableCoverBouncer>true</EnableCoverBouncer>
<CoverBouncerFailOnViolation>true</CoverBouncerFailOnViolation>
<CoverBouncerVerbosity>normal</CoverBouncerVerbosity>
```

**AND** Coverlet config:
```xml
<CollectCoverage>true</CollectCoverage>
<CoverletOutput>...</CoverletOutput>
<CoverletOutputFormat>json</CoverletOutputFormat>
```

### ✅ AFTER (Simple)
```json
{
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": { "minLine": 0.70 },
    "Critical": { "minLine": 1.00 },
    "Dto": { "minLine": 0.00 }
  }
}
```

**Everything else** handled by Coverlet (existing MSBuild props).

---

## Implementation Impact

### Core Library
```csharp
public class PolicyConfiguration
{
    public string CoverageReportPath { get; set; } = "TestResults/coverage.json";
    public string DefaultProfile { get; set; } = null!;
    public Dictionary<string, ProfileThresholds> Profiles { get; set; } = new();
}

public class ProfileThresholds
{
    public decimal? MinLine { get; set; }
    public decimal? MinBranch { get; set; }
    public decimal? MinMethod { get; set; }
}
```

**Simple classes, easy serialization, minimal validation.**

---

## Benefits

### For Users
- 🎯 **Crystal clear** - One file, minimal properties
- 🚀 **Quick setup** - 5 minutes from install to enforced
- 📖 **Easy to understand** - No mysterious settings
- 🔧 **Easy to maintain** - Change thresholds in one place

### For Developers
- 💻 **Easy to implement** - Small surface area
- 🧪 **Easy to test** - Simple models
- 📝 **Easy to document** - Less to explain
- 🐛 **Easy to debug** - Fewer moving parts

### For Teams
- 🤝 **Easy to agree on** - Clear, simple policies
- 📊 **Easy to enforce** - Automatic via CI/CD
- 🔄 **Easy to evolve** - Just update profiles
- ✅ **Easy to adopt** - Low barrier to entry

---

## Edge Cases Handled

### Q: What if I have a weird Coverlet output path?
**A:** Set `coverageReportPath` in config.

### Q: What if I don't want to test some files?
**A:** Tag them with `Dto` profile (0.00 threshold).

### Q: What if I want file-specific thresholds?
**A:** Create a profile for that purpose, tag the file.

### Q: What if I want to disable enforcement locally?
**A:** Set MSBuild property `<EnableCoverBouncer>false</EnableCoverBouncer>`

### Q: What if my team disagrees on thresholds?
**A:** That's a team problem, not a config problem 😉

---

## Future Additions (Maybe)

If we find real-world need:
- `minMethod` threshold support
- Custom profile inheritance
- Multiple configs per monorepo (probably not)

**But default is: Don't add unless proven necessary!**

---

## Documentation Coverage

- ✅ [docs/configuration.md](./docs/configuration.md) - Complete property reference
- ✅ [docs/getting-started.md](./docs/getting-started.md) - Quick setup guide
- ✅ [PROJECT-SPEC.md](./PROJECT-SPEC.md) - Technical specification
- ✅ [README.md](./README.md) - Overview and quick start

---

## Status: ✅ FINALIZED

This configuration design is **locked in** for v1.0.

Simple, clean, effective. Let's build it! 🚀
