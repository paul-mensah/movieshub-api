using Bogus;
using Mapster;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Models.Response.HubtelSms;
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;

namespace MoviesHub.Api.Services.Providers;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly ISmsService _smsService;
    private readonly Faker _faker;
    private readonly BearerTokenConfig _bearerTokenConfig;

    public AuthService(IUserService userService,
        IOptions<BearerTokenConfig> bearerTokenConfig,
        IOtpCodeRepository otpCodeRepository,
        ISmsService smsService)
    {
        _userService = userService;
        _otpCodeRepository = otpCodeRepository;
        _smsService = smsService;
        _bearerTokenConfig = bearerTokenConfig.Value;
        _faker = new Faker();
    }

    public async Task<BaseResponse<OtpCodeResponse>> RequestOtpCode(string mobileNumber)
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

        bool smsSent = await _smsService.SendSms(mobileNumber, new SendSmsRequest
        {
            Code = otpCode.Code,
            Prefix = otpCode.Prefix
        });

        if (!smsSent) return CommonResponses.ErrorResponse.InternalServerErrorResponse<OtpCodeResponse>();

        var otpCodeResponse = otpCode.Adapt<OtpCodeResponse>();
        return CommonResponses.SuccessResponse.OkResponse(otpCodeResponse, "OTP code sent successfully");
    }

    public async Task<BaseResponse<LoginResponse>> VerifyOtpCode(string mobileNumber, VerifyOtpRequest request)
    {
        var otpCode = await _otpCodeRepository.GetOtpCode(mobileNumber, request.RequestId);

        if (otpCode is null)
            return CommonResponses.ErrorResponse
                .FailedDependencyErrorResponse<LoginResponse>();

        if (!otpCode.Code.Equals(request.Code) || !otpCode.Prefix.Equals(request.Prefix, StringComparison.OrdinalIgnoreCase))
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
}