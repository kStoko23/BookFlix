using System.Security.Claims;
using Api.Data;
using Api.Entities;
using Api.Features.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using UpdateBookRequest = Api.Features.Books.CreateBookRequest;

namespace Api.Features.Books;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books").RequireRateLimiting("fixed");

        group.MapGet("/", GetBooks);
        group.MapGet("/categorized", GetBooksCategorized);
        group.MapGet("/{id:long}", GetBook);
        group.MapGet("/mine", GetMyBooks).RequireAuthorization();
        group.MapPost("/", CreateBook).RequireAuthorization();
        group.MapPut("/{id:long}", PutBook).RequireAuthorization();
        group.MapDelete("/{id:long}", DeleteBook).RequireAuthorization();
    }

    private static async Task<IResult> GetBooks(BooksDbContext db, [AsParameters] BookQueryParameters parameters)
    {
        var page = Math.Max(parameters.Page ?? 1, 1);
        var pageSize = Math.Clamp(parameters.PageSize ?? 20, 1, 100);

        var query = db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var pattern = $"%{parameters.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, pattern) ||
                EF.Functions.ILike(x.Author, pattern));
        }

        if (parameters.Category.HasValue)
        {
            query = query.Where(x => x.Category == parameters.Category.Value);
        }

        var totalCount = await query.CountAsync();

        var books = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BookResponse
            {
                Id = x.Id,
                Title = x.Title,
                Author = x.Author,
                Description = x.Description,
                Category = x.Category ?? BookCategory.Other,
                Isbn = x.Isbn,
                Pages = x.Pages,
                Rating = x.Rating
            })
            .ToListAsync();

        return Results.Ok(new
        {
            items = books,
            totalCount,
            page,
            pageSize
        });
    }

    private static async Task<IResult> GetMyBooks(BooksDbContext db, [AsParameters] BookQueryParameters parameters, ClaimsPrincipal user)
    {
        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var page = Math.Max(parameters.Page ?? 1, 1);
        var pageSize = Math.Clamp(parameters.PageSize ?? 20, 1, 100);

        var query = db.Books.AsNoTracking().Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var pattern = $"%{parameters.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, pattern) ||
                EF.Functions.ILike(x.Author, pattern));
        }

        if (parameters.Category.HasValue)
        {
            query = query.Where(x => x.Category == parameters.Category.Value);
        }

        var totalCount = await query.CountAsync();

        var books = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BookResponse
            {
                Id = x.Id,
                Title = x.Title,
                Author = x.Author,
                Description = x.Description,
                Category = x.Category ?? BookCategory.Other,
                Isbn = x.Isbn,
                Pages = x.Pages,
                Rating = x.Rating
            })
            .ToListAsync();

        return Results.Ok(new
        {
            items = books,
            totalCount,
            page,
            pageSize
        });
    }

    private static async Task<IResult> GetBooksCategorized(BooksDbContext db, [AsParameters] BookQueryParameters parameters)
    {
        var page = Math.Max(parameters.Page ?? 1, 1);
        var pageSize = Math.Clamp(parameters.PageSize ?? 15, 1, 100);

        var baseQuery = db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var pattern = $"%{parameters.Search.Trim()}%";
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Title, pattern) ||
                EF.Functions.ILike(x.Author, pattern));
        }

        var books = new List<BookResponse>();

        foreach (var category in Enum.GetValues<BookCategory>())
        {
            var categoryBooks = await baseQuery
                .Where(x => x.Category == category)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BookResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Author = x.Author,
                    Description = x.Description,
                    Category = x.Category ?? BookCategory.Other,
                    Isbn = x.Isbn,
                    Pages = x.Pages,
                    Rating = x.Rating
                })
                .ToListAsync();

            books.AddRange(categoryBooks);
        }

        var totalCount = books.Count;
        var categoriesCount = Enum.GetValues<BookCategory>().Length;

        return Results.Ok(new
        {
            items = books,
            totalCount,
            page,
            pageSize = pageSize * categoriesCount
        });
    }
    private static async Task<IResult> GetBook([FromRoute] long id, BooksDbContext db)
    {
        var book = await db.Books
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BookResponse
            {
                Id = x.Id,
                Title = x.Title,
                Author = x.Author,
                Description = x.Description,
                Category = x.Category ?? BookCategory.Other,
                Isbn = x.Isbn,
                Pages = x.Pages,
                Rating = x.Rating,
                UserId = x.UserId
            })
            .FirstOrDefaultAsync();

        if (book == null) return Results.NotFound();

        return Results.Ok(book);
    }
    private static async Task<IResult> CreateBook(BooksDbContext db, [FromBody] CreateBookRequest request, ClaimsPrincipal user)
    {
        var validator = new BookValidator();
        var errors = validator.ValidateCreateOrUpdateRequest(request);
        var cleanedIsbn = request.Isbn.Replace("-", "").Trim();
        if (validator.HasErrors)
        {
            return Results.ValidationProblem(errors);
        }
        if (await db.Books.AnyAsync(x => x.Isbn == cleanedIsbn))
            return Results.Conflict(new { message = "Duplicate ISBN number" });

        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = new Book()
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Description = request.Description?.Trim(),
            Category =  request.Category ?? BookCategory.Other,
            Isbn = cleanedIsbn,
            Pages = request.Pages,
            Rating = request.Rating,
            UserId = userId
        };

        db.Books.Add(book);
        await db.SaveChangesAsync();

        var response = new BookResponse
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Description =  book.Description,
            Category = book.Category ?? BookCategory.Other,
            Isbn = book.Isbn,
            Pages = book.Pages,
            Rating = book.Rating,
            UserId = book.UserId
        };

        return Results.Created($"/api/books/{book.Id}", response);
    }
    private static async Task<IResult> PutBook([FromRoute] long id, [FromBody] UpdateBookRequest request, BooksDbContext db, ClaimsPrincipal user)
    {
        var validator = new BookValidator();
        var errors = validator.ValidateCreateOrUpdateRequest(request);
        var cleanedIsbn = request.Isbn.Replace("-", "").Trim();
        if (validator.HasErrors)
        {
            return Results.ValidationProblem(errors);
        }
        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = await db.Books.FindAsync(id);
        if (book == null) return Results.NotFound();
        if(book.UserId != userId) return Results.Forbid();

        if (await db.Books.AnyAsync(x => x.Isbn == cleanedIsbn && x.Id != id))
            return Results.Conflict(new { message = "Duplicate ISBN number" });

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Description = request.Description?.Trim();
        book.Category = request.Category ?? BookCategory.Other;
        book.Isbn = cleanedIsbn;
        book.Pages = request.Pages;
        book.Rating = request.Rating;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    private static async Task<IResult> DeleteBook([FromRoute] long id, BooksDbContext db, ClaimsPrincipal user)
    {
        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = await db.Books.FindAsync(id);
        if (book == null) return Results.NotFound();
        if(book.UserId != userId) return Results.Forbid();

        db.Books.Remove(book);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
