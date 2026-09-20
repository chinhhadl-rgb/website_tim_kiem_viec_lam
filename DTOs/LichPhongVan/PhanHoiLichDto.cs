using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.LichPhongVan
{
    public class PhanHoiLichDto
    {
        [Required]
        public string PhanHoiUngVien { get; set; } = string.Empty;
        
        public string? LyDoDoiLich { get; set; }
    }
}
