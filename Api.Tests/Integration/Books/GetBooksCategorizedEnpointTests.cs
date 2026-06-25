using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Entities;

namespace Api.Tests.Integration.Books;

public class GetBooksCategorizedEnpointTests: IntegrationTestBase
{
    public GetBooksCategorizedEnpointTests(BooksApiFactory factory) : base(factory){ }

    [Fact]
    public async Task GetBooksCategorized_NoBooks_Returns200WithEmptyList()
    {
        var response = await Client.GetAsync("/api/books/categorized");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
     
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetBooksCategorized_WithBooks_ReturnsBooksAcrossAllCategories()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(
            MakeBook(userId, title: "book1", isbn: "1111111111", category: BookCategory.Other), 
            MakeBook(userId, title: "book2", isbn: "2222222222", category: BookCategory.NonFiction), 
            MakeBook(userId, title: "book3", isbn: "3333333333", category: BookCategory.Fiction),
            MakeBook(userId, title: "book4", isbn: "4444444444", category: BookCategory.Fantasy), 
            MakeBook(userId, title: "book5", isbn: "5555555555", category: BookCategory.ScienceFiction), 
            MakeBook(userId, title: "book6", isbn: "6666666666", category: BookCategory.Thriller),
            MakeBook(userId, title: "book7", isbn: "7777777777", category: BookCategory.Horror), 
            MakeBook(userId, title: "book8", isbn: "8888888888", category: BookCategory.Romance), 
            MakeBook(userId, title: "book9", isbn: "9999999999", category: BookCategory.Biography),
            MakeBook(userId, title: "book10", isbn: "1010101010", category: BookCategory.History), 
            MakeBook(userId, title: "book11", isbn: "1231231230", category: BookCategory.Educational)
        );

        var response = await Client.GetAsync("/api/books/categorized");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(11, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(11, body.GetProperty("items").GetArrayLength());
        
        var items = body.GetProperty("items").EnumerateArray().ToList();
        var categories = items.Select(x => x.GetProperty("category").GetInt32()).ToList();
        Assert.Equal(11, categories.Distinct().Count());
    }

    [Fact]
    public async Task GetBooksCategorized_WithSearch_ReturnsOnlyMatchingBooks()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(
            MakeBook(userId, title: "Clean Code", isbn: "1111111111", category: BookCategory.Educational),
            MakeBook(userId, title: "Clean Architecture", isbn: "2222222222", category: BookCategory.Educational),
            MakeBook(userId, title: "DDD", isbn: "3333333333", category: BookCategory.Other)
        );

        var response = await Client.GetAsync("/api/books/categorized?search=clean");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, body.GetProperty("items").GetArrayLength());

        var titles = body.GetProperty("items").EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .ToList();

        Assert.Contains("Clean Code", titles);
        Assert.Contains("Clean Architecture", titles);
        Assert.DoesNotContain("DDD", titles);
    }
    [Fact]
    public async Task GetBooksCategorized_WithSearchNoMatch_ReturnsEmptyList()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(
            MakeBook(userId, title: "Clean Code", isbn: "1111111111", category: BookCategory.Educational),
            MakeBook(userId, title: "Clean Architecture", isbn: "2222222222", category: BookCategory.Educational),
            MakeBook(userId, title: "DDD", isbn: "3333333333", category: BookCategory.Other)
        );

        var response = await Client.GetAsync("/api/books/categorized?search=michael");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, body.GetProperty("items").GetArrayLength());
        Assert.Equal(0, body.GetProperty("totalCount").GetInt32());
    }
}