using Ivy.Tendril.Services;

namespace Ivy.Tendril.Test;

public class TimeCacheTests
{
    [Fact]
    public void GetOrCompute_CallsComputeOnFirstAccess()
    {
        var cache = new TimeCache<int>(TimeSpan.FromMinutes(1));
        int callCount = 0;

        var result = cache.GetOrCompute(() => { callCount++; return 42; });

        Assert.Equal(42, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetOrCompute_ReturnsCachedValueWithinExpiration()
    {
        var cache = new TimeCache<int>(TimeSpan.FromSeconds(1));
        int callCount = 0;

        cache.GetOrCompute(() => { callCount++; return 42; });
        var result = cache.GetOrCompute(() => { callCount++; return 99; });

        Assert.Equal(42, result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void GetOrCompute_RecomputesAfterExpiration()
    {
        var cache = new TimeCache<int>(TimeSpan.FromMilliseconds(100));
        int callCount = 0;

        cache.GetOrCompute(() => { callCount++; return 42; });
        Thread.Sleep(150);
        var result = cache.GetOrCompute(() => { callCount++; return 99; });

        Assert.Equal(99, result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Invalidate_ForcesCacheRecompute()
    {
        var cache = new TimeCache<int>(TimeSpan.FromMinutes(1));
        int callCount = 0;

        cache.GetOrCompute(() => { callCount++; return 42; });
        cache.Invalidate();
        var result = cache.GetOrCompute(() => { callCount++; return 99; });

        Assert.Equal(99, result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void IsValid_ReturnsTrueForValidCache()
    {
        var cache = new TimeCache<int>(TimeSpan.FromMinutes(1));
        cache.GetOrCompute(() => 42);

        Assert.True(cache.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsFalseForExpiredCache()
    {
        var cache = new TimeCache<int>(TimeSpan.FromMilliseconds(50));
        cache.GetOrCompute(() => 42);
        Thread.Sleep(100);

        Assert.False(cache.IsValid);
    }
}
