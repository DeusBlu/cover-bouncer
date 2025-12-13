#!/bin/bash
#
# NuGet Publishing Script for CoverBouncer
#
# Usage:
#   1. Create nuget-config.sh from nuget-config.template.sh
#   2. Add your NuGet API key to nuget-config.sh
#   3. Run: ./publish-nuget.sh
#

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "========================================"
echo "CoverBouncer NuGet Publishing Script"
echo "========================================"

# Check if nuget-config.sh exists
if [ ! -f "./nuget-config.sh" ]; then
    echo -e "${RED}Error: nuget-config.sh not found!${NC}"
    echo ""
    echo "To set up:"
    echo "  1. Copy nuget-config.template.sh to nuget-config.sh"
    echo "  2. Edit nuget-config.sh and add your NuGet API key"
    echo "  3. Run this script again"
    echo ""
    echo "Command: cp nuget-config.template.sh nuget-config.sh"
    exit 1
fi

# Load API key
source ./nuget-config.sh

if [ -z "$NUGET_API_KEY" ]; then
    echo -e "${RED}Error: NUGET_API_KEY is not set in nuget-config.sh${NC}"
    exit 1
fi

# Check if packages exist
if [ ! -f "nupkg/CoverBouncer.MSBuild.1.0.0-preview.1.nupkg" ]; then
    echo -e "${YELLOW}Warning: MSBuild package not found. Building packages...${NC}"
    ./build.sh pack
fi

if [ ! -f "nupkg/CoverBouncer.CLI.1.0.0-preview.1.nupkg" ]; then
    echo -e "${YELLOW}Warning: CLI package not found. Building packages...${NC}"
    ./build.sh pack
fi

echo ""
echo "Packages to publish:"
ls -lh nupkg/*.nupkg
echo ""

# Confirm before publishing
read -p "Publish these packages to NuGet.org? (y/N) " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Publishing cancelled."
    exit 0
fi

echo ""
echo "→ Publishing CoverBouncer.MSBuild..."
dotnet nuget push nupkg/CoverBouncer.MSBuild.1.0.0-preview.1.nupkg \
    --api-key "$NUGET_API_KEY" \
    --source https://api.nuget.org/v3/index.json \
    --skip-duplicate

echo ""
echo "→ Publishing CoverBouncer.CLI..."
dotnet nuget push nupkg/CoverBouncer.CLI.1.0.0-preview.1.nupkg \
    --api-key "$NUGET_API_KEY" \
    --source https://api.nuget.org/v3/index.json \
    --skip-duplicate

echo ""
echo -e "${GREEN}✓ Publishing complete!${NC}"
echo ""
echo "Next steps:"
echo "  1. Wait 5-10 minutes for NuGet indexing"
echo "  2. Verify packages at:"
echo "     - https://www.nuget.org/packages/CoverBouncer.MSBuild"
echo "     - https://www.nuget.org/packages/CoverBouncer.CLI"
echo "  3. Create GitHub release (v1.0.0-preview.1)"
echo "  4. Test installation from NuGet.org"
echo ""
