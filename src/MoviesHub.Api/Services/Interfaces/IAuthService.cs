using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;

namespace MoviesHub.Api.Services.Interfaces;

public interface IAuthService
{
    Task<BaseResponse<OtpCodeResponse>> RequestOtpCode(string mobileNumber);
    Task<BaseResponse<LoginResponse>> VerifyOtpCode(string mobileNumber, VerifyOtpRequest request);
}