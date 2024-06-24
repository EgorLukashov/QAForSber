namespace QAForSber.Models
{
    public class LoginResponse
    {
        public Admin Admin { get; set; }
        public string Token { get; set; }
        public string ErrorMessage { get; set; }
    }
}
