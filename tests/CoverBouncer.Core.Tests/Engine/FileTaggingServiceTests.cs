using CoverBouncer.Core.Engine;
using Xunit;

namespace CoverBouncer.Core.Tests.Engine;

public class FileTaggingServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileTaggingService _service;

    public FileTaggingServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"CoverBouncerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _service = new FileTaggingService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    private string CreateTestFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_testDirectory, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    private string GetSimpleClassContent(string className)
    {
        return $@"namespace MyApp.Services
{{
    public class {className} {{ }}
}}";
    }

    #region TagByPattern - Positive Tests

    [Fact]
    public void TagByPattern_TagsMatchingFiles()
    {
        // Arrange
        CreateTestFile("Services/CustomerService.cs", GetSimpleClassContent("CustomerService"));
        CreateTestFile("Services/OrderService.cs", GetSimpleClassContent("OrderService"));
        CreateTestFile("Models/Customer.cs", GetSimpleClassContent("Customer"));

        // Act
        var result = _service.TagByPattern(_testDirectory, "**/*Service.cs", "BusinessLogic");

        // Assert
        Assert.Equal(2, result.FilesMatched);
        Assert.Equal(2, result.FilesTagged);
        Assert.Equal(0, result.FilesSkipped);
        Assert.Equal(0, result.FilesErrored);
    }

    [Fact]
    public void TagByPattern_OnlyTagsCsFiles()
    {
        // Arrange
        CreateTestFile("Services/CustomerService.cs", GetSimpleClassContent("CustomerService"));
        CreateTestFile("Services/data.json", "{\"test\": true}");
        CreateTestFile("Services/README.md", "# README");

        // Act
        var result = _service.TagByPattern(_testDirectory, "Services/*.*", "Standard");

        // Assert
        Assert.Equal(1, result.FilesTagged); // Only .cs file
    }

    [Fact]
    public void TagByPattern_DryRun_DoesNotModifyFiles()
    {
        // Arrange
        var filePath = CreateTestFile("MyService.cs", GetSimpleClassContent("MyService"));
        var originalContent = File.ReadAllText(filePath);

        // Act
        var result = _service.TagByPattern(_testDirectory, "*.cs", "Critical", dryRun: true);

        // Assert
        Assert.Equal(1, result.FilesTagged);
        var currentContent = File.ReadAllText(filePath);
        Assert.Equal(originalContent, currentContent); // File not modified
    }

    [Fact]
    public void TagByPattern_SkipsAlreadyTaggedFiles()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Critical"")]
namespace MyApp.Services
{
    public class MyService { }
}";
        CreateTestFile("MyService.cs", content);

        // Act
        var result = _service.TagByPattern(_testDirectory, "*.cs", "Critical");

        // Assert
        Assert.Equal(1, result.FilesMatched);
        Assert.Equal(0, result.FilesTagged);
        Assert.Equal(1, result.FilesSkipped);
    }

    [Fact]
    public void TagByPattern_CreatesBackups_WhenRequested()
    {
        // Arrange
        var filePath = CreateTestFile("MyService.cs", GetSimpleClassContent("MyService"));

        // Act
        _service.TagByPattern(_testDirectory, "*.cs", "Standard", createBackup: true);

        // Assert
        Assert.True(File.Exists(filePath + ".backup"));
    }

    #endregion

    #region TagByPattern - Negative Tests

    [Fact]
    public void TagByPattern_ReturnsZero_WhenNoFilesMatch()
    {
        // Arrange
        CreateTestFile("Models/Customer.cs", GetSimpleClassContent("Customer"));

        // Act
        var result = _service.TagByPattern(_testDirectory, "**/*Service.cs", "BusinessLogic");

        // Assert
        Assert.Equal(0, result.FilesMatched);
        Assert.Equal(0, result.FilesTagged);
    }

    [Fact]
    public void TagByPattern_HandlesInvalidPattern_Gracefully()
    {
        // Arrange
        CreateTestFile("MyService.cs", GetSimpleClassContent("MyService"));

        // Act
        var result = _service.TagByPattern(_testDirectory, "", "Standard");

        // Assert
        // Should handle gracefully without throwing
        Assert.Equal(0, result.FilesMatched);
    }

    #endregion

    #region TagByDirectory - Positive Tests

    [Fact]
    public void TagByDirectory_TagsAllFilesInDirectory()
    {
        // Arrange
        var servicesDir = Path.Combine(_testDirectory, "Services");
        Directory.CreateDirectory(servicesDir);
        CreateTestFile("Services/CustomerService.cs", GetSimpleClassContent("CustomerService"));
        CreateTestFile("Services/OrderService.cs", GetSimpleClassContent("OrderService"));

        // Act
        var result = _service.TagByDirectory(servicesDir, "BusinessLogic");

        // Assert
        Assert.Equal(2, result.FilesTagged);
    }

    [Fact]
    public void TagByDirectory_Recursive_TagsSubdirectories()
    {
        // Arrange
        var servicesDir = Path.Combine(_testDirectory, "Services");
        Directory.CreateDirectory(servicesDir);
        CreateTestFile("Services/CustomerService.cs", GetSimpleClassContent("CustomerService"));
        CreateTestFile("Services/Internal/InternalService.cs", GetSimpleClassContent("InternalService"));

        // Act
        var result = _service.TagByDirectory(servicesDir, "BusinessLogic", recursive: true);

        // Assert
        Assert.Equal(2, result.FilesTagged);
    }

    [Fact]
    public void TagByDirectory_NonRecursive_SkipsSubdirectories()
    {
        // Arrange
        var servicesDir = Path.Combine(_testDirectory, "Services");
        Directory.CreateDirectory(servicesDir);
        CreateTestFile("Services/CustomerService.cs", GetSimpleClassContent("CustomerService"));
        CreateTestFile("Services/Internal/InternalService.cs", GetSimpleClassContent("InternalService"));

        // Act
        var result = _service.TagByDirectory(servicesDir, "BusinessLogic", recursive: false);

        // Assert
        Assert.Equal(1, result.FilesTagged); // Only top-level file
    }

    #endregion

    #region TagByDirectory - Negative Tests

    [Fact]
    public void TagByDirectory_ThrowsDirectoryNotFoundException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "DoesNotExist");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() =>
            _service.TagByDirectory(nonExistentDir, "Standard"));
    }

    #endregion

    #region TagFiles - Positive Tests

    [Fact]
    public void TagFiles_TagsSpecifiedFiles()
    {
        // Arrange
        var file1 = CreateTestFile("Service1.cs", GetSimpleClassContent("Service1"));
        var file2 = CreateTestFile("Service2.cs", GetSimpleClassContent("Service2"));
        var files = new[] { file1, file2 };

        // Act
        var result = _service.TagFiles(files, "Critical");

        // Assert
        Assert.Equal(2, result.FilesTagged);
        Assert.Equal(0, result.FilesErrored);
    }

    [Fact]
    public void TagFiles_ReportsErrors_ForNonExistentFiles()
    {
        // Arrange
        var existingFile = CreateTestFile("Service1.cs", GetSimpleClassContent("Service1"));
        var nonExistentFile = Path.Combine(_testDirectory, "DoesNotExist.cs");
        var files = new[] { existingFile, nonExistentFile };

        // Act
        var result = _service.TagFiles(files, "Standard");

        // Assert
        Assert.Equal(1, result.FilesTagged);
        Assert.Equal(1, result.FilesErrored);
        Assert.Single(result.Errors);
        Assert.Contains("DoesNotExist.cs", result.Errors[0].File);
    }

    [Fact]
    public void TagFiles_DryRun_CountsFilesWithoutModifying()
    {
        // Arrange
        var file = CreateTestFile("MyService.cs", GetSimpleClassContent("MyService"));
        var originalContent = File.ReadAllText(file);

        // Act
        var result = _service.TagFiles(new[] { file }, "Critical", dryRun: true);

        // Assert
        Assert.Equal(1, result.FilesTagged);
        var currentContent = File.ReadAllText(file);
        Assert.Equal(originalContent, currentContent);
    }

    #endregion

    #region SuggestProfiles - Positive Tests

    [Fact]
    public void SuggestProfiles_SuggestsCritical_ForSecurityFiles()
    {
        // Arrange
        var files = new[]
        {
            CreateTestFile("Security/AuthenticationService.cs", GetSimpleClassContent("AuthenticationService")),
            CreateTestFile("Auth/AuthorizationService.cs", GetSimpleClassContent("AuthorizationService")),
            CreateTestFile("PaymentProcessor.cs", GetSimpleClassContent("PaymentProcessor"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.All(suggestions.Values, profile => Assert.Equal("Critical", profile));
    }

    [Fact]
    public void SuggestProfiles_SuggestsBusinessLogic_ForServiceFiles()
    {
        // Arrange
        var files = new[]
        {
            CreateTestFile("CustomerService.cs", GetSimpleClassContent("CustomerService")),
            CreateTestFile("OrderManager.cs", GetSimpleClassContent("OrderManager")),
            CreateTestFile("Services/InventoryService.cs", GetSimpleClassContent("InventoryService"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.All(suggestions.Values, profile => Assert.Equal("BusinessLogic", profile));
    }

    [Fact]
    public void SuggestProfiles_SuggestsIntegration_ForControllers()
    {
        // Arrange
        var files = new[]
        {
            CreateTestFile("CustomersController.cs", GetSimpleClassContent("CustomersController")),
            CreateTestFile("Controllers/OrdersController.cs", GetSimpleClassContent("OrdersController")),
            CreateTestFile("ApiAdapter.cs", GetSimpleClassContent("ApiAdapter"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.All(suggestions.Values, profile => Assert.Equal("Integration", profile));
    }

    [Fact]
    public void SuggestProfiles_SuggestsDto_ForModelFiles()
    {
        // Arrange
        var files = new[]
        {
            CreateTestFile("CustomerDto.cs", GetSimpleClassContent("CustomerDto")),
            CreateTestFile("OrderViewModel.cs", GetSimpleClassContent("OrderViewModel")),
            CreateTestFile("Models/Product.cs", GetSimpleClassContent("Product"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.All(suggestions.Values, profile => Assert.Equal("Dto", profile));
    }

    [Fact]
    public void SuggestProfiles_SuggestsStandard_ForOtherFiles()
    {
        // Arrange
        var files = new[]
        {
            CreateTestFile("Helper.cs", GetSimpleClassContent("Helper")),
            CreateTestFile("Utilities/StringExtensions.cs", GetSimpleClassContent("StringExtensions"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.All(suggestions.Values, profile => Assert.Equal("Standard", profile));
    }

    [Fact]
    public void SuggestProfiles_PrioritizesCritical_OverOtherPatterns()
    {
        // Arrange - File in Security folder that's also a Service
        var files = new[]
        {
            CreateTestFile("Security/AuthenticationService.cs", GetSimpleClassContent("AuthenticationService"))
        };

        // Act
        var suggestions = _service.SuggestProfiles(files);

        // Assert
        Assert.Equal("Critical", suggestions.Values.First()); // Security takes priority
    }

    #endregion

    #region ReadFileList - Positive Tests

    [Fact]
    public void ReadFileList_ReadsFilePathsFromTextFile()
    {
        // Arrange
        var listFilePath = Path.Combine(_testDirectory, "files.txt");
        File.WriteAllText(listFilePath, @"file1.cs
file2.cs
file3.cs");

        // Act
        var files = FileTaggingService.ReadFileList(listFilePath);

        // Assert
        Assert.Equal(3, files.Count);
        Assert.Contains("file1.cs", files);
        Assert.Contains("file2.cs", files);
        Assert.Contains("file3.cs", files);
    }

    [Fact]
    public void ReadFileList_SkipsEmptyLines()
    {
        // Arrange
        var listFilePath = Path.Combine(_testDirectory, "files.txt");
        File.WriteAllText(listFilePath, @"file1.cs

file2.cs

file3.cs");

        // Act
        var files = FileTaggingService.ReadFileList(listFilePath);

        // Assert
        Assert.Equal(3, files.Count);
    }

    [Fact]
    public void ReadFileList_SkipsCommentLines()
    {
        // Arrange
        var listFilePath = Path.Combine(_testDirectory, "files.txt");
        File.WriteAllText(listFilePath, @"# This is a comment
file1.cs
# Another comment
file2.cs");

        // Act
        var files = FileTaggingService.ReadFileList(listFilePath);

        // Assert
        Assert.Equal(2, files.Count);
        Assert.DoesNotContain(files, f => f.StartsWith("#"));
    }

    [Fact]
    public void ReadFileList_TrimsWhitespace()
    {
        // Arrange
        var listFilePath = Path.Combine(_testDirectory, "files.txt");
        File.WriteAllText(listFilePath, @"  file1.cs  
   file2.cs
file3.cs   ");

        // Act
        var files = FileTaggingService.ReadFileList(listFilePath);

        // Assert
        Assert.All(files, f => Assert.Equal(f.Trim(), f));
    }

    #endregion

    #region ReadFileList - Negative Tests

    [Fact]
    public void ReadFileList_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "DoesNotExist.txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            FileTaggingService.ReadFileList(nonExistentPath));
    }

    #endregion

    #region Edge Cases and Integration

    [Fact]
    public void TagByPattern_HandlesMultipleExtensions()
    {
        // Arrange
        CreateTestFile("Service.cs", GetSimpleClassContent("Service"));
        CreateTestFile("Service.txt", "not a cs file");
        CreateTestFile("Service.json", "{}");

        // Act
        var result = _service.TagByPattern(_testDirectory, "Service.*", "Standard");

        // Assert
        Assert.Equal(1, result.FilesTagged); // Only .cs file
    }

    [Fact]
    public void TagFiles_UpdatesExistingTags()
    {
        // Arrange
        var content = @"// [CoverageProfile(""Standard"")]
namespace MyApp.Services
{
    public class MyService { }
}";
        var file = CreateTestFile("MyService.cs", content);

        // Act
        var result = _service.TagFiles(new[] { file }, "Critical");

        // Assert
        Assert.Equal(1, result.FilesTagged);
        var newContent = File.ReadAllText(file);
        Assert.Contains("[CoverageProfile(\"Critical\")]", newContent);
        Assert.DoesNotContain("[CoverageProfile(\"Standard\")]", newContent);
    }

    [Fact]
    public void TagByDirectory_HandlesEmptyDirectory()
    {
        // Arrange
        var emptyDir = Path.Combine(_testDirectory, "Empty");
        Directory.CreateDirectory(emptyDir);

        // Act
        var result = _service.TagByDirectory(emptyDir, "Standard");

        // Assert
        Assert.Equal(0, result.FilesMatched);
        Assert.Equal(0, result.FilesTagged);
    }

    [Fact]
    public void TagByPattern_HandlesNestedDirectories()
    {
        // Arrange
        CreateTestFile("Level1/Service1.cs", GetSimpleClassContent("Service1"));
        CreateTestFile("Level1/Level2/Service2.cs", GetSimpleClassContent("Service2"));
        CreateTestFile("Level1/Level2/Level3/Service3.cs", GetSimpleClassContent("Service3"));

        // Act
        var result = _service.TagByPattern(_testDirectory, "**/*Service*.cs", "BusinessLogic");

        // Assert
        Assert.Equal(3, result.FilesTagged);
    }

    [Fact]
    public void TagFiles_ContinuesOnError()
    {
        // Arrange
        var goodFile = CreateTestFile("Good.cs", GetSimpleClassContent("Good"));
        var badFile = Path.Combine(_testDirectory, "NonExistent.cs");
        var anotherGoodFile = CreateTestFile("AlsoGood.cs", GetSimpleClassContent("AlsoGood"));

        // Act
        var result = _service.TagFiles(new[] { goodFile, badFile, anotherGoodFile }, "Standard");

        // Assert
        Assert.Equal(2, result.FilesTagged); // Both good files should be tagged
        Assert.Equal(1, result.FilesErrored);
    }

    #endregion
}
