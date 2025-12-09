namespace TestApp;

/// <summary>
/// Utilities class - no explicit profile, uses Standard (60%).
/// </summary>
public class Utilities
{
    public static string FormatCurrency(decimal amount)
    {
        return $"${amount:N2}";
    }

    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@");
    }

    // This method won't be tested but still allows 60% coverage
    public static void LogMessage(string message)
    {
        Console.WriteLine(message);
    }
}
