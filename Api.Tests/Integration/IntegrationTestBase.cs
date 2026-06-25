using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<BooksApiFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    private readonly BooksApiFactory _factory;

    protected IntegrationTestBase(BooksApiFactory factory)
    {
        _factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }
    public async Task InitializeAsync()
    {
        
    }
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        db.RefreshTokens.RemoveRange(db.RefreshTokens);
        db.Books.RemoveRange(db.Books);
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<long> SeedUserAsync(string email = "test@test.com", string username = "User0123", string password = "Test1234")
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new { Email = email, Username = username, Password = password });

        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed ({registerResponse.StatusCode}): {error}");
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var user = await db.Users.FirstAsync(x => x.Email == email.Trim().ToLower());
        return user.Id;
    }

    protected async Task<string> GetAuthTokenAsync(string email = "test@test.com", string password = "Test1234")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("token").GetString()!;
    }

    protected void Authorize(string token) =>
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    protected async Task SeedBooksAsync(params Book[] books)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }

    protected static Book MakeBook(long userId, string title = "Clean Code", string author = "Robert C. Martin", string isbn = "9780132350884", BookCategory category = BookCategory.Other) =>
        new()
        {
            Title = title,
            Author = author,
            Isbn = isbn,
            Category = category,
            Pages = 100,
            Rating = 5,
            UserId = userId
        };
    
    protected async Task<long> SeedBookAndGetIdAsync(Book book)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        return book.Id;
    }
}