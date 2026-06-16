using Api.Entities;

namespace Api.Features.Books;

public class UpdateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int Pages  { get; set; }
    public int Rating { get; set; }
}
