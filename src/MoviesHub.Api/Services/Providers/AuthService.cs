using System.Net;
using Bogus;
using Flurl.Http;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Models.Response.HubtelSms;
using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace MoviesHub.Api.Services.Providers;

public class AuthService : IAuthService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IUserService _userService;
    private readonly ILogger _logger;
    private readonly HubtelSmsConfig _hubtelSmsConfig;
    private readonly Faker _faker;
    private readonly BearerTokenConfig _bearerTokenConfig;

    public AuthService(IConnectionMultiplexer redis,
        IOptions<HubtelSmsConfig> hubtelSmsConfig,
        IUserService userService,
        ILogger logger, 
        IOptions<BearerTokenConfig> bearerTokenConfig)
    {
        _redis = redis;
        _userService = userService;
        _logger = logger;
        _bearerTokenConfig = bearerTokenConfig.Value;
        _hubtelSmsConfig = hubtelSmsConfig.Value;
        _faker = new Faker();
    }
    
    public async Task<BaseResponse<OtpCodeResponse>> RequestOtpCode(string mobileNumber)
    {
        try
        {
            var userResponse = await _userService.GetUserAccount(mobileNumber);

            if (!200.Equals(userResponse.Code))
            {
                return new BaseResponse<OtpCodeResponse>
                {
                    Code = userResponse.Code,
                    Message = userResponse.Message
                };
            }

            var key = $"movieshub:otp:{mobileNumber}";
            var otpCode = _faker.Random.Number(100000, 999999);
            var prefix = _faker.Random.String2(4).ToUpper();
            
            var otpCodeResponse = new RedisOtpCodeResponse
            {
                Prefix = prefix,
                Code = otpCode
            };

            var savedInRedis = await _redis.GetDatabase()
                .StringSetAsync(key, JsonConvert.SerializeObject(otpCodeResponse), TimeSpan.FromMinutes(15));
            
            if (!savedInRedis)
            {
                return CommonResponses.ErrorResponse.FailedDependencyErrorResponse<OtpCodeResponse>();
            }

            var sendSmsResponse = await SendSms(mobileNumber, new SendSmsRequest
            {
                Code = otpCode,
                Prefix = prefix
            });

            return !201.Equals(sendSmsResponse.Code) 
                ? CommonResponses.ErrorResponse.InternalServerErrorResponse<OtpCodeResponse>()
                : CommonResponses.SuccessResponse.OkResponse(new OtpCodeResponse
                {
                    Prefix = otpCodeResponse.Prefix
                }, "OTP code sent successfully");
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occured sending otp to user:{mobileNumber}", mobileNumber);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<OtpCodeResponse>();
        }
    }

    public async Task<BaseResponse<LoginResponse>> VerifyOtpCode(string mobileNumber, VerifyOtpRequest request)
    {
        try
        {
            var userResponse = await _userService.GetUserAccount(mobileNumber);

            if (!200.Equals(userResponse.Code))
            {
                return new BaseResponse<LoginResponse>
                {
                    Code = userResponse.Code,
                    Message = userResponse.Message
                };
            }
            
            var otpKey = $"movieshub:otp:{mobileNumber}";
            var otpRedisResponse = await _redis.GetDatabase().StringGetAsync(otpKey);

            if (!otpRedisResponse.HasValue)
            {
                return CommonResponses.ErrorResponse.BadRequestResponse<LoginResponse>("OTP verification failed. Expiry limit reached");
            }
            
            var otpData = JsonConvert.DeserializeObject<RedisOtpCodeResponse>(otpRedisResponse);

            if (!otpData.Code.Equals(request.Code) || !otpData.Prefix.Equals(request.Prefix))
            {
                return new BaseResponse<LoginResponse>
                {
                    Code = (int) HttpStatusCode.BadRequest,
                    Message = "Incorrect authentication code"
                };
            }

            var tokenResponse = userResponse.Data.GenerateToken(_bearerTokenConfig);

            var loginResponse = new LoginResponse
            {
                User = userResponse.Data,
                Expiry = tokenResponse.Expiry,
                Token = tokenResponse.BearerToken
            };

            return CommonResponses.SuccessResponse.OkResponse(loginResponse, "Verification successful");
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occured verifying user:{mobileNumber} otp request\nRequest => {request}", 
                mobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<LoginResponse>();
        }
    }

    private async Task<BaseResponse<SmsResponse>> SendSms(string mobileNumber, SendSmsRequest request)
    {
        try
        {
            var content = $"Your authentication code for MoviesHub is {request.Prefix}-{request.Code}. " +
                          "Your OTP code expires in 15 minutes.";
            
            var url = $"https://smsc.hubtel.com/v1/messages/send" +
                      $"?clientid={_hubtelSmsConfig.ClientKey}" +
                      $"&clientsecret={_hubtelSmsConfig.ClientSecret}" +
                      $"&from={_hubtelSmsConfig.SenderId}" +
                      $"&to={mobileNumber}" +
                      $"&content={content}";
            
            var serverResponse = await url.AllowAnyHttpStatus().GetAsync();

            if (!serverResponse.ResponseMessage.IsSuccessStatusCode)
            {
                _logger.Error( "An error occured sending sms to user:{mobileNumber}\nResponse: {response}", 
                    mobileNumber, serverResponse.ResponseMessage.ReasonPhrase);
                
                return new BaseResponse<SmsResponse>
                {
                    Code = serverResponse.StatusCode,
                    Message = serverResponse.ResponseMessage.ReasonPhrase
                };
            }
            
            var rawResponse = await serverResponse.GetStringAsync();
            var response = JsonConvert.DeserializeObject<SmsResponse>(rawResponse);

            return CommonResponses.SuccessResponse.CreatedResponse<SmsResponse>(response);

        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occured sending sms to user:{mobileNumber}", mobileNumber);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<SmsResponse>();
        }
    }
}