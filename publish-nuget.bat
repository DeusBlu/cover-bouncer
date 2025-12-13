@echo off
REM
REM NuGet Publishing Script for CoverBouncer (Windows)
REM
REM Usage:
REM   1. Create nuget-config.bat from nuget-config.template.bat
REM   2. Add your NuGet API key to nuget-config.bat
REM   3. Run: publish-nuget.bat
REM

echo ========================================
echo CoverBouncer NuGet Publishing Script
echo ========================================

REM Check if nuget-config.bat exists
if not exist "nuget-config.bat" (
    echo Error: nuget-config.bat not found!
    echo.
    echo To set up:
    echo   1. Copy nuget-config.template.bat to nuget-config.bat
    echo   2. Edit nuget-config.bat and add your NuGet API key
    echo   3. Run this script again
    echo.
    echo Command: copy nuget-config.template.bat nuget-config.bat
    exit /b 1
)

REM Load API key
call nuget-config.bat

if "%NUGET_API_KEY%"=="" (
    echo Error: NUGET_API_KEY is not set in nuget-config.bat
    exit /b 1
)

REM Check if packages exist
if not exist "nupkg\CoverBouncer.MSBuild.1.0.0-preview.1.nupkg" (
    echo Warning: MSBuild package not found. Building packages...
    call build.bat pack
)

if not exist "nupkg\CoverBouncer.CLI.1.0.0-preview.1.nupkg" (
    echo Warning: CLI package not found. Building packages...
    call build.bat pack
)

echo.
echo Packages to publish:
dir /b nupkg\*.nupkg
echo.

REM Confirm before publishing
set /p CONFIRM="Publish these packages to NuGet.org? (y/N): "
if /i not "%CONFIRM%"=="y" (
    echo Publishing cancelled.
    exit /b 0
)

echo.
echo Publishing CoverBouncer.MSBuild...
dotnet nuget push nupkg\CoverBouncer.MSBuild.1.0.0-preview.1.nupkg --api-key %NUGET_API_KEY% --source https://api.nuget.org/v3/index.json --skip-duplicate

echo.
echo Publishing CoverBouncer.CLI...
dotnet nuget push nupkg\CoverBouncer.CLI.1.0.0-preview.1.nupkg --api-key %NUGET_API_KEY% --source https://api.nuget.org/v3/index.json --skip-duplicate

echo.
echo Publishing complete!
echo.
echo Next steps:
echo   1. Wait 5-10 minutes for NuGet indexing
echo   2. Verify packages at:
echo      - https://www.nuget.org/packages/CoverBouncer.MSBuild
echo      - https://www.nuget.org/packages/CoverBouncer.CLI
echo   3. Create GitHub release (v1.0.0-preview.1)
echo   4. Test installation from NuGet.org
echo.
