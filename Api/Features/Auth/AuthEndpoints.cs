using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Api.Data;
using Api.Entities;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Auth;

public static class AuthEndpoints
{
    private static readonly int ExpirationDays = 7;
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");
        group.MapPost("/login", Login).RequireRateLimiting("auth");
        group.MapPost("/register", Register).RequireRateLimiting("auth");
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout);
    }

    private static async Task<IResult> Login([FromBody] LoginRequest request, BooksDbContext db, JwtService jwtService, HttpContext context, IWebHostEnvironment environment)
    {
        var validator = new AuthValidator();
        var errors = validator.ValidateLoginRequest(request);
        if (validator.HasErrors)
            return Results.ValidationProblem(errors);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Results.Unauthorized();

        var token = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GetRefreshToken();
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);
        
        db.RefreshTokens.RemoveRange(
            db.RefreshTokens
                .Where(
                    x => x.UserId == user.Id 
                         && (x.Revoked || x.ExpiresAt <= DateTime.UtcNow)));
        
        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays),
            CreatedAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync();
        
        context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires =  DateTime.UtcNow.AddDays(ExpirationDays),
            Path = "/api/auth",
        });
        
        return Results.Ok(new { token });
    }

    private static async Task<IResult> Register([FromBody] RegisterRequest request, BooksDbContext db)
    {
        var validator = new AuthValidator();
        var errors = validator.ValidateRegisterRequest(request);
        if (validator.HasErrors)
            return Results.ValidationProblem(errors);

        if (await db.Users.AnyAsync(x => x.Email == request.Email.Trim().ToLower()))
            return Results.Conflict(new { message = "Email already taken" });

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();

        return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, user.Username });
    }

    private static async Task<IResult> Refresh(HttpContext context, BooksDbContext db, JwtService jwtService, IWebHostEnvironment environment)
    {
        var rawToken = context.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(rawToken))
            return Results.Unauthorized();
        
        var refreshTokenHash = jwtService.HashRefreshToken(rawToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash);

        if (stored == null || DateTime.UtcNow > stored.ExpiresAt)
            return Results.Unauthorized();

        if (stored.Revoked)
        {
            await db.RefreshTokens.Where(x => x.UserId == stored.UserId && !x.Revoked)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Revoked, true));

            return Results.Unauthorized();
        }
        
        var user = await db.Users.FindAsync(stored.UserId);
        if (user ==null)
            return Results.Unauthorized();

        stored.Revoked = true;
        
        var newAccessToken = jwtService.GenerateJwtToken(user);
        var newRefreshToken = jwtService.GetRefreshToken();
        var newRefreshTokenHash = jwtService.HashRefreshToken(newRefreshToken);

        db.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(ExpirationDays),
            CreatedAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync();
        
        context.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires =  DateTime.UtcNow.AddDays(ExpirationDays),
            Path = "/api/auth"
        });
        
        return Results.Ok(new { token = newAccessToken });
    }

    private static async Task<IResult> Logout(HttpContext context, BooksDbContext db, JwtService jwtService)
    {
        var rawToken = context.Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(rawToken))
        {
            var tokenHash = jwtService.HashRefreshToken(rawToken);
            var stored = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (stored != null && !stored.Revoked)
            {
                stored.Revoked = true;
                await db.SaveChangesAsync();
            }
        }

        context.Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });

        return Results.Ok();
    }
}