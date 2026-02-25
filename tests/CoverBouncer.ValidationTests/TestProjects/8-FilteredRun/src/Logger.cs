namespace FilteredRunDemo.Infrastructure;

/// <summary>
/// Logger — no tag, uses default "Standard" profile (60% required).
/// This file is NOT targeted by the filtered test run.
/// Coverlet reports 0% because no tests in the filter touched this code.
/// </summary>
public class Logger
{
    public void Log(string message) => Console.WriteLine(message);
    public void Debug(string message) { }
}
