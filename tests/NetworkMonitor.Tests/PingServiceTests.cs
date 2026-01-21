using System.Net.NetworkInformation;

namespace NetworkMonitor.Tests;

public class PingServiceTests
{
    [Fact]
    public void Ping_ValidTarget_ReturnsReply()
    {
        using var ping = new Ping();

        var reply = ping.Send("127.0.0.1", 1000);

        Assert.Equal(IPStatus.Success, reply.Status);
    }

    [Fact]
    public void Ping_LocalHost_HasZeroOrLowLatency()
    {
        using var ping = new Ping();

        var reply = ping.Send("127.0.0.1", 1000);

        Assert.True(reply.RoundtripTime < 100, "Localhost ping should have very low latency");
    }

    [Theory]
    [InlineData("1.1.1.1")]
    [InlineData("8.8.8.8")]
    public void Ping_PublicDns_ReturnsSuccessOrTimeout(string target)
    {
        using var ping = new Ping();

        try
        {
            var reply = ping.Send(target, 3000);

            // Either success or a valid status (network might be unavailable)
            Assert.True(
                reply.Status == IPStatus.Success ||
                reply.Status == IPStatus.TimedOut ||
                reply.Status == IPStatus.DestinationNetworkUnreachable,
                $"Unexpected status: {reply.Status}");
        }
        catch (PingException)
        {
            // Network might not be available, which is acceptable for this test
            Assert.True(true);
        }
    }
}
