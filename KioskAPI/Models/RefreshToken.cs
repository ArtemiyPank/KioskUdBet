using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskAPI.Models
{
    public class RefreshToken
    {
        // Primary key for the refresh token record
        [Key]
        public int Id { get; set; }

        // Foreign key to the associated user
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // The refresh token string
        [Required]
        public string Token { get; set; }

        // UTC timestamp when the token expires
        public DateTime ExpiryDate { get; set; }

        // Indicates whether the token has been revoked
        public bool IsRevoked { get; set; }

        // UTC timestamp when the token was created
        public DateTime Created { get; set; } = DateTime.UtcNow;

        // True if the current time is past the expiry date
        [NotMapped]
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;

        // True if token is neither expired nor revoked
        [NotMapped]
        public bool IsActive => !IsRevoked && !IsExpired;

        // Navigation property for the related user
        public User User { get; set; }
    }
}
