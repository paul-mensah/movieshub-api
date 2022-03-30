using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace MoviesHub.Api.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Request for OTP code
    /// </summary>
    /// <param name="mobileNumber"></param>
    /// <returns></returns>
    [HttpGet("otp/request/{mobileNumber}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<OtpCodeResponse>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Request for otp code", OperationId = nameof(RequestOtpCode))]
    public async Task<IActionResult> RequestOtpCode([FromRoute] string mobileNumber)
    {
        var response = await _authService.RequestOtpCode(mobileNumber);
        return StatusCode(response.Code, response);
    }
    
    /// <summary>
    /// Verify OTP code
    /// </summary>
    /// <param name="mobileNumber"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("otp/verify/{mobileNumber}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<LoginResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
    [SwaggerOperation("Verify otp code", OperationId = nameof(VerifyOtpCode))]
    public async Task<IActionResult> VerifyOtpCode([FromRoute] string mobileNumber, [FromBody] VerifyOtpRequest request)
    {
        var response = await _authService.VerifyOtpCode(mobileNumber, request);
        return StatusCode(response.Code, response);
    }
}