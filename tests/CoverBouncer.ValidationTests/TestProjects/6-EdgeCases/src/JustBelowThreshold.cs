namespace TestApp;

/// <summary>
/// File with 79.9% coverage (just below 80% threshold).
/// Should FAIL since it doesn't meet the requirement.
/// </summary>
public class JustBelowThreshold
{
    public int M1() => 1;
    public int M2() => 2;
    public int M3() => 3;
    public int M4() => 4;
    public int M5() => 5;
    public int M6() => 6; // Not covered
}
