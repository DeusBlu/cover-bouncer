// [CoverageProfile("Critical")]
namespace FilteredRunDemo.Services;

/// <summary>
/// Payment processing — Critical profile, requires 100% coverage.
/// This file IS targeted by the filtered test run and has full coverage.
/// </summary>
public class PaymentService
{
    public bool ProcessPayment(decimal amount) => amount > 0;
    public bool ValidateCard(string cardNumber) => !string.IsNullOrEmpty(cardNumber);
}
