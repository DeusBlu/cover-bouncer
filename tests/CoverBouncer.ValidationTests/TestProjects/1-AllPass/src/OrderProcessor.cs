// [CoverageProfile("BusinessLogic")]
namespace TestApp;

/// <summary>
/// Order processing logic - requires 80% coverage.
/// </summary>
public class OrderProcessor
{
    public string ProcessOrder(int orderId)
    {
        if (orderId <= 0)
            throw new ArgumentException("Invalid order ID");
        
        return $"Order {orderId} processed";
    }

    public decimal CalculateTotal(decimal subtotal, decimal tax)
    {
        return subtotal + tax;
    }

    // This method won't be tested but still allows 80% coverage
    public void LogOrder(int orderId)
    {
        Console.WriteLine($"Order {orderId} logged");
    }
}
