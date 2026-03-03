namespace DVLD.CORE.DTOs.Users
{
    public class AuthResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresOn { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
