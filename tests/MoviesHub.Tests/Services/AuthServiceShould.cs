using System.Net;
using AutoMapper;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Models.Response.Auth;
using MoviesHub.Api.Models.Response.HubtelSms;
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Services.Providers;
using MoviesHub.Tests.TestData;
using MoviesHub.Tests.TestSetup;
using Xunit;

namespace MoviesHub.Tests.Services;

public class AuthServiceShould : IClassFixture<TestFixture>
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IOtpCodeRepository> _otpCodeRepositoryMock;
    private readonly TestFixture _fixture;
    private static readonly Faker Faker = new Faker();

    public AuthServiceShould(TestFixture fixture)
    {
        _fixture = fixture;
        _userServiceMock = new Mock<IUserService>();
        _otpCodeRepositoryMock = new Mock<IOtpCodeRepository>();
        _smsServiceMock = new Mock<ISmsService>();
    }

    private AuthService GetAuthService()
    {
        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<AuthService>>();
        var mapper = _fixture.ServiceProvider.GetRequiredService<IMapper>();
        var bearerTokenConfig = _fixture.ServiceProvider.GetRequiredService<IOptions<BearerTokenConfig>>();

        return new AuthService(
            userService: _userServiceMock.Object,
            logger: logger,
            bearerTokenConfig: bearerTokenConfig,
            otpCodeRepository: _otpCodeRepositoryMock.Object,
            mapper: mapper,
            smsService: _smsServiceMock.Object);
    }

    [Fact]
    public async Task RequestOtpCode_Should_Return_200_Response_Code_And_Otp_Prefix_And_RequestId_When_User_Exists()
    {
        // Arrange
        var user = UserTestData.GenerateUsers(1)[0];
        user.MobileNumber = "0548015476";

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = (int) HttpStatusCode.OK,
                Data = new UserResponse
                {
                    Id = user.Id,
                    CreatedAt = user.CreatedAt,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    UpdatedAt = user.UpdatedAt
                }
            });

        _otpCodeRepositoryMock.Setup(x => x.CacheOtpCodeAsync(It.IsAny<string>(), It.IsAny<OtpCode>()))
            .ReturnsAsync(true);

        _smsServiceMock.Setup(x => x.SendSms(It.IsAny<string>(), It.IsAny<SendSmsRequest>()))
            .ReturnsAsync(true);
        
        var authService = GetAuthService();
        
        // Act
        var response = await authService.RequestOtpCode(user.MobileNumber);

        // Assert
        response.Code.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data.Prefix.Should().NotBeNullOrEmpty();
        response.Data.Prefix.Length.Should().Be(4);
        response.Data.RequestId.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task RequestOtpCode_Should_Return_404_Response_Code_When_User_Does_Exist()
    {
        // Arrange
        var user = UserTestData.GenerateUsers(1)[0];
        user.MobileNumber = "0548015476";

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = (int) HttpStatusCode.NotFound,
                Message = "User not found"
            });
        
        var authService = GetAuthService();
        
        // Act
        var response = await authService.RequestOtpCode(user.MobileNumber);

        // Assert
        response.Code.Should().Be(404);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User not found");
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task RequestOtpCode_Should_Return_424_Response_Code_When_Persisting_OtpCode_Fails()
    {
        // Arrange
        var user = UserTestData.GenerateUsers(1)[0];
        user.MobileNumber = "0548015476";

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = (int) HttpStatusCode.OK,
                Data = new UserResponse
                {
                    Id = user.Id,
                    CreatedAt = user.CreatedAt,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    UpdatedAt = user.UpdatedAt
                }
            });

        _otpCodeRepositoryMock.Setup(x => x.CacheOtpCodeAsync(It.IsAny<string>(), It.IsAny<OtpCode>()))
            .ReturnsAsync(false);
        
        var authService = GetAuthService();
        
        // Act
        var response = await authService.RequestOtpCode(user.MobileNumber);

        // Assert
        response.Code.Should().Be(424);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("An error occured, try again later");
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task RequestOtpCode_Should_Return_500_Response_Code_When_Sending_OtpCode_Sms_Fails()
    {
        // Arrange
        var user = UserTestData.GenerateUsers(1)[0];
        user.MobileNumber = "0548015476";

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = (int) HttpStatusCode.OK,
                Data = new UserResponse
                {
                    Id = user.Id,
                    CreatedAt = user.CreatedAt,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobileNumber = user.MobileNumber,
                    UpdatedAt = user.UpdatedAt
                }
            });

        _otpCodeRepositoryMock.Setup(x => x.CacheOtpCodeAsync(It.IsAny<string>(), It.IsAny<OtpCode>()))
            .ReturnsAsync(true);
        
        _smsServiceMock.Setup(x => x.SendSms(It.IsAny<string>(), It.IsAny<SendSmsRequest>()))
            .ReturnsAsync(false);
        
        var authService = GetAuthService();
        
        // Act
        var response = await authService.RequestOtpCode(user.MobileNumber);

        // Assert
        response.Code.Should().Be(500);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("Something bad happened, try again later");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task VerifyOtpCode_Should_Return_424_Response_Code_When_OtpCode_Has_Expired_Or_Does_Not_Exist()
    {
        // Arrange
        string userMobileNumber = Faker.Person.Phone;
        var verifyOtpRequest = new VerifyOtpRequest
        {
            Code = 123456,
            Prefix = "BODY",
            RequestId = Guid.NewGuid().ToString("N")
        };

        _otpCodeRepositoryMock.Setup(x => x.GetOtpCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null);

        var authService = GetAuthService();
        
        // Act
        var response = await authService.VerifyOtpCode(userMobileNumber, verifyOtpRequest);

        // Assert
        response.Code.Should().Be(424);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("An error occured, try again later");
        response.Data.Should().BeNull();
    }
    
    [Theory]
    [InlineData("BASE", 123456)]
    [InlineData("BODY", 7891572)]
    [InlineData("HOSE", 154821)]
    public async Task VerifyOtpCode_Should_Return_400_Response_Code_For_Incorrect_Otp_Verification_Details(string prefix, int code)
    {
        // Arrange
        string userMobileNumber = Faker.Person.Phone;
        var verifyOtpRequest = new VerifyOtpRequest
        {
            Code = 123456,
            Prefix = "BODY",
            RequestId = Guid.NewGuid().ToString("N")
        };

        _otpCodeRepositoryMock.Setup(x => x.GetOtpCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OtpCode(prefix, code));

        var authService = GetAuthService();
        
        // Act
        var response = await authService.VerifyOtpCode(userMobileNumber, verifyOtpRequest);

        // Assert
        response.Code.Should().Be(400);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("Incorrect authentication code");
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task VerifyOtpCode_Should_Return_404_Response_Code_When_User_Does_Not_Exist()
    {
        // Arrange
        string userMobileNumber = Faker.Person.Phone;
        var verifyOtpRequest = new VerifyOtpRequest
        {
            Code = 123456,
            Prefix = "BODY",
            RequestId = Guid.NewGuid().ToString("N")
        };

        _otpCodeRepositoryMock.Setup(x => x.GetOtpCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OtpCode(verifyOtpRequest.Prefix, verifyOtpRequest.Code));

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = 404,
                Message = "User not found"
            });

        var authService = GetAuthService();
        
        // Act
        var response = await authService.VerifyOtpCode(userMobileNumber, verifyOtpRequest);

        // Assert
        response.Code.Should().Be(404);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("User not found");
        response.Data.Should().BeNull();
    }
    
    [Fact]
    public async Task VerifyOtpCode_Should_Return_200_Response_Code_And_Bearer_Token_When_User_Exists_And_OtpVerificationRequests_Are_Correct()
    {
        // Arrange
        var user = UserTestData.GenerateUsers(1)[0];
        
        var userResponse = new UserResponse
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MobileNumber = user.MobileNumber,
            UpdatedAt = user.UpdatedAt
        };
        
        var verifyOtpRequest = new VerifyOtpRequest
        {
            Code = 123456,
            Prefix = "BODY",
            RequestId = Guid.NewGuid().ToString("N")
        };

        _otpCodeRepositoryMock.Setup(x => x.GetOtpCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OtpCode(verifyOtpRequest.Prefix, verifyOtpRequest.Code));

        _userServiceMock.Setup(x => x.GetUserAccount(It.IsAny<string>()))
            .ReturnsAsync(new BaseResponse<UserResponse>
            {
                Code = 200,
                Data = userResponse
            });
        
        var authService = GetAuthService();
        
        // Act
        var response = await authService.VerifyOtpCode(user.MobileNumber, verifyOtpRequest);

        // Assert
        response.Code.Should().Be(200);
        response.Message.Should().NotBeNullOrEmpty();
        response.Message.Should().Be("Verification successful");
        response.Data.Should().NotBeNull();
        response.Data.User.Should().NotBeNull();
        response.Data.User.Should().Be(userResponse);
        response.Data.Token.Should().NotBeNullOrEmpty();
    }
}