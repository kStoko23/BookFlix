using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Entities;

namespace Api.Tests.Integration.Books;

public class CreateBookEndpointTests : IntegrationTestBase
{
    public CreateBookEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateBook_Unauthenticated_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/books",
            new
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
    public async Task CreateBook_ValidRequest_Returns201WithCreatedBook()
    {
        var userId = await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);
        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5,
                Category = BookCategory.Educational
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Clean Code", body.GetProperty("title").GetString());
        Assert.Equal("Robert C. Martin", body.GetProperty("author").GetString());
        Assert.Equal("1111111111", body.GetProperty("isbn").GetString());
    }

    [Fact]
    public async Task CreateBook_SetsUserIdFromJwtClaim()
    {
        var userId = await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);
        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(userId, body.GetProperty("userId").GetInt64());
    }

    [Fact]
    public async Task CreateBook_DuplicateIsbn_Returns409()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, isbn: "1111111111"));
        var token = await GetAuthTokenAsync();
        Authorize(token);
        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Different Book",
                Author = "Different Author",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_IsbnWithDashes_NormalizesBeforeSaving()
    {
        var userId = await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);
        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "111-1111-111",
                Pages = 100,
                Rating = 5
            });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("1111111111", body.GetProperty("isbn").GetString());
    }

    [Fact]
    public async Task CreateBook_InvalidRequest_Returns422WithValidationErrors()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
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
        Assert.True(errors.GetProperty("isbn").GetArrayLength() > 0);
        Assert.True(errors.GetProperty("pages").GetArrayLength() > 0);
        Assert.True(errors.GetProperty("rating").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateBook_TitleTooLong_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = new string('a', 301),
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_AuthorTooLong_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = new string('a', 201),
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_DescriptionOnlyWhitespace_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Description = "     ",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetProperty("description").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateBook_DescriptionTooLong_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Description = new string('a', 1201),
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_RatingZero_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 0
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetProperty("rating").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateBook_PagesZero_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 0,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetProperty("pages").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateBook_PagesTooLarge_Returns422()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 10001,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").GetProperty("pages").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateBook_IsbnWithDashesIsDuplicate_Returns409()
    {
        var userId = await SeedUserAsync();
        await SeedBooksAsync(MakeBook(userId, isbn: "1111111111"));

        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Different Book",
                Author = "Different Author",
                Isbn = "111-111-1111",
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_NullDescription_Returns201()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Description = (string?)null,
                Pages = 100,
                Rating = 5
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_NullCategory_Returns201WithDefaultCategoryOther()
    {
        await SeedUserAsync();
        var token = await GetAuthTokenAsync();
        Authorize(token);

        var response = await Client.PostAsJsonAsync("/api/books",
            new
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Isbn = "1111111111",
                Pages = 100,
                Rating = 5,
                Category = (BookCategory?)null
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)BookCategory.Other, body.GetProperty("category").GetInt32());
    }
}