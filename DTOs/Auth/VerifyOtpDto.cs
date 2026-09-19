using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải bao gồm đúng 6 chữ số.")]
        public string MaOTP { get; set; } = string.Empty;
    }
}
