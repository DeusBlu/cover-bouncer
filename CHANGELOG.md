# Changelog

All notable changes to CoverBouncer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[1.0.0-preview.1]: https://github.com/DeusBlu/cover-bouncer/releases/tag/v1.0.0-preview.1
