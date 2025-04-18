using System.Text.Json.Serialization;

namespace KioskAPI.Models
{
    public class User
    {
        // Primary key for the user
        public int Id { get; set; }

        // User email (used for login)
        public string Email { get; set; }

        // Hashed password (hidden in JSON responses)
        [JsonIgnore]
        public string? PasswordHash { get; set; }

        // Preferred UI language ("English", "Russian", "Hebrew")
        public string Language { get; set; }

        // User's first name
        public string FirstName { get; set; }

        // User's last name
        public string LastName { get; set; }

        // Dormitory building name ("Paz", "Degel", "Lavan", "Thelet")
        public string Building { get; set; }

        // Room number within the building
        public string RoomNumber { get; set; }

        // Role determines permissions ("User", "Admin", "SuperAdmin")
        public string Role { get; set; } = "User";

        // Salt used for hashing the password (hidden in JSON)
        [JsonIgnore]
        public string? Salt { get; set; }

        // Current refresh token for the user (hidden in JSON)
        [JsonIgnore]
        public RefreshToken? RefreshToken { get; set; }

        // Optional field for user's place of birth
        public string? PlaceOfBirth { get; set; }

        // Return a concise string representation of the user
        public override string ToString() =>
            $"Id: {Id}\n" +
            $"Email: {Email}\n" +
            $"Name: {FirstName} {LastName}\n" +
            $"Building: {Building}, Room: {RoomNumber}\n" +
            $"Language: {Language}, Role: {Role}";
    }
}
