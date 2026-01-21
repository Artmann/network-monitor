using System.Net.NetworkInformation;

// Main program
string[] targets = ["1.1.1.1", "8.8.8.8"];
var statistics = targets.ToDictionary(t => t, _ => new TargetStatistics());

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

Console.CursorVisible = false;
Console.Clear();

using var ping = new Ping();

try
{
    while (!cts.Token.IsCancellationRequested)
    {
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

        // Update display
        DrawUI(statistics);

        // Wait random 200-500ms
        await Task.Delay(Random.Shared.Next(200, 501), cts.Token);
    }
}
catch (OperationCanceledException)
{
    // Expected on Ctrl+C
}
finally
{
    Console.CursorVisible = true;
    Console.SetCursorPosition(0, 10);
    Console.WriteLine("\nMonitoring stopped.");
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

string FormatMs(long? value) => value.HasValue ? $"{value}ms" : "  - ";

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
