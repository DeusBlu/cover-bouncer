// [CoverageProfile("BusinessLogic")]
namespace FilteredRunDemo.Services;

/// <summary>
/// Inventory management — BusinessLogic profile, requires 80% coverage.
/// This file is NOT targeted by the filtered test run.
/// On a FULL run this would need tests, but the filter skipped them.
/// Coverlet reports 0% because no tests in the filter touched this code.
/// </summary>
public class InventoryService
{
    public int GetStock(int productId) => 100;
    public void UpdateStock(int productId, int quantity) { }
}
