using Api.Entities;

namespace Api.Features.Books;

public record CreateBookRequest(
    string Title,
    string Author,
    string? Description,
    BookCategory? Category,
    string Isbn,
    int Pages,
    int Rating);
