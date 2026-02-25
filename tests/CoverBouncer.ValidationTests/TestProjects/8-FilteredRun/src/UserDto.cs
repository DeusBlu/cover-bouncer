// [CoverageProfile("Dto")]
namespace FilteredRunDemo.Models;

/// <summary>
/// Simple DTO — Dto profile, requires 0% coverage.
/// This file is NOT targeted by the filtered test run.
/// Even on a FULL run, this passes because the Dto profile allows 0%.
/// On a FILTERED run, it's skipped (not counted either way).
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
