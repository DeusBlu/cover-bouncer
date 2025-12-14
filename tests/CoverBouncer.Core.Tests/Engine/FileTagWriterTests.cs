using CoverBouncer.Core.Engine;
using Xunit;

namespace CoverBouncer.Core.Tests.Engine;

public class FileTagWriterTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileTagWriter _writer;

    public FileTagWriterTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CoverBouncerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _writer = new FileTagWriter();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private string CreateTestFile(string filename, string content)
    {
        var path = Path.Combine(_testDirectory, filename);
        File.WriteAllText(path, content);
        return path;
    }

    #region WriteProfileTag - Positive Tests

    [Fact]
    public void WriteProfileTag_AddsTagToFileWithoutTag()
    {
        // Arrange
        var content = @"namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        var result = _writer.WriteProfileTag(filePath, "Critical", createBackup: false);

        // Assert
        Assert.True(result); // File was modified
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Critical\")]", newContent);
        Assert.Contains("namespace MyApp.Services", newContent);
    }

    [Fact]
    public void WriteProfileTag_AddsTagBeforeNamespace()
    {
        // Arrange
        var content = @"using System;

namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "BusinessLogic", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        var lines = newContent.Split('\n');
        
        var tagIndex = Array.FindIndex(lines, l => l.Contains("[CoverageProfile(\"BusinessLogic\")]"));
        var namespaceIndex = Array.FindIndex(lines, l => l.Contains("namespace MyApp.Services"));
        
        Assert.True(tagIndex >= 0);
        Assert.True(namespaceIndex > tagIndex); // Tag comes before namespace
    }

    [Fact]
    public void WriteProfileTag_AddsTagBeforeClass_WhenNoNamespace()
    {
        // Arrange
        var content = @"using System;

public class MyService
{
    public void DoWork() { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Standard\")]", newContent);
        
        var lines = newContent.Split('\n');
        var tagIndex = Array.FindIndex(lines, l => l.Contains("[CoverageProfile(\"Standard\")]"));
        var classIndex = Array.FindIndex(lines, l => l.Contains("public class MyService"));
        
        Assert.True(classIndex > tagIndex);
    }

    [Fact]
    public void WriteProfileTag_UpdatesExistingTag()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Standard"")]
namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        var result = _writer.WriteProfileTag(filePath, "Critical", createBackup: false);

        // Assert
        Assert.True(result); // File was modified
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Critical\")]", newContent);
        Assert.DoesNotContain("// [CoverageProfile(\"Standard\")]", newContent);
    }

    [Fact]
    public void WriteProfileTag_ReturnsFalse_WhenAlreadyHasSameTag()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Critical"")]
namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        var result = _writer.WriteProfileTag(filePath, "Critical", createBackup: false);

        // Assert
        Assert.False(result); // File was not modified
        var newContent = File.ReadAllText(filePath);
        Assert.Equal(content, newContent); // Content unchanged
    }

    [Fact]
    public void WriteProfileTag_CreatesBackup_WhenRequested()
    {
        // Arrange
        var content = @"namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);
        var backupPath = filePath + ".backup";

        // Act
        _writer.WriteProfileTag(filePath, "Critical", createBackup: true);

        // Assert
        Assert.True(File.Exists(backupPath));
        var backupContent = File.ReadAllText(backupPath);
        Assert.Equal(content, backupContent);
    }

    [Fact]
    public void WriteProfileTag_HandlesFileScopedNamespace()
    {
        // Arrange
        var content = @"namespace MyApp.Services;

public class MyService
{
    public void DoWork() { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "BusinessLogic", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"BusinessLogic\")]", newContent);
        
        var lines = newContent.Split('\n');
        var tagIndex = Array.FindIndex(lines, l => l.Contains("[CoverageProfile(\"BusinessLogic\")]"));
        var namespaceIndex = Array.FindIndex(lines, l => l.Contains("namespace MyApp.Services;"));
        
        Assert.True(tagIndex >= 0);
        Assert.True(namespaceIndex > tagIndex);
    }

    [Fact]
    public void WriteProfileTag_HandlesRecord()
    {
        // Arrange
        var content = @"namespace MyApp.Models;

public record CustomerDto(int Id, string Name);";
        var filePath = CreateTestFile("CustomerDto.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Dto", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Dto\")]", newContent);
    }

    [Fact]
    public void WriteProfileTag_HandlesInterface()
    {
        // Arrange
        var content = @"namespace MyApp.Interfaces;

public interface IMyService
{
    void DoWork();
}";
        var filePath = CreateTestFile("IMyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Standard\")]", newContent);
    }

    #endregion

    #region WriteProfileTag - Negative Tests

    [Fact]
    public void WriteProfileTag_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "DoesNotExist.cs");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            _writer.WriteProfileTag(nonExistentPath, "Critical", createBackup: false));
    }

    [Fact]
    public void WriteProfileTag_PreservesFileContent_WhenAlreadyTagged()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Standard"")]
using System;

namespace MyApp.Services
{
    public class MyService
    {
        public void DoWork()
        {
            Console.WriteLine(""Working..."");
        }
    }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Equal(content, newContent); // Exactly the same
    }

    #endregion

    #region RemoveProfileTag - Positive Tests

    [Fact]
    public void RemoveProfileTag_RemovesExistingTag()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Critical"")]
namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        var result = _writer.RemoveProfileTag(filePath);

        // Assert
        Assert.True(result); // Tag was removed
        var newContent = File.ReadAllText(filePath);
        Assert.DoesNotContain("[CoverageProfile", newContent);
        Assert.Contains("namespace MyApp.Services", newContent);
    }

    [Fact]
    public void RemoveProfileTag_ReturnsFalse_WhenNoTag()
    {
        // Arrange
        var content = @"namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        var result = _writer.RemoveProfileTag(filePath);

        // Assert
        Assert.False(result); // No tag to remove
        var newContent = File.ReadAllText(filePath);
        Assert.Equal(content, newContent);
    }

    [Fact]
    public void RemoveProfileTag_CleansUpExtraBlankLines()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Critical"")]


namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.RemoveProfileTag(filePath);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.DoesNotContain("[CoverageProfile", newContent);
        // Should not have triple newlines
        Assert.DoesNotContain("\n\n\n", newContent);
    }

    #endregion

    #region RemoveProfileTag - Negative Tests

    [Fact]
    public void RemoveProfileTag_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "DoesNotExist.cs");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            _writer.RemoveProfileTag(nonExistentPath));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void WriteProfileTag_HandlesEmptyFile()
    {
        // Arrange
        var content = "";
        var filePath = CreateTestFile("Empty.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Standard\")]", newContent);
    }

    [Fact]
    public void WriteProfileTag_HandlesFileWithOnlyUsings()
    {
        // Arrange
        var content = @"using System;
using System.Collections.Generic;";
        var filePath = CreateTestFile("OnlyUsings.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Standard\")]", newContent);
        
        // Tag should be after usings
        var lines = newContent.Split('\n');
        var lastUsingIndex = Array.FindLastIndex(lines, l => l.Contains("using "));
        var tagIndex = Array.FindIndex(lines, l => l.Contains("[CoverageProfile"));
        
        Assert.True(tagIndex > lastUsingIndex);
    }

    [Fact]
    public void WriteProfileTag_PreservesIndentation()
    {
        // Arrange
        var content = @"    namespace MyApp.Services
    {
        public class MyService { }
    }";
        var filePath = CreateTestFile("Indented.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Standard", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Standard\")]", newContent);
        // Original indentation should be preserved
        Assert.Contains("    namespace MyApp.Services", newContent);
    }

    [Fact]
    public void WriteProfileTag_HandlesProfileNameWithSpecialCharacters()
    {
        // Arrange
        var content = @"namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);

        // Act
        _writer.WriteProfileTag(filePath, "Critical-V2", createBackup: false);

        // Assert
        var newContent = File.ReadAllText(filePath);
        Assert.Contains("// [CoverageProfile(\"Critical-V2\")]", newContent);
    }

    [Fact]
    public void WriteProfileTag_OverwritesBackupFile_IfAlreadyExists()
    {
        // Arrange
        var content = @"namespace MyApp.Services
{
    public class MyService { }
}";
        var filePath = CreateTestFile("MyService.cs", content);
        var backupPath = filePath + ".backup";
        
        // Create an old backup
        File.WriteAllText(backupPath, "old backup content");

        // Act
        _writer.WriteProfileTag(filePath, "Critical", createBackup: true);

        // Assert
        Assert.True(File.Exists(backupPath));
        var backupContent = File.ReadAllText(backupPath);
        Assert.Equal(content, backupContent); // Should have new backup, not old
        Assert.NotEqual("old backup content", backupContent);
    }

    #endregion
}
