namespace Api.Entities;

public class Book
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BookCategory? Category { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int Pages { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
}
