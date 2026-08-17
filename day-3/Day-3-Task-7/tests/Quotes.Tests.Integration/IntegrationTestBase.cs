using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using QuotesApi.Extensions;
using QuotesApi.Models.Dtos;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace Quotes.Tests.Integration;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();
        Client = Factory.CreateClient();
    }

    protected QuoteDbContext GetDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    protected async Task AuthenticateAsync(string username = "testuser", string password = "password")
    {
        var request = new LoginRequest { Username = username, Password = password };
        var response = await Client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!.Token);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
