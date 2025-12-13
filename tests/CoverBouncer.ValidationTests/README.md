# CoverBouncer Validation Tests

This project contains end-to-end validation tests for CoverBouncer using realistic test scenarios.

## Test Scenarios

### ✅ 1-AllPass
All files meet or exceed their profile thresholds.
- **Expected:** Build succeeds, 0 violations
- Files use Critical, BusinessLogic, Dto, and Standard (default) profiles

### ⚠️ 2-MixedResults
Some files pass, some fail.
- **Expected:** Build fails with 3 violations
- Tests mixed coverage results across different files

### ❌ 3-CriticalViolation
Critical profile file with insufficient coverage.
- **Expected:** Build fails with critical violation clearly reported
- Demonstrates enforcement of strict 100% requirement

### 📋 4-UntaggedFiles
Files without profile tags use default profile.
- **Expected:** Build fails when untagged file is below default threshold
- Tests default profile application

### 🎯 5-MultipleProfiles
Complex scenario with many files across all profile types.
- **Expected:** 4 violations (2 BusinessLogic, 2 Standard)
- Tests profile grouping in output

### 🔬 6-EdgeCases
Boundary testing for threshold comparisons.
- **Expected:** 0 violations (all pass)
- Tests exactly at threshold, just above threshold, 0% allowed, 100% required

### 🌍 7-RealWorld
Realistic application simulation with layered architecture.
- **Expected:** 4 violations (2 BusinessLogic, 2 Standard)
- API, Business Logic, Data Access layers with DTOs and Infrastructure

## Running Tests

```bash
dotnet test tests/CoverBouncer.ValidationTests
```

## Test Data Structure

Each scenario folder contains:
- `coverbouncer.json` - Policy configuration
- `coverage.json` - Simulated Coverlet coverage report
- `src/` - Source files with profile tags

## Notes

The coverage.json files are hand-crafted to simulate specific coverage scenarios.
They follow the Coverlet JSON format but contain simplified data for testing purposes.
All line counts reflect only executable lines (matching Coverlet behavior).
