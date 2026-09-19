namespace RecruitmentSystem.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public int MaTaiKhoan { get; set; }
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = string.Empty;
    }
}
