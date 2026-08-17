using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models.Dtos;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Quotes.Tests.Integration;

public class CollectionEndpointsTests : IntegrationTestBase
{
    public CollectionEndpointsTests(DatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task PostCollection_ValidRequest_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateCollectionRequest { Name = "My Collection", OwnerId = "user1" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/collections", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        created.Should().NotBeNull();
        created!.Name.Should().Be("My Collection");
    }

    [Fact]
    public async Task PostCollection_InvalidRequest_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        await AuthenticateAsync();
        var request = new CreateCollectionRequest { Name = "", OwnerId = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/collections", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Name");
        problem.Errors.Should().ContainKey("OwnerId");
    }

    [Fact]
    public async Task AddItemToCollection_ValidRequest_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        
        // 1. Create Quote
        var quoteReq = new CreateQuoteRequest { Author = "Author", Text = "Quote" };
        var quoteRes = await Client.PostAsJsonAsync("/api/quotes", quoteReq);
        var quote = await quoteRes.Content.ReadFromJsonAsync<QuotesApi.Models.Quote>();
        
        // 2. Create Collection
        var collReq = new CreateCollectionRequest { Name = "Collection", OwnerId = "user1" };
        var collRes = await Client.PostAsJsonAsync("/api/collections", collReq);
        var coll = await collRes.Content.ReadFromJsonAsync<CollectionResponse>();

        var addItemReq = new AddItemRequest { QuoteId = quote!.Id };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/collections/{coll!.Id}/items", addItemReq);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CollectionResponse>();
        updated.Should().NotBeNull();
        updated!.Items.Should().ContainSingle(i => i.QuoteId == quote.Id);
    }
}
