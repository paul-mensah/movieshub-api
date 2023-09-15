using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Storage.Entities;
using Newtonsoft.Json;

namespace MoviesHub.Api.Repositories;

public class UserCacheRepository : IUserCacheRepository
{
    private readonly RedisConfig _redisConfig;
    private readonly IRedisService _redisService;

    public UserCacheRepository(IOptions<RedisConfig> redisConfig,
        IRedisService redisService)
    {
        _redisConfig = redisConfig.Value;
        _redisService = redisService;
    }

    public async Task<User> GetUserByMobileNumberAsync(string mobileNumber)
    {
        string userKey = CommonConstants.User.GetUserKey(mobileNumber);
        string userRedisValue = await _redisService.StringGetAsync(userKey);

        return string.IsNullOrEmpty(userRedisValue)
            ? null : JsonConvert.DeserializeObject<User>(userRedisValue);
    }

    public async Task<bool> CacheUserAccount(User user)
    {
        string userKey = CommonConstants.User.GetUserKey(user.MobileNumber);
        return await _redisService.StringSetAsync(userKey, user, TimeSpan.FromDays(_redisConfig.DataExpiryDays));
    }
}

public interface IUserCacheRepository
{
    Task<User> GetUserByMobileNumberAsync(string mobileNumber);
    Task<bool> CacheUserAccount(User user);
}