namespace EZJob.Store.Redis;

public class RedisStoreOptions
{
    public string Configuration { get; set; } = "localhost:6379";
    public string KeyPrefix { get; set; } = "ez_job:";
}
