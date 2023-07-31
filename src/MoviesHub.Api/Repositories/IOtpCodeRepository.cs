using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response.Auth;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MoviesHub.Api.Repositories;

public interface IOtpCodeRepository
{
    Task<bool> CacheOtpCodeAsync(string mobileNumber, OtpCode otpCode);
    Task<OtpCode> GetOtpCode(string mobileNumber, string requestId);
}

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<OtpCodeRepository> _logger;
    private readonly RedisConfig _redisConfig;

    public OtpCodeRepository(IConnectionMultiplexer redis,
        IOptions<RedisConfig> redisConfig,
        ILogger<OtpCodeRepository> logger)
    {
        _redis = redis;
        _logger = logger;
        _redisConfig = redisConfig.Value;
    }
    
    public async Task<bool> CacheOtpCodeAsync(string mobileNumber, OtpCode otpCode)
    {
        try
        {
            string key = CommonConstants.Authentication.GetUserOtpKey(mobileNumber, otpCode.RequestId);

            return await _redis.GetDatabase()
                .StringSetAsync(
                    key,
                    JsonConvert.SerializeObject(otpCode),
                    TimeSpan.FromMinutes(_redisConfig.OtpCodeExpiryInMinutes));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured caching user otp\nOtpCode:{otpCode}",
                mobileNumber, JsonConvert.SerializeObject(otpCode));
            
            return false;
        }
    }

    public async Task<OtpCode> GetOtpCode(string mobileNumber, string requestId)
    {
        try
        {
            string otpKey = CommonConstants.Authentication.GetUserOtpKey(mobileNumber, requestId);
            var otpRedisResponse = await _redis.GetDatabase().StringGetAsync(otpKey);

            return !otpRedisResponse.HasValue
                ? null
                : JsonConvert.DeserializeObject<OtpCode>(otpRedisResponse.ToString());}
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured getting user otp with requestId:{requestId}",
                mobileNumber, requestId);
            
            return null;
        }
    }
}