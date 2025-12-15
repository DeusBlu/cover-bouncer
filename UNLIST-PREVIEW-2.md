# Unlisting Preview.2 and Publishing Preview.3

## Issue
Preview.2 generates Coverlet instrumentation warnings when users run tests:
```
warning : Mono.Cecil.Cil.SymbolsNotFoundException: No symbol found for file: ...\CoverBouncer.MSBuild.dll
```

## Fix
Added `ExcludeByFile` pattern in `CoverBouncer.MSBuild.targets` to prevent Coverlet from trying to instrument CoverBouncer assemblies.

## Steps to Remediate

### 1. Unlist Preview.2 from NuGet.org

**CoverBouncer.MSBuild:**
1. Go to https://www.nuget.org/packages/CoverBouncer.MSBuild/1.0.0-preview.2
2. Click "Manage Package"
3. Click "Unlist" (don't delete - keeps existing users working, just hides from search)

**CoverBouncer.CLI:**
1. Go to https://www.nuget.org/packages/CoverBouncer.CLI/1.0.0-preview.2
2. Click "Manage Package"
3. Click "Unlist"

### 2. Build and Publish Preview.3

```bash
# Build packages
./build.bat

# Verify packages created
ls nupkg/*.nupkg

# Should see:
# - CoverBouncer.MSBuild.1.0.0-preview.3.nupkg
# - CoverBouncer.CLI.1.0.0-preview.3.nupkg

# Publish to NuGet (requires API key)
./publish-nuget.bat
```

### 3. Test in Real Project

Before publishing, test in ReActiveBox.Api.Tests:

```bash
# In ReActiveBox project
dotnet remove package CoverBouncer.MSBuild
dotnet add package CoverBouncer.MSBuild --version 1.0.0-preview.3 --source D:\Projects\cover-bouncer\nupkg

# Run tests and verify NO warnings
dotnet test
```

### 4. Update Documentation

The fix is already documented in:
- ✅ CHANGELOG.md entry for preview.3
- ✅ Package release notes in .csproj files

## Technical Details

**Before (preview.2):**
```xml
<Exclude>$(Exclude);[CoverBouncer.*]*</Exclude>
```

**After (preview.3):**
```xml
<ExcludeByFile>$(ExcludeByFile);**/*CoverBouncer.*.dll</ExcludeByFile>
<Exclude>$(Exclude);[CoverBouncer.*]*</Exclude>
```

The `ExcludeByFile` pattern prevents Coverlet from even attempting to load the CoverBouncer assemblies, avoiding the symbol lookup entirely.
