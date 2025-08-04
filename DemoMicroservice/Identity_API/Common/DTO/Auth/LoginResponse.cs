namespace Identity_API.Common.DTO.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserProfileResponse User { get; set; } = null!;
    }
}
