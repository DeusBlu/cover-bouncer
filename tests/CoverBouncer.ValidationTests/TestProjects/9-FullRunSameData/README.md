# Scenario 9: Full Run With Same Data (Contrast to Scenario 8)

## Purpose

Uses the **exact same coverage data** as Scenario 8, but validates as a FULL run 
(`isFilteredTestRun = false`). This demonstrates the contrast — the same 0% coverage 
files that were safely skipped in Scenario 8 now correctly fail validation.

**Note:** This scenario reuses Scenario 8's source files and coverage data. 
The `coverage.json` paths point to `8-FilteredRun/src/` — this is intentional.

## Expected Results

### Full run (isFilteredTestRun = false):
- **6 files validated**, **0 files skipped**, **3 violations** → ❌ FAILURE

| File | Profile | Coverage | Threshold | Result |
|------|---------|----------|-----------|--------|
| `PaymentService.cs` | Critical | 100% | 100% | ✅ Pass |
| `OrderService.cs` | BusinessLogic | 100% | 80% | ✅ Pass |
| `AuthenticationService.cs` | Critical | 0% | 100% | ❌ Fail |
| `InventoryService.cs` | BusinessLogic | 0% | 80% | ❌ Fail |
| `Logger.cs` | Standard | 0% | 60% | ❌ Fail |
| `UserDto.cs` | Dto | 0% | 0% | ✅ Pass |

### Compare with Scenario 8 (filtered run):
- Same data → **0 violations**, 4 files skipped → ✅ SUCCESS

## Key Takeaway

**Best practice**: Always use a FULL test run (no `--filter`) as your final CI gate. 
Filtered runs are useful for fast feedback during development, but only a full run 
validates coverage across your entire codebase.
