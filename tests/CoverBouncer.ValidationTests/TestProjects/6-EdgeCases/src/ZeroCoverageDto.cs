// [CoverageProfile("Dto")]
namespace TestApp;

/// <summary>
/// DTO with 0% coverage - should PASS since DTOs don't require coverage.
/// </summary>
public class ZeroCoverageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string GetDisplay() => $"{Id}: {Name}";
}
