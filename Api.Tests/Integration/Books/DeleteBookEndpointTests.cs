using System.Net;

namespace Api.Tests.Integration.Books;

public class DeleteBookEndpointTests : IntegrationTestBase
{
    public DeleteBookEndpointTests(BooksApiFactory factory) : base(factory) { }

    [Fact]
    public async Task DeleteBook_Unauthenticated_Returns401()
    {
        var response = await Client.DeleteAsync("/api/books/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ValidRequest_Returns204()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.DeleteAsync($"/api/books/{bookId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_ActuallyRemovesBookFromDatabase()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        await Client.DeleteAsync($"/api/books/{bookId}");

        var response = await Client.GetAsync($"/api/books/{bookId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_BookNotFound_Returns404()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.DeleteAsync("/api/books/999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_BookBelongsToAnotherUser_Returns403()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");
        var bookId = await SeedBookAndGetIdAsync(MakeBook(otherUserId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.DeleteAsync($"/api/books/{bookId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}