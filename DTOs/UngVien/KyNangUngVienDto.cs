using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.UngVien
{
    public class KyNangUngVienDto
    {
        [Required(ErrorMessage = "Mã kỹ năng là bắt buộc.")]
        public int MaKyNang { get; set; }
        
        public decimal? SoNamKinhNghiem { get; set; }
    }
}
