using System.Security.Claims;
using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Books;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");
        group.MapGet("/", GetBooks);
        group.MapGet("/categorized", GetBooksCategorized);
        group.MapGet("/{id:long}", GetBook);
        group.MapGet("/mine", GetMyBooks).RequireAuthorization();
        group.MapPost("/", CreateBook).RequireAuthorization();
        group.MapPut("/{id:long}", PutBook).RequireAuthorization();
        group.MapDelete("/{id:long}", DeleteBook).RequireAuthorization();
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
                Category = x.Category,
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
                Category = x.Category,
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
                    Category = x.Category,
                    Isbn = x.Isbn,
                    Pages = x.Pages,
                    Rating = x.Rating
                })
                .ToListAsync();

            books.AddRange(categoryBooks);
        }

        var totalCount = books.Count;

        return Results.Ok(new
        {
            items = books,
            totalCount,
            page,
            pageSize
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
                Category = x.Category,
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
        var errors = ValidateCreateBookRequest(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = new Book()
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Description = request.Description.Trim(),
            Category =  request.Category,
            Isbn = request.Isbn.Trim(),
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
            Category = book.Category,
            Isbn = book.Isbn,
            Pages = book.Pages,
            Rating = book.Rating,
            UserId = book.UserId
        };

        return Results.Created($"/api/books/{book.Id}", response);
    }
    private static Dictionary<string, string[]> ValidateCreateBookRequest(CreateBookRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title))
            errors["title"] = ["Title is required"];
        if (string.IsNullOrWhiteSpace(request.Author))
            errors["author"] = ["Author is required"];
        if (string.IsNullOrWhiteSpace(request.Description))
            request.Description = "No description provided";
        if (!Enum.IsDefined(typeof(BookCategory), request.Category))
            errors["category"] = ["Invalid category"];
        if (string.IsNullOrWhiteSpace(request.Isbn))
            errors["isbn"] = ["ISBN is required"];
        if (request.Pages <= 0)
            errors["pages"] = ["Pages can't be less or equal to 0"];
        if (request.Rating < 1 || request.Rating > 5)
            errors["rating"] = ["Rating must be between 1 and 5"];

        return errors;
    }
    private static async Task<IResult> PutBook([FromRoute] long id, [FromBody] UpdateBookRequest request, BooksDbContext db, ClaimsPrincipal user)
    {
        var errors = ValidateUpdateBookRequest(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }
        var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var book = await db.Books.FindAsync(id);
        if (book == null) return Results.NotFound();
        if(book.UserId != userId) return Results.Forbid();

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Description = request.Description.Trim();
        book.Category = request.Category;
        book.Isbn = request.Isbn.Trim();
        book.Pages = request.Pages;
        book.Rating = request.Rating;

        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    private static Dictionary<string, string[]> ValidateUpdateBookRequest(UpdateBookRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Title))
            errors["title"] = ["Title is required"];
        if (string.IsNullOrWhiteSpace(request.Author))
            errors["author"] = ["Author is required"];
        if (string.IsNullOrWhiteSpace(request.Description))
            request.Description = "No description provided";
        if (!Enum.IsDefined(request.Category))
            errors["category"] = ["Invalid category"];
        if (string.IsNullOrWhiteSpace(request.Isbn))
            errors["isbn"] = ["ISBN is required"];
        if (request.Pages <= 0)
            errors["pages"] = ["Pages can't be less or equal to 0"];
        if (request.Rating < 1 || request.Rating > 5)
            errors["rating"] = ["Rating must be between 1 and 5"];

        return errors;
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
