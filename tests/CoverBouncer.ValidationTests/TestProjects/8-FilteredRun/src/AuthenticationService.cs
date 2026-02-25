// [CoverageProfile("Critical")]
namespace FilteredRunDemo.Services;

/// <summary>
/// Authentication — Critical profile, requires 100% coverage.
/// This file is NOT targeted by the filtered test run.
/// On a FULL run this would need tests, but the filter skipped them.
/// Coverlet reports 0% because no tests in the filter touched this code.
/// </summary>
public class AuthenticationService
{
    public bool Authenticate(string username, string password) => true;
    public void Logout(string sessionId) { }
}
