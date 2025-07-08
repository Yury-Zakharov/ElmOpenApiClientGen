# Contributing to Elm OpenAPI Client Generator

Thank you for your interest in contributing to ElmOpenApiClientGen! We welcome contributions from the community and are pleased to have you join us.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation Guidelines](#documentation-guidelines)
- [Issue Reporting](#issue-reporting)
- [Community Guidelines](#community-guidelines)

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to [Yury Zakharov](mailto:colonelcolt+github@gmail.com).

### Our Pledge

We pledge to make participation in our project and our community a harassment-free experience for everyone, regardless of age, body size, disability, ethnicity, sex characteristics, gender identity and expression, level of experience, education, socio-economic status, nationality, personal appearance, race, religion, or sexual identity and orientation.

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Git](https://git-scm.com/)
- [Elm 0.19.1](https://guide.elm-lang.org/install/elm.html) (for testing generated code)
- [Docker](https://www.docker.com/get-started) (for containerized testing)

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:

```bash
git clone https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git
cd ElmOpenApiClientGen
```

3. Add the original repository as an upstream remote:

```bash
git remote add upstream https://github.com/Yury-Zakharov/ElmOpenApiClientGen.git
```

## How to Contribute

### Types of Contributions

We welcome several types of contributions:

- **Bug Reports** - Help us identify and fix issues
- **Feature Requests** - Suggest new features or improvements
- **Code Contributions** - Submit bug fixes, new features, or improvements
- **Documentation** - Improve existing documentation or add new guides
- **Testing** - Add test cases or improve test coverage
- **Examples** - Provide usage examples or sample integrations

### Before You Start

1. **Check existing issues** - Look through existing issues to see if your contribution is already being discussed
2. **Create an issue** - For significant changes, create an issue first to discuss your approach
3. **Get feedback** - Wait for feedback from maintainers before starting major work

## Development Setup

### Initial Setup

```bash
# Navigate to the project directory
cd ElmOpenApiClientGen

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests to verify everything works
dotnet test
```

### Verify Installation

```bash
# Run integration tests
cd integration-tests
./run-integration-test.sh

# Run containerized tests (optional)
./run-containerized-test.sh
```

### Development Workflow

1. **Create a branch** for your feature or bug fix:
```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

2. **Make your changes** following our coding standards

3. **Test your changes**:
```bash
# Run unit tests
dotnet test

# Run integration tests
cd integration-tests && ./run-integration-test.sh
```

4. **Commit your changes** with clear, descriptive commit messages

5. **Push to your fork** and create a pull request

## Pull Request Process

### Before Submitting

- [ ] All tests pass (`dotnet test`)
- [ ] Integration tests pass (`./run-integration-test.sh`)
- [ ] Code follows our coding standards
- [ ] Documentation is updated if needed
- [ ] Commit messages are clear and descriptive

### Pull Request Guidelines

1. **Title**: Use a clear, descriptive title
   - ✅ Good: "Add URL input support for remote OpenAPI specs"
   - ❌ Bad: "Fixed stuff"

2. **Description**: Include:
   - What changes were made and why
   - Link to related issues
   - Screenshots (if applicable)
   - Breaking changes (if any)

3. **Size**: Keep PRs focused and reasonably sized
   - Large changes should be discussed first
   - Consider breaking large changes into smaller PRs

### Review Process

- **Maintainer Review**: All PRs require approval from @Yury-Zakharov or assigned maintainers
- **Automated Checks**: PRs must pass all automated tests and checks
- **Feedback**: Address review feedback promptly and professionally
- **Merge**: Only maintainers can merge approved PRs

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No breaking changes (or clearly documented)
```

## Coding Standards

### F# Style Guidelines

- Follow [F# Style Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/style-guide/)
- Use 4 spaces for indentation
- Prefer immutable data structures
- Use meaningful variable and function names
- Add XML documentation for public APIs

### Code Organization

```fsharp
// Good: Clear module structure
module ElmOpenApiClientGen.Generator.Parser

open System
open System.IO

let private helperFunction input =
    // implementation

let publicFunction input =
    // implementation
```

### Error Handling

- **Never throw exceptions** - Use defensive programming
- **Provide diagnostics** - Generate helpful error comments
- **Graceful degradation** - Always try to generate something useful

```fsharp
// Good: Defensive programming
let processInput input =
    try
        // main processing
        result
    with
    | ex -> 
        // Generate comment explaining the issue
        generateErrorComment ex.Message
        fallbackResult
```

### Defensive Programming

This project follows strict defensive programming principles:

- **Never crash** on invalid input
- **Always generate output** even for malformed specs
- **Provide clear diagnostics** through code comments
- **Validate inputs** before processing
- **Handle edge cases** gracefully

## Testing Guidelines

### Test Categories

1. **Unit Tests** - Test individual functions and components
2. **Integration Tests** - Test complete workflows
3. **Negative Tests** - Test error handling and edge cases
4. **Performance Tests** - Test with large specifications

### Writing Tests

```fsharp
[<Test>]
let ``should handle invalid OpenAPI spec gracefully`` () =
    // Arrange
    let invalidSpec = "invalid: yaml: content"
    
    // Act
    let result = Parser.loadSpec invalidSpec
    
    // Assert
    // Should not throw, should provide diagnostic
    Assert.IsNotNull(result)
```

### Test Requirements

- **All tests must pass** before submission
- **Add tests** for new functionality
- **Test edge cases** and error conditions
- **Test both positive and negative scenarios**

## Documentation Guidelines

### Documentation Types

1. **Code Documentation** - XML docs for public APIs
2. **README Updates** - Keep README.md current
3. **API Documentation** - Document new features and changes
4. **Examples** - Provide usage examples

### Writing Guidelines

- Use clear, concise language
- Provide practical examples
- Keep documentation up-to-date with code changes
- Include both basic and advanced usage scenarios

## Issue Reporting

### Bug Reports

When reporting bugs, please include:

- **Clear title** describing the issue
- **Steps to reproduce** the problem
- **Expected vs actual behaviour**
- **Environment details** (OS, .NET version, etc.)
- **Sample OpenAPI spec** that demonstrates the issue (if applicable)
- **Generated code output** (if relevant)

### Feature Requests

For feature requests, please include:

- **Use case** - Why is this feature needed?
- **Proposed solution** - How should it work?
- **Alternatives considered** - What other approaches were considered?
- **Examples** - Provide concrete examples if possible

### Issue Labels

We use the following labels to categorize issues:

- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `question` - Further information is requested

## Community Guidelines

### Communication

- **Be respectful** and inclusive in all interactions
- **Be patient** - maintainers and contributors volunteer their time
- **Be constructive** - provide helpful feedback and suggestions
- **Ask questions** - don't hesitate to ask for clarification

### Getting Help

- **GitHub Issues** - For bugs and feature requests
- **GitHub Discussions** - For questions and general discussion
- **Code Reviews** - For feedback on your contributions

### Recognition

We value all contributions and will:

- **Credit contributors** in release notes
- **Respond promptly** to issues and PRs
- **Provide feedback** to help you improve your contributions
- **Maintain a welcoming environment** for all skill levels

## Maintainer Information

### Current Maintainers

- **@Yury-Zakharov** - Project Owner and Lead Maintainer

### Maintainer Responsibilities

Maintainers are responsible for:

- **Reviewing and merging** pull requests
- **Triaging issues** and feature requests
- **Maintaining project** direction and standards
- **Releasing new versions** and managing releases
- **Community moderation** and support

### Becoming a Maintainer

Exceptional contributors may be invited to become maintainers based on:

- **Quality contributions** over time
- **Community involvement** and helpfulness
- **Technical expertise** and project knowledge
- **Alignment with project values** and goals

## Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality additions
- **PATCH** version for backwards-compatible bug fixes

### Release Cycle

- **Regular releases** based on accumulated changes
- **Hotfix releases** for critical bug fixes
- **Release notes** documenting all changes
- **Migration guides** for breaking changes

## License

By contributing to ElmOpenApiClientGen, you agree that your contributions will be licensed under the same [MIT License](LICENSE) that covers the project.

---

Thank you for contributing to ElmOpenApiClientGen! Your efforts help make this tool better for the entire Elm and F# community.
