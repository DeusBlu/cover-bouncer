namespace TestApp;

/// <summary>
/// File with exactly 80.0% coverage (exactly at threshold).
/// Should PASS since it meets the requirement.
/// </summary>
public class ExactlyAtThreshold
{
    public int M1() => 1;
    public int M2() => 2;
    public int M3() => 3;
    public int M4() => 4;
    public int M5() => 5; // Not covered
}
