# Changelog

All notable changes to CoverBouncer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Automated File Tagging** 🏷️
  - Interactive tagging mode: `dotnet coverbouncer tag --interactive`
    - Prompts for file pattern
    - Preview matching files
    - Select profile from list
    - Confirm before applying
  - Auto-suggest mode: `dotnet coverbouncer tag --auto-suggest`
    - Automatically suggests profiles based on file naming patterns
    - Smart detection: Controllers→Integration, Services→BusinessLogic, etc.
    - Shows grouped suggestions for review
  - Batch tagging modes:
    - Pattern-based: `--pattern "**/*Service.cs" --profile BusinessLogic`
    - Directory-based: `--path "./Security" --profile Critical`
    - File list: `--files services.txt --profile Standard`
  - Safety features:
    - `--dry-run` to preview changes without modifying files
    - `--backup` to create backup files before tagging
    - Validation against profiles in coverbouncer.json
    - Skip files already tagged with same profile

- **New Core Components**
  - `FileTagWriter` class for writing profile tags to source files
  - `FileTaggingService` class for batch operations and pattern matching
  - Support for glob patterns via Microsoft.Extensions.FileSystemGlobbing

- **Comprehensive Test Coverage**
  - Added 50 new tests for file tagging features (71 tests total, all passing)
  - `FileTagWriterTests` - 20 tests for tag writing, removal, and edge cases
  - `FileTaggingServiceTests` - 30 tests for batch operations, pattern matching, and suggestions
  - Full coverage of positive paths, negative paths, and edge cases
  - Ensures file safety with backup, dry-run, and error handling tests
  - See TEST-COVERAGE-SUMMARY.md for details

### Changed
- **Documentation Overhaul - "Profiles Are Customizable!"**
  - README now prominently states profiles are completely customizable
  - Added examples of custom profile names (MustHaveTests, LegacyCode, WillFixLater, etc.)
  - Emphasizes starting with achievable goals based on current coverage
  - Removed intimidating tone suggesting 80-100% is mandatory
  - Added realistic examples: "30% now? Start with 35%"
  - Focus on improvement, not perfection
  
- **Enhanced Documentation**
  - Updated getting-started.md with profile customization section
  - Added comprehensive tagging workflows (manual + automated)
  - Created new docs/tagging-guide.md with:
    - All tagging modes explained with examples
    - Best practices and troubleshooting
    - Use cases: new project, legacy project, gradual migration
    - Git workflow integration
  - Updated CLI help to include tag command options

### Dependencies
- Added Microsoft.Extensions.FileSystemGlobbing 8.0.0 to CoverBouncer.Core

## [1.0.0-preview.2] - 2024-12-13

### Added
- **Automatic Coverlet Exclusions**
  - CoverBouncer assemblies are now automatically excluded from Coverlet instrumentation via `buildTransitive` targets
  - Eliminates "missing symbols" warnings for CoverBouncer NuGet packages
  - No manual configuration required - works out of the box

- **Enhanced Init Command**
  - Detects Coverlet usage in project files
  - Prompts user to add recommended test framework exclusions to Directory.Build.props
  - Auto-configures exclusions for: xunit, FluentAssertions, Moq, NSubstitute
  - Creates or updates Directory.Build.props with optimal Coverlet settings
  - User-friendly prompts with clear explanations

### Changed
- Updated MSBuild targets to automatically append `[CoverBouncer.*]*` to Coverlet's `<Exclude>` property

### Documentation
- Added comprehensive "Coverlet Integration" section to README
- Enhanced getting-started.md with Coverlet best practices
- Added troubleshooting section for Coverlet-related warnings
- Documented automatic exclusions and optional test framework exclusions
- Included recommended Directory.Build.props patterns

## [1.0.0-preview.1] - 2024-12-13

### Added
- **Core Features**
  - Profile-based code coverage enforcement
  - File-level profile tagging via comments: `// [CoverageProfile("ProfileName")]`
  - Configurable coverage thresholds per profile
  - Support for multiple profiles: Critical, BusinessLogic, Standard, Dto, etc.
  - Default profile for untagged files

- **MSBuild Integration** (CoverBouncer.MSBuild)
  - Automatic execution after `dotnet test`
  - Integrates with coverlet.msbuild for coverage collection
  - Clear violation reporting grouped by profile
  - Build fails when coverage violations detected
  - Configurable via MSBuild properties

- **CLI Tool** (CoverBouncer.CLI)
  - `coverbouncer init` - Generate configuration file with templates (basic, strict, relaxed)
  - `coverbouncer verify` - Validate coverage against policy
  - Installable as global or local dotnet tool

- **Configuration**
  - JSON-based configuration (`coverbouncer.json`)
  - Profile definitions with thresholds and descriptions
  - Hierarchical configuration (searches parent directories)
  - Built-in templates for quick setup

- **Testing**
  - 7 comprehensive validation test scenarios
  - Integration with Coverlet JSON format
  - Coverage report parsing and validation

### Technical Details
- Target Framework: .NET 8.0
- Dependencies: Minimal (MSBuild APIs, System.Text.Json)
- Package Size: MSBuild (38KB), CLI (254KB)
- Coverage Format: Coverlet JSON

### Known Limitations
- Only line coverage supported (branch/method coverage planned for future)
- Requires coverlet.msbuild for coverage collection
- Configuration file must be named `coverbouncer.json`
- Profile tags are case-sensitive

### Installation

```bash
# Install MSBuild package in test project
dotnet add package CoverBouncer.MSBuild --version 1.0.0-preview.1
dotnet add package coverlet.msbuild

# Install CLI tool globally
dotnet tool install -g CoverBouncer.CLI --version 1.0.0-preview.1
```

### Usage

1. Initialize configuration: `coverbouncer init`
2. Tag source files: `// [CoverageProfile("Critical")]`
3. Enable in test project:
   ```xml
   <PropertyGroup>
     <CollectCoverage>true</CollectCoverage>
     <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
     <CoverletOutputFormat>json</CoverletOutputFormat>
     <EnableCoverBouncer>true</EnableCoverBouncer>
   </PropertyGroup>
   ```
4. Run tests: `dotnet test`

### Feedback & Support

This is a preview release. Please report issues and provide feedback:
- GitHub Issues: https://github.com/DeusBlu/cover-bouncer/issues
- Discussions: https://github.com/DeusBlu/cover-bouncer/discussions

### What's Next

See [ROADMAP.md](ROADMAP.md) for planned features including:
- Branch and method coverage support
- Roslyn analyzers for IDE integration
- HTML coverage reports
- Baseline coverage tracking
- VS Code extension

---

## [Unreleased]

### Planned for 1.0.0 Stable
- Incorporate preview feedback
- Additional test framework support verification
- Performance optimizations for large projects
- Enhanced error messages
- Additional configuration options

---

[1.0.0-preview.2]: https://github.com/DeusBlu/cover-bouncer/releases/tag/v1.0.0-preview.2
[1.0.0-preview.1]: https://github.com/DeusBlu/cover-bouncer/releases/tag/v1.0.0-preview.1
