using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.DanhGia
{
    public class CreateDanhGiaDto
    {
        [Required]
        public int MaDoanhNghiep { get; set; }
        
        [Required]
        public int MaUngVien { get; set; }
        
        public int? MaUngTuyen { get; set; }
        
        [Required]
        public string NguoiDanhGia { get; set; } = string.Empty; // 'Candidate' | 'Employer'
        
        [Required]
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        public int SoSao { get; set; }
        
        public string? TieuDe { get; set; }
        public string? NhanXet { get; set; }
    }
}
