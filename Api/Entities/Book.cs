namespace Api.Entities;

public class Book
{
    public long Id { get; set; }
    public string Title { get; set; } = null;
    public string Author { get; set; } = null;
    public string Isbn { get; set; } = null;
    public int Pages { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public long UserId { get; set; }
    public User User { get; set; } = null;
}
