using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Storage.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MoviesHub.Api.Repositories;

public class UserCacheRepository : IUserCacheRepository
{
    private readonly ILogger<UserCacheRepository> _logger;
    private readonly RedisConfig _redisConfig;
    private readonly IConnectionMultiplexer _redis;

    public UserCacheRepository(ILogger<UserCacheRepository> logger,
        IOptions<RedisConfig> redisConfig,
        IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisConfig = redisConfig.Value;
        _redis = redis;
    }

    public async Task<User> GetUserByMobileNumberAsync(string mobileNumber)
    {
        try
        {
            string userKey = CommonConstants.User.GetUserKey(mobileNumber);
            var userRedisValue = await _redis.GetDatabase().StringGetAsync(userKey);

            return !userRedisValue.HasValue
                ? null
                : JsonConvert.DeserializeObject<User>(userRedisValue.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured getting cached user data", mobileNumber);
            return null;
        }
    }

    public async Task<bool> CacheUserAccount(User user)
    {
        try
        {
            string userKey = CommonConstants.User.GetUserKey(user.MobileNumber);
            string serializedUserAccount = JsonConvert.SerializeObject(user);
            
            return await _redis.GetDatabase()
                .StringSetAsync(userKey, serializedUserAccount, TimeSpan.FromDays(_redisConfig.DataExpiryDays));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured caching user account\nUser:{userAccount}",
                user.MobileNumber, JsonConvert.SerializeObject(user));
            return false;
        }
    }
}

public interface IUserCacheRepository
{
    Task<User> GetUserByMobileNumberAsync(string mobileNumber);
    Task<bool> CacheUserAccount(User user);
}