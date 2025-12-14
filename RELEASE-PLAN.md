# 🚀 CoverBouncer Release Plan

## Phase 1: Local Package Testing ✅ COMPLETED

### Objective
Validate that NuGet packages work correctly in real-world scenarios before publishing.

**Status:** ✅ COMPLETED - Packages built (v1.0.0-preview.1) and validated with private test project.

### Steps

#### 1.1 CLI Tool Testing
- Install CLI tool globally from local package
- Verify command registration and help output
- Test `init` command in fresh directory
- Test `verify` command with real coverage report
- Test error handling (missing files, invalid JSON)
- Verify uninstall and reinstall workflow

#### 1.2 MSBuild Package Testing
- Create fresh test project outside CoverBouncer workspace
- Add local package source to NuGet.config
- Install CoverBouncer.MSBuild package
- Enable task via project property
- Generate real coverage with Coverlet
- Verify task executes automatically after test
- Test pass scenario (all coverage meets thresholds)
- Test fail scenario (coverage violations block build)
- Verify error messages are clear and actionable

#### 1.3 Integration Testing
- Test with .NET 6, 7, and 8 projects
- Test with xUnit, NUnit, and MSTest
- Test in solution with multiple test projects
- Test with CI/CD simulation (GitHub Actions locally)
- Verify package works in both Windows and Linux (WSL)

#### 1.4 Edge Case Testing
- Empty coverage report
- Malformed coverage.json
- Missing coverbouncer.json (should use defaults)
- Large projects (100+ files)
- Nested directory structures
- Files with special characters in paths

### Success Criteria
- ✅ CLI installs and runs without errors
- ✅ MSBuild task integrates seamlessly
- ✅ Clear error messages for common issues
- ✅ Works across .NET versions and test frameworks
- ✅ Performance acceptable for real projects
- ✅ No breaking behavior in CI/CD scenarios

### Completed Actions
- ✅ NuGet packages built (v1.0.0-preview.1)
  - CoverBouncer.CLI.1.0.0-preview.1.nupkg
  - CoverBouncer.MSBuild.1.0.0-preview.1.nupkg
- ✅ Private test project created and validated
- ✅ MSBuild integration confirmed working
- ✅ Coverage validation working as expected

---

## Phase 2: Documentation & Polish ✅ COMPLETED

**Status:** ✅ COMPLETED - Documentation reviewed and packages prepared for publication.

### Objective
Ensure users can adopt CoverBouncer with minimal friction.

### Steps

#### 2.1 Documentation Review
- Verify README accuracy with actual packages
- Ensure getting-started.md matches real workflow
- Update configuration.md with all options
- Add troubleshooting section for common issues
- Create migration guide for similar tools

#### 2.2 Package Metadata
- Review package descriptions and tags
- Verify license and repository URLs
- Add release notes for v1.0.0-preview.1
- Ensure icon/logo if available
- Check package size and dependencies

#### 2.3 Verification
- Verify cover-bouncer-nuget-verification works end-to-end
- Ensure it demonstrates key features  
- Test from user perspective (no prior knowledge)

### Success Criteria
- ✅ User can install and configure in <5 minutes
- ✅ Documentation answers common questions
- ✅ Verification project runs successfully
- ✅ Package metadata is professional

### Time Estimate
1-2 hours

---

## Phase 3: Preview Release to NuGet.org ✅ COMPLETED

### Objective
Release 1.0.0-preview.1 to gather community feedback.

**Status:** ✅ PUBLISHED - Both packages are live on NuGet.org!

### Steps

#### 3.1 Pre-Release Checklist
- All tests passing (unit + validation)
- Local testing complete
- Documentation reviewed
- CHANGELOG.md created
- Git tags applied (v1.0.0-preview.1)

#### 3.2 NuGet Publishing
- ✅ Generate API key from NuGet.org
- ✅ Push CoverBouncer.MSBuild package
- ✅ Push CoverBouncer.CLI package
- ✅ Verify packages appear on NuGet.org
- ✅ Test installation from NuGet.org (not local)

**Published Packages:**
- 📦 [CoverBouncer.CLI](https://www.nuget.org/packages/CoverBouncer.CLI/1.0.0-preview.1)
- 📦 [CoverBouncer.MSBuild](https://www.nuget.org/packages/CoverBouncer.MSBuild/1.0.0-preview.1)
- **Published:** December 13, 2025
- **Downloads:** 0 (just published)

#### 3.3 GitHub Release
- Create release on GitHub (v1.0.0-preview.1)
- Attach build artifacts
- Write release notes highlighting:
  - Profile-based coverage enforcement
  - MSBuild integration
  - CLI tool availability
  - Known limitations
  - Feedback channels

#### 3.4 Community Announcement
- Share on relevant .NET communities (optional)
- Request feedback on approach
- Note it's a preview/beta release

### Success Criteria
- ✅ Packages available on NuGet.org
- ✅ GitHub release created
- ✅ Can install from fresh machine
- ✅ Feedback channels established

### Time Estimate
1 hour

---

## Phase 4: Feedback & Iteration ⏭️ CURRENT PHASE

### Objective
Gather real-world feedback and improve before stable release.

**Status:** 🔄 IN PROGRESS - Monitoring usage and gathering feedback

### Steps

#### 4.1 Monitor Usage
- Track NuGet download stats
- Monitor GitHub issues
- Watch for questions/problems
- Collect feature requests

#### 4.2 Address Critical Issues
- Fix any blocking bugs immediately
- Clarify confusing documentation
- Improve error messages based on feedback
- Consider quick wins for UX improvements

#### 4.3 Plan v1.0.0 Stable
- Evaluate feedback themes
- Prioritize fixes/features for stable
- Set timeline for stable release
- Consider breaking changes now (before v1.0)

### Success Criteria
- ✅ No critical bugs reported
- ✅ Users successfully adopting tool
- ✅ Positive feedback on approach
- ✅ Clear roadmap to v1.0.0

### Time Estimate
Ongoing (1-2 weeks)

---

## Phase 5: Stable Release (v1.0.0) 🎯

### Objective
Release production-ready version with confidence.

### Steps

#### 5.1 Final Testing
- Regression testing all scenarios
- Performance testing on large projects
- Security review of dependencies
- Accessibility of documentation

#### 5.2 Release v1.0.0
- Remove "-preview" suffix
- Update all documentation
- Publish to NuGet.org
- Create GitHub release
- Announce stable release

#### 5.3 Post-Release
- Monitor adoption
- Provide support
- Begin work on next features (from ROADMAP.md)

### Success Criteria
- ✅ Stable release on NuGet.org
- ✅ Comprehensive documentation
- ✅ Active user base
- ✅ Clear support channels

### Time Estimate
After preview feedback (2-4 weeks)

---

## Risk Assessment

### Low Risk
- ✅ Core functionality tested and working
- ✅ Validation scenarios comprehensive
- ✅ Code quality high
- ✅ Well-structured codebase

### Medium Risk
- ⚠️ Real-world edge cases not yet discovered
- ⚠️ Performance on very large projects unknown
- ⚠️ User adoption/interest uncertain

### Mitigation
- Preview release limits blast radius
- Clear labeling as preview/beta
- Active monitoring and quick response
- Gather feedback before v1.0.0 commitment

---

## Next Immediate Actions

### Today
1. **Install CLI locally**: `./build.sh install-cli`
2. **Create test project**: New .NET 8 project with tests
3. **Test MSBuild integration**: Add package, verify execution
4. **Test CLI validation**: Run against real coverage
5. **Document any issues**: Track in GitHub issues

### This Week
1. Complete local testing (all scenarios)
2. Polish documentation based on testing
3. Prepare release notes
4. Set up NuGet.org account if needed
5. Push preview release

### This Month
1. Monitor preview feedback
2. Address critical issues
3. Plan stable release
4. Continue ROADMAP features

---

## Success Metrics

### Preview Release
- 50+ downloads in first week
- 2-3 GitHub stars
- 1-2 community feedback items
- 0 critical bugs

### Stable Release
- 500+ downloads in first month
- 10+ GitHub stars
- Active users reporting success
- Consideration for official .NET tooling

---

**Current Status**: Phases 1-3 Complete ✅ | Phase 4 In Progress 🔄  
**Published on NuGet.org**: December 13, 2025  
**Next Action**: Monitor downloads, gather feedback, and prepare for v1.0.0 stable release

---

## 🎉 Milestone Achieved!

**CoverBouncer v1.0.0-preview.1 is now publicly available on NuGet.org!**

Users can now install with:
```bash
dotnet add package CoverBouncer.MSBuild --version 1.0.0-preview.1
dotnet tool install --global CoverBouncer.CLI --version 1.0.0-preview.1
```

Package URLs:
- 📦 https://www.nuget.org/packages/CoverBouncer.CLI
- 📦 https://www.nuget.org/packages/CoverBouncer.MSBuild
