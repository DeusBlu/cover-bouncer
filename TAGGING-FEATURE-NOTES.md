# CoverBouncer Tagging Feature - Release Notes

## What's New: Automated File Tagging 🏷️

We've added powerful CLI commands to make tagging files with coverage profiles much easier!

### Key Features

#### 1. Interactive Mode
The easiest way to tag files:
```bash
dotnet coverbouncer tag --interactive
```
- Prompts for file pattern
- Shows preview of matching files
- Lists available profiles from your config
- Confirms before applying changes

#### 2. Auto-Suggest Mode
Smart pattern detection:
```bash
dotnet coverbouncer tag --auto-suggest
```
Automatically suggests profiles based on file naming conventions:
- `*Controller.cs` → Integration
- `*Service.cs` → BusinessLogic
- `Security/*` → Critical
- `Models/*` or `*Dto.cs` → Dto

#### 3. Batch Mode
For scripting and automation:
```bash
# Tag by pattern
dotnet coverbouncer tag --pattern "**/*Service.cs" --profile BusinessLogic

# Tag by directory
dotnet coverbouncer tag --path "./Security" --profile Critical

# Tag from file list
dotnet coverbouncer tag --files services.txt --profile Standard
```

#### 4. Safety Features
- `--dry-run` - Preview changes without modifying files
- `--backup` - Create `.backup` files before modifying
- Validation against your config profiles
- Skip files already tagged with same profile

## Documentation Updates

### README.md
- Added prominent section explaining profiles are fully customizable
- Emphasized starting with achievable goals based on current coverage
- Added examples of custom profile names and realistic thresholds
- Included tagging examples in Quick Start

### docs/getting-started.md
- New section: "Profiles Are YOUR Choice!"
- Practical examples of customizing profiles to match team reality
- Comprehensive tagging options (manual + automated)
- Step-by-step workflow examples

### docs/tagging-guide.md (NEW)
Comprehensive guide covering:
- Manual tagging syntax and placement
- All CLI tagging modes with examples
- Advanced options (dry-run, backup, etc.)
- Best practices and troubleshooting
- Use case examples (new project, legacy project, gradual migration)
- Git workflow integration

## New Core Components

### FileTagWriter
- Writes coverage profile tags to source files
- Handles updating existing tags
- Smart placement (before namespace/class)
- Supports removal of tags

### FileTaggingService
- Batch operations on multiple files
- Pattern matching with glob support
- Directory-based tagging
- Profile suggestions based on file patterns
- Dry-run and backup support

## Impact on User Experience

### Before
Users had to manually add `// [CoverageProfile("Name")]` to every file, which was tedious for large projects.

### After
Users can:
1. Use `--auto-suggest` to tag entire project in seconds
2. Use `--interactive` for guided experience
3. Use batch commands for specific patterns
4. Preview changes with `--dry-run` before committing

## Breaking Changes
None - this is purely additive functionality.

## Migration Path
No migration needed. Existing manual tags continue to work exactly as before.

## Examples

### Tagging a New Project
```bash
# 1. Initialize with realistic thresholds
dotnet coverbouncer init relaxed

# 2. Auto-tag entire project
dotnet coverbouncer tag --auto-suggest

# 3. Adjust critical areas
dotnet coverbouncer tag --path "./Security" --profile Critical
```

### Tagging Legacy Project
```bash
# 1. Create config with low thresholds
dotnet coverbouncer init

# Edit coverbouncer.json to add:
# "Legacy": { "minLine": 0.05 }

# 2. Tag everything as Legacy initially
dotnet coverbouncer tag --pattern "**/*.cs" --profile Legacy

# 3. Gradually increase standards for new code
dotnet coverbouncer tag --path "./NewFeatures" --profile Standard
```

## Messaging Improvements

### Old Messaging
The built-in examples (Critical: 100%, BusinessLogic: 90%, Standard: 80%) could be intimidating and suggest these were required values.

### New Messaging
- **Clear statement**: "Profiles Are Completely Customizable!"
- **Examples** show custom names like "MustHaveTests", "LegacyCode", "WillFixLater"
- **Emphasis** on starting where you are (30% → 35%, not jumping to 80%)
- **Goal**: Improvement, not perfection
- **Tone**: Encouraging and realistic

## CLI Help Output

```
dotnet coverbouncer tag --help

Commands:
  tag                       Tag files with coverage profiles
    --pattern <glob>        Tag files matching glob pattern (e.g., "**/*Service.cs")
    --path <dir>            Tag all files in directory
    --files <list>          Tag files listed in text file (one per line)
    --profile <name>        Profile to apply (required)
    --auto-suggest          Suggest profiles based on file patterns
    --interactive           Interactive mode with prompts
    --dry-run               Show what would happen without modifying files
    --backup                Create backup files before tagging
```

## Technical Details

### Dependencies Added
- `Microsoft.Extensions.FileSystemGlobbing` (8.0.0) - For pattern matching

### Files Modified
- `src/CoverBouncer.CLI/Program.cs` - Added tag command and handlers
- `README.md` - Updated with customization messaging and tagging examples
- `docs/getting-started.md` - Added profile customization section and tagging guide
- `src/CoverBouncer.Core/CoverBouncer.Core.csproj` - Added glob package reference

### Files Created
- `src/CoverBouncer.Core/Engine/FileTagWriter.cs` - Tag writing logic
- `src/CoverBouncer.Core/Engine/FileTaggingService.cs` - Batch tagging service
- `docs/tagging-guide.md` - Comprehensive tagging documentation

## Testing Recommendations

Before releasing, test:
1. ✅ Build succeeds (verified)
2. Interactive mode workflow
3. Auto-suggest on sample project
4. Batch tagging with various patterns
5. Dry-run mode
6. Backup creation
7. Error handling (invalid profiles, missing files)

## User Feedback Incorporated

This update directly addresses the feedback:
- ✅ Built-in bulk tagging script
- ✅ Interactive mode for user-friendliness
- ✅ Batch mode for automation
- ✅ Smart detection/auto-suggest
- ✅ Dry-run safety feature
- ✅ Clear documentation about profile customization
- ✅ Emphasis on starting with achievable goals
- ✅ Examples showing realistic thresholds
