using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;
using QuotesApi.Models.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Quotes.Tests.Integration;

public class QuoteEndpointsTests : IntegrationTestBase
{
    public QuoteEndpointsTests(DatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetQuotes_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/quotes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quotes = await response.Content.ReadFromJsonAsync<List<Quote>>();
        quotes.Should().NotBeNull();
    }

    [Fact]
    public async Task PostQuote_ValidRequest_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateQuoteRequest { Author = "New Author", Text = "New Quote" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/quotes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Quote>();
        created.Should().NotBeNull();
        created!.Author.Should().Be("New Author");
    }

    [Fact]
    public async Task PostQuote_InvalidRequest_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateQuoteRequest { Author = "", Text = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/quotes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Author");
    }

    [Fact]
    public async Task GetQuoteById_Existing_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateQuoteRequest { Author = "Author 1", Text = "Text 1" };
        var postResponse = await Client.PostAsJsonAsync("/api/quotes", request);
        var created = await postResponse.Content.ReadFromJsonAsync<Quote>();

        // Act
        var response = await Client.GetAsync($"/api/quotes/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<Quote>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetQuoteById_NonExisting_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/quotes/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuote_Owner_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateQuoteRequest { Author = "Author 1", Text = "Text 1" };
        var postResponse = await Client.PostAsJsonAsync("/api/quotes", request);
        var created = await postResponse.Content.ReadFromJsonAsync<Quote>();

        // Act
        var response = await Client.DeleteAsync($"/api/quotes/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteQuote_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange - Do NOT authenticate
        
        // Act
        var response = await Client.DeleteAsync("/api/quotes/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
