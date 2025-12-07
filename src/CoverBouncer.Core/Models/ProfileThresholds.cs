namespace CoverBouncer.Core.Models;

/// <summary>
/// Coverage thresholds for a profile.
/// MVP: Line coverage only.
/// </summary>
public sealed class ProfileThresholds
{
    /// <summary>
    /// Minimum line coverage required (0.0 to 1.0).
    /// </summary>
    public decimal MinLine { get; set; }

    /// <summary>
    /// Validates that threshold value is in valid range (0.0 to 1.0).
    /// </summary>
    /// <param name="profileName">Profile name for error messages.</param>
    /// <exception cref="ArgumentException">Thrown when threshold value is out of range.</exception>
    public void Validate(string profileName)
    {
        if (MinLine < 0 || MinLine > 1)
        {
            throw new ArgumentException(
                $"Profile '{profileName}': MinLine must be between 0.0 and 1.0, got {MinLine}",
                nameof(MinLine));
        }
    }
}
