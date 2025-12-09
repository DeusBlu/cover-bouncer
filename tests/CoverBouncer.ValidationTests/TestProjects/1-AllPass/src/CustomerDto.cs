// [CoverageProfile("Dto")]
namespace TestApp;

/// <summary>
/// Customer DTO - no coverage required.
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string GetDisplayName()
    {
        return $"{Name} ({Email})";
    }
}
