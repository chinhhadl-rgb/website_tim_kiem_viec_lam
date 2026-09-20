using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.LichPhongVan
{
    public class KetQuaPhongVanDto
    {
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10.")]
        public decimal? DiemChuyenMon { get; set; }
        
        public string? NhanXetNoiBo { get; set; }
        
        [Required]
        public string KetQuaNoiBo { get; set; } = string.Empty;
    }
}
