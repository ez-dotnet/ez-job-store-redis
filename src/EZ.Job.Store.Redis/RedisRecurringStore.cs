using System.Text.Json;
using EZ.Job.Core;
using StackExchange.Redis;

namespace EZJob.Store.Redis;

public sealed class RedisRecurringStore : IRecurringStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ConnectionMultiplexer _redis;
    private const string IndexKey = "ez_recurring:index";

    public RedisRecurringStore(string configuration)
    {
        _redis = ConnectionMultiplexer.Connect(configuration);
    }

    private IDatabase Db => _redis.GetDatabase();

    private static string Key(Guid id) => $"ez_recurring:{id}";

    public async ValueTask AddOrUpdateAsync(RecurringDefinition definition, CancellationToken cancellationToken = default)
    {
        var db = Db;
        var key = Key(definition.Id);
        var json = JsonSerializer.Serialize(definition, JsonOptions);
        var tran = db.CreateTransaction();

        _ = tran.StringSetAsync(key, json);
        _ = tran.SetAddAsync(IndexKey, definition.Id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
    }

    public async ValueTask RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var db = Db;
        var key = Key(id);
        var tran = db.CreateTransaction();

        _ = tran.KeyDeleteAsync(key);
        _ = tran.SetRemoveAsync(IndexKey, id.ToString());

        await tran.ExecuteAsync().ConfigureAwait(false);
    }

    public async ValueTask<RecurringDefinition?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var json = (string?)await Db.StringGetAsync(Key(id)).ConfigureAwait(false);
        return json is not null ? JsonSerializer.Deserialize<RecurringDefinition>(json, JsonOptions) : null;
    }

    public async ValueTask<IEnumerable<RecurringDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var db = Db;
        var ids = await db.SetMembersAsync(IndexKey).ConfigureAwait(false);
        if (ids.Length == 0) return [];

        var keys = ids.Select(id => (RedisKey)$"ez_recurring:{id}").ToArray();
        var values = await db.StringGetAsync(keys).ConfigureAwait(false);

        var definitions = new List<RecurringDefinition>(values.Length);
        foreach (var val in values)
        {
            if (val.HasValue)
            {
                var def = JsonSerializer.Deserialize<RecurringDefinition>((string)val!, JsonOptions);
                if (def is not null) definitions.Add(def);
            }
        }
        return definitions;
    }

    public async ValueTask SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var json = (string?)await Db.StringGetAsync(Key(id)).ConfigureAwait(false);
        if (json is null) return;

        var def = JsonSerializer.Deserialize<RecurringDefinition>(json, JsonOptions);
        if (def is null) return;

        var updated = def with { IsActive = isActive };
        var updatedJson = JsonSerializer.Serialize(updated, JsonOptions);

        await Db.StringSetAsync(Key(id), updatedJson).ConfigureAwait(false);
    }
}
