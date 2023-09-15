using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MoviesHub.Api.Services.Providers;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    public async Task<string> StringGetAsync(string key)
    {
        RedisValue redisValue = await _redis.GetDatabase().StringGetAsync(key);
        return !redisValue.HasValue ? null : redisValue.ToString();
    }

    public async Task<bool> StringSetAsync<T>(string key, T value,  TimeSpan expiry)
    {
        string serializedValue = JsonConvert.SerializeObject(value);
        return await _redis.GetDatabase()
            .StringSetAsync(key, serializedValue, expiry);
    }
}