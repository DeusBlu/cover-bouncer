// [CoverageProfile("Critical")]
namespace TestApp;

/// <summary>
/// Payment processing service - requires 100% coverage.
/// </summary>
public class PaymentService
{
    public decimal ProcessPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        return amount * 1.05m; // Add processing fee
    }

    public bool ValidatePayment(decimal amount)
    {
        return amount > 0 && amount <= 10000;
    }
}
