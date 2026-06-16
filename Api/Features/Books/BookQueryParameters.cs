using Api.Entities;

namespace Api.Features.Books;

public class BookQueryParameters
{
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public BookCategory? Category { get; set; }
}
