# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Currently supported versions:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.0-preview.x   | :white_check_mark: |

## Reporting a Vulnerability

We take the security of Cover-Bouncer seriously. If you believe you have found a security vulnerability, please report it to us as described below.

### GitHub Security Advisories

**Please do NOT report security vulnerabilities through public GitHub issues.**

Instead, please report them using [GitHub Security Advisories](https://github.com/DeusBlu/cover-bouncer/security/advisories/new).

### What to Include

Please include the following information in your report:

- **Type of vulnerability** (e.g., code injection, path traversal, etc.)
- **Full paths of source file(s)** related to the vulnerability
- **Location of the affected source code** (tag/branch/commit or direct URL)
- **Step-by-step instructions to reproduce** the issue
- **Proof-of-concept or exploit code** (if possible)
- **Impact of the vulnerability**, including how an attacker might exploit it

### What to Expect

- **Acknowledgment**: We will acknowledge receipt of your vulnerability report within 3 business days
- **Communication**: We will keep you informed about our progress in addressing the issue
- **Timeline**: We aim to release security patches within 30 days of disclosure for critical vulnerabilities
- **Credit**: If you wish, we will publicly credit you for the discovery once the vulnerability is fixed

### Security Update Process

1. **Receive report** via GitHub Security Advisory
2. **Assess severity** and impact
3. **Develop and test fix** in private repository
4. **Release patched version** to NuGet.org
5. **Publish security advisory** on GitHub
6. **Notify users** through release notes and GitHub notifications

## Security Best Practices

When using Cover-Bouncer in your projects:

- **Keep updated**: Always use the latest version of Cover-Bouncer
- **Review configuration**: Ensure your `coverbouncer.json` doesn't contain sensitive information
- **File paths**: Be cautious with file path patterns in exclude lists
- **CI/CD integration**: Use secure methods to manage coverage reports in your pipeline

## Scope

This security policy applies to:

- The Cover-Bouncer core library (`CoverBouncer.Core`)
- The MSBuild integration package (`CoverBouncer.MSBuild`)
- The CLI tool (`CoverBouncer.CLI`)
- All adapter libraries (e.g., `CoverBouncer.Coverlet`)

## Contact

For general security questions (not vulnerability reports), you can reach out through:

- GitHub Issues: https://github.com/DeusBlu/cover-bouncer/issues
- GitHub Discussions: https://github.com/DeusBlu/cover-bouncer/discussions

For security vulnerabilities, please use the [GitHub Security Advisory](https://github.com/DeusBlu/cover-bouncer/security/advisories/new) process.

---

**Thank you for helping keep Cover-Bouncer and its users safe!** 🛡️
