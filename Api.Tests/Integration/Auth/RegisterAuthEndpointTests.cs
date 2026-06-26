using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Integration.Auth;

public class RegisterAuthEndpointTests: IntegrationTestBase
{
    public RegisterAuthEndpointTests(BooksApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WhenRequestInvalid_ShouldReturn422UnprocessableEntity()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "bad-email",
            Username = "ab",
            Password = "password"
        });
        
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldReturn409Conflict()
    {
        await SeedUserAsync();
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
            {
                Email = "test@test.com",
                Username = "User0",
                Password = "Password1234"
            }
        );
        
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task Register_WhenRequestIsValid_ShouldCreateUser()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "test@test.com",
            Username = "User0",
            Password = "Password1234"
        });
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();

        Assert.True(await db.Users.AnyAsync(x => x.Email == "test@test.com"));
    }
    
    [Fact]
    public async Task Register_WhenRequestIsValid_ShouldReturnUserData()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "test@test.com",
            Username = "User0",
            Password = "Password1234"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("test@test.com", body.GetProperty("email").GetString());
        Assert.Equal("User0", body.GetProperty("username").GetString());
        Assert.True(body.GetProperty("id").GetInt64() > 0);
    }

    [Fact]
    public async Task Register_ShouldHashPassword()
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "test@test.com",
            Username = "User0",
            Password = "Password1234"
        });

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        var user = await db.Users.FirstAsync(x => x.Email == "test@test.com");

        Assert.NotEqual("Password1234", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Password1234", user.PasswordHash));
    }

    [Fact]
    public async Task Register_ShouldNormalizeEmail()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "  TEST@TEST.COM  ",
            Username = "User0",
            Password = "Password1234"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BooksDbContext>();
        Assert.True(await db.Users.AnyAsync(x => x.Email == "test@test.com"));
    }
}