using System.Text.Json;
using EZ.Job.Core;
using StackExchange.Redis;

namespace EZJob.Store.Redis;

public sealed class RedisJobStore : IJobStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ConnectionMultiplexer _redis;
    private readonly string _prefix;

    public RedisJobStore(string configuration, string keyPrefix = "ez_job:")
    {
        _redis = ConnectionMultiplexer.Connect(configuration);
        _prefix = keyPrefix;
    }

    private IDatabase Db => _redis.GetDatabase();

    private string Key(string id) => $"{_prefix}{id}";
    private const string IndexKey = "ez_jobs:index";

    public async ValueTask AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        var db = Db;
        var key = Key(job.Id);
        var json = JsonSerializer.Serialize(job, JsonOptions);
        var tran = db.CreateTransaction();

        _ = tran.StringSetAsync(key, json);
        _ = tran.SortedSetAddAsync(IndexKey, job.Id, job.CreatedAt.Ticks);

        await tran.ExecuteAsync().ConfigureAwait(false);
    }

    public async ValueTask<Job?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var json = (string?)await Db.StringGetAsync(Key(id)).ConfigureAwait(false);
        return json is not null ? JsonSerializer.Deserialize<Job>(json, JsonOptions) : null;
    }

    public async ValueTask<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var db = Db;
        var ids = await db.SortedSetRangeByScoreAsync(IndexKey, order: Order.Ascending).ConfigureAwait(false);
        if (ids.Length == 0) return [];

        var keys = ids.Select(id => (RedisKey)Key(id!)).ToArray();
        var values = await db.StringGetAsync(keys).ConfigureAwait(false);

        var jobs = new List<Job>(values.Length);
        foreach (var val in values)
        {
            if (val.HasValue)
            {
                var job = JsonSerializer.Deserialize<Job>((string)val!, JsonOptions);
                if (job is not null) jobs.Add(job);
            }
        }
        return jobs;
    }

    public async ValueTask UpdateStatusAsync(string id, JobStatus status, string? error = null, CancellationToken cancellationToken = default)
    {
        var json = (string?)await Db.StringGetAsync(Key(id)).ConfigureAwait(false);
        if (json is null) return;

        var job = JsonSerializer.Deserialize<Job>(json, JsonOptions);
        if (job is null) return;

        var updated = job with { Status = status, Error = error };
        var updatedJson = JsonSerializer.Serialize(updated, JsonOptions);

        await Db.StringSetAsync(Key(id), updatedJson).ConfigureAwait(false);
    }

    public async ValueTask<IEnumerable<Job>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        return all.Where(j => j.Status == JobStatus.Enqueued).ToList();
    }
}
