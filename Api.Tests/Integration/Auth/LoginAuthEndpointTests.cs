using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class LoginAuthEndpointTests : IntegrationTestBase
{
    public LoginAuthEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldReturn401()
    {
        await SeedUserAsync();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@test.com",
            Password = "WrongPassword1234"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task Login_WithInvalidRequest_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "not-an-email",
            Password = ""
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenUserDoesNotExist_ShouldReturn401()
    {
        await SeedUserAsync();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "doesntexist@test.com",
            Password = "Password1234"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenCredentialsValid_ShouldReturnToken()
    {
        await SeedUserAsync();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@test.com",
            Password = "Test1234"
        });
        var body =  await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("token", out _));
    }
    
    [Fact]
    public async Task Login_ShouldStoreRefreshToken()
    {
        var userId = await SeedUserAsync();

        await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@test.com",
            Password = "Test1234"
        });

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();

        Assert.True(await db.RefreshTokens.AnyAsync(x => x.UserId == userId));
    }
    
    [Fact]
    public async Task Login_ShouldSetRefreshTokenCookie()
    {
        await SeedUserAsync();

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@test.com",
            Password = "Test1234"
        });

        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            x => x.Contains("refreshToken"));
    }
}