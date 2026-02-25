# Coverlet Integration Quick Reference

## What's New

CoverBouncer now automatically excludes itself from Coverlet instrumentation, preventing warnings about missing debug symbols.

## Automatic Features (No Configuration Required)

When you install `CoverBouncer.MSBuild`, the following happens automatically:

### ✅ CoverBouncer Assemblies Excluded

All CoverBouncer DLLs are automatically excluded from coverage:
- `[CoverBouncer.Core]*`
- `[CoverBouncer.Coverlet]*`
- `[CoverBouncer.MSBuild]*`

**You don't need to do anything!** This happens via `buildTransitive` targets.

## Optional Enhancements

### Enhanced `init` Command

When you run `dotnet coverbouncer init`, you'll now be prompted to optimize your Coverlet configuration:

```bash
dotnet coverbouncer init

# Output:
# ✅ Created coverbouncer.json with 'basic' template
# 
# ℹ️  Coverlet detected!
# 
# 💡 Recommendation: Exclude test frameworks from coverage to improve performance
#    and reduce noise. CoverBouncer assemblies are already excluded automatically.
# 
# Would you like to add recommended exclusions to Directory.Build.props? (Y/n):
```

**If you answer Yes (default):**
- Creates or updates `Directory.Build.props`
- Adds exclusions for common test frameworks
- Improves test performance
- Reduces coverage report noise

### What Gets Excluded

Test frameworks and mocking libraries that don't need coverage:
```xml
<Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
```

## Understanding Coverlet's Assembly-Level Instrumentation

This is important context for understanding CoverBouncer's behavior with filtered test runs.

### How Coverlet Works

Coverlet instruments assemblies **before** tests run. It rewrites IL code to insert hit counters on every line and branch. This happens at the **assembly level** — every file in the assembly gets instrumented, regardless of which tests will actually run.

### The Filtered Run Problem

When you run `dotnet test --filter "Category=Unit"`, the sequence is:

1. **Coverlet instruments** the entire assembly (all files get hit counters)
2. **Test runner applies** the `--filter` (only matching tests execute)
3. **Coverlet reports** results for ALL instrumented files

Files not targeted by the filtered tests appear with **0 hits on every line** — they look like they have 0% coverage. But this is misleading: no tests were supposed to run against them.

### How CoverBouncer Handles This

CoverBouncer automatically detects filtered runs via `$(VSTestTestCaseFilter)` and **skips** files with zero covered lines. This eliminates false failures.

**No Coverlet configuration can solve this** — Coverlet has no concept of "which files a test targets." The solution must be in the validation layer, which is exactly where CoverBouncer handles it.

See [README - Filtered Test Runs](../README.md#-filtered-test-runs---filter) for usage details and examples.

## Best Practices

### 1. Use Directory.Build.props for Test Projects

Create `Directory.Build.props` in your test folder:

```xml
<Project>
  <PropertyGroup Condition="'$(IsTestProject)' == 'true'">
    <!-- Coverlet Configuration -->
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
    <CoverletOutputFormat>json</CoverletOutputFormat>
    
    <!-- CoverBouncer Configuration -->
    <EnableCoverBouncer>true</EnableCoverBouncer>
    
    <!-- Optional: Exclude test frameworks -->
    <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*;[Moq]*;[NSubstitute]*</Exclude>
  </PropertyGroup>
</Project>
```

### 2. Include Only Production Code (Alternative Approach)

For maximum control, explicitly include only your production assemblies:

```xml
<PropertyGroup>
  <Include>[MyApp]*;[MyApp.Core]*;[MyApp.Services]*</Include>
</PropertyGroup>
```

This approach:
- ✅ Ensures only production code is covered
- ✅ Automatically excludes test code and dependencies
- ✅ Provides the cleanest coverage reports

### 3. Per-Project Configuration

If you need different settings per test project, configure directly in `.csproj`:

```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerConfigFile>../coverbouncer.json</CoverBouncerConfigFile>
  
  <!-- Project-specific coverage settings -->
  <Include>[MySpecificAssembly]*</Include>
</PropertyGroup>
```

## Migration Guide

### If You Previously Configured Exclusions Manually

**Before (manual configuration):**
```xml
<PropertyGroup>
  <Include>[MyApp]*</Include>
  <!-- Manual exclusion to avoid CoverBouncer warnings -->
  <Exclude>$(Exclude);[CoverBouncer.*]*</Exclude>
</PropertyGroup>
```

**After (automatic):**
```xml
<PropertyGroup>
  <!-- CoverBouncer exclusions are automatic! -->
  <Include>[MyApp]*</Include>
</PropertyGroup>
```

Or even simpler, use exclusions instead:
```xml
<PropertyGroup>
  <!-- Only need to exclude test frameworks now -->
  <Exclude>$(Exclude);[xunit.*]*;[FluentAssertions]*</Exclude>
</PropertyGroup>
```

### Recommended Migration Steps

1. **Update CoverBouncer.MSBuild** to latest version
2. **Remove manual exclusions** for CoverBouncer assemblies
3. **Run `dotnet coverbouncer init`** to get recommended test framework exclusions
4. **Test your build** to verify no warnings

## Troubleshooting

### Still Seeing Warnings?

If you still see warnings about CoverBouncer assemblies:

1. **Check your version:**
   ```bash
   dotnet list package | grep CoverBouncer
   ```
   Should be v1.0.0-preview.1 or later

2. **Verify targets are imported:**
   ```bash
   # Check that buildTransitive targets are in your packages
   ls ~/.nuget/packages/coverbouncer.msbuild/*/buildTransitive/
   ```

3. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet build
   dotnet test
   ```

### Manual Override (if needed)

If automatic exclusions don't work for some reason:

```xml
<PropertyGroup>
  <Exclude>$(Exclude);[CoverBouncer.*]*</Exclude>
</PropertyGroup>
```

## Performance Tips

For fastest test execution:

1. **Exclude test frameworks** (done via `init` command)
2. **Use Include patterns** to target only production code
3. **Exclude third-party libraries** you don't control
4. **Use parallel test execution** with coverage:
   ```xml
   <CollectCoverage>true</CollectCoverage>
   <CoverletOutputFormat>json</CoverletOutputFormat>
   ```

## Questions?

- 📖 See [Getting Started Guide](docs/getting-started.md)
- 📖 See [Configuration Reference](docs/configuration.md)
- 💬 Open an issue on GitHub
