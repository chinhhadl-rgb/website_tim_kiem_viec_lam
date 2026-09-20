using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.TinTuyenDung
{
    public class CreateTinTuyenDungDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        public string TieuDe { get; set; } = string.Empty;
        public int? MaNganhNghe { get; set; }
        public int? MaViTri { get; set; }
        public int? MaCapBac { get; set; }
        public int? MaTrinhDoHocVan { get; set; }
        public int? MaDiaDiem { get; set; }
        public string? DiaChiLamViec { get; set; }
        public string? HinhThucLamViec { get; set; }
        public int? SoNamKinhNghiemToiThieu { get; set; }
        public int? SoLuongTuyen { get; set; }
        public string? MoTaCongViec { get; set; }
        public string? YeuCauUngVien { get; set; }
        public string? QuyenLoi { get; set; }
        public decimal? LuongToiThieu { get; set; }
        public decimal? LuongToiDa { get; set; }
        public DateTime? HanNopHoSo { get; set; }
        public List<int> DanhSachMaKyNang { get; set; } = new List<int>();
    }
}
