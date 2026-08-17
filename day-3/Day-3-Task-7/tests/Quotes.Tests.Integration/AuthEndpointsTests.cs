using FluentAssertions;
using QuotesApi.Extensions;
using QuotesApi.Models.Dtos;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Quotes.Tests.Integration;

public class AuthEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "password" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
