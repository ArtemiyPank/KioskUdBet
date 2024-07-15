namespace KioskApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "User"; // "User" или "Admin"

        public override string ToString()
        {
            return $"Id: {Id}, User: {Username}, Role: {Role}";
        }
    }
}
