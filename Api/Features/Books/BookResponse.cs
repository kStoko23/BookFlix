namespace Api.Features.Books;

public class BookResponse
{
    public long Id  { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int Pages { get; set; }
    public int Rating { get; set; }
    public DateTime? CreatedAt { get; set; } =  DateTime.UtcNow;
    public long UserId { get; set; }
}
