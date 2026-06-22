namespace Api.Entities;

public class User
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
    public ICollection<Book> Books { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}