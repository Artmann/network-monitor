using System.Net.NetworkInformation;

// Parse command-line arguments
int? iterationLimit = null;
TimeSpan? timeLimit = null;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "-n" && i + 1 < args.Length)
    {
        if (int.TryParse(args[i + 1], out int count) && count > 0)
        {
            iterationLimit = count;
        }
        else
        {
            Console.Error.WriteLine($"Invalid iteration count: {args[i + 1]}");
            return 1;
        }
        i++;
    }
    else if (args[i] == "-t" && i + 1 < args.Length)
    {
        var duration = ParseDuration(args[i + 1]);
        if (duration.HasValue)
        {
            timeLimit = duration.Value;
        }
        else
        {
            Console.Error.WriteLine($"Invalid duration: {args[i + 1]}");
            return 1;
        }
        i++;
    }
}

bool isInteractive = !iterationLimit.HasValue && !timeLimit.HasValue;

// Main program
string[] targets = ["1.1.1.1", "8.8.8.8"];
var statistics = targets.ToDictionary(t => t, _ => new TargetStatistics());

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Apply time limit if specified
if (timeLimit.HasValue)
{
    cts.CancelAfter(timeLimit.Value);
}

// Only manipulate console in interactive mode
if (isInteractive)
{
    Console.CursorVisible = false;
    Console.Clear();
}

using var ping = new Ping();
int totalIterations = 0;

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        // Check iteration limit
        if (iterationLimit.HasValue && totalIterations >= iterationLimit.Value)
        {
            break;
        }

        // Pick random target
        var target = targets[Random.Shared.Next(targets.Length)];
        var stats = statistics[target];

        // Send ping
        try
        {
            var reply = ping.Send(target, 3000);
            var result = new PingResult(reply.Status == IPStatus.Success, reply.RoundtripTime);
            stats.AddResult(result);
        }
        catch (PingException)
        {
            stats.AddResult(new PingResult(false, 0));
        }

        totalIterations++;

        // Update display only in interactive mode
        if (isInteractive)
        {
            DrawUI(statistics);
        }

        // Wait random 200-500ms
        await Task.Delay(Random.Shared.Next(200, 501), cts.Token);
    }
}
catch (OperationCanceledException)
{
    // Expected on Ctrl+C or timeout
}
finally
{
    if (isInteractive)
    {
        Console.CursorVisible = true;
        Console.SetCursorPosition(0, 10);
        Console.WriteLine("\nMonitoring stopped.");
    }
    else
    {
        PrintResults(statistics);
    }
}

return 0;

TimeSpan? ParseDuration(string input)
{
    // Try simple formats first: "30s", "1m", "2h"
    if (input.Length >= 2)
    {
        var unit = input[^1];
        var numberPart = input[..^1];

        if (double.TryParse(numberPart, out double value))
        {
            return unit switch
            {
                's' => TimeSpan.FromSeconds(value),
                'm' => TimeSpan.FromMinutes(value),
                'h' => TimeSpan.FromHours(value),
                _ => null
            };
        }
    }

    // Try compound format: "1m30s", "2h30m"
    var totalSeconds = 0.0;
    var currentNumber = "";

    foreach (var c in input)
    {
        if (char.IsDigit(c) || c == '.')
        {
            currentNumber += c;
        }
        else if (currentNumber.Length > 0 && double.TryParse(currentNumber, out double num))
        {
            totalSeconds += c switch
            {
                's' => num,
                'm' => num * 60,
                'h' => num * 3600,
                _ => 0
            };
            currentNumber = "";
        }
    }

    return totalSeconds > 0 ? TimeSpan.FromSeconds(totalSeconds) : null;
}

void DrawUI(Dictionary<string, TargetStatistics> stats)
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                  Network Monitor - Live Stats                  ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║ Target        │ Sent  │    Loss    │  Min │  Avg │  P95 │ Last ║");
    Console.WriteLine("╠───────────────┼───────┼────────────┼──────┼──────┼──────┼──────╣");

    foreach (var (target, s) in stats)
    {
        var min = FormatMs(s.GetMin());
        var avg = FormatMs(s.GetAvg());
        var p95 = FormatMs(s.GetP95());
        var last = FormatMs(s.LastRoundtrip);
        var loss = $"{s.LossPercentage,4:F1}% ({s.LostCount})";

        Console.WriteLine($"║ {target,-13} │ {s.TotalSent,5} │ {loss,-10} │ {min,4} │ {avg,4} │ {p95,4} │ {last,4} ║");
    }

    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("                        Press Ctrl+C to exit                      ");
}

void PrintResults(Dictionary<string, TargetStatistics> stats)
{
    Console.WriteLine();
    Console.WriteLine($"{"Target",-15} {"Sent",6} {"Loss",12} {"Min",8} {"Avg",8} {"P95",8}");
    Console.WriteLine(new string('-', 60));

    foreach (var (target, s) in stats)
    {
        var min = FormatMsPlain(s.GetMin());
        var avg = FormatMsPlain(s.GetAvg());
        var p95 = FormatMsPlain(s.GetP95());
        var loss = $"{s.LossPercentage:F1}% ({s.LostCount})";

        Console.WriteLine($"{target,-15} {s.TotalSent,6} {loss,12} {min,8} {avg,8} {p95,8}");
    }

    Console.WriteLine();
    Console.WriteLine($"Total pings: {stats.Values.Sum(s => s.TotalSent)}");
}

string FormatMs(long? value) => value.HasValue ? $"{value}ms" : "  - ";

string FormatMsPlain(long? value) => value.HasValue ? $"{value}ms" : "-";

// Data structures (must be after top-level statements)
record PingResult(bool Success, long RoundtripTime);

class TargetStatistics
{
    private readonly List<PingResult> _results = [];

    public void AddResult(PingResult result) => _results.Add(result);

    public int TotalSent => _results.Count;

    public int LostCount => _results.Count(r => !r.Success);

    public double LossPercentage => TotalSent == 0 ? 0 : (double)LostCount / TotalSent * 100;

    public long? LastRoundtrip => _results.LastOrDefault(r => r.Success)?.RoundtripTime;

    public long? GetMin()
    {
        var successful = _results.Where(r => r.Success).ToList();
        return successful.Count == 0 ? null : successful.Min(r => r.RoundtripTime);
    }

    public long? GetAvg()
    {
        var successful = _results.Where(r => r.Success).ToList();
        return successful.Count == 0 ? null : (long)successful.Average(r => r.RoundtripTime);
    }

    public long? GetP95()
    {
        var successful = _results.Where(r => r.Success).OrderBy(r => r.RoundtripTime).ToList();
        if (successful.Count == 0) return null;
        int index = (int)Math.Ceiling(successful.Count * 0.95) - 1;
        return successful[Math.Max(0, index)].RoundtripTime;
    }
}
