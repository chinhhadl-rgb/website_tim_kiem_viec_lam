using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        public string MatKhau { get; set; } = string.Empty;
    }
}
