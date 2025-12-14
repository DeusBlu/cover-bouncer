# 🚪 Cover-Bouncer

**The Coverage Doorman for .NET Projects**

Cover-Bouncer enforces profile-based code coverage policies in your .NET projects. Tag your files, set thresholds, and let the bouncer keep your coverage standards high.

## Why Cover-Bouncer?

Standard coverage tools give you one number: "X% coverage." But not all code is equal:
- Your critical business logic should have 90%+ coverage
- Your DTOs might not need any tests
- Your integration adapters need moderate coverage

**Cover-Bouncer lets you enforce different coverage requirements for different parts of your codebase.**

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

### 4. Tag Your Files
```csharp
// [CoverageProfile("Critical")]
namespace MyApp.PaymentProcessing
{
    public class PaymentService { }
}
```

### 5. Run Tests
```bash
dotnet test
```

Coverage policy automatically enforced! ✅

## Features

- 🎯 **Profile-Based Coverage** - Different thresholds for different code types
- 🔌 **Drop-In Integration** - Works with your existing `dotnet test` workflow
- 🚫 **CI/CD Ready** - Blocks merges when coverage drops below thresholds
- 📦 **NuGet Packaged** - Easy to install and distribute
- 🏷️ **File-Level Tags** - Simple attribute-based profile assignment
- ⚙️ **Single Config File** - No configuration sprawl
- 🔧 **Auto-Configuration** - Automatically excludes CoverBouncer from coverage

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
  "defaultProfile": "Standard",
  "profiles": {
    "Critical": {
      "lineCoverageThreshold": 90.0,
      "description": "Business-critical code (payment, security, etc.)"
    },
    "Standard": {
      "lineCoverageThreshold": 60.0,
      "description": "Regular application code"
    },
    "Integration": {
      "lineCoverageThreshold": 40.0,
      "description": "External service adapters"
    },
    "Dto": {
      "lineCoverageThreshold": 0.0,
      "description": "Data transfer objects"
    }
  }
}
```

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
</PropertyGroup>
```

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

- [Getting Started Guide](./docs/getting-started.md)
- [Configuration Reference](./docs/configuration.md)

## Contributing

We welcome contributions! See [CONTRIBUTING.md](./CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](./LICENSE) for details.

---

Built with ❤️ for the .NET community
