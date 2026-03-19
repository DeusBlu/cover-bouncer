namespace CoverBouncer.Core;

/// <summary>
/// ANSI escape code helpers for colorized terminal output.
/// Automatically disables colors when output is redirected or NO_COLOR is set.
/// </summary>
public static class Ansi
{
    private static bool? _enabled;

    /// <summary>
    /// Whether ANSI color output is enabled.
    /// Respects NO_COLOR (https://no-color.org/) and detects redirected output.
    /// Can be overridden for testing.
    /// </summary>
    public static bool Enabled
    {
        get => _enabled ?? DetectColorSupport();
        set => _enabled = value;
    }

    /// <summary>
    /// Resets to auto-detection (for testing).
    /// </summary>
    public static void ResetDetection() => _enabled = null;

    private static bool DetectColorSupport()
    {
        // Respect NO_COLOR convention: https://no-color.org/
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR")))
            return false;

        // Check for explicit opt-in (useful in CI)
        if (Environment.GetEnvironmentVariable("FORCE_COLOR") == "1")
            return true;

        // Detect if stdout is redirected (piped to file, etc.)
        try
        {
            if (Console.IsOutputRedirected)
                return false;
        }
        catch
        {
            // Console.IsOutputRedirected can throw in some environments (e.g., MSBuild task host)
            // Default to enabled in that case — most modern terminals support ANSI
        }

        return true;
    }

    // Reset
    public static string Reset => Enabled ? "\x1b[0m" : "";

    // Styles
    public static string Bold => Enabled ? "\x1b[1m" : "";
    public static string Dim => Enabled ? "\x1b[2m" : "";

    // Foreground colors
    public static string Red => Enabled ? "\x1b[31m" : "";
    public static string Green => Enabled ? "\x1b[32m" : "";
    public static string Yellow => Enabled ? "\x1b[33m" : "";
    public static string Blue => Enabled ? "\x1b[34m" : "";
    public static string Cyan => Enabled ? "\x1b[36m" : "";
    public static string White => Enabled ? "\x1b[37m" : "";
    public static string BrightRed => Enabled ? "\x1b[91m" : "";
    public static string BrightGreen => Enabled ? "\x1b[92m" : "";
    public static string BrightYellow => Enabled ? "\x1b[93m" : "";
    public static string BrightCyan => Enabled ? "\x1b[96m" : "";
    public static string Gray => Enabled ? "\x1b[90m" : "";

    // Semantic helpers for CoverBouncer output
    public static string Pass => BrightGreen;
    public static string Fail => BrightRed;
    public static string Warn => BrightYellow;
    public static string Info => BrightCyan;
    public static string Heading => Cyan + Bold;
    public static string Muted => Gray;
    public static string File => White + Bold;
    public static string Threshold => Yellow;
}
