using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.UngVien
{
    public class UpdateHoSoUngVienDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        public string HoTen { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public int? MaDiaDiem { get; set; }
        public string? DiaChiCuThe { get; set; }
        public int? MaViTriMongMuon { get; set; }
        public string? GioiThieuBanThan { get; set; }
        public bool ChoPhepNhaTuyenDung { get; set; }
        public decimal? MucLuongMongMuonMin { get; set; }
        public decimal? MucLuongMongMuonMax { get; set; }
        public string? DanhSachNgoaiNgu { get; set; }
        public string? DanhSachChungChi { get; set; }
        public string? DanhSachHocVan { get; set; }
    }
}
