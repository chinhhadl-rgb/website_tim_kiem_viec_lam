using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.UngVien
{
    public class KinhNghiemLamViecDto
    {
        [Required(ErrorMessage = "Tên công ty là bắt buộc.")]
        public string TenCongTy { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Chức danh là bắt buộc.")]
        public string ChucDanh { get; set; } = string.Empty;
        
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? MoTaChiTiet { get; set; }
        public int? MaNganhNghe { get; set; }
        public int? MaViTri { get; set; }
    }
}
