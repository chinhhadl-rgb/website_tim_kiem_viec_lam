namespace RecruitmentSystem.DTOs.TinTuyenDung
{
    public class JobItemDto
    {
        public int MaTinTuyenDung { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? TenCongTy { get; set; }
        public string? DuongDanLogo { get; set; }
        
        public int? MaNganhNghe { get; set; }
        public string TenNganhNghe { get; set; } = string.Empty;
        
        public int? MaViTri { get; set; }
        public string TenViTri { get; set; } = string.Empty;
        
        public int? MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; } = string.Empty;
        
        public decimal? LuongToiThieu { get; set; }
        public decimal? LuongToiDa { get; set; }
        public DateTime? HanNopHoSo { get; set; }
    }
}
