namespace TestApp;

/// <summary>
/// Helper 1 - No profile tag, uses default Standard (60%).
/// Has 70% coverage - should PASS.
/// </summary>
public class Helper1
{
    public string FormatName(string firstName, string lastName)
    {
        return $"{firstName} {lastName}";
    }

    public int Add(int a, int b)
    {
        return a + b;
    }

    // Not covered - but still meets 60% threshold
    public void LogOperation(string operation)
    {
        Console.WriteLine($"Operation: {operation}");
    }
}
