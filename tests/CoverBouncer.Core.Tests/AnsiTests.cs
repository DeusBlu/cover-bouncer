using CoverBouncer.Core;
using Xunit;

namespace CoverBouncer.Core.Tests;

public class AnsiTests
{
    [Fact]
    public void Ansi_WhenEnabled_ReturnsEscapeCodes()
    {
        Ansi.Enabled = true;
        try
        {
            Assert.Equal("\x1b[31m", Ansi.Red);
            Assert.Equal("\x1b[32m", Ansi.Green);
            Assert.Equal("\x1b[1m", Ansi.Bold);
            Assert.Equal("\x1b[0m", Ansi.Reset);
            Assert.Equal("\x1b[91m", Ansi.BrightRed);
            Assert.Equal("\x1b[92m", Ansi.BrightGreen);
            Assert.Equal("\x1b[93m", Ansi.BrightYellow);
            Assert.Equal("\x1b[96m", Ansi.BrightCyan);
            Assert.Equal("\x1b[90m", Ansi.Gray);
        }
        finally
        {
            Ansi.ResetDetection();
        }
    }

    [Fact]
    public void Ansi_WhenDisabled_ReturnsEmptyStrings()
    {
        Ansi.Enabled = false;
        try
        {
            Assert.Equal("", Ansi.Red);
            Assert.Equal("", Ansi.Green);
            Assert.Equal("", Ansi.Bold);
            Assert.Equal("", Ansi.Reset);
            Assert.Equal("", Ansi.BrightRed);
            Assert.Equal("", Ansi.Fail);
            Assert.Equal("", Ansi.Pass);
            Assert.Equal("", Ansi.Heading);
            Assert.Equal("", Ansi.Muted);
        }
        finally
        {
            Ansi.ResetDetection();
        }
    }

    [Fact]
    public void Ansi_SemanticHelpers_MapToCorrectCodes()
    {
        Ansi.Enabled = true;
        try
        {
            Assert.Equal(Ansi.BrightGreen, Ansi.Pass);
            Assert.Equal(Ansi.BrightRed, Ansi.Fail);
            Assert.Equal(Ansi.BrightYellow, Ansi.Warn);
            Assert.Equal(Ansi.BrightCyan, Ansi.Info);
            Assert.Equal(Ansi.Yellow, Ansi.Threshold);
            Assert.Equal(Ansi.Gray, Ansi.Muted);
        }
        finally
        {
            Ansi.ResetDetection();
        }
    }

    [Fact]
    public void Ansi_ResetDetection_ReturnsToAutoDetect()
    {
        Ansi.Enabled = false;
        Assert.Equal("", Ansi.Red);
        
        Ansi.ResetDetection();
        // After reset, it goes back to auto-detection
        // We can't assert the exact value since it depends on the environment,
        // but we can verify it doesn't throw
        _ = Ansi.Enabled;
        _ = Ansi.Red;
    }

    [Fact]
    public void Ansi_ColoredOutput_InterpolatesCleanly()
    {
        Ansi.Enabled = true;
        try
        {
            var output = $"{Ansi.Fail}❌ MyFile.cs{Ansi.Reset}: {Ansi.Fail}47.4%{Ansi.Reset}/{Ansi.Threshold}80%{Ansi.Reset}";
            Assert.Contains("❌ MyFile.cs", output);
            Assert.Contains("47.4%", output);
            Assert.Contains("80%", output);
            Assert.Contains("\x1b[", output); // Has escape codes
        }
        finally
        {
            Ansi.ResetDetection();
        }
    }

    [Fact]
    public void Ansi_DisabledOutput_HasNoEscapeCodes()
    {
        Ansi.Enabled = false;
        try
        {
            var output = $"{Ansi.Fail}❌ MyFile.cs{Ansi.Reset}: {Ansi.Fail}47.4%{Ansi.Reset}/{Ansi.Threshold}80%{Ansi.Reset}";
            Assert.Equal("❌ MyFile.cs: 47.4%/80%", output);
            Assert.DoesNotContain("\x1b[", output);
        }
        finally
        {
            Ansi.ResetDetection();
        }
    }
}
