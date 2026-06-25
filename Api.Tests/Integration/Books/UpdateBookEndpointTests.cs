using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Api.Tests.Integration.Books;

public class UpdateBookEndpointTests : IntegrationTestBase
{
    public UpdateBookEndpointTests(BooksApiFactory factory) : base(factory) { }

    [Fact]
    public async Task PutBook_Unauthenticated_Returns401()
    {
        var response = await Client.PutAsJsonAsync("/api/books/1", new
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "1111111111",
            Pages = 100,
            Rating = 5
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_ValidRequest_Returns204()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Isbn = "1111111111",
            Pages = 200,
            Rating = 4
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_ActuallyUpdatesDataInDatabase()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, title: "Old Title", isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "New Title",
            Author = "New Author",
            Isbn = "1111111111",
            Pages = 200,
            Rating = 4
        });

        var response = await Client.GetAsync($"/api/books/{bookId}");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("New Title", body.GetProperty("title").GetString());
        Assert.Equal("New Author", body.GetProperty("author").GetString());
    }

    [Fact]
    public async Task PutBook_BookNotFound_Returns404()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PutAsJsonAsync("/api/books/999999999", new
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "1111111111",
            Pages = 100,
            Rating = 5
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_BookBelongsToAnotherUser_Returns403()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");
        var bookId = await SeedBookAndGetIdAsync(MakeBook(otherUserId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "Hacked Title",
            Author = "Hacked Author",
            Isbn = "1111111111",
            Pages = 100,
            Rating = 5
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_DuplicateIsbnOfDifferentBook_Returns409()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, isbn: "2222222222"));
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "2222222222",
            Pages = 100,
            Rating = 5
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_IsbnWithDashes_NormalizesBeforeSaving()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Isbn = "111-111-1111",
            Pages = 100,
            Rating = 5
        });

        var response = await Client.GetAsync($"/api/books/{bookId}");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("1111111111", body.GetProperty("isbn").GetString());
    }

    [Fact]
    public async Task PutBook_InvalidRequest_Returns422WithValidationErrors()
    {
        var userId = await SeedUserAsync();
        var bookId = await SeedBookAndGetIdAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PutAsJsonAsync($"/api/books/{bookId}", new
        {
            Title = "",
            Author = "",
            Isbn = "not-a-valid-isbn",
            Pages = -1,
            Rating = 99
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = body.GetProperty("errors");
        Assert.True(errors.GetProperty("title").GetArrayLength() > 0);
        Assert.True(errors.GetProperty("author").GetArrayLength() > 0);
        Assert.True(errors.GetProperty("pages").GetArrayLength() > 0);
        Assert.True(errors.GetProperty("rating").GetArrayLength() > 0);
    }
}