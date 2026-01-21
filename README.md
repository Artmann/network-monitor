# Network Monitor

A simple command-line network monitoring tool that pings multiple targets and displays live statistics.

## Features

- Live updating display with latency statistics
- Tracks min, average, and P95 latency per target
- Packet loss monitoring
- Interactive and non-interactive modes
- Configurable iteration and time limits

## Requirements

- .NET 10.0 SDK or later

## Installation

```bash
git clone <repository-url>
cd network-monitor
dotnet build
```

## Usage

### Interactive Mode (Default)

Run without arguments for a live-updating display:

```bash
dotnet run --project src/NetworkMonitor.Console
```

Press `Ctrl+C` to stop monitoring.

### Non-Interactive Mode

Run for a fixed number of pings:

```bash
# Run 100 total pings across all targets
dotnet run --project src/NetworkMonitor.Console -- -n 100
```

Run for a fixed duration:

```bash
# Run for 30 seconds
dotnet run --project src/NetworkMonitor.Console -- -t 30s

# Run for 1 minute
dotnet run --project src/NetworkMonitor.Console -- -t 1m

# Run for 1 minute 30 seconds
dotnet run --project src/NetworkMonitor.Console -- -t 1m30s
```

## Command-Line Options

| Option | Description | Example |
|--------|-------------|---------|
| `-n <count>` | Stop after N total pings | `-n 100` |
| `-t <duration>` | Stop after duration | `-t 30s`, `-t 1m`, `-t 1m30s` |

## License

MIT License - see [LICENSE](LICENSE) for details.
