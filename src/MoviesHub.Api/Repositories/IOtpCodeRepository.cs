using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;

namespace MoviesHub.Api.Repositories;

public interface IOtpCodeRepository
{
    Task<bool> CacheOtpCodeAsync(string mobileNumber, OtpCode otpCode);
    Task<OtpCode> GetOtpCode(string mobileNumber, string requestId);
}

public class OtpCodeRepository : IOtpCodeRepository
{
    private readonly IRedisService _redisService;
    private readonly RedisConfig _redisConfig;

    public OtpCodeRepository(IRedisService redisService,
        IOptions<RedisConfig> redisConfig)
    {
        _redisService = redisService;
        _redisConfig = redisConfig.Value;
    }
    
    public async Task<bool> CacheOtpCodeAsync(string mobileNumber, OtpCode otpCode)
    {
        string key = CommonConstants.Authentication.GetUserOtpKey(mobileNumber, otpCode.RequestId);
        return await _redisService.StringSetAsync(key, otpCode,TimeSpan.FromMinutes(_redisConfig.OtpCodeExpiryInMinutes));
    }

    public async Task<OtpCode> GetOtpCode(string mobileNumber, string requestId)
    {
        string otpKey = CommonConstants.Authentication.GetUserOtpKey(mobileNumber, requestId);
        string otpRedisResponse = await _redisService.StringGetAsync(otpKey);

        return string.IsNullOrEmpty(otpRedisResponse) 
            ? null : JsonConvert.DeserializeObject<OtpCode>(otpRedisResponse);
    }
}