namespace NetworkMonitor.Tests;

public class TargetStatisticsTests
{
    [Fact]
    public void AddResult_TracksLifetimeTotalSent()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(true, 20));
        stats.AddResult(new PingResult(false, 0));

        Assert.Equal(3, stats.TotalSent);
    }

    [Fact]
    public void AddResult_TracksLifetimeLostCount()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(false, 0));
        stats.AddResult(new PingResult(false, 0));

        Assert.Equal(2, stats.LostCount);
    }

    [Fact]
    public void LossPercentage_CalculatesCorrectly()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(false, 0));
        stats.AddResult(new PingResult(true, 20));
        stats.AddResult(new PingResult(false, 0));

        Assert.Equal(50.0, stats.LossPercentage);
    }

    [Fact]
    public void GetMin_ReturnsMinimumLatency()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 50));
        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(true, 30));

        Assert.Equal(10, stats.GetMin());
    }

    [Fact]
    public void GetAvg_ReturnsAverageLatency()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(true, 20));
        stats.AddResult(new PingResult(true, 30));

        Assert.Equal(20, stats.GetAvg());
    }

    [Fact]
    public void GetP95_ReturnsPercentileLatency()
    {
        var stats = new TargetStatistics();

        // Add 100 results with latencies 1-100
        for (int i = 1; i <= 100; i++)
        {
            stats.AddResult(new PingResult(true, i));
        }

        // P95 of 1-100 should be 95
        Assert.Equal(95, stats.GetP95());
    }

    [Fact]
    public void SlidingWindow_BoundsMemoryAt1000Results()
    {
        var stats = new TargetStatistics();

        // Add 1500 results
        for (int i = 0; i < 1500; i++)
        {
            stats.AddResult(new PingResult(true, i));
        }

        // Lifetime counter should track all 1500
        Assert.Equal(1500, stats.TotalSent);

        // Min should be from sliding window (last 1000 results: 500-1499)
        // So minimum latency in window should be 500
        Assert.Equal(500, stats.GetMin());
    }

    [Fact]
    public void SlidingWindow_LifetimeCountersRemainAccurateAfterEviction()
    {
        var stats = new TargetStatistics();

        // Add 500 failed results
        for (int i = 0; i < 500; i++)
        {
            stats.AddResult(new PingResult(false, 0));
        }

        // Add 1000 successful results (this evicts the failed ones from window)
        for (int i = 0; i < 1000; i++)
        {
            stats.AddResult(new PingResult(true, 10));
        }

        // Lifetime totals should reflect all 1500 pings
        Assert.Equal(1500, stats.TotalSent);
        Assert.Equal(500, stats.LostCount);

        // Loss percentage should be based on lifetime (500/1500 = 33.33%)
        Assert.Equal(500.0 / 1500 * 100, stats.LossPercentage, precision: 2);
    }

    [Fact]
    public void LastRoundtrip_ReturnsMostRecentSuccessfulPing()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(true, 10));
        stats.AddResult(new PingResult(true, 25));
        stats.AddResult(new PingResult(false, 0));

        Assert.Equal(25, stats.LastRoundtrip);
    }

    [Fact]
    public void Statistics_ReturnNullWhenNoSuccessfulPings()
    {
        var stats = new TargetStatistics();

        stats.AddResult(new PingResult(false, 0));
        stats.AddResult(new PingResult(false, 0));

        Assert.Null(stats.GetMin());
        Assert.Null(stats.GetAvg());
        Assert.Null(stats.GetP95());
        Assert.Null(stats.LastRoundtrip);
    }
}
