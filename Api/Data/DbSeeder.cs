using Api.Entities;
using Api.Features.Books;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BooksDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test1234");

        var userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => $"user{f.IndexFaker}")
            .RuleFor(u => u.Email, f => $"user{f.IndexFaker}@test.com")
            .RuleFor(u => u.PasswordHash, _ => passwordHash)
            .RuleFor(u => u.CreatedAt, _ => DateTime.UtcNow);

        var users = userFaker.Generate(20);
        db.Users.AddRange(users);
        await db.SaveChangesAsync();

        var userIds = users.Select(u => u.Id).ToList();

        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Commerce.ProductName())
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Description, f => f.Lorem.Paragraph())
            .RuleFor(b => b.Category, f => f.PickRandom<BookCategory>())
            .RuleFor(b => b.Isbn, f => (9780000000000 + f.IndexFaker).ToString())
            .RuleFor(b => b.Pages, f => f.Random.Int(50, 1200))
            .RuleFor(b => b.Rating, f => f.Random.Int(1, 5))
            .RuleFor(b => b.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(b => b.UserId, f => f.PickRandom(userIds));

        var books = bookFaker.Generate(1000);
        db.Books.AddRange(books);
        await db.SaveChangesAsync();
    }
}