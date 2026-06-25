using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Entities;

namespace Api.Tests.Integration.Books;

public class GetMyBooksEnpointTests : IntegrationTestBase
{
    public GetMyBooksEnpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetMyBooks_Unauthenticated_Returns401()
    {
        var response = await Client.GetAsync("/api/books/mine");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyBooks_NoBooks_Returns200WithEmptyList()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");

        await SeedBooksAsync(MakeBook(otherUserId, title: "Other User Book", isbn: "1111111111"),
            MakeBook(otherUserId, title: "Other User Book", isbn: "2222222222"),
            MakeBook(otherUserId, title: "Other User Book", isbn: "3333333333"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.GetAsync("/api/books/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetMyBooks_WithBooks_ReturnsOnlyCurrentUsersBooks()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");

        await SeedBooksAsync(MakeBook(userId, title: "My Book 1", isbn: "1111111111"),
            MakeBook(userId, title: "My Book 2", isbn: "2222222222"),
            MakeBook(otherUserId, title: "Other User Book", isbn: "3333333333"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.GetAsync("/api/books/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, body.GetProperty("items").GetArrayLength());

        var titles = body.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .ToList();

        Assert.Contains("My Book 1", titles);
        Assert.Contains("My Book 2", titles);
        Assert.DoesNotContain("Other User Book", titles);
    }

    [Fact]
    public async Task GetMyBooks_WithSearchFilter_ReturnsOnlyMatchingOwnBooks()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");

        await SeedBooksAsync(MakeBook(userId, title: "My Book 1 animals", isbn: "1111111111"),
            MakeBook(userId, title: "My Book 2 cars", isbn: "2222222222"),
            MakeBook(otherUserId, title: "Other User Book animals", isbn: "3333333333"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.GetAsync("/api/books/mine?search=animal");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("items").GetArrayLength());

        var titles = body.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .ToList();

        Assert.DoesNotContain("Other User Book animals", titles);
    }

    [Fact]
    public async Task GetMyBooks_WithCategoryFilter_ReturnsOnlyMatchingCategoryOwnBooks()
    {
        var userId = await SeedUserAsync();
        var otherUserId = await SeedUserAsync(email: "other@test.com", username: "OtherUser");

        await SeedBooksAsync(
            MakeBook(userId, title: "My Book 1 animals", isbn: "1111111111", category: BookCategory.Educational),
            MakeBook(userId, title: "My Book 2 cars", isbn: "2222222222"),
            MakeBook(otherUserId, title: "Other User Book", isbn: "3333333333", category: BookCategory.Educational));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.GetAsync("/api/books/mine?category=Educational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("items").GetArrayLength());

        var titles = body.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .ToList();

        Assert.DoesNotContain("Other User Book", titles);
    }

    [Fact]
    public async Task GetMyBooks_WithPagination_ReturnsCorrectSlice()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "My Book 1", isbn: "1111111111"),
            MakeBook(userId, title: "My Book 2", isbn: "2222222222"),
            MakeBook(userId, title: "My Book 3", isbn: "3333333333"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.GetAsync("/api/books/mine?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("items").GetArrayLength());
        Assert.Equal(2, body.GetProperty("page").GetInt32());
    }
}