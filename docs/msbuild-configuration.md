# MSBuild Configuration Reference

CoverBouncer.MSBuild provides several MSBuild properties to customize behavior in your test projects.

## Available Properties

### `EnableCoverBouncer`
**Type:** `boolean`  
**Default:** `false`  
**Required:** Yes (must be set to `true` to activate)

Enables or disables CoverBouncer coverage verification during test runs.

**Usage:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
</PropertyGroup>
```

**Why opt-in by default?**
- Prevents accidental activation in projects not yet configured
- Allows gradual rollout across teams
- No surprise build failures

---

### `CoverBouncerCoverageReport`
**Type:** `string`  
**Default:** `$(MSBuildProjectDirectory)/TestResults/coverage.json`  
**Required:** No

Path to the Coverlet JSON coverage report file.

**When to customize:**
- Using a custom Coverlet output path via `CollectCoverageFile`
- Non-standard test project structure
- Custom test result directory

**Usage:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/custom-coverage/report.json</CoverBouncerCoverageReport>
</PropertyGroup>
```

**Must match your Coverlet configuration:**
```xml
<PropertyGroup>
  <!-- Coverlet generates the file -->
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>json</CoverletOutputFormat>
  <CoverletOutput>custom-coverage/report.json</CoverletOutput>
  
  <!-- CoverBouncer reads from same location -->
  <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/custom-coverage/report.json</CoverBouncerCoverageReport>
</PropertyGroup>
```

---

### `CoverBouncerConfigFile`
**Type:** `string`  
**Default:** `coverbouncer.json`  
**Required:** No

Name or path of the CoverBouncer configuration file.

**When to customize:**
- Using a different config file name
- Config file in a non-root location
- Multiple test projects sharing a config (use relative path)

**Usage:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerConfigFile>coverage-policy.json</CoverBouncerConfigFile>
</PropertyGroup>
```

**Relative paths:**
```xml
<!-- Load config from solution root -->
<PropertyGroup>
  <CoverBouncerConfigFile>../../coverbouncer.json</CoverBouncerConfigFile>
</PropertyGroup>
```

---

### `CoverBouncerFailOnViolations`
**Type:** `boolean`  
**Default:** `true`  
**Required:** No

Whether to fail the build when coverage thresholds are violated.

**When to set to `false`:**
- Introducing CoverBouncer to legacy projects (warning-only mode)
- CI/CD reporting without blocking builds
- Gradual enforcement strategy

**Usage:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerFailOnViolations>false</CoverBouncerFailOnViolations>
</PropertyGroup>
```

**Warning-only output:**
```
CoverBouncer: Coverage violations detected (build not failed):
  ❌ MyService.cs: Line coverage 65.00% < 70.00% (Standard)
  ❌ Calculator.cs: Branch coverage 55.00% < 60.00% (Standard)
```

---

## Complete Example

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="CoverBouncer.MSBuild" Version="1.0.0-preview.4" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp\MyApp.csproj" />
  </ItemGroup>

  <!-- Coverlet Configuration -->
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>json</CoverletOutputFormat>
    <CoverletOutput>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverletOutput>
    <Exclude>[xunit.*]*,[*.Tests]*</Exclude>
  </PropertyGroup>

  <!-- CoverBouncer Configuration -->
  <PropertyGroup>
    <EnableCoverBouncer>true</EnableCoverBouncer>
    <!-- Optional: Use custom paths -->
    <!-- <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/custom/coverage.json</CoverBouncerCoverageReport> -->
    <!-- <CoverBouncerConfigFile>../shared-coverage-policy.json</CoverBouncerConfigFile> -->
    <!-- <CoverBouncerFailOnViolations>false</CoverBouncerFailOnViolations> -->
  </PropertyGroup>

</Project>
```

---

## Common Scenarios

### Scenario 1: Custom Coverage Report Location

**Problem:** Using a custom Coverlet output directory.

**Solution:**
```xml
<PropertyGroup>
  <!-- Coverlet writes here -->
  <CoverletOutput>$(MSBuildProjectDirectory)/coverage-output/report.json</CoverletOutput>
  
  <!-- CoverBouncer reads from same place -->
  <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/coverage-output/report.json</CoverBouncerCoverageReport>
</PropertyGroup>
```

### Scenario 2: Shared Configuration Across Test Projects

**Problem:** Multiple test projects, want one shared `coverbouncer.json`.

**Solution:**
```
MySolution/
├── coverbouncer.json              ← Shared config in solution root
├── src/
│   └── MyApp/
└── tests/
    ├── MyApp.UnitTests/
    │   └── MyApp.UnitTests.csproj  ← Use relative path
    └── MyApp.IntegrationTests/
        └── MyApp.IntegrationTests.csproj  ← Use relative path
```

**In both test .csproj files:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerConfigFile>../../coverbouncer.json</CoverBouncerConfigFile>
</PropertyGroup>
```

### Scenario 3: Gradual Rollout (Warning-Only Mode)

**Problem:** Want to see violations but not fail builds yet.

**Solution:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <CoverBouncerFailOnViolations>false</CoverBouncerFailOnViolations>
</PropertyGroup>
```

### Scenario 4: CI/CD vs Local Development

**Problem:** Enforce in CI, warn locally.

**Solution:**
```xml
<PropertyGroup>
  <EnableCoverBouncer>true</EnableCoverBouncer>
  <!-- Fail in CI, warn locally -->
  <CoverBouncerFailOnViolations Condition="'$(CI)' == 'true'">true</CoverBouncerFailOnViolations>
  <CoverBouncerFailOnViolations Condition="'$(CI)' != 'true'">false</CoverBouncerFailOnViolations>
</PropertyGroup>
```

---

## Troubleshooting

### "Coverage report not found"

**Error:**
```
CoverBouncer: Coverage report not found: D:\MyProject\TestResults\coverage.json
```

**Causes:**
1. Coverlet isn't generating a report (check `CollectCoverage=true`)
2. Coverlet output path doesn't match `CoverBouncerCoverageReport`
3. Tests haven't run yet (report generated during test execution)

**Fix:**
```xml
<PropertyGroup>
  <!-- Ensure Coverlet is configured -->
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>json</CoverletOutputFormat>
  <CoverletOutput>TestResults/coverage.json</CoverletOutput>
  
  <!-- Ensure CoverBouncer path matches -->
  <CoverBouncerCoverageReport>$(MSBuildProjectDirectory)/TestResults/coverage.json</CoverBouncerCoverageReport>
</PropertyGroup>
```

### "Configuration file not found"

**Warning:**
```
CoverBouncer: Configuration file 'coverbouncer.json' not found.
```

**Fix:**
```bash
# Run in test project directory
dotnet tool install --global CoverBouncer.CLI
dotnet coverbouncer init
```

---

## See Also

- [Configuration Reference](configuration.md) - `coverbouncer.json` schema
- [Getting Started](getting-started.md) - Quick setup guide
- [Coverlet Integration](coverlet-integration.md) - Coverlet-specific setup
