using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.UngVien;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Services
{
    public class UngVienService : IUngVienService
    {
        private readonly string _connectionString;

        public UngVienService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private async Task<int> GetOrCreateMaUngVienAsync(SqlConnection connection, int maTaiKhoan)
        {
            string checkQuery = "SELECT MaUngVien FROM HoSoUngVien WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(checkQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }

            string insertQuery = "INSERT INTO HoSoUngVien (MaTaiKhoan) OUTPUT INSERTED.MaUngVien VALUES (@MaTaiKhoan)";
            using (var cmd = new SqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task<ApiResponse<object>> GetProfileAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);
                    
                    var hoSo = new Dictionary<string, object>();
                    string queryHoSo = @"SELECT * FROM HoSoUngVien WHERE MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(queryHoSo, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    hoSo[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                }
                            }
                        }
                    }

                    var dsKinhNghiem = new List<Dictionary<string, object>>();
                    string queryKinhNghiem = "SELECT * FROM KinhNghiemLamViec WHERE MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(queryKinhNghiem, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                }
                                dsKinhNghiem.Add(row);
                            }
                        }
                    }

                    var dsKyNang = new List<Dictionary<string, object>>();
                    string queryKyNang = "SELECT * FROM KyNangUngVien WHERE MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(queryKyNang, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                }
                                dsKyNang.Add(row);
                            }
                        }
                    }

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Lấy hồ sơ ứng viên thành công",
                        Data = new {
                            HoSo = hoSo,
                            KinhNghiemLamViec = dsKinhNghiem,
                            KyNang = dsKyNang
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UpdateProfileAsync(int maTaiKhoan, UpdateHoSoUngVienDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    // Tính toán % hoàn thiện hồ sơ
                    int totalFields = 12;
                    int filledFields = 0;
                    if (!string.IsNullOrWhiteSpace(dto.HoTen)) filledFields++;
                    if (dto.NgaySinh.HasValue) filledFields++;
                    if (!string.IsNullOrWhiteSpace(dto.GioiTinh)) filledFields++;
                    if (dto.MaDiaDiem.HasValue) filledFields++;
                    if (!string.IsNullOrWhiteSpace(dto.DiaChiCuThe)) filledFields++;
                    if (dto.MaViTriMongMuon.HasValue) filledFields++;
                    if (!string.IsNullOrWhiteSpace(dto.GioiThieuBanThan)) filledFields++;
                    if (dto.MucLuongMongMuonMin.HasValue) filledFields++;
                    if (dto.MucLuongMongMuonMax.HasValue) filledFields++;
                    
                    bool isJsonEmpty(string? json) => string.IsNullOrWhiteSpace(json) || json == "[]" || json == "null" || json == "{}";
                    if (!isJsonEmpty(dto.DanhSachNgoaiNgu)) filledFields++;
                    if (!isJsonEmpty(dto.DanhSachChungChi)) filledFields++;
                    if (!isJsonEmpty(dto.DanhSachHocVan)) filledFields++;

                    int phanTramHoanThien = (int)Math.Round((double)filledFields / totalFields * 100);

                    string query = @"
                        UPDATE HoSoUngVien
                        SET HoTen = @HoTen, NgaySinh = @NgaySinh, GioiTinh = @GioiTinh, MaDiaDiem = @MaDiaDiem, 
                            DiaChiCuThe = @DiaChiCuThe, MaViTriMongMuon = @MaViTriMongMuon, GioiThieuBanThan = @GioiThieuBanThan,
                            ChoPhepNhaTuyenDung = @ChoPhepNhaTuyenDung, MucLuongMongMuonMin = @MucLuongMongMuonMin,
                            MucLuongMongMuonMax = @MucLuongMongMuonMax, DanhSachNgoaiNgu = @DanhSachNgoaiNgu,
                            DanhSachChungChi = @DanhSachChungChi, DanhSachHocVan = @DanhSachHocVan,
                            PhanTramHoanThien = @PhanTramHoanThien
                        WHERE MaUngVien = @MaUngVien";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@HoTen", dto.HoTen);
                        cmd.Parameters.AddWithValue("@NgaySinh", (object?)dto.NgaySinh ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GioiTinh", (object?)dto.GioiTinh ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaDiaDiem", (object?)dto.MaDiaDiem ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DiaChiCuThe", (object?)dto.DiaChiCuThe ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaViTriMongMuon", (object?)dto.MaViTriMongMuon ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GioiThieuBanThan", (object?)dto.GioiThieuBanThan ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ChoPhepNhaTuyenDung", dto.ChoPhepNhaTuyenDung);
                        cmd.Parameters.AddWithValue("@MucLuongMongMuonMin", (object?)dto.MucLuongMongMuonMin ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MucLuongMongMuonMax", (object?)dto.MucLuongMongMuonMax ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DanhSachNgoaiNgu", (object?)dto.DanhSachNgoaiNgu ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DanhSachChungChi", (object?)dto.DanhSachChungChi ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DanhSachHocVan", (object?)dto.DanhSachHocVan ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PhanTramHoanThien", phanTramHoanThien);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object> { 
                        Success = true, 
                        StatusCode = 200, 
                        Message = $"Cập nhật hồ sơ thành công. Mức độ hoàn thiện: {phanTramHoanThien}%", 
                        Data = dto 
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> AddKinhNghiemAsync(int maTaiKhoan, KinhNghiemLamViecDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = @"
                        INSERT INTO KinhNghiemLamViec (MaUngVien, TenCongTy, ChucDanh, NgayBatDau, NgayKetThuc, MoTaChiTiet, MaNganhNghe, MaViTri)
                        VALUES (@MaUngVien, @TenCongTy, @ChucDanh, @NgayBatDau, @NgayKetThuc, @MoTaChiTiet, @MaNganhNghe, @MaViTri)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@TenCongTy", dto.TenCongTy);
                        cmd.Parameters.AddWithValue("@ChucDanh", dto.ChucDanh);
                        cmd.Parameters.AddWithValue("@NgayBatDau", (object?)dto.NgayBatDau ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@NgayKetThuc", (object?)dto.NgayKetThuc ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MoTaChiTiet", (object?)dto.MoTaChiTiet ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaNganhNghe", (object?)dto.MaNganhNghe ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaViTri", (object?)dto.MaViTri ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Thêm kinh nghiệm làm việc thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> DeleteKinhNghiemAsync(int maTaiKhoan, int idKinhNghiem)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = "DELETE FROM KinhNghiemLamViec WHERE MaKinhNghiem = @MaKinhNghiem AND MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaKinhNghiem", idKinhNghiem);
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        int rows = await cmd.ExecuteNonQueryAsync();

                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Không tìm thấy kinh nghiệm hoặc không có quyền xóa." };
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Xóa kinh nghiệm làm việc thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UpdateKyNangAsync(int maTaiKhoan, KyNangUngVienDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = @"
                        IF EXISTS (SELECT 1 FROM KyNangUngVien WHERE MaUngVien = @MaUngVien AND MaKyNang = @MaKyNang)
                            UPDATE KyNangUngVien SET SoNamKinhNghiem = @SoNamKinhNghiem WHERE MaUngVien = @MaUngVien AND MaKyNang = @MaKyNang
                        ELSE
                            INSERT INTO KyNangUngVien (MaUngVien, MaKyNang, SoNamKinhNghiem) VALUES (@MaUngVien, @MaKyNang, @SoNamKinhNghiem)";

                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaKyNang", dto.MaKyNang);
                        cmd.Parameters.AddWithValue("@SoNamKinhNghiem", (object?)dto.SoNamKinhNghiem ?? DBNull.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật kỹ năng thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        #region ViecLamDaLuu
        public async Task<ApiResponse<object>> SaveJobAsync(int maTaiKhoan, int maTinTuyenDung)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string checkQuery = "SELECT COUNT(1) FROM ViecLamDaLuu WHERE MaUngVien = @MaUngVien AND MaTinTuyenDung = @MaTin";
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Bạn đã lưu tin tuyển dụng này rồi." };
                    }

                    string insertQuery = "INSERT INTO ViecLamDaLuu (MaUngVien, MaTinTuyenDung, NgayLuu) VALUES (@MaUngVien, @MaTin, GETDATE())";
                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Lưu tin tuyển dụng thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UnsaveJobAsync(int maTaiKhoan, int maTinTuyenDung)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string deleteQuery = "DELETE FROM ViecLamDaLuu WHERE MaUngVien = @MaUngVien AND MaTinTuyenDung = @MaTin";
                    using (var cmd = new SqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Chưa lưu tin này." };
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Bỏ lưu tin tuyển dụng thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetSavedJobsAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = @"
                        SELECT v.NgayLuu, t.*, d.TenCongTy, d.DuongDanLogo 
                        FROM ViecLamDaLuu v
                        JOIN TinTuyenDung t ON v.MaTinTuyenDung = t.MaTinTuyenDung
                        LEFT JOIN HoSoDoanhNghiep d ON t.MaDoanhNghiep = d.MaDoanhNghiep
                        WHERE v.MaUngVien = @MaUngVien
                        ORDER BY v.NgayLuu DESC";

                    var results = new List<Dictionary<string, object>>();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                results.Add(row);
                            }
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công.", Data = results };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }
        #endregion

        #region TheoDoiDoanhNghiep
        public async Task<ApiResponse<object>> FollowCompanyAsync(int maTaiKhoan, int maDoanhNghiep)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string checkQuery = "SELECT COUNT(1) FROM TheoDoiDoanhNghiep WHERE MaUngVien = @MaUngVien AND MaDoanhNghiep = @MaDN";
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaDN", maDoanhNghiep);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Bạn đã theo dõi doanh nghiệp này rồi." };
                    }

                    string insertQuery = "INSERT INTO TheoDoiDoanhNghiep (MaUngVien, MaDoanhNghiep, NgayTheoDoi) VALUES (@MaUngVien, @MaDN, GETDATE())";
                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaDN", maDoanhNghiep);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Theo dõi doanh nghiệp thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UnfollowCompanyAsync(int maTaiKhoan, int maDoanhNghiep)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string deleteQuery = "DELETE FROM TheoDoiDoanhNghiep WHERE MaUngVien = @MaUngVien AND MaDoanhNghiep = @MaDN";
                    using (var cmd = new SqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaDN", maDoanhNghiep);
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Chưa theo dõi doanh nghiệp này." };
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Bỏ theo dõi doanh nghiệp thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetFollowedCompaniesAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = @"
                        SELECT td.NgayTheoDoi, d.*
                        FROM TheoDoiDoanhNghiep td
                        JOIN HoSoDoanhNghiep d ON td.MaDoanhNghiep = d.MaDoanhNghiep
                        WHERE td.MaUngVien = @MaUngVien
                        ORDER BY td.NgayTheoDoi DESC";

                    var results = new List<Dictionary<string, object>>();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                results.Add(row);
                            }
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công.", Data = results };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }
        #endregion

        #region Delete Extensions
        public async Task<ApiResponse<bool>> DeleteKyNangAsync(int maTaiKhoan, int maKyNang)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = "DELETE FROM KyNangUngVien WHERE MaUngVien = @MaUngVien AND MaKyNang = @MaKyNang";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaKyNang", maKyNang);
                        int rows = await cmd.ExecuteNonQueryAsync();

                        if (rows == 0)
                        {
                            return new ApiResponse<bool> { Success = false, StatusCode = 404, Message = "Không tìm thấy kỹ năng này.", Data = false };
                        }
                    }
                    return new ApiResponse<bool> { Success = true, StatusCode = 200, Message = "Xóa kỹ năng thành công.", Data = true };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Data = false, Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<bool>> DeleteHocVanAsync(int maTaiKhoan, int idHocVan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    // Đọc DanhSachHocVan hiện tại (chuỗi JSON)
                    string querySelect = "SELECT DanhSachHocVan FROM HoSoUngVien WHERE MaUngVien = @MaUngVien";
                    string? currentJson = null;

                    using (var cmd = new SqlCommand(querySelect, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            currentJson = result.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(currentJson))
                    {
                        return new ApiResponse<bool> { Success = false, StatusCode = 404, Message = "Không tìm thấy học vấn.", Data = false };
                    }

                    // Sử dụng System.Text.Json.Nodes để parse JSON Array
                    var jsonArray = System.Text.Json.Nodes.JsonNode.Parse(currentJson) as System.Text.Json.Nodes.JsonArray;
                    if (jsonArray == null)
                    {
                        return new ApiResponse<bool> { Success = false, StatusCode = 404, Message = "Dữ liệu học vấn không hợp lệ.", Data = false };
                    }

                    bool found = false;
                    for (int i = 0; i < jsonArray.Count; i++)
                    {
                        var item = jsonArray[i];
                        if (item != null)
                        {
                            // Kiểm tra các field phổ biến thường dùng làm ID
                            var idNode = item["id"] ?? item["Id"] ?? item["maHocVan"] ?? item["MaHocVan"];
                            if (idNode != null && idNode.GetValue<int>() == idHocVan)
                            {
                                jsonArray.RemoveAt(i);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        return new ApiResponse<bool> { Success = false, StatusCode = 404, Message = "Không tìm thấy học vấn với ID này.", Data = false };
                    }

                    // Cập nhật lại vào DB
                    string updateQuery = "UPDATE HoSoUngVien SET DanhSachHocVan = @DanhSachHocVan WHERE MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@DanhSachHocVan", jsonArray.ToJsonString());
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<bool> { Success = true, StatusCode = 200, Message = "Xóa học vấn thành công.", Data = true };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Data = false, Errors = new[] { ex.Message } };
            }
        }
        #endregion
    }
}
