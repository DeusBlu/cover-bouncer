// [CoverageProfile("BusinessLogic")]
namespace FilteredRunDemo.Services;

/// <summary>
/// Order processing — BusinessLogic profile, requires 80% coverage.
/// This file IS targeted by the filtered test run and has full coverage.
/// </summary>
public class OrderService
{
    public string CreateOrder(int productId) => $"ORD-{productId}";
    public decimal CalculateTotal(decimal price, int qty) => price * qty;
}
