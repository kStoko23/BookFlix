using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Data;
using Api.Entities;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class RefreshAuthEndpointTests : IntegrationTestBase
{
    public RefreshAuthEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        var response = await Client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_Returns200WithNewToken()
    {
        await SeedUserAsync();
        var loginResponse =
            await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var oldToken = loginBody.GetProperty("token").GetString();
        
        await Task.Delay(1000);

        var response = await Client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var newToken = body.GetProperty("token").GetString();
        Assert.NotNull(newToken);
        Assert.NotEqual(oldToken, newToken);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_SetsNewRefreshTokenCookie()
    {
        await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        var response = await Client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), x => x.Contains("refreshToken"));
    }

    [Fact]
    public async Task Refresh_RevokesOldRefreshToken()
    {
        var userId = await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        await Client.PostAsync("/api/auth/refresh", null);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

        Assert.Equal(1, tokens.Count(x => x.Revoked));
        Assert.Equal(1, tokens.Count(x => !x.Revoked));
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_Returns401AndRevokesAllTokens()
    {
        var userId = await SeedUserAsync();

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var rawToken = jwtService.GetRefreshToken();
        var tokenHash = jwtService.HashRefreshToken(rawToken);
        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            Revoked = true
        });

        var activeRawToken = jwtService.GetRefreshToken();
        var activeTokenHash = jwtService.HashRefreshToken(activeRawToken);
        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = activeTokenHash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            Revoked = false
        });
        await db.SaveChangesAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", $"refreshToken={rawToken}");
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var activeTokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && !x.Revoked)
            .ToListAsync();

        Assert.Empty(activeTokens);
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_Returns401()
    {
        var userId = await SeedUserAsync();

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var rawToken = jwtService.GetRefreshToken();
        var tokenHash = jwtService.HashRefreshToken(rawToken);

        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            Revoked = false
        });
        await db.SaveChangesAsync();

        Client.DefaultRequestHeaders.Add("Cookie", $"refreshToken={rawToken}");

        var response = await Client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}