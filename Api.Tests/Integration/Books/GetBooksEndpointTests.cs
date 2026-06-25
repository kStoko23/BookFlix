using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Entities;

namespace Api.Tests.Integration.Books;

[Collection("Integration")]
public class GetBooksEndpointTests : IntegrationTestBase
{
    public GetBooksEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBooks_WithNoBooks_Returns200WithEmptyList()
    {
        var response = await Client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetBooks_WithBooks_ReturnsAllBooksWithCorrectCount()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Clean Code", isbn: "1112223334"),
            MakeBook(userId, title: "DDadD", isbn: "1231231230"), MakeBook(userId, title: "DDD", isbn: "3213213218"));

        var response = await Client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetBooks_WithSearchMatchingTitle_ReturnsFilteredResults()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(
            MakeBook(userId, title: "Clean Code", isbn: "1111111111", category: BookCategory.Educational),
            MakeBook(userId, title: "Clean Architecture", isbn: "2222222222", category: BookCategory.Educational),
            MakeBook(userId, title: "DDD", isbn: "3333333333", category: BookCategory.Other));

        var response = await Client.GetAsync("/api/books?search=clean");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, body.GetProperty("items").GetArrayLength());

        var titles = body.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .ToList();

        Assert.Contains("Clean Code", titles);
        Assert.Contains("Clean Architecture", titles);
        Assert.DoesNotContain("DDD", titles);
    }

    [Fact]
    public async Task GetBooks_WithSearchMatchingAuthor_ReturnsFilteredResults()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Book 1", author: "Leonardo da Vinci", isbn: "1111111111"),
            MakeBook(userId, title: "Book 2", author: "Galileo Galilei", isbn: "2222222222"),
            MakeBook(userId, title: "Book 3", author: "Robert C. Martin", isbn: "3333333333"));

        var response = await Client.GetAsync("/api/books?search=galileo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());

        var authors = body.GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("author").GetString())
            .ToList();

        Assert.Contains("Galileo Galilei", authors);
        Assert.DoesNotContain("Leonardo da Vinci", authors);
        Assert.DoesNotContain("Robert C. Martin", authors);
    }

    [Fact]
    public async Task GetBooks_WithSearchNoMatch_ReturnsEmptyList()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Clean Code", author: "Robert C. Martin", isbn: "1112223334"),
            MakeBook(userId, title: "DDD", isbn: "1231231230"), MakeBook(userId, title: "DDD", isbn: "3213213218"));

        var response = await Client.GetAsync("/api/books?search=michael");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task GetBooks_WithCategoryFilter_ReturnsOnlyBooksFromThatCategory()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(
            MakeBook(userId, title: "Clean Code", author: "Robert C. Martin", isbn: "1112223334",
                category: BookCategory.Educational), MakeBook(userId, title: "DDD", isbn: "1231231230"),
            MakeBook(userId, title: "DDD", isbn: "3213213218", category: BookCategory.Fantasy));

        var response = await Client.GetAsync("/api/books?category=Educational");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(10, body.GetProperty("items")[0].GetProperty("category").GetInt32());
    }

    [Fact]
    public async Task GetBooks_WithInvalidCategoryFilter_Returns400()
    {
        var response = await Client.GetAsync("/api/books?category=Sport");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithPageAndPageSize_ReturnsCorrectSlice()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Clean Code", isbn: "1112223334"),
            MakeBook(userId, title: "DDD", isbn: "1231231230"), MakeBook(userId, title: "DDD", isbn: "3213213218"));

        var response = await Client.GetAsync("/api/books?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.GetProperty("items").GetArrayLength());
        Assert.Equal(2, body.GetProperty("page").GetInt32());
    }

    [Fact]
    public async Task GetBooks_WithPageSizeExceeding100_ClampsTo100()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Clean Code", isbn: "1112223334"),
            MakeBook(userId, title: "DDD", isbn: "1231231230"), MakeBook(userId, title: "DDD", isbn: "3213213218"));

        var response = await Client.GetAsync("/api/books?pageSize=200");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, body.GetProperty("items").GetArrayLength());
        Assert.Equal(100, body.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task GetBooks_WithPageBelow1_DefaultsToPage1()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, title: "Clean Code", isbn: "1112223334"),
            MakeBook(userId, title: "DDD", isbn: "1231231230"), MakeBook(userId, title: "DDD", isbn: "3213213218"));

        var response = await Client.GetAsync("/api/books?page=0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, body.GetProperty("items").GetArrayLength());
        Assert.Equal(1, body.GetProperty("page").GetInt32());
    }
}