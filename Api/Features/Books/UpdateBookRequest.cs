namespace Api.Features.Books;

public class UpdateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Pages  { get; set; }
    public int Rating { get; set; }
}
