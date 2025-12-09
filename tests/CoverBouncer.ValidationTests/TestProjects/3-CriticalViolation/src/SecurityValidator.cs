// [CoverageProfile("Critical")]
namespace TestApp;

/// <summary>
/// Security validator - CRITICAL but only 75% coverage.
/// This must FAIL to demonstrate critical violations are caught.
/// </summary>
public class SecurityValidator
{
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;
        
        return token.StartsWith("VALID_");
    }

    public bool CheckPermissions(string user, string resource)
    {
        if (string.IsNullOrEmpty(user))
            return false;
        
        return user == "admin";
    }

    // NOT COVERED - causes Critical violation
    public void AuditSecurityEvent(string eventType, string details)
    {
        var timestamp = DateTime.UtcNow;
        Console.WriteLine($"[{timestamp}] {eventType}: {details}");
    }

    // NOT COVERED - causes Critical violation
    public string EncryptData(string data)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
    }
}
