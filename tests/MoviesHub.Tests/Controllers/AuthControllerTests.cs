using System.Net;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoviesHub.Api.Controllers.v1;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Tests.TestSetup;
using Xunit;

namespace MoviesHub.Tests.Controllers;

public class AuthControllerTests : IClassFixture<TestFixture>
{
    private readonly Mock<IAuthService> _authServiceMock;
    private static readonly Faker Faker = new Faker();

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
    }

    private AuthController GetAuthController()
    {
        return new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task RequestOtpCode_Should_Return_200_OK_StatusCode_For_An_Account_Which_Exists()
    {
        // Arrange
        _authServiceMock.Setup(x => x.RequestOtpCode(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<OtpCodeResponse>
            {
                Code = (int) HttpStatusCode.OK,
                Message = "OTP code sent successfully",
                Data = new OtpCodeResponse
                {
                    Prefix = Faker.Random.String2(4).ToUpper(),
                    RequestId = Guid.NewGuid().ToString("N")
                }
            });

        var authController = GetAuthController();

        // Act
        var response = (ObjectResult) await authController.RequestOtpCode("0548015476");
        var otpResponse = response.Value as BaseResponse<OtpCodeResponse>;

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        otpResponse?.Code.Should().Be((int)HttpStatusCode.OK);
        otpResponse?.Message.Should().NotBeNullOrEmpty();
        otpResponse?.Data.Should().NotBeNull();
        otpResponse?.Data.Prefix.Should().NotBeNullOrEmpty();
        otpResponse?.Data.RequestId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task RequestOtpCode_Should_Return_404_Not_Found_StatusCode_For_An_Account_Which_Does_Not_Exist()
    {
        // Arrange
        _authServiceMock.Setup(x => x.RequestOtpCode(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<OtpCodeResponse>
            {
                Code = (int) HttpStatusCode.NotFound,
                Message = "Account not found"
            });

        var authController = GetAuthController();

        // Act
        var response = (ObjectResult) await authController.RequestOtpCode("");
        var otpResponse = response.Value as BaseResponse<EmptyResponse>;

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        otpResponse?.Code.Should().Be((int)HttpStatusCode.NotFound);
        otpResponse?.Message.Should().NotBeNullOrEmpty();
        otpResponse?.Data.Should().BeNull();
    }

    [Theory]
    [InlineData("949c4b254a044d4194fe530be3319b8a", "", 0)]
    [InlineData("", "BODY", 0)]
    [InlineData("949c4b254a044d4194fe530be3319b8a", "APEX", null)]
    public async Task VerifyOtpCode_Should_Return_400_BadRequest_StatusCode_When_Required_Fields_Are_Not_Passed(string requestId, string prefix, int code)
    {
        // Arrange
        _authServiceMock.Setup(x => x.VerifyOtpCode(It.IsAny<string>(), It.IsAny<VerifyOtpRequest>()))
            .ReturnsAsync(new BaseResponse<LoginResponse>
            {
                Code = (int) HttpStatusCode.BadRequest
            });

        var authController = GetAuthController();
        
        // Act
        var response = (ObjectResult) await authController.VerifyOtpCode("054242115", new VerifyOtpRequest
        {
            Code = code,
            Prefix = prefix,
            RequestId = requestId
        });
        var verifyResponse = response.Value as BaseResponse<EmptyResponse>;

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        verifyResponse?.Code.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyOtpCode_Should_Return_200_Ok_StatusCode_When_All_Required_Fields_Are_Provided()
    {
        // Arrange
        _authServiceMock.Setup(x => x.VerifyOtpCode(It.IsAny<string>(), It.IsAny<VerifyOtpRequest>()))
            .ReturnsAsync(new BaseResponse<LoginResponse>
            {
                Code = (int) HttpStatusCode.OK
            });

        var authController = GetAuthController();
        
        // Act
        var response = (ObjectResult) await authController.VerifyOtpCode("054242115", new VerifyOtpRequest
        {
            Code = 124576,
            Prefix = Faker.Random.String2(4).ToUpper(),
            RequestId = Guid.NewGuid().ToString("N")
        });
        var verifyResponse = response.Value as BaseResponse<EmptyResponse>;

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        verifyResponse?.Code.Should().Be((int)HttpStatusCode.OK);
    }
}