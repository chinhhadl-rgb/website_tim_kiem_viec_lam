using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.LichPhongVan
{
    public class CreateLichPhongVanDto
    {
        [Required]
        public int MaUngTuyen { get; set; }
        
        [Required]
        public DateTime ThoiGianPhongVan { get; set; }
        
        [Required]
        public string HinhThuc { get; set; } = string.Empty;
        
        [Required]
        public string DiaDiemHoacDuongDan { get; set; } = string.Empty;
    }
}
