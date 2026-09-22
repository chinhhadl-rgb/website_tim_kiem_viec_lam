using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.TinTuyenDung;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Services
{
    public class TinTuyenDungService : ITinTuyenDungService
    {
        private readonly string _connectionString;

        public TinTuyenDungService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private async Task<int> GetMaDoanhNghiepAsync(SqlConnection connection, int maTaiKhoan)
        {
            string query = "SELECT MaDoanhNghiep FROM HoSoDoanhNghiep WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<ApiResponse<object>> GetTinTuyenDungsAsync(FilterTinTuyenDungDto filter)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string whereClause = "1=1";
                    if (!string.IsNullOrEmpty(filter.TuKhoa)) whereClause += " AND (TieuDe LIKE @TuKhoa OR d.TenCongTy LIKE @TuKhoa)";
                    if (filter.MaNganhNghe.HasValue) whereClause += " AND t.MaNganhNghe = @MaNganhNghe";
                    if (filter.MaViTri.HasValue) whereClause += " AND t.MaViTri = @MaViTri";
                    if (filter.MaDiaDiem.HasValue) whereClause += " AND t.MaDiaDiem = @MaDiaDiem";
                    if (!string.IsNullOrEmpty(filter.HinhThucLamViec)) whereClause += " AND t.HinhThucLamViec = @HinhThucLamViec";
                    if (filter.LuongMin.HasValue) whereClause += " AND t.LuongToiThieu >= @LuongMin";
                    if (filter.LuongMax.HasValue) whereClause += " AND t.LuongToiDa <= @LuongMax";
                    if (filter.SoNamKinhNghiem.HasValue) whereClause += " AND t.SoNamKinhNghiemToiThieu <= @SoNamKinhNghiem";

                    string countQuery = $@"
                        SELECT COUNT(1) 
                        FROM TinTuyenDung t 
                        LEFT JOIN HoSoDoanhNghiep d ON t.MaDoanhNghiep = d.MaDoanhNghiep 
                        WHERE {whereClause}";

                    int totalItems = 0;
                    using (var cmd = new SqlCommand(countQuery, connection))
                    {
                        if (!string.IsNullOrEmpty(filter.TuKhoa)) cmd.Parameters.AddWithValue("@TuKhoa", "%" + filter.TuKhoa + "%");
                        if (filter.MaNganhNghe.HasValue) cmd.Parameters.AddWithValue("@MaNganhNghe", filter.MaNganhNghe.Value);
                        if (filter.MaViTri.HasValue) cmd.Parameters.AddWithValue("@MaViTri", filter.MaViTri.Value);
                        if (filter.MaDiaDiem.HasValue) cmd.Parameters.AddWithValue("@MaDiaDiem", filter.MaDiaDiem.Value);
                        if (!string.IsNullOrEmpty(filter.HinhThucLamViec)) cmd.Parameters.AddWithValue("@HinhThucLamViec", filter.HinhThucLamViec);
                        if (filter.LuongMin.HasValue) cmd.Parameters.AddWithValue("@LuongMin", filter.LuongMin.Value);
                        if (filter.LuongMax.HasValue) cmd.Parameters.AddWithValue("@LuongMax", filter.LuongMax.Value);
                        if (filter.SoNamKinhNghiem.HasValue) cmd.Parameters.AddWithValue("@SoNamKinhNghiem", filter.SoNamKinhNghiem.Value);
                        
                        totalItems = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }

                    int totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                    int offset = (filter.Page - 1) * filter.PageSize;

                    string dataQuery = $@"
                        SELECT 
                            t.MaTinTuyenDung, t.TieuDe, t.LuongToiThieu, t.LuongToiDa, t.HanNopHoSo,
                            t.MaNganhNghe, c1.TenDanhMuc AS TenNganhNghe,
                            t.MaViTri, c2.TenDanhMuc AS TenViTri,
                            t.MaDiaDiem, c3.TenDanhMuc AS TenDiaDiem,
                            d.TenCongTy, d.DuongDanLogo 
                        FROM TinTuyenDung t
                        LEFT JOIN HoSoDoanhNghiep d ON t.MaDoanhNghiep = d.MaDoanhNghiep
                        LEFT JOIN DanhMuc c1 ON t.MaNganhNghe = c1.MaDanhMuc
                        LEFT JOIN DanhMuc c2 ON t.MaViTri = c2.MaDanhMuc
                        LEFT JOIN DanhMuc c3 ON t.MaDiaDiem = c3.MaDanhMuc
                        WHERE {whereClause}
                        ORDER BY t.NgayDang DESC
                        OFFSET {offset} ROWS FETCH NEXT {filter.PageSize} ROWS ONLY";

                    var items = new List<JobItemDto>();
                    using (var cmd = new SqlCommand(dataQuery, connection))
                    {
                        if (!string.IsNullOrEmpty(filter.TuKhoa)) cmd.Parameters.AddWithValue("@TuKhoa", "%" + filter.TuKhoa + "%");
                        if (filter.MaNganhNghe.HasValue) cmd.Parameters.AddWithValue("@MaNganhNghe", filter.MaNganhNghe.Value);
                        if (filter.MaViTri.HasValue) cmd.Parameters.AddWithValue("@MaViTri", filter.MaViTri.Value);
                        if (filter.MaDiaDiem.HasValue) cmd.Parameters.AddWithValue("@MaDiaDiem", filter.MaDiaDiem.Value);
                        if (!string.IsNullOrEmpty(filter.HinhThucLamViec)) cmd.Parameters.AddWithValue("@HinhThucLamViec", filter.HinhThucLamViec);
                        if (filter.LuongMin.HasValue) cmd.Parameters.AddWithValue("@LuongMin", filter.LuongMin.Value);
                        if (filter.LuongMax.HasValue) cmd.Parameters.AddWithValue("@LuongMax", filter.LuongMax.Value);
                        if (filter.SoNamKinhNghiem.HasValue) cmd.Parameters.AddWithValue("@SoNamKinhNghiem", filter.SoNamKinhNghiem.Value);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new JobItemDto
                                {
                                    MaTinTuyenDung = reader["MaTinTuyenDung"] != DBNull.Value ? Convert.ToInt32(reader["MaTinTuyenDung"]) : 0,
                                    TieuDe = reader["TieuDe"]?.ToString() ?? "",
                                    TenCongTy = reader["TenCongTy"]?.ToString(),
                                    DuongDanLogo = reader["DuongDanLogo"]?.ToString(),
                                    MaNganhNghe = reader["MaNganhNghe"] != DBNull.Value ? Convert.ToInt32(reader["MaNganhNghe"]) : null,
                                    TenNganhNghe = reader["TenNganhNghe"]?.ToString() ?? "",
                                    MaViTri = reader["MaViTri"] != DBNull.Value ? Convert.ToInt32(reader["MaViTri"]) : null,
                                    TenViTri = reader["TenViTri"]?.ToString() ?? "",
                                    MaDiaDiem = reader["MaDiaDiem"] != DBNull.Value ? Convert.ToInt32(reader["MaDiaDiem"]) : null,
                                    TenDiaDiem = reader["TenDiaDiem"]?.ToString() ?? "",
                                    LuongToiThieu = reader["LuongToiThieu"] != DBNull.Value ? Convert.ToDecimal(reader["LuongToiThieu"]) : null,
                                    LuongToiDa = reader["LuongToiDa"] != DBNull.Value ? Convert.ToDecimal(reader["LuongToiDa"]) : null,
                                    HanNopHoSo = reader["HanNopHoSo"] != DBNull.Value ? Convert.ToDateTime(reader["HanNopHoSo"]) : null
                                });
                            }
                        }
                    }

                    var pagedResult = new PagedResult<JobItemDto>
                    {
                        Items = items,
                        PageIndex = filter.Page,
                        PageSize = filter.PageSize,
                        TotalRecords = totalItems,
                        TotalPages = totalPages
                    };

                    return new ApiResponse<object>
                    {
                        Success = true, StatusCode = 200, Message = "Thành công",
                        Data = pagedResult
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetJobByIdAsync(int maTinTuyenDung)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var jobInfo = new Dictionary<string, object>();
                    string query = @"
                        SELECT t.*, d.TenCongTy, d.DuongDanLogo, d.QuyMo, d.MaDiaDiem as DoanhNghiepMaDiaDiem 
                        FROM TinTuyenDung t
                        LEFT JOIN HoSoDoanhNghiep d ON t.MaDoanhNghiep = d.MaDoanhNghiep
                        WHERE t.MaTinTuyenDung = @MaTin";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    jobInfo[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                            }
                            else return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Không tìm thấy tin tuyển dụng" };
                        }
                    }

                    var skills = new List<int>();
                    string skillQuery = "SELECT MaKyNang FROM KyNangTinTuyenDung WHERE MaTinTuyenDung = @MaTin";
                    using (var cmd = new SqlCommand(skillQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) skills.Add(reader.GetInt32(0));
                        }
                    }
                    jobInfo["DanhSachMaKyNang"] = skills;

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công", Data = jobInfo };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> CreateJobAsync(int maTaiKhoan, CreateTinTuyenDungDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);
                    if (maDoanhNghiep == 0) return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Bạn chưa có hồ sơ doanh nghiệp." };

                    int maTin;
                    string insertQuery = @"
                        INSERT INTO TinTuyenDung (MaDoanhNghiep, TieuDe, MaNganhNghe, MaViTri, MaCapBac, MaTrinhDoHocVan, 
                                                  MaDiaDiem, DiaChiLamViec, HinhThucLamViec, SoNamKinhNghiemToiThieu, 
                                                  SoLuongTuyen, MoTaCongViec, YeuCauUngVien, QuyenLoi, LuongToiThieu, 
                                                  LuongToiDa, HanNopHoSo, TrangThaiTin, NgayDang, NgayTao)
                        OUTPUT INSERTED.MaTinTuyenDung
                        VALUES (@MaDoanhNghiep, @TieuDe, @MaNganhNghe, @MaViTri, @MaCapBac, @MaTrinhDoHocVan, 
                                @MaDiaDiem, @DiaChiLamViec, @HinhThucLamViec, @SoNamKinhNghiemToiThieu, 
                                @SoLuongTuyen, @MoTaCongViec, @YeuCauUngVien, @QuyenLoi, @LuongToiThieu, 
                                @LuongToiDa, @HanNopHoSo, 'ChoDuyet', GETDATE(), GETDATE())";

                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        cmd.Parameters.AddWithValue("@TieuDe", dto.TieuDe);
                        cmd.Parameters.AddWithValue("@MaNganhNghe", (object?)dto.MaNganhNghe ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaViTri", (object?)dto.MaViTri ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaCapBac", (object?)dto.MaCapBac ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaTrinhDoHocVan", (object?)dto.MaTrinhDoHocVan ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaDiaDiem", (object?)dto.MaDiaDiem ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DiaChiLamViec", (object?)dto.DiaChiLamViec ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HinhThucLamViec", (object?)dto.HinhThucLamViec ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoNamKinhNghiemToiThieu", (object?)dto.SoNamKinhNghiemToiThieu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoLuongTuyen", (object?)dto.SoLuongTuyen ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MoTaCongViec", (object?)dto.MoTaCongViec ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@YeuCauUngVien", (object?)dto.YeuCauUngVien ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@QuyenLoi", (object?)dto.QuyenLoi ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LuongToiThieu", (object?)dto.LuongToiThieu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LuongToiDa", (object?)dto.LuongToiDa ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HanNopHoSo", (object?)dto.HanNopHoSo ?? DBNull.Value);

                        maTin = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }

                    if (dto.DanhSachMaKyNang != null && dto.DanhSachMaKyNang.Any())
                    {
                        foreach (var maKyNang in dto.DanhSachMaKyNang)
                        {
                            string insertSkill = "INSERT INTO KyNangTinTuyenDung (MaTinTuyenDung, MaKyNang) VALUES (@MaTin, @MaKyNang)";
                            using (var cmd = new SqlCommand(insertSkill, connection))
                            {
                                cmd.Parameters.AddWithValue("@MaTin", maTin);
                                cmd.Parameters.AddWithValue("@MaKyNang", maKyNang);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Tạo tin tuyển dụng thành công. Đang chờ duyệt." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UpdateJobStatusAsync(int maTaiKhoan, int maTinTuyenDung, string newStatus)
        {
            try
            {
                if (newStatus != "TamDung" && newStatus != "DaDong")
                    return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Trạng thái không hợp lệ." };

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);

                    string updateQuery = "UPDATE TinTuyenDung SET TrangThaiTin = @Status WHERE MaTinTuyenDung = @MaTin AND MaDoanhNghiep = @MaDoanhNghiep";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Status", newStatus);
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Tin không tồn tại hoặc bạn không có quyền sửa." };
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật trạng thái thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }
    }
}
