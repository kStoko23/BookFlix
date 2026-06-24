using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.Entities;
using Api.Services;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Unit;

public class JwtTests
{
    private static IConfiguration CreateConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super_secret_key_12345678901234567890",
                ["Jwt:Issuer"] = "test_issuer",
                ["Jwt:Audience"] = "test_audience",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
    }
    
    [Fact]
    public void GenerateJwtToken_ShouldContainUserClaims()
    {
        var config = CreateConfig();

        var service = new JwtService(config);

        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "testuser"
        };

        var token = service.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("test@test.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("testuser", jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal("1", jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }
    
    [Fact]
    public void GenerateJwtToken_ShouldSetIssuerAndAudience()
    {
        var config = CreateConfig();

        var service = new JwtService(config);

        var token = service.GenerateJwtToken(new User
        {
            Id = 1,
            Email = "a@a.com",
            Username = "a"
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("test_issuer", jwt.Issuer);
        Assert.Contains("test_audience", jwt.Audiences);
    }
    
    [Fact]
    public void GenerateJwtToken_ShouldHaveExpiration()
    {
        var config = CreateConfig();

        var service = new JwtService(config);

        var token = service.GenerateJwtToken(new User
        {
            Id = 1,
            Email = "a@a.com",
            Username = "a"
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }
    
    [Fact]
    public void GetRefreshToken_ShouldReturnDifferentValues()
    {
        var service = new JwtService(null!);

        var t1 = service.GetRefreshToken();
        var t2 = service.GetRefreshToken();

        Assert.NotEqual(t1, t2);
    }
    
    [Fact]
    public void GetRefreshToken_ShouldBeBase64String()
    {
        var service = new JwtService(null!);

        var token = service.GetRefreshToken();

        Assert.False(string.IsNullOrWhiteSpace(token));

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(32, bytes.Length);
    }
    
    [Fact]
    public void HashRefreshToken_ShouldBeDeterministic()
    {
        var service = new JwtService(null!);

        var token = "refresh_token_example";

        var hash1 = service.HashRefreshToken(token);
        var hash2 = service.HashRefreshToken(token);

        Assert.Equal(hash1, hash2);
    }
    
    [Fact]
    public void HashRefreshToken_DifferentTokens_ShouldProduceDifferentHashes()
    {
        var service = new JwtService(null!);

        var hash1 = service.HashRefreshToken("token1");
        var hash2 = service.HashRefreshToken("token2");

        Assert.NotEqual(hash1, hash2);
    }
}