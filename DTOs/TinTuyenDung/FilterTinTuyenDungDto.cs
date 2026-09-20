namespace RecruitmentSystem.DTOs.TinTuyenDung
{
    public class FilterTinTuyenDungDto
    {
        public string? TuKhoa { get; set; }
        public int? MaNganhNghe { get; set; }
        public int? MaViTri { get; set; }
        public int? MaDiaDiem { get; set; }
        public string? HinhThucLamViec { get; set; }
        public decimal? LuongMin { get; set; }
        public decimal? LuongMax { get; set; }
        public int? SoNamKinhNghiem { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
