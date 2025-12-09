namespace TestApp;

/// <summary>
/// Email sender - meets Standard (60%) requirement.
/// This should PASS validation.
/// </summary>
public class EmailSender
{
    public bool SendEmail(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(to))
            return false;
        
        Console.WriteLine($"Sending email to {to}");
        return true;
    }

    // Not covered - but that's OK for Standard profile
    public void SendBulkEmails(List<string> recipients)
    {
        foreach (var recipient in recipients)
        {
            SendEmail(recipient, "Bulk", "Message");
        }
    }
}
