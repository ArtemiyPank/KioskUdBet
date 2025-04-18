using System.Text.Json.Serialization;

namespace KioskApp.Models
{
    public class User
    {
        // Unique identifier for the user
        public int Id { get; set; }

        // User's email address (used for authentication)
        public string Email { get; set; }

        // Plain-text password (only used for login/registration requests)
        [JsonIgnore]
        public string Password { get; set; }

        // Preferred UI language ("English", "Russian", "Hebrew")
        public string Language { get; set; }

        // User's first and last name
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Dormitory building and room information
        public string Building { get; set; }
        public string RoomNumber { get; set; }

        // Role determines available app features ("User" or "Admin")
        public string Role { get; set; } = "User";

        // Optional place of birth field
        public string? PlaceOfBirth { get; set; }

        // Computed full name (not serialized)
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";

        // Return a readable representation of the user
        public override string ToString() =>
            $"Id: {Id}\n" +
            $"Email: {Email}\n" +
            $"Name: {FullName}\n" +
            $"Building: {Building}, Room: {RoomNumber}\n" +
            $"Language: {Language}, Role: {Role}\n" +
            $"PlaceOfBirth: {PlaceOfBirth}";
    }
}
