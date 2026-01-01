# File Tagging Guide

This guide covers all the ways to tag files with coverage profiles in CoverBouncer.

## Overview

Coverage profiles are assigned to files using a simple comment attribute:

```csharp
// [CoverageProfile("ProfileName")]
namespace MyApp.Services
{
    public class MyService { }
}
```

## What Happens to Untagged Files?

**Untagged files automatically use the `defaultProfile` from your `coverbouncer.json`.**

This means:
- ✅ You can start using CoverBouncer immediately without tagging any files
- ✅ All files are evaluated against your default threshold
- ✅ You only need to tag files that require a *different* threshold

**Example output showing untagged files:**
```
Coverage Summary by Profile
─────────────────────────────────────────
  ✅ Critical: 3 passed, 0 failed (100% required)
  ❌ Standard: 5 passed, 2 failed (60% required)
  ✅ NoCoverage (default): 15 passed, 0 failed (exempt)

  ℹ️  15 file(s) untagged → using 'NoCoverage' profile
     Tip: Tag files with // [CoverageProfile("ProfileName")] for explicit control
─────────────────────────────────────────
```

> **Note:** The default profile is `NoCoverage` (0%), so untagged files won't cause build failures. Tag files with `Standard`, `BusinessLogic`, or `Critical` to opt-in to coverage enforcement.

For small projects, manual tagging works fine. For larger projects, use the CLI's automated tagging features.

## Manual Tagging

### Basic Syntax

Add the profile attribute as a comment before your namespace or class declaration:

```csharp
// [CoverageProfile("Critical")]
namespace MyApp.Security
{
    public class AuthenticationService { }
}
```

### Placement

The attribute can be placed:
- Before the namespace declaration (recommended)
- Before the class/interface/record declaration
- After using statements

Examples:

```csharp
// After usings, before namespace
using System;
using System.Collections.Generic;

// [CoverageProfile("BusinessLogic")]
namespace MyApp.Services
{
    public class CustomerService { }
}
```

```csharp
// Before class (if no namespace)
using System;

// [CoverageProfile("Dto")]
public class CustomerDto
{
    public int Id { get; set; }
}
```

## Automated Tagging with CLI

For projects with many files, use the CLI tool to tag files in bulk.

### 1. Interactive Mode (Recommended)

The easiest way to get started:

```bash
dotnet coverbouncer tag --interactive
```

**Workflow:**
1. Enter a file pattern (e.g., `**/*Service.cs`)
2. See a preview of matching files
3. Choose a profile from the list in your config
4. Confirm to apply changes

**Example session:**
```
🎨 Interactive Tagging Mode

Enter file pattern to tag (e.g., **/*Service.cs, ./Security/**/*.cs):
> **/*Service.cs

Found 15 files matching pattern:
  • CustomerService.cs
  • OrderService.cs
  • PaymentService.cs
  ... and 12 more

Available profiles:
  1. Critical (100% line coverage)
  2. BusinessLogic (90% line coverage)
  3. Standard (70% line coverage)
  4. Dto (0% line coverage)

Select profile (number or name): 2

Selected profile: BusinessLogic

Apply changes? (Y/n): y

✅ Tagged 15 file(s):
   • CustomerService.cs
   • OrderService.cs
   ...
```

### 2. Auto-Suggest Mode

Let CoverBouncer suggest profiles based on file patterns:

```bash
dotnet coverbouncer tag --auto-suggest
```

**How it works:**
- Analyzes all `.cs` files in your project
- Suggests profiles based on naming patterns:
  - `*Controller.cs` → Integration
  - `*Service.cs` → BusinessLogic
  - `*Adapter.cs` → Integration
  - Files in `Security/` folder → Critical
  - Files in `Models/` or `*Dto.cs` → Dto
  - Everything else → Standard
- Shows you the suggestions grouped by profile
- Asks for confirmation before applying

**Example output:**
```
🤖 Auto-Suggest Mode

Analyzing project files...

📋 Suggested Profile Assignments:

Profile: Critical (8 files)
  • Security/AuthenticationService.cs
  • Security/AuthorizationService.cs
  • Payment/PaymentProcessor.cs
  ... and 5 more

Profile: BusinessLogic (23 files)
  • Services/CustomerService.cs
  • Services/OrderService.cs
  • Managers/InventoryManager.cs
  ... and 20 more

Profile: Dto (45 files)
  • Models/CustomerDto.cs
  • ViewModels/OrderViewModel.cs
  ... and 43 more

Apply these suggestions? (Y/n): y

✅ Tagging complete:
   Tagged: 76
   Skipped: 0 (already tagged)
```

### 3. Batch Mode (Pattern-Based)

Tag files matching specific patterns:

**Basic usage:**
```bash
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic
```

**More examples:**
```bash
# Tag all controllers
dotnet coverbouncer tag --pattern "**/*Controller.cs" --profile Integration

# Tag all files in Security folder
dotnet coverbouncer tag --pattern "Security/**/*.cs" --profile Critical

# Tag specific file
dotnet coverbouncer tag --pattern "PaymentProcessor.cs" --profile Critical
```

**Glob pattern syntax:**
- `**` - Match any number of directories
- `*` - Match any characters in filename
- `?` - Match single character
- `[abc]` - Match one of: a, b, or c

**Examples:**
- `**/*Service.cs` - All files ending in Service.cs
- `src/**/*.cs` - All .cs files under src folder
- `**/Models/*.cs` - All .cs files in any Models folder
- `**/*[Aa]dapter.cs` - Files ending in Adapter or adapter

### 4. Directory-Based Tagging

Tag all files in a directory:

```bash
# Tag all files in directory (recursive)
dotnet coverbouncer tag --path "./Security" --profile Critical

# Tag just the directory, not subdirectories
dotnet coverbouncer tag --path "./Models" --profile Dto --no-recursive
```

### 5. File List Tagging

Tag files listed in a text file:

```bash
dotnet coverbouncer tag --files critical-files.txt --profile Critical
```

**File format (critical-files.txt):**
```
# Lines starting with # are comments
src/Security/AuthenticationService.cs
src/Security/AuthorizationService.cs
src/Payment/PaymentProcessor.cs

# Can include relative or absolute paths
./Services/OrderService.cs
```

## Advanced Options

### Dry Run

Preview what would happen without modifying files:

```bash
dotnet coverbouncer tag --pattern "**/*.cs" --profile Standard --dry-run
```

Output:
```
✅ [DRY RUN] Would tag 42 file(s):
   • CustomerService.cs
   • OrderService.cs
   ...
```

### Backup Files

Create `.backup` copies before modifying:

```bash
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic --backup
```

This creates:
- `CustomerService.cs.backup`
- `OrderService.cs.backup`
- etc.

### Combining Options

```bash
# Preview auto-suggest without making changes
dotnet coverbouncer tag --auto-suggest --dry-run

# Create backups when using pattern
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic --backup

# Tag directory with backups
dotnet coverbouncer tag --path "./Security" --profile Critical --backup
```

## Best Practices

### Start with Auto-Suggest

For new projects:

```bash
# 1. Generate config
dotnet coverbouncer init

# 2. Let auto-suggest tag most files
dotnet coverbouncer tag --auto-suggest

# 3. Manually adjust critical areas
dotnet coverbouncer tag --path "./Security" --profile Critical
dotnet coverbouncer tag --path "./Payment" --profile Critical
```

### Use Dry Run First

Before bulk operations:

```bash
# Preview first
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic --dry-run

# If it looks good, apply it
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic
```

### Create Backups for Large Changes

When tagging many files:

```bash
dotnet coverbouncer tag --auto-suggest --backup
```

### Tag Incrementally

For large projects, tag in stages:

```bash
# Stage 1: Critical areas
dotnet coverbouncer tag --path "./Security" --profile Critical
dotnet coverbouncer tag --path "./Payment" --profile Critical

# Stage 2: Business logic
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic

# Stage 3: Integration layer
dotnet coverbouncer tag --pattern "**/*Controller.cs" --profile Integration

# Stage 4: DTOs and models
dotnet coverbouncer tag --pattern "**/*Dto.cs" --profile Dto
dotnet coverbouncer tag --path "./Models" --profile Dto

# Stage 5: Everything else gets default
# (No need to tag - uses defaultProfile from config)
```

## Troubleshooting

### Profile Not Found

If you see:
```
⚠️  Warning: Profile 'MyProfile' not found in coverbouncer.json
```

Check that the profile exists in your `coverbouncer.json`:

```json
{
  "profiles": {
    "MyProfile": {
      "minLine": 0.80
    }
  }
}
```

### Files Already Tagged

If files are already tagged with the same profile, they'll be skipped:

```
ℹ️  Skipped 5 file(s) (already tagged with same profile)
```

To change the profile, just run the command again with a different profile name.

### No Files Matched

If no files match your pattern:

```
✅ Tagged 0 file(s)
```

Check your pattern syntax:
- Use `**` for recursive directory matching
- Use forward slashes or backslashes (both work on Windows)
- Use quotes around patterns with wildcards

```bash
# Good
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile Standard

# Also good
dotnet coverbouncer tag --pattern "src/**/*.cs" --profile Standard

# Bad (missing quotes)
dotnet coverbouncer tag --pattern **/*Service.cs --profile Standard
```

## Examples by Use Case

### New Project Setup

```bash
# 1. Initialize with relaxed template (lower thresholds)
dotnet coverbouncer init relaxed

# 2. Auto-tag everything
dotnet coverbouncer tag --auto-suggest

# 3. Adjust critical areas
dotnet coverbouncer tag --path "./Security" --profile Critical
```

### Legacy Project Adoption

```bash
# 1. Start with very low thresholds
dotnet coverbouncer init relaxed

# 2. Create a "Legacy" profile in coverbouncer.json
# {
#   "Legacy": { "minLine": 0.05 }  // 5% - start where you are!
# }

# 3. Tag everything as Legacy initially
dotnet coverbouncer tag --pattern "**/*.cs" --profile Legacy

# 4. Tag new code with higher standards
dotnet coverbouncer tag --path "./NewFeatures" --profile Standard
```

### Gradual Migration

```bash
# Tag one module at a time
dotnet coverbouncer tag --path "./Modules/Customer" --profile Standard
dotnet coverbouncer tag --path "./Modules/Order" --profile Standard

# Or by pattern
dotnet coverbouncer tag --pattern "Modules/Customer/**/*.cs" --profile Standard
```

### Refactoring Session

```bash
# Before refactoring, tag files with higher requirements
dotnet coverbouncer tag --files refactoring-list.txt --profile BusinessLogic

# After refactoring, tests must meet 90% threshold
dotnet test  # CoverBouncer validates automatically
```

## Tips

1. **Use interactive mode** when learning - it shows you what will happen
2. **Use auto-suggest** for initial setup - it's surprisingly accurate
3. **Use dry-run** before bulk operations - preview is free!
4. **Use backups** when unsure - easy to revert if needed
5. **Tag incrementally** in large projects - easier to review changes
6. **Commit tags separately** - easier to review in pull requests

## Integration with Version Control

### Git Workflow

```bash
# Tag files
dotnet coverbouncer tag --auto-suggest

# Review changes
git diff

# Commit tags separately from code changes
git add "**/*.cs"
git commit -m "Add coverage profile tags"
```

### Pull Request Review

When reviewing PRs with new files:

```bash
# In the PR branch
dotnet coverbouncer tag --files new-files.txt --profile Standard

# Verify coverage requirements are met
dotnet test
```
