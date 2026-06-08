using EZ.Job.Core;
using EZJob.Store.Redis;

namespace Microsoft.Extensions.DependencyInjection;

public static class EZJobRedisExtensions
{
    public static EZJobBuilder AddRedisStore(this EZJobBuilder builder, string configuration)
    {
        return AddRedisStore(builder, o => o.Configuration = configuration);
    }

    public static EZJobBuilder AddRedisStore(this EZJobBuilder builder, Action<RedisStoreOptions> configure)
    {
        var options = new RedisStoreOptions();
        configure(options);

        builder.Services.AddSingleton<IJobStore>(_ =>
            new RedisJobStore(options.Configuration, options.KeyPrefix));
        builder.Services.AddSingleton<IRecurringStore>(_ =>
            new RedisRecurringStore(options.Configuration));

        return builder;
    }
}
