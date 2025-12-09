// [CoverageProfile("Critical")]
namespace TestApp;

/// <summary>
/// Auth service - requires 100% but only has 95%.
/// This should FAIL validation.
/// </summary>
public class AuthService
{
    public bool ValidateUser(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            return false;
        
        if (string.IsNullOrEmpty(password))
            return false;
        
        return username.Length > 3 && password.Length > 6;
    }

    public string GenerateToken(string username)
    {
        return $"TOKEN_{username}_{DateTime.Now.Ticks}";
    }

    // This method is NOT covered - causes violation
    public void RevokeToken(string token)
    {
        Console.WriteLine($"Token revoked: {token}");
    }
}
