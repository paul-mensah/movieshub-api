using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Tests.TestSetup;
using Xunit;

namespace MoviesHub.Tests.Helpers;

public class TokenGeneratorTests : IClassFixture<TestFixture>
{
    private static readonly Faker Faker = new Faker();
    private readonly IOptions<BearerTokenConfig> _tokenConfig;
    
    public TokenGeneratorTests(TestFixture fixture)
    {
        _tokenConfig = fixture.ServiceProvider.GetRequiredService<IOptions<BearerTokenConfig>>();
    }

    [Fact]
    public void GenerateToken_Should_Return_Valid_Token_When_User_Account_Is_Provided()
    {
        // Arrange
        var userResponse = new UserResponse
        {
            FirstName = Faker.Person.FirstName,
            Id = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            LastName = Faker.Person.LastName,
            MobileNumber = Faker.Person.Phone
        };

        // Act
        var tokenResponse = userResponse.GenerateToken(_tokenConfig.Value);

        // Assert
        tokenResponse.Should().NotBeNull();
        tokenResponse.BearerToken.Should().NotBeNullOrEmpty();
        tokenResponse.Expiry.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void GenerateToken_Should_Throw_Null_Exception_When_BearerTokenConfig_Is_Null()
    {
        // Arrange
        var userResponse = new UserResponse
        {
            FirstName = Faker.Person.FirstName,
            Id = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            LastName = Faker.Person.LastName,
            MobileNumber = Faker.Person.Phone
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => userResponse.GenerateToken(null));

        exception.ParamName.Should().NotBeNullOrEmpty();
        exception.ParamName.Should().Be("config");
        exception.Message.Should().NotBeNullOrEmpty();
        exception.Message.Should().Contain("Bearer token configuration must not be null or empty");
    }
    
    [Fact]
    public void GenerateToken_Should_Throw_Null_Exception_When_User_Data_Is_Null()
    {
        // Arrange
        UserResponse userResponse = null;

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() => userResponse.GenerateToken(_tokenConfig.Value));

        exception.ParamName.Should().NotBeNullOrEmpty();
        exception.ParamName.Should().Be("user");
        exception.Message.Should().NotBeNullOrEmpty();
        exception.Message.Should().Contain("User data must not be null or empty");
    }
}