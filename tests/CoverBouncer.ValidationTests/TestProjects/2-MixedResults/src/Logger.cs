namespace TestApp;

/// <summary>
/// Logger - below Standard (60%) requirement at 40%.
/// This should FAIL validation.
/// </summary>
public class Logger
{
    public void LogInfo(string message)
    {
        Console.WriteLine($"INFO: {message}");
    }

    // These methods are NOT covered
    public void LogWarning(string message)
    {
        Console.WriteLine($"WARN: {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"ERROR: {message}");
    }

    public void LogDebug(string message)
    {
        Console.WriteLine($"DEBUG: {message}");
    }
}
