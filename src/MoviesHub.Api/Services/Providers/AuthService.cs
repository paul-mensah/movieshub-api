using AutoMapper;
using Bogus;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Models.Response.HubtelSms;
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;
using Newtonsoft.Json;

namespace MoviesHub.Api.Services.Providers;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IMapper _mapper;
    private readonly RedisConfig _redisConfig;
    private readonly HubtelSmsConfig _hubtelSmsConfig;
    private readonly Faker _faker;
    private readonly BearerTokenConfig _bearerTokenConfig;

    public AuthService(IOptions<HubtelSmsConfig> hubtelSmsConfig,
        IUserService userService,
        ILogger<AuthService> logger,
        IOptions<BearerTokenConfig> bearerTokenConfig,
        IOptions<RedisConfig> redisConfig,
        IOtpCodeRepository otpCodeRepository,
        IMapper mapper)
    {
        _userService = userService;
        _logger = logger;
        _otpCodeRepository = otpCodeRepository;
        _mapper = mapper;
        _redisConfig = redisConfig.Value;
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

            var otpCode = new OtpCode(
                prefix: _faker.Random.String2(4).ToUpper(),
                code: _faker.Random.Number(100000, 999999));

            bool otpCodeIsCached = await _otpCodeRepository.CacheOtpCodeAsync(mobileNumber, otpCode);
            
            if (!otpCodeIsCached)
                return CommonResponses.ErrorResponse
                    .FailedDependencyErrorResponse<OtpCodeResponse>();

            bool smsSent = await SendSms(mobileNumber, new SendSmsRequest
            {
                Code = otpCode.Code,
                Prefix = otpCode.Prefix
            });

            if (!smsSent) return CommonResponses.ErrorResponse.InternalServerErrorResponse<OtpCodeResponse>();

            var otpCodeResponse = _mapper.Map<OtpCodeResponse>(otpCode);
            return CommonResponses.SuccessResponse.OkResponse(otpCodeResponse, "OTP code sent successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured sending otp to user", mobileNumber);
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<OtpCodeResponse>();
        }
    }

    public async Task<BaseResponse<LoginResponse>> VerifyOtpCode(string mobileNumber, VerifyOtpRequest request)
    {
        try
        {
            var otpCode = await _otpCodeRepository.GetOtpCode(mobileNumber, request.RequestId);

            if (otpCode is null)
                return CommonResponses.ErrorResponse
                    .FailedDependencyErrorResponse<LoginResponse>();

            if (!otpCode.Code.Equals(request.Code) || !otpCode.Prefix.Equals(request.Prefix))
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<LoginResponse>("Incorrect authentication code");

            var userResponse = await _userService.GetUserAccount(mobileNumber);

            if (!200.Equals(userResponse.Code))
            {
                return new BaseResponse<LoginResponse>
                {
                    Code = userResponse.Code,
                    Message = userResponse.Message
                };
            }
            var tokenResponse = userResponse.Data.GenerateToken(_bearerTokenConfig);
            
            return CommonResponses.SuccessResponse
                .OkResponse(new LoginResponse(tokenResponse, userResponse.Data),
                    "Verification successful");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{mobileNumber}: An error occured verifying user otp request\nRequest => {request}",
                mobileNumber, JsonConvert.SerializeObject(request, Formatting.Indented));

            return CommonResponses.ErrorResponse.InternalServerErrorResponse<LoginResponse>();
        }
    }

    private async Task<bool> SendSms(string mobileNumber, SendSmsRequest request)
    {
        try
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
            
            string? rawResponse = await serverResponse.GetStringAsync();
            
            _logger.LogError("{mobileNumber}: An error occured sending sms to user\nResponse => {response}",
                mobileNumber, rawResponse);

            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured sending sms to user:{mobileNumber}", mobileNumber);
            return false;
        }
    }
}