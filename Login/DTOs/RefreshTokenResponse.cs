namespace Login.DTOs
{
    public class RefreshTokenResponse
    {
        public string Token { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
