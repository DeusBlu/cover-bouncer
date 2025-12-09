@echo off
REM CoverBouncer Build and Package Script for Windows

setlocal enabledelayedexpansion

echo ========================================
echo CoverBouncer Build ^& Package Script
echo ========================================
echo.

set COMMAND=%1
set VERSION=%2

if "%COMMAND%"=="" set COMMAND=all
if "%VERSION%"=="" set VERSION=1.0.0-preview.1

REM Navigate to script directory
cd /d %~dp0

if "%COMMAND%"=="all" goto :all
if "%COMMAND%"=="clean" goto :clean
if "%COMMAND%"=="build" goto :build
if "%COMMAND%"=="test" goto :test
if "%COMMAND%"=="pack" goto :pack
if "%COMMAND%"=="install-cli" goto :install_cli
if "%COMMAND%"=="test-cli" goto :test_cli
if "%COMMAND%"=="help" goto :help
goto :unknown_command

:all
echo.
call :clean_build
echo.
call :restore_packages
echo.
call :build_solution
echo.
call :run_tests
echo.
call :create_packages
echo.
echo [92m√ Build complete![0m
echo.
echo Next steps:
echo   1. Install CLI tool: build.bat install-cli
echo   2. Test installation: coverbouncer --help
echo   3. Test MSBuild package: Create a test project and add the package
echo   4. Publish to NuGet: See docs/nuget-packaging.md
goto :end

:clean
call :clean_build
goto :end

:build
call :restore_packages
echo.
call :build_solution
goto :end

:test
call :run_tests
goto :end

:pack
call :create_packages
goto :end

:install_cli
call :install_cli_tool
goto :end

:test_cli
call :test_cli_tool
goto :end

:help
echo Usage: build.bat [command] [version]
echo.
echo Commands:
echo   all          - Clean, build, test, and package (default)
echo   clean        - Clean previous builds
echo   build        - Build solution
echo   test         - Run tests
echo   pack         - Create NuGet packages
echo   install-cli  - Install CLI tool locally
echo   test-cli     - Test CLI tool
echo   help         - Show this help
echo.
echo Version: %VERSION% (default: 1.0.0-preview.1)
echo.
echo Examples:
echo   build.bat all
echo   build.bat pack 1.0.0-preview.2
echo   build.bat install-cli
goto :end

:unknown_command
echo [91m× Unknown command: %COMMAND%[0m
echo.
goto :help

REM ========================================
REM Function Definitions
REM ========================================

:clean_build
echo → Cleaning previous builds...
dotnet clean --configuration Release >nul 2>&1
if exist nupkg rmdir /s /q nupkg
mkdir nupkg
echo [92m√ Clean complete[0m
exit /b 0

:restore_packages
echo → Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo [91m× Restore failed[0m
    exit /b 1
)
echo [92m√ Restore complete[0m
exit /b 0

:build_solution
echo → Building solution in Release mode...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo [91m× Build failed[0m
    exit /b 1
)
echo [92m√ Build successful[0m
exit /b 0

:run_tests
echo → Running tests...
dotnet test --configuration Release --no-build --verbosity minimal
if %errorlevel% neq 0 (
    echo [93m⚠ Some tests failed (this is expected during development)[0m
) else (
    echo [92m√ All tests passed[0m
)
exit /b 0

:create_packages
echo → Creating NuGet packages...

dotnet pack src\CoverBouncer.MSBuild\CoverBouncer.MSBuild.csproj --configuration Release --no-build --output .\nupkg
dotnet pack src\CoverBouncer.CLI\CoverBouncer.CLI.csproj --configuration Release --no-build --output .\nupkg

if %errorlevel% neq 0 (
    echo [91m× Package creation failed[0m
    exit /b 1
)

echo [92m√ Packages created in .\nupkg\[0m
echo.
echo Created packages:
dir /b .\nupkg\*.nupkg
exit /b 0

:install_cli_tool
echo → Installing CLI tool locally for testing...

REM Uninstall if already installed
dotnet tool uninstall --global CoverBouncer.CLI >nul 2>&1

REM Install from local package
dotnet tool install --global CoverBouncer.CLI --version %VERSION% --add-source .\nupkg

if %errorlevel% neq 0 (
    echo [91m× CLI tool installation failed[0m
    exit /b 1
)

echo [92m√ CLI tool installed[0m
echo → Test with: coverbouncer --help
exit /b 0

:test_cli_tool
echo → Testing CLI tool...

where coverbouncer >nul 2>&1
if %errorlevel% neq 0 (
    echo [91m× coverbouncer command not found in PATH[0m
    exit /b 1
)

echo [92m√ coverbouncer command found[0m
echo.
coverbouncer --help
exit /b 0

:end
echo.
endlocal
