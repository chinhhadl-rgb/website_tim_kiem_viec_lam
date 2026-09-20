using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Admin
{
    public class SaoLuuDto
    {
        [Required]
        public string LoaiSaoLuu { get; set; } = "FULL"; // 'FULL' | 'DIFFERENTIAL' | 'LOG'
        
        [Required]
        public string DuongDanThuMuc { get; set; } = string.Empty;
    }
}
