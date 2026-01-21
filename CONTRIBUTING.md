# Contributing

## Development Setup

1. Install .NET 10.0 SDK or later
2. Clone the repository
3. Build the project:

```bash
dotnet build
```

## Project Structure

```
network-monitor/
├── src/
│   └── NetworkMonitor.Console/    # Main console application
│       └── Program.cs             # Entry point and all logic
├── tests/
│   └── NetworkMonitor.Tests/      # Unit tests
└── NetworkMonitor.sln             # Solution file
```

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Running the Application

```bash
# Interactive mode
dotnet run --project src/NetworkMonitor.Console

# Non-interactive with iteration limit
dotnet run --project src/NetworkMonitor.Console -- -n 20

# Non-interactive with time limit
dotnet run --project src/NetworkMonitor.Console -- -t 5s
```

## Code Style

- Use C# 12 features (collection expressions, primary constructors, etc.)
- Keep the codebase simple - single file for small utilities is acceptable
- Use top-level statements for console applications
- Prefer `var` for local variables when the type is obvious

## Testing Guidelines

- Write tests for any new statistical calculations
- Tests should not require network access (mock ping results)
- Keep tests fast and deterministic

## Pull Request Process

1. Ensure `dotnet build` succeeds with no warnings
2. Ensure `dotnet test` passes
3. Test both interactive and non-interactive modes manually
4. Update README.md if adding new features or options
