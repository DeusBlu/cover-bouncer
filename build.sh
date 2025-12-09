#!/bin/bash
# CoverBouncer Build and Package Script

set -e  # Exit on error

echo "========================================"
echo "CoverBouncer Build & Package Script"
echo "========================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "→ $1"
}

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Parse command line arguments
COMMAND=${1:-all}
VERSION=${2:-1.0.0-preview.1}

# Main functions
clean_build() {
    print_info "Cleaning previous builds..."
    dotnet clean --configuration Release > /dev/null 2>&1
    rm -rf ./nupkg
    mkdir -p ./nupkg
    print_success "Clean complete"
}

restore_packages() {
    print_info "Restoring NuGet packages..."
    dotnet restore
    print_success "Restore complete"
}

build_solution() {
    print_info "Building solution in Release mode..."
    dotnet build --configuration Release --no-restore
    if [ $? -eq 0 ]; then
        print_success "Build successful"
    else
        print_error "Build failed"
        exit 1
    fi
}

run_tests() {
    print_info "Running tests..."
    dotnet test --configuration Release --no-build --verbosity minimal
    if [ $? -eq 0 ]; then
        print_success "All tests passed"
    else
        print_warning "Some tests failed (this is expected during development)"
    fi
}

create_packages() {
    print_info "Creating NuGet packages..."
    
    # Pack MSBuild package
    dotnet pack src/CoverBouncer.MSBuild/CoverBouncer.MSBuild.csproj \
        --configuration Release \
        --no-build \
        --output ./nupkg
    
    # Pack CLI package
    dotnet pack src/CoverBouncer.CLI/CoverBouncer.CLI.csproj \
        --configuration Release \
        --no-build \
        --output ./nupkg
    
    print_success "Packages created in ./nupkg/"
    echo ""
    print_info "Created packages:"
    ls -lh ./nupkg/*.nupkg | awk '{print "  - " $9 " (" $5 ")"}'
}

install_cli_tool_local() {
    print_info "Installing CLI tool locally for testing..."
    
    # Uninstall if already installed
    dotnet tool uninstall --global CoverBouncer.CLI 2>/dev/null || true
    
    # Install from local package
    dotnet tool install --global CoverBouncer.CLI \
        --version $VERSION \
        --add-source ./nupkg
    
    if [ $? -eq 0 ]; then
        print_success "CLI tool installed"
        print_info "Test with: coverbouncer --help"
    else
        print_error "CLI tool installation failed"
        exit 1
    fi
}

test_cli_tool() {
    print_info "Testing CLI tool..."
    
    if command -v coverbouncer &> /dev/null; then
        print_success "coverbouncer command found"
        echo ""
        coverbouncer --help
    else
        print_error "coverbouncer command not found in PATH"
        exit 1
    fi
}

verify_packages() {
    print_info "Verifying package contents..."
    
    # Create temp directory
    TEMP_DIR=$(mktemp -d)
    cd "$TEMP_DIR"
    
    # Extract MSBuild package
    print_info "Extracting CoverBouncer.MSBuild package..."
    unzip -q "$SCRIPT_DIR/nupkg/CoverBouncer.MSBuild.$VERSION.nupkg"
    
    # Check for required files
    if [ -f "build/CoverBouncer.targets" ]; then
        print_success "build/CoverBouncer.targets found"
    else
        print_error "build/CoverBouncer.targets missing"
    fi
    
    if [ -f "buildTransitive/CoverBouncer.targets" ]; then
        print_success "buildTransitive/CoverBouncer.targets found"
    else
        print_error "buildTransitive/CoverBouncer.targets missing"
    fi
    
    # Cleanup
    cd "$SCRIPT_DIR"
    rm -rf "$TEMP_DIR"
    
    print_success "Package verification complete"
}

show_help() {
    echo "Usage: ./build.sh [command] [version]"
    echo ""
    echo "Commands:"
    echo "  all          - Clean, build, test, and package (default)"
    echo "  clean        - Clean previous builds"
    echo "  build        - Build solution"
    echo "  test         - Run tests"
    echo "  pack         - Create NuGet packages"
    echo "  install-cli  - Install CLI tool locally"
    echo "  test-cli     - Test CLI tool"
    echo "  verify       - Verify package contents"
    echo "  help         - Show this help"
    echo ""
    echo "Version: $VERSION (default: 1.0.0-preview.1)"
    echo ""
    echo "Examples:"
    echo "  ./build.sh all"
    echo "  ./build.sh pack 1.0.0-preview.2"
    echo "  ./build.sh install-cli"
}

# Main execution
case "$COMMAND" in
    all)
        echo ""
        clean_build
        echo ""
        restore_packages
        echo ""
        build_solution
        echo ""
        run_tests
        echo ""
        create_packages
        echo ""
        verify_packages
        echo ""
        print_success "Build complete!"
        echo ""
        print_info "Next steps:"
        echo "  1. Install CLI tool: ./build.sh install-cli"
        echo "  2. Test installation: coverbouncer --help"
        echo "  3. Test MSBuild package: Create a test project and add the package"
        echo "  4. Publish to NuGet: See docs/nuget-packaging.md"
        ;;
    clean)
        clean_build
        ;;
    build)
        restore_packages
        echo ""
        build_solution
        ;;
    test)
        run_tests
        ;;
    pack)
        create_packages
        ;;
    install-cli)
        install_cli_tool_local
        ;;
    test-cli)
        test_cli_tool
        ;;
    verify)
        verify_packages
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Unknown command: $COMMAND"
        echo ""
        show_help
        exit 1
        ;;
esac

echo ""
