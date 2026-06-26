using System.Net;
using System.Net.Http.Json;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class LogoutAuthEndpointTests : IntegrationTestBase
{
    public LogoutAuthEndpointTests(BooksApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Logout_WithoutCookie_Returns200()
    {
        var response = await Client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidCookie_Returns200()
    {
        await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        var response = await Client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var userId = await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        await Client.PostAsync("/api/auth/logout", null);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var tokens = await db.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();

        Assert.All(tokens, t => Assert.True(t.Revoked));
    }

    [Fact]
    public async Task Logout_DeletesRefreshTokenCookie()
    {
        await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        var response = await Client.PostAsync("/api/auth/logout", null);

        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            x => x.Contains("refreshToken") && x.Contains("expires=Thu, 01 Jan 1970"));
    }

    [Fact]
    public async Task Logout_PreventsSubsequentRefresh()
    {
        await SeedUserAsync();
        await Client.PostAsJsonAsync("/api/auth/login", new { Email = "test@test.com", Password = "Test1234" });

        await Client.PostAsync("/api/auth/logout", null);

        var response = await Client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}