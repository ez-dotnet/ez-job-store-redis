using EZ.Job.Core;
using EZJob.Store.Redis;

namespace EZ.Job.Store.Redis.Tests;

public sealed class RedisJobStoreTests
{
    private const string Configuration = "localhost:6379";

    [Fact(Skip = "Requires Redis container")]
    public async Task AddAsync_should_store_job()
    {
        var store = new RedisJobStore(Configuration);
        var job = new Job("test-id", "T", "M", [], [], JobStatus.Enqueued, DateTime.UtcNow, null);

        await store.AddAsync(job);
        var result = await store.GetAsync("test-id");

        Assert.NotNull(result);
        Assert.Equal("test-id", result!.Id);
    }
}
