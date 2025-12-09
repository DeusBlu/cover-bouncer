namespace TestApp;

/// <summary>
/// Helper 2 - No profile tag, uses default Standard (60%).
/// Has only 50% coverage - should FAIL.
/// </summary>
public class Helper2
{
    public string ToUpper(string input)
    {
        return input.ToUpper();
    }

    // These are NOT covered - drops below 60%
    public string ToLower(string input)
    {
        return input.ToLower();
    }

    public string Trim(string input)
    {
        return input.Trim();
    }

    public bool IsEmpty(string input)
    {
        return string.IsNullOrEmpty(input);
    }
}
