namespace KioskApp.Models
{
    public class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public User Data { get; set; }


        public override string ToString()
        {
            return $"IsSuccess - {IsSuccess} \nMessage - {Message} \nUser - {Data.ToString()} \n";
        }
    }
}
