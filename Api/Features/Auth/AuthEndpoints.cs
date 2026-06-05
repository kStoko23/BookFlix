using Api.Data;
using Api.Entities;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");
        group.MapPost("/login", Login);
        group.MapPost("/register", Register);
    }

    private static async Task<IResult> Login([FromBody] LoginRequest request, BooksDbContext db, JwtService jwtService)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == request.Email.ToLower());
        
        if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return Results.Unauthorized();
        
        var token = jwtService.GenerateJwtToken(user);
        return Results.Ok(new { token });
    }

    private static async Task<IResult> Register([FromBody] RegisterRequest request, BooksDbContext db)
    {
        if (await db.Users.AnyAsync(x => x.Email == request.Email))
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
}