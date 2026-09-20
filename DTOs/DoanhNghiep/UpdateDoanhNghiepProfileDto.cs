using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.DoanhNghiep
{
    public class UpdateDoanhNghiepProfileDto
    {
        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
        public string TenCongTy { get; set; } = string.Empty;
        
        public string? MaSoThue { get; set; }
        public string? QuyMo { get; set; }
        public string? MoTa { get; set; }
        public int? MaDiaDiem { get; set; }
        public string? DiaChiChiTiet { get; set; }
        public string? DuongDanWebsite { get; set; }
        
        public IFormFile? Logo { get; set; }
        public IFormFile? GiayPhepKinhDoanh { get; set; }
    }
}
