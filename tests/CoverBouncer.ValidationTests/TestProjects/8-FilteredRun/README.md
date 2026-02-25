# Scenario 8: Filtered Test Run

## Purpose

Demonstrates CoverBouncer behavior when tests are run with `--filter` (e.g., `dotnet test --filter "Category=OrderTests"`).

## The Problem

When you run a filtered subset of tests, Coverlet still instruments the **entire assembly**. 
Files not targeted by the filtered tests appear in the coverage report with **0% coverage**, 
even though no tests were supposed to run against them. Without filtered-run awareness, 
CoverBouncer would report false failures for all those untargeted files.

## What This Scenario Simulates

A developer runs: `dotnet test --filter "Category=OrderTests"`

Only order-related tests execute. The coverage report contains:

### Files WITH coverage (targeted by the filter):
| File | Profile | Coverage | Expected |
|------|---------|----------|----------|
| `PaymentService.cs` | Critical | 100% | ✅ Pass |
| `OrderService.cs` | BusinessLogic | 100% | ✅ Pass |

### Files with 0% coverage (NOT targeted by the filter):
| File | Profile | Coverage | Expected (Filtered) | Expected (Full) |
|------|---------|----------|---------------------|------------------|
| `AuthenticationService.cs` | Critical | 0% | ⏭️ Skip | ❌ Fail |
| `InventoryService.cs` | BusinessLogic | 0% | ⏭️ Skip | ❌ Fail |
| `Logger.cs` | Standard (default) | 0% | ⏭️ Skip | ❌ Fail |
| `UserDto.cs` | Dto | 0% | ⏭️ Skip | ✅ Pass (0% threshold) |

## Expected Results

### With `isFilteredTestRun = true`:
- **2 files validated**, **4 files skipped**, **0 violations** → ✅ SUCCESS

### With `isFilteredTestRun = false` (Scenario 9 — same data):
- **6 files validated**, **0 files skipped**, **3 violations** → ❌ FAILURE
- AuthenticationService.cs: 0% < 100% (Critical)
- InventoryService.cs: 0% < 80% (BusinessLogic)
- Logger.cs: 0% < 60% (Standard)
- UserDto.cs passes because Dto profile allows 0%

## Key Insight

The **same coverage data** produces different (correct!) results depending on whether 
CoverBouncer knows the test run was filtered. This is why `$(VSTestTestCaseFilter)` 
auto-detection is critical for CI pipelines.
