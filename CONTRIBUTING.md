# Contributing to Cover-Bouncer

Thank you for your interest in contributing to Cover-Bouncer! 🎉

## Getting Started

1. **Fork the repository**
2. **Clone your fork:**
   ```bash
   git clone https://github.com/YOUR-USERNAME/cover-bouncer.git
   cd cover-bouncer
   ```
3. **Build the solution:**
   ```bash
   dotnet build
   ```
4. **Run tests:**
   ```bash
   dotnet test
   ```

## Development Workflow

### Branch Naming
- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring

### Commit Messages
Follow conventional commits:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `test:` - Adding or updating tests
- `refactor:` - Code refactoring
- `chore:` - Maintenance tasks

Example:
```
feat: add support for branch coverage thresholds
fix: handle missing coverage report gracefully
docs: update getting started guide
```

### Pull Request Process

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/my-awesome-feature
   ```

2. **Make your changes** and commit them

3. **Write or update tests** for your changes

4. **Ensure all tests pass:**
   ```bash
   dotnet test
   ```

5. **Push to your fork:**
   ```bash
   git push origin feature/my-awesome-feature
   ```

6. **Open a Pull Request** with:
   - Clear description of changes
   - Link to related issues
   - Screenshots (if UI changes)

## Code Standards

### C# Style Guide
- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Testing
- Write unit tests for all new functionality
- Aim for high code coverage (we dogfood our own tool!)
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Use FluentAssertions for assertions

Example:
```csharp
[Fact]
public void PolicyEngine_WhenCoverageBelowThreshold_ReturnsViolation()
{
    // Arrange
    var policy = new CoveragePolicy { MinLine = 0.80 };
    var coverage = new FileCoverage { LineRate = 0.70 };
    
    // Act
    var result = engine.Validate(policy, coverage);
    
    // Assert
    result.Should().ContainViolation();
}
```

## Project Structure

```
cover-bouncer/
├── src/
│   ├── CoveragePolicy.Core/          # Core policy engine
│   ├── CoveragePolicy.Coverlet/      # Coverlet adapter
│   ├── CoveragePolicy.CLI/           # CLI tool
│   └── CoveragePolicy.MSBuild/       # MSBuild integration
├── tests/
│   ├── CoveragePolicy.Core.Tests/
│   ├── CoveragePolicy.Coverlet.Tests/
│   └── CoveragePolicy.Integration.Tests/
└── docs/                             # Documentation
```

## Areas for Contribution

### Good First Issues
- Documentation improvements
- Example projects
- Bug fixes with clear reproduction steps

### Feature Development
- New coverage report adapters
- Enhanced reporting formats
- Roslyn analyzer features

### Documentation
- Tutorial content
- CI/CD integration examples
- Best practices guides

## Questions?

- Open an issue for questions
- Join discussions in GitHub Discussions
- Check existing issues and PRs first

## Code of Conduct

- Be respectful and inclusive
- Provide constructive feedback
- Help others learn and grow
- Focus on what's best for the community

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to Cover-Bouncer! 🚀
