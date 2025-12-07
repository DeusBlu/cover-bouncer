# Cover-Bouncer: Implementation Roadmap

**Status:** ✅ Project scaffolded and ready for implementation  
**Current Phase:** Ready to begin Phase 1 - Core Foundation

---

## ✅ Completed Setup Tasks

### Repository & Infrastructure
- [x] Git repository initialized
- [x] Solution structure created (`CoverBouncer.sln`)
- [x] Directory.Build.props configured with common settings
- [x] .gitignore configured for .NET projects
- [x] MIT License added
- [x] Initial commit created

### Project Structure
- [x] `src/CoverBouncer.Core` - Core policy engine library
- [x] `src/CoverBouncer.Coverlet` - Coverlet adapter library
- [x] `src/CoverBouncer.CLI` - CLI console application
- [x] `src/CoverBouncer.MSBuild` - MSBuild integration library
- [x] `tests/CoverBouncer.Core.Tests` - Core unit tests
- [x] `tests/CoverBouncer.Coverlet.Tests` - Coverlet adapter tests
- [x] `tests/CoverBouncer.Integration.Tests` - Integration tests

### Documentation
- [x] README.md with project overview
- [x] PROJECT-SPEC.md with complete technical specification
- [x] CONTRIBUTING.md with contribution guidelines
- [x] docs/getting-started.md with user onboarding guide

### Build Verification
- [x] Solution builds successfully
- [x] All projects compile without errors
- [x] Test projects are configured with xUnit

---

## 🎯 Next Steps: Phase 1 - Core Foundation

### Overview
Build the foundational components that make Cover-Bouncer work.

### Tasks Breakdown

#### 1. CoverBouncer.Core - Policy Models & Configuration (Priority: High)

**Models to Create:**
- [ ] `PolicyConfiguration.cs` - Configuration model
  - DefaultProfile property
  - Dictionary of profiles
  - Coverlet report path
  - Exclude patterns
  - Validation logic

- [ ] `CoverageProfile.cs` - Profile model
  - MinLine, MinBranch, MinMethod thresholds
  - Profile name
  - Validation methods

- [ ] `CoverageReport.cs` - Normalized coverage model
  - File → Line → Hit count mapping
  - Branch coverage data
  - Method coverage data
  - From Coverlet adapter

- [ ] `FileCoverage.cs` - Per-file coverage data
  - File path
  - Line rate, branch rate, method rate
  - Line details (number, hits)

- [ ] `ViolationResult.cs` - Violation reporting
  - File path
  - Profile name
  - Required vs actual coverage
  - Specific lines/branches missing
  - Formatted message

**Configuration Serialization:**
- [ ] JSON serialization/deserialization
- [ ] Schema validation
- [ ] Default config generation

#### 2. CoverBouncer.Core - Policy Engine (Priority: High)

**Core Logic:**
- [ ] `PolicyEngine.cs` - Main validation logic
  - `Validate()` method: takes config + coverage → returns violations
  - Per-file profile matching
  - Threshold comparison
  - Violation aggregation

- [ ] `ProfileMatcher.cs` - Maps files to profiles
  - Read file tags from source
  - Apply default profile to untagged files
  - Handle tag parsing errors

#### 3. CoverBouncer.Coverlet - Adapter (Priority: High)

**Coverlet Integration:**
- [ ] `CoverletReportParser.cs` - Parse Coverlet JSON
  - Read Coverlet JSON schema
  - Extract modules, classes, methods
  - Calculate line/branch/method coverage per file
  
- [ ] `CoverletToCoverageReportAdapter.cs` - Convert to normalized model
  - Map Coverlet data → `CoverageReport`
  - Handle edge cases (partial classes, generated code)

**Sample Coverlet JSON handling:**
```json
{
  "MyApp.dll": {
    "MyNamespace.MyClass": {
      "Methods": {
        "MyMethod": {
          "Lines": {
            "10": 5,
            "11": 5,
            "12": 0
          }
        }
      }
    }
  }
}
```

#### 4. Tag Reader (Priority: Medium)

**File Tag Parsing:**
- [ ] `FileTagReader.cs` - Extract profile tags from files
  - Regex-based parsing (Phase 1)
  - Find `// [CoverageProfile("Name")]` comments
  - Handle multiple files efficiently
  - Return file → profile mapping

**Pattern to match:**
```csharp
// [CoverageProfile("Critical")]
// or
// [CoverageProfile("BusinessLogic")]
```

#### 5. CoverBouncer.CLI - Basic Commands (Priority: High)

**CLI Structure:**
- [ ] Command-line argument parsing (use `System.CommandLine`)
- [ ] `InitCommand.cs` - Generate default config
  - Template selection (basic, strict, relaxed)
  - Output path configuration
  
- [ ] `VerifyCommand.cs` - Validate coverage
  - Load config
  - Load Coverlet report
  - Run policy engine
  - Display violations
  - Return appropriate exit codes (0=success, 1=violations, 2=error)

**Console Output:**
- [ ] Formatted violation reporting
- [ ] Color-coded output (green=pass, red=fail)
- [ ] Summary statistics

#### 6. Testing (Priority: High)

**Unit Tests:**
- [ ] `PolicyEngineTests.cs` - Core validation logic
  - Test violation detection
  - Test threshold comparisons
  - Test edge cases (0%, 100%, exactly at threshold)
  
- [ ] `CoverletParserTests.cs` - Coverlet JSON parsing
  - Valid JSON handling
  - Invalid JSON handling
  - Various Coverlet formats
  
- [ ] `ConfigurationTests.cs` - Config validation
  - Valid configs
  - Invalid configs
  - Default config generation
  
- [ ] `FileTagReaderTests.cs` - Tag parsing
  - Valid tags
  - Missing tags
  - Malformed tags

**Test Data:**
- [ ] Sample Coverlet JSON files
- [ ] Sample configuration files
- [ ] Sample source files with tags

---

## 📋 Implementation Checklist for Phase 1

### Week 1: Core Models & Configuration
- [ ] Create all model classes
- [ ] Implement JSON serialization
- [ ] Write configuration validation
- [ ] Unit tests for models
- [ ] Documentation comments (XML docs)

### Week 2: Policy Engine & Coverlet Adapter
- [ ] Implement PolicyEngine core logic
- [ ] Implement Coverlet parser
- [ ] Create adapter to normalized model
- [ ] Unit tests for engine
- [ ] Unit tests for adapter

### Week 3: Tag Reader & CLI
- [ ] Implement file tag reader (regex)
- [ ] Create CLI project structure
- [ ] Implement `init` command
- [ ] Implement `verify` command
- [ ] Unit tests for tag reader
- [ ] Integration tests for CLI

### Week 4: Polish & Testing
- [ ] Comprehensive test coverage
- [ ] Error handling refinement
- [ ] Console output formatting
- [ ] Documentation updates
- [ ] End-to-end testing

---

## 🎨 Code Quality Standards

### All Code Must Have:
- ✅ XML documentation comments on public APIs
- ✅ Unit tests with >80% coverage (dogfooding!)
- ✅ Meaningful variable/method names
- ✅ Error handling with clear messages
- ✅ Null safety (nullable reference types enabled)

### Naming Conventions:
- Classes: PascalCase
- Methods: PascalCase
- Properties: PascalCase
- Private fields: _camelCase
- Parameters: camelCase
- Constants: PascalCase

### File Organization:
- One class per file
- File name matches class name
- Organize by feature/layer

---

## 🚀 Success Criteria for Phase 1

**Must Have:**
1. ✅ Core engine validates coverage against policies
2. ✅ Coverlet JSON is parsed correctly
3. ✅ CLI `init` command generates valid config
4. ✅ CLI `verify` command detects violations
5. ✅ Exit codes work for CI integration
6. ✅ All unit tests pass
7. ✅ Code coverage >80% on core components

**Nice to Have:**
1. Color-coded console output
2. Multiple config templates
3. Detailed violation messages
4. Performance benchmarks

---

## 📊 Definition of Done

A task is complete when:
- [ ] Code is written and compiles
- [ ] Unit tests written and passing
- [ ] XML documentation added
- [ ] No compiler warnings
- [ ] Code reviewed (self or peer)
- [ ] Committed to Git with meaningful message

---

## 🔄 Daily Development Workflow

1. **Pull latest** (if collaborating)
2. **Pick a task** from the checklist
3. **Write failing test** (TDD approach)
4. **Implement feature** to make test pass
5. **Refactor** for clarity
6. **Run all tests** (`dotnet test`)
7. **Commit** with conventional commit message
8. **Repeat**

---

## 📝 Useful Commands

```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=json

# Run specific test project
dotnet test tests/CoverBouncer.Core.Tests

# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Add package to project
dotnet add src/CoverBouncer.CLI package System.CommandLine

# Create new test class
dotnet new xunit -n MyNewTests -o tests/MyNewTests
```

---

## 🎯 Focus Areas

**This Week:**
- Core models and configuration (start here!)
- Policy engine foundation
- Basic Coverlet parsing

**Next Week:**
- Complete Coverlet adapter
- Tag reader implementation
- CLI scaffolding

**Following Week:**
- CLI commands implementation
- Integration testing
- Documentation refinement

---

## 📚 Resources

### .NET Documentation
- [System.Text.Json](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to)
- [xUnit Testing](https://xunit.net/)
- [Nullable Reference Types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)

### Coverlet
- [Coverlet GitHub](https://github.com/coverlet-coverage/coverlet)
- [Coverlet JSON Format](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md)

### CLI Tools
- [System.CommandLine](https://github.com/dotnet/command-line-api)

---

**Ready to start coding!** 🚀

**Next Action:** Begin implementing `PolicyConfiguration.cs` in `CoverBouncer.Core`
