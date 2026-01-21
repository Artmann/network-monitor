using System.Net.NetworkInformation;

Console.WriteLine("Network Monitor - Ping Test");
Console.WriteLine(new string('-', 40));

string[] targets = ["1.1.1.1", "8.8.8.8"];

using var ping = new Ping();

foreach (var target in targets)
{
    try
    {
        var reply = ping.Send(target, 3000);

        if (reply.Status == IPStatus.Success)
        {
            Console.WriteLine($"{target}: {reply.RoundtripTime}ms (TTL: {reply.Options?.Ttl})");
        }
        else
        {
            Console.WriteLine($"{target}: {reply.Status}");
        }
    }
    catch (PingException ex)
    {
        Console.WriteLine($"{target}: Error - {ex.Message}");
    }
}
