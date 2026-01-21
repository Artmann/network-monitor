# CLAUDE.md

This file provides guidance for AI agents working on this codebase.

## Project Overview

Network Monitor is a simple .NET console application that pings network targets and displays latency statistics. It supports both interactive (live-updating) and non-interactive (batch) modes.

## Build and Test Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run interactive mode
dotnet run --project src/NetworkMonitor.Console

# Run non-interactive mode
dotnet run --project src/NetworkMonitor.Console -- -n 20
dotnet run --project src/NetworkMonitor.Console -- -t 5s
```

## Architecture

The entire application is in a single file: `src/NetworkMonitor.Console/Program.cs`

Key components:
- **Argument parsing**: Lines 3-36, handles `-n` and `-t` flags
- **Main loop**: Lines 67-103, pings random targets at 200-500ms intervals
- **ParseDuration()**: Parses duration strings like `30s`, `1m`, `1m30s`
- **DrawUI()**: Box-drawing UI for interactive mode
- **PrintResults()**: Plain text output for non-interactive mode
- **TargetStatistics class**: Tracks ping results and calculates min/avg/P95

## Key Design Decisions

1. **Single file**: Intentionally kept simple. Don't split into multiple files unless complexity warrants it.
2. **Random target selection**: Pings are distributed randomly across targets, not round-robin.
3. **Random delay**: 200-500ms between pings to avoid predictable patterns.
4. **Top-level statements**: Uses C# top-level statements pattern; classes must be defined after all executable statements.

## Common Tasks

### Adding a new command-line flag
Add parsing in the argument loop (lines 7-35), then use the parsed value in the main loop or UI functions.

### Adding a new statistic
1. Add calculation method to `TargetStatistics` class
2. Add column to `DrawUI()` for interactive mode
3. Add column to `PrintResults()` for non-interactive mode

### Changing ping targets
Modify the `targets` array on line 41.

## Gotchas

- `Console.SetCursorPosition()` is only called in interactive mode
- The `TargetStatistics` class must be defined after all top-level statements
- Duration parsing supports both simple (`30s`) and compound (`1m30s`) formats
