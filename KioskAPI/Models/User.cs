namespace KioskAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Language { get; set; } // "English" / "Russian" / "Hebrew"
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Building { get; set; } // "Paz" / "Degel" / "Lavan / "Thelet"
        public string RoomNumber { get; set; }
        public string Role { get; set; } = "User"; // "User" / "Admin" / "SuperAdmin"

        public string? Salt { get; set; }
        public RefreshToken? RefreshToken { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} \nEmail: {Email} \nFirstName: {FirstName} \nLastName: {LastName} " +
                   $"\nBuilding: {Building}\n RoomNumber: {RoomNumber} \nLanguage: {Language} \nRole: {Role}";
        }
    }
}
