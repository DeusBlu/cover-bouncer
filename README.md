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
dotnet add MyApp.Tests package CoverBouncer.MSBuild
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

## How It Works

1. **Tag files** with coverage profiles using file-level attributes
2. **Set thresholds** in `coverage-policy.json`
3. **Run tests** - Cover-Bouncer validates coverage automatically
4. **Build fails** if any file violates its profile's threshold

## Project Structure

- `CoverBouncer.Core` - Core coverage policy engine
- `CoverBouncer.Coverlet` - Coverlet adapter for coverage data
- `CoverBouncer.MSBuild` - MSBuild integration (main user package)
- `CoverBouncer.Analyzers` - Roslyn analyzers (coming soon)

## Documentation

- [Getting Started Guide](./docs/getting-started.md)
- [Configuration Reference](./docs/configuration.md)
- [CI/CD Integration](./docs/ci-cd-integration.md)
- [Profile Tagging Guide](./docs/profile-tagging.md)

## Contributing

We welcome contributions! See [CONTRIBUTING.md](./CONTRIBUTING.md) for details.

## License

MIT License - see [LICENSE](./LICENSE) for details.

---

Built with ❤️ for the .NET community
