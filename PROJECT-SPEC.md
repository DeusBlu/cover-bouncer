# Cover-Bouncer: Project Specification

**Version:** 1.0  
**Last Updated:** December 7, 2025  
**Status:** Initial Planning

---

## Executive Summary

Cover-Bouncer is a profile-based code coverage enforcement tool for .NET projects. It allows teams to set different coverage thresholds for different types of code (e.g., critical business logic vs. DTOs), integrates seamlessly with existing testing workflows, and blocks merges when coverage standards aren't met.

---

## Product Vision

### The Problem
Standard coverage tools provide a single metric (e.g., "75% coverage") that doesn't reflect code importance. Teams end up with:
- High coverage on trivial code (DTOs, auto-properties)
- Low coverage on critical business logic
- No way to enforce standards per code category
- "Coverage theater" - good numbers, poor quality

### The Solution
Cover-Bouncer enables profile-based coverage policies:
- Tag files with profiles (`[CoverageProfile("Critical")]`)
- Set different thresholds per profile
- Automatic enforcement via MSBuild integration
- CI/CD ready with merge blocking

### Target Audience
- .NET development teams using xUnit/NUnit/MSTest with Coverlet
- Teams wanting fine-grained coverage control
- Organizations with compliance or quality mandates
- Open source projects maintaining quality standards

---

## Architecture

### Three-Layer Design

#### 1. Core Engine (`CoverBouncer.Core`)
**Pure logic library with zero external dependencies**

**Inputs:**
- Configuration (profiles + thresholds)
- Coverage report (normalized: `file → line → hits`)
- Source tree (for reading profile tags)

**Outputs:**
- List of violations: `"file X violates profile Y; coverage A < required B"`

**Key Components:**
- `PolicyEngine` - Core validation logic
- `CoverageReport` - Normalized coverage data model
- `PolicyConfiguration` - Configuration model
- `ViolationResult` - Violation reporting

#### 2. Adapters
**Bridge between external tools and core engine**

**CoverBouncer.Coverlet:**
- Parses Coverlet JSON output
- Converts to normalized coverage model
- Handles Coverlet-specific quirks

**Tag Reader:**
- Uses Roslyn or text parsing
- Extracts `[CoverageProfile("Name")]` attributes from files
- Maps files to profile names
- Handles multiple tags per file

#### 3. Frontends
**User-facing components**

**CoverBouncer.CLI:**
- Dotnet global/local tool
- Commands: `init`, `verify`, `report`
- Exit codes for CI integration

**CoverBouncer.MSBuild:**
- NuGet package with MSBuild targets
- Auto-runs after `dotnet test`
- Configurable via MSBuild properties
- Main user-facing package

**CoverBouncer.Analyzers (Future):**
- Roslyn analyzer
- IDE warnings for missing/incorrect tags
- Quick fixes for common issues

---

## User Experience

### Installation & Setup

#### Step 1: Install Package
```bash
dotnet add MyApp.Tests package CoverBouncer.MSBuild
```

#### Step 2: Initialize Config
```bash
dotnet coverbouncer init
```

Generates `coverage-policy.json`:
```json
{
  "coverletReportPath": "TestResults/coverage.json",
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

#### Step 3: Enable Coverage Collection
Add to `Directory.Build.props` or test `.csproj`:
```xml
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
  <CoverletOutputFormat>json</CoverletOutputFormat>
  <EnableCoverBouncer>true</EnableCoverBouncer>
</PropertyGroup>
```

#### Step 4: Tag Files
```csharp
// [CoverageProfile("Critical")]
namespace MyApp.PaymentProcessing
{
    public class PaymentService
    {
        // This file must have 100% coverage
    }
}

// [CoverageProfile("Dto")]
namespace MyApp.Models
{
    public class CustomerDto
    {
        // This file doesn't need coverage
    }
}
```

#### Step 5: CI Integration
**No changes needed!** Existing `dotnet test` step now enforces coverage.

For explicit control:
```yaml
- name: Run tests
  run: dotnet test

- name: Verify coverage policy
  run: dotnet coverbouncer verify --coverage TestResults/coverage.json --config coverage-policy.json
```

Mark as required status check for branch protection.

---

## Configuration

### Single Configuration File: `coverage-policy.json`

Located at solution or project root. Contains all settings.

#### Full Schema
```json
{
  "$schema": "https://coverbouncer.dev/schema/v1/config.json",
  "coverletReportPath": "TestResults/coverage.json",
  "sourceRoot": "src/",
  "defaultProfile": "Standard",
  "profiles": {
    "Standard": {
      "minLine": 0.70,
      "minBranch": 0.60,
      "minMethod": 0.70
    },
    "BusinessLogic": {
      "minLine": 0.90,
      "minBranch": 0.80,
      "minMethod": 0.85
    },
    "Critical": {
      "minLine": 1.00,
      "minBranch": 1.00,
      "minMethod": 1.00
    },
    "Dto": {
      "minLine": 0.00,
      "minBranch": 0.00,
      "minMethod": 0.00
    },
    "Integration": {
      "minLine": 0.60,
      "minBranch": 0.50,
      "minMethod": 0.60
    }
  },
  "excludePatterns": [
    "**/obj/**",
    "**/bin/**",
    "**/*.Designer.cs",
    "**/Migrations/**"
  ],
  "failOnViolation": true,
  "reportFormat": "console"
}
```

### MSBuild Properties

Users can override via MSBuild properties:

```xml
<PropertyGroup>
  <!-- Enable/disable policy enforcement -->
  <EnableCoverBouncer>true</EnableCoverBouncer>
  
  <!-- Path to config file -->
  <CoverBouncerConfigFile>$(SolutionDir)coverage-policy.json</CoverBouncerConfigFile>
  
  <!-- Fail build on violation (default: true) -->
  <CoverBouncerFailOnViolation>true</CoverBouncerFailOnViolation>
  
  <!-- Verbosity: quiet, normal, detailed -->
  <CoverBouncerVerbosity>normal</CoverBouncerVerbosity>
</PropertyGroup>
```

---

## CLI Commands

### `dotnet coverbouncer init`
Generates default `coverage-policy.json` in current directory.

**Options:**
- `--output <path>` - Output path (default: `./coverage-policy.json`)
- `--template <name>` - Template: `basic`, `strict`, `relaxed` (default: `basic`)

**Example:**
```bash
dotnet coverbouncer init --template strict --output ./policies/coverage.json
```

---

### `dotnet coverbouncer verify`
Validates coverage against policy. Exits with non-zero code on violation.

**Options:**
- `--coverage <path>` - Path to Coverlet JSON (default: `TestResults/coverage.json`)
- `--config <path>` - Path to policy config (default: `coverage-policy.json`)
- `--fail-on-violation` - Exit non-zero on violation (default: true)
- `--output <format>` - Output format: `console`, `json`, `markdown` (default: `console`)

**Example:**
```bash
dotnet coverbouncer verify \
  --coverage TestResults/coverage.json \
  --config coverage-policy.json \
  --output json
```

**Exit Codes:**
- `0` - No violations
- `1` - Policy violations found
- `2` - Configuration error
- `3` - Coverage report not found

---

### `dotnet coverbouncer report`
Generates detailed coverage report by profile.

**Options:**
- `--coverage <path>` - Path to Coverlet JSON
- `--config <path>` - Path to policy config
- `--format <type>` - Format: `console`, `html`, `markdown`, `json`
- `--output <path>` - Output file path (default: stdout)

**Example:**
```bash
dotnet coverbouncer report --format html --output coverage-report.html
```

---

## File Tagging System

### Syntax
File-level comment at the top of C# files:
```csharp
// [CoverageProfile("ProfileName")]
```

### Multiple Profiles (Future)
```csharp
// [CoverageProfile("BusinessLogic,Critical")]
```
File must meet requirements for ALL specified profiles.

### Detection Mechanism
1. **Roslyn-based** (preferred): Parse syntax tree, find attribute-like comments
2. **Regex fallback**: Simple pattern matching for comment-based tags

### Untagged Files
Files without tags use `defaultProfile` from config.

---

## CI/CD Integration Strategy

### Philosophy
**Don't hack Git - integrate with CI/CD workflows**

### GitHub Actions
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
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Run tests with coverage
        run: dotnet test
        # CoverBouncer.MSBuild auto-enforces policy
      
      # Alternatively, explicit verification:
      # - name: Verify coverage policy
      #   run: dotnet coverbouncer verify
```

**Branch Protection:**
- Mark "test" job as required status check
- PR cannot merge if job fails
- Coverage violations = blocked merge

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
  displayName: 'Run tests'
  inputs:
    command: 'test'
    # Auto-enforces via MSBuild target

- task: PublishTestResults@2
  condition: always()
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/TestResults/*.trx'
```

### GitLab CI
```yaml
test:
  stage: test
  script:
    - dotnet restore
    - dotnet test
  rules:
    - if: '$CI_PIPELINE_SOURCE == "merge_request_event"'
```

---

## MSBuild Integration Details

### Target Flow
```xml
<Target Name="RunCoverBouncer"
        AfterTargets="VSTest"
        Condition="'$(EnableCoverBouncer)' == 'true'">
  
  <PropertyGroup>
    <CoverBouncerConfig Condition="'$(CoverBouncerConfigFile)' == ''">
      $(SolutionDir)coverage-policy.json
    </CoverBouncerConfig>
  </PropertyGroup>
  
  <!-- Execute policy verification -->
  <Exec Command="dotnet coverbouncer verify --coverage $(CoverletOutput) --config $(CoverBouncerConfig)"
        IgnoreExitCode="false"
        WorkingDirectory="$(MSBuildProjectDirectory)" />
</Target>
```

### Package Structure
```
CoverBouncer.MSBuild.nupkg
├── build/
│   └── CoverBouncer.MSBuild.targets
├── tools/
│   └── coverbouncer.exe (or dotnet tool reference)
└── buildTransitive/
    └── CoverBouncer.MSBuild.targets
```

---

## Implementation Phases

### Phase 1: Core Foundation (MVP)
**Goal:** Working end-to-end prototype

- [ ] `CoverBouncer.Core` library
  - [ ] Policy configuration model
  - [ ] Normalized coverage model
  - [ ] Policy engine with violation detection
- [ ] `CoverBouncer.Coverlet` adapter
  - [ ] Parse Coverlet JSON
  - [ ] Convert to normalized model
- [ ] Tag reader (regex-based)
  - [ ] Parse `// [CoverageProfile("Name")]` comments
  - [ ] Map files to profiles
- [ ] `CoverBouncer.CLI` tool
  - [ ] `init` command
  - [ ] `verify` command
  - [ ] Console output
- [ ] Basic tests for all components

**Success Criteria:**
- Can run `dotnet coverbouncer verify` and get violations
- Config file schema validated
- Coverlet JSON parsed correctly

---

### Phase 2: MSBuild Integration
**Goal:** Drop-in NuGet package experience

- [ ] `CoverBouncer.MSBuild` package
  - [ ] MSBuild targets file
  - [ ] Auto-run after `dotnet test`
  - [ ] Property-based configuration
- [ ] NuGet packaging
  - [ ] Package metadata
  - [ ] Icons, README
  - [ ] Version management
- [ ] Integration tests
  - [ ] Test project setup
  - [ ] Verify auto-execution
  - [ ] Verify build failures

**Success Criteria:**
- `dotnet add package CoverBouncer.MSBuild` works
- `dotnet test` auto-enforces policy
- Build fails on violations

---

### Phase 3: Polish & Documentation
**Goal:** Production-ready release

- [ ] Enhanced CLI
  - [ ] `report` command
  - [ ] Multiple output formats (JSON, Markdown, HTML)
  - [ ] Better error messages
- [ ] Documentation
  - [ ] Getting started guide
  - [ ] Configuration reference
  - [ ] CI/CD integration examples
  - [ ] Profile tagging guide
- [ ] Sample projects
  - [ ] Demo repository
  - [ ] CI/CD examples (GitHub, Azure, GitLab)
- [ ] GitHub repository setup
  - [ ] CI/CD for the project itself
  - [ ] Issue templates
  - [ ] Contributing guide

**Success Criteria:**
- Complete documentation
- Working examples for all major CI platforms
- Ready for public release

---

### Phase 4: Advanced Features (Future)
**Goal:** Enhanced developer experience

- [ ] Roslyn-based tag reader
  - [ ] Proper syntax tree parsing
  - [ ] Better error reporting
- [ ] `CoverBouncer.Analyzers`
  - [ ] IDE warnings for missing tags
  - [ ] Quick fixes
  - [ ] Code snippets
- [ ] Advanced reporting
  - [ ] Trend analysis
  - [ ] HTML dashboards
  - [ ] Badge generation
- [ ] Additional adapters
  - [ ] Fine Code Coverage
  - [ ] OpenCover
  - [ ] dotCover

---

## Technical Stack

### Languages & Frameworks
- **C# 12** / **.NET 8** (target framework)
- **MSBuild** for integration
- **Roslyn** for tag parsing (Phase 4)

### Dependencies
- **Minimal external dependencies**
- `System.Text.Json` for JSON parsing
- `Microsoft.Build.Framework` for MSBuild targets
- `Microsoft.CodeAnalysis.CSharp` for Roslyn (Phase 4)

### Testing
- **xUnit** for unit tests
- **FluentAssertions** for assertions
- **Verify** for snapshot testing
- Integration tests with real projects

### Tooling
- **NuGet** for packaging
- **GitHub Actions** for CI/CD
- **SonarCloud** or similar for code quality
- **Codecov** for coverage (dogfooding!)

---

## Success Metrics

### Adoption
- NuGet package downloads
- GitHub stars/forks
- Community contributions

### Quality
- Minimal bug reports
- Fast issue resolution
- High test coverage (dogfooding our own tool!)

### Developer Satisfaction
- Positive feedback on ease of use
- Low setup friction
- Clear, helpful error messages

---

## Risks & Mitigations

### Risk: Coverlet JSON Format Changes
**Mitigation:** Version detection, graceful degradation, clear error messages

### Risk: MSBuild Target Conflicts
**Mitigation:** Unique target names, conservative AfterTargets, allow opt-out

### Risk: Performance Impact on Build
**Mitigation:** Fast policy engine, parallel processing, caching where possible

### Risk: Tag Detection Failures
**Mitigation:** Regex fallback, clear error messages, validation tooling

---

## Open Questions (To Be Resolved)

1. **Tag syntax:** Comment-based vs. actual C# attributes?
   - **Current decision:** Comments for Phase 1 (simpler), attributes later
   
2. **Multi-profile support:** Should one file support multiple profiles?
   - **Tentative:** Yes, for Phase 2+
   
3. **Branch vs. line coverage priority:** Which is more important?
   - **Current:** Support both, let users decide
   
4. **Report storage:** Should we store historical coverage data?
   - **Future consideration:** Phase 4+

---

## Next Steps

1. ✅ Complete project specification
2. ✅ Set up Git repository
3. ⏭️ **Create solution structure**
4. ⏭️ **Implement Phase 1: Core Foundation**
5. ⏭️ **Write comprehensive tests**
6. ⏭️ **Build MSBuild integration**
7. ⏭️ **Documentation & samples**
8. ⏭️ **Public release**

---

**Document Version:** 1.0  
**Status:** Ready for Implementation
