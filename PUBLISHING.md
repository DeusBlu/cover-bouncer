# Publishing to NuGet.org

This guide explains how to publish CoverBouncer packages to NuGet.org.

## Prerequisites

1. **NuGet Account**: Create an account at https://www.nuget.org
2. **API Key**: Generate an API key with "Push" permission from https://www.nuget.org/account/apikeys
3. **Packages Built**: Run `./build.sh pack` to create packages

## Setup (One-Time)

### Linux/macOS

```bash
# Copy the template
cp nuget-config.template.sh nuget-config.sh

# Edit nuget-config.sh and add your API key
nano nuget-config.sh
# Or: code nuget-config.sh

# Change:
#   export NUGET_API_KEY="YOUR_API_KEY_HERE"
# To:
#   export NUGET_API_KEY="oy2abc...your-actual-key"
```

### Windows

```bat
REM Copy the template
copy nuget-config.template.bat nuget-config.bat

REM Edit nuget-config.bat and add your API key
notepad nuget-config.bat

REM Change:
REM   set NUGET_API_KEY=YOUR_API_KEY_HERE
REM To:
REM   set NUGET_API_KEY=oy2abc...your-actual-key
```

## Publishing

### Using the Script (Recommended)

**Linux/macOS:**
```bash
./publish-nuget.sh
```

**Windows:**
```bat
publish-nuget.bat
```

The script will:
1. Check if packages exist (build if needed)
2. Show you what will be published
3. Ask for confirmation
4. Push both packages to NuGet.org

### Manual Publishing

If you prefer to publish manually:

```bash
# Set your API key (one-time, stored locally)
dotnet nuget setapikey YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Push MSBuild package
dotnet nuget push nupkg/CoverBouncer.MSBuild.1.0.0-preview.1.nupkg \
    --source https://api.nuget.org/v3/index.json

# Push CLI package
dotnet nuget push nupkg/CoverBouncer.CLI.1.0.0-preview.1.nupkg \
    --source https://api.nuget.org/v3/index.json
```

## After Publishing

1. **Wait for Indexing**: NuGet takes 5-10 minutes to index new packages

2. **Verify on NuGet.org**:
   - https://www.nuget.org/packages/CoverBouncer.MSBuild
   - https://www.nuget.org/packages/CoverBouncer.CLI

3. **Create GitHub Release**:
   ```bash
   # Create tag
   git tag -a v1.0.0-preview.1 -m "Release v1.0.0-preview.1"
   git push origin v1.0.0-preview.1
   
   # Then create release on GitHub:
   # - Go to https://github.com/DeusBlu/cover-bouncer/releases/new
   # - Select tag: v1.0.0-preview.1
   # - Mark as "pre-release"
   # - Copy CHANGELOG content to release notes
   # - Attach .nupkg files
   # - Publish release
   ```

4. **Test Installation**:
   ```bash
   # Create test project
   mkdir test-install && cd test-install
   dotnet new console -n TestApp
   cd TestApp
   
   # Install from NuGet.org (not local)
   dotnet add package CoverBouncer.MSBuild --version 1.0.0-preview.1
   
   # Install CLI tool
   dotnet tool install -g CoverBouncer.CLI --version 1.0.0-preview.1
   
   # Verify
   coverbouncer --help
   ```

## Security Notes

- ✅ `nuget-config.sh` and `nuget-config.bat` are in `.gitignore`
- ✅ **NEVER commit these files** - they contain your API key
- ✅ Only use templates (`.template.sh`/`.template.bat`) in git
- ✅ Treat API keys like passwords
- ✅ Regenerate keys if accidentally exposed

## Troubleshooting

### "Package already exists"
- Use `--skip-duplicate` flag
- Or increment version number

### "Unauthorized"
- Check API key is correct
- Verify API key has "Push" permission
- Try regenerating the API key

### "Invalid package"
- Verify packages were built correctly
- Check package metadata in `.csproj` files
- Run `dotnet pack` again

## Version Management

Before publishing a new version:

1. Update version in both `.csproj` files:
   - `src/CoverBouncer.MSBuild/CoverBouncer.MSBuild.csproj`
   - `src/CoverBouncer.CLI/CoverBouncer.CLI.csproj`

2. Update CHANGELOG.md

3. Rebuild packages: `./build.sh pack`

4. Update publish script with new version numbers

5. Test locally before publishing

## Preview vs Stable Releases

**Preview** (`1.0.0-preview.1`):
- For testing and feedback
- May have bugs or incomplete features
- Mark GitHub release as "pre-release"

**Stable** (`1.0.0`):
- Production-ready
- Fully tested and documented
- Mark GitHub release as "latest"

---

**Ready to publish!** 🚀
