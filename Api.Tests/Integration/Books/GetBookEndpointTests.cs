using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;

namespace Api.Tests.Integration.Books;

[Collection("Integration")]
public class GetBookEndpointTests : IntegrationTestBase
{
    public GetBookEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBook_BookExists_Returns200WithCorrectData()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, title: "Clean Code", isbn: "1112223334"));

        var response = await Client.GetAsync($"/api/books/{bookId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Clean Code", body.GetProperty("title").GetString());
        Assert.Equal(bookId, body.GetProperty("id").GetInt64());
    }

    [Fact]
    public async Task GetBook_BookNotFound_Returns404()
    {
        var response = await Client.GetAsync($"/api/books/999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_InvalidId_Returns404()
    {
        var response = await Client.GetAsync("/api/books/-1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_InvalidNonNumericalId_Returns404()
    {
        var response = await Client.GetAsync("/api/books/abc");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}