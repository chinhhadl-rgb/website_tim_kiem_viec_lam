namespace RecruitmentSystem.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string MaOTP { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}
