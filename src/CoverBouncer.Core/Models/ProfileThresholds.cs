namespace CoverBouncer.Core.Models;

/// <summary>
/// Coverage thresholds for a profile.
/// </summary>
public sealed class ProfileThresholds
{
    /// <summary>
    /// Minimum line coverage required (0.0 to 1.0).
    /// </summary>
    public decimal? MinLine { get; set; }

    /// <summary>
    /// Minimum branch coverage required (0.0 to 1.0). Optional.
    /// </summary>
    /// <remarks>
    /// Branch coverage can be unreliable in Coverlet, so this is optional.
    /// If not specified, only line coverage will be enforced.
    /// </remarks>
    public decimal? MinBranch { get; set; }

    /// <summary>
    /// Validates that threshold values are in valid range (0.0 to 1.0).
    /// </summary>
    /// <param name="profileName">Profile name for error messages.</param>
    /// <exception cref="ArgumentException">Thrown when threshold values are out of range.</exception>
    public void Validate(string profileName)
    {
        if (MinLine.HasValue && (MinLine.Value < 0 || MinLine.Value > 1))
        {
            throw new ArgumentException(
                $"Profile '{profileName}': MinLine must be between 0.0 and 1.0, got {MinLine.Value}",
                nameof(MinLine));
        }

        if (MinBranch.HasValue && (MinBranch.Value < 0 || MinBranch.Value > 1))
        {
            throw new ArgumentException(
                $"Profile '{profileName}': MinBranch must be between 0.0 and 1.0, got {MinBranch.Value}",
                nameof(MinBranch));
        }

        // At least one threshold must be specified
        if (!MinLine.HasValue && !MinBranch.HasValue)
        {
            throw new ArgumentException(
                $"Profile '{profileName}': At least one threshold (MinLine or MinBranch) must be specified",
                nameof(profileName));
        }
    }
}
