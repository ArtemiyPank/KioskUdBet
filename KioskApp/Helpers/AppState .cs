using KioskApp.Models;

namespace KioskApp.Helpers
{
    public class AppState
    {
        public User? CurrentUser { get; set; } = null;
        public string? AccessToken { get; set; } = null;
    }
}
