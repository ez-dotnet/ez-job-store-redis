using Xunit;
using EZJob.Store.Redis;
using Xunit;

namespace EZ.Job.Store.Redis.Tests;

public sealed class RedisJobStoreTests
{
    private const string Configuration = "localhost:6379";

    [Fact(Skip = "Requires Redis container")]
    public async Task AddAsync_should_store_job()
    {
        var store = new RedisJobStore(Configuration);
        var job = new EZ.Job.Core.Job("test-id", "T", "M", [], [], EZ.Job.Core.JobStatus.Enqueued, System.DateTime.UtcNow, null, null, null, null);

        await store.AddAsync(job);
        var result = await store.GetAsync("test-id");

        Assert.NotNull(result);
        Assert.Equal("test-id", result!.Id);
    }
}
