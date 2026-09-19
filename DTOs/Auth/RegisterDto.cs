using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên.")]
        public string MatKhau { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? DienThoai { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public string VaiTro { get; set; } = "UngVien"; // 'NhaTuyenDung' | 'UngVien'
    }
}
