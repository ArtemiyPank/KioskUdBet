using KioskAPI.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; } 
    public bool IsRevoked { get; set; }
    public DateTime Created { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Внешний ключ к таблице пользователей
    public int UserId { get; set; }
    public User User { get; set; }
}

