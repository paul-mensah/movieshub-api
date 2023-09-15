using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response.HubtelSms;
using MoviesHub.Api.Services.Interfaces;

namespace MoviesHub.Api.Services.Providers;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly RedisConfig _redisConfig;
    private readonly HubtelSmsConfig _hubtelSmsConfig;

    public SmsService(ILogger<SmsService> logger,
        IOptions<HubtelSmsConfig> hubtelSmsConfig,
        IOptions<RedisConfig> redisConfig)
    {
        _logger = logger;
        _redisConfig = redisConfig.Value;
        _hubtelSmsConfig = hubtelSmsConfig.Value;
    }
    
    public async Task<bool> SendSms(string mobileNumber, SendSmsRequest request)
    {
        var url = new Url(_hubtelSmsConfig.BaseUrl);
        url.SetQueryParams(new
        {
            clientid = _hubtelSmsConfig.ClientKey,
            clientsecret = _hubtelSmsConfig.ClientSecret,
            from = _hubtelSmsConfig.SenderId,
            to = mobileNumber,
            content = CommonConstants.Authentication
                .GetOtpSmsContent(request, _redisConfig.OtpCodeExpiryInMinutes)
        });

        var serverResponse = await url.AllowAnyHttpStatus().GetAsync();

        if (serverResponse.ResponseMessage.IsSuccessStatusCode) return true;
        
        string rawResponse = await serverResponse.GetStringAsync();
        
        _logger.LogError("{mobileNumber}: An error occured sending sms to user\nResponse => {response}",
            mobileNumber, rawResponse);

        return false;
    }
}