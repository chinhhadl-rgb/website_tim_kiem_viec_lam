using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.UngTuyen;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Services
{
    public class UngTuyenService : IUngTuyenService
    {
        private readonly string _connectionString;

        public UngTuyenService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        private async Task<int> GetMaUngVienAsync(SqlConnection conn, int maTaiKhoan)
        {
            string query = "SELECT MaUngVien FROM HoSoUngVien WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        private async Task<int> GetMaDoanhNghiepAsync(SqlConnection conn, int maTaiKhoan)
        {
            string query = "SELECT MaDoanhNghiep FROM HoSoDoanhNghiep WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<ApiResponse<object>> ApplyJobAsync(int maTaiKhoan, ApplyJobDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetMaUngVienAsync(connection, maTaiKhoan);
                    if (maUngVien == 0) return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Chưa có hồ sơ ứng viên." };

                    // Kiểm tra tồn tại
                    string checkDupQuery = "SELECT COUNT(1) FROM HoSoUngTuyen WHERE MaUngVien = @MaUngVien AND MaTinTuyenDung = @MaTin";
                    using (var cmd = new SqlCommand(checkDupQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@MaTin", dto.MaTinTuyenDung);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count > 0) return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Bạn đã nộp hồ sơ cho tin tuyển dụng này rồi." };
                    }

                    // Kiểm tra trạng thái tin
                    string checkJobQuery = "SELECT TrangThaiTin, HanNopHoSo FROM TinTuyenDung WHERE MaTinTuyenDung = @MaTin";
                    using (var cmd = new SqlCommand(checkJobQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", dto.MaTinTuyenDung);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync()) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Tin tuyển dụng không tồn tại." };
                            string trangThai = reader.GetString(0);
                            DateTime? hanNop = reader.IsDBNull(1) ? null : reader.GetDateTime(1);
                            
                            if (trangThai != "DaDuyet") return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Tin tuyển dụng chưa được duyệt hoặc đã đóng." };
                            if (hanNop.HasValue && hanNop.Value < DateTime.Now.Date) return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Tin tuyển dụng đã hết hạn nộp hồ sơ." };
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string insertQuery = @"
                                INSERT INTO HoSoUngTuyen (MaTinTuyenDung, MaUngVien, MaCV, ThuGioiThieu, TrangThaiUngTuyen, NgayNop)
                                OUTPUT INSERTED.MaUngTuyen
                                VALUES (@MaTin, @MaUngVien, @MaCV, @ThuGioiThieu, 'DaNop', GETDATE())";
                                
                            int maUngTuyen;
                            using (var cmd = new SqlCommand(insertQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaTin", dto.MaTinTuyenDung);
                                cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                                cmd.Parameters.AddWithValue("@MaCV", dto.MaCV);
                                cmd.Parameters.AddWithValue("@ThuGioiThieu", (object?)dto.ThuGioiThieu ?? DBNull.Value);
                                maUngTuyen = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            }

                            string logQuery = @"
                                INSERT INTO LichSuTrangThaiUngTuyen (MaUngTuyen, TrangThaiChuyen, NgayCapNhat, NguoiCapNhat, GhiChu)
                                VALUES (@MaUngTuyen, 'DaNop', GETDATE(), @MaTaiKhoan, N'Ứng viên nộp hồ sơ')";
                                
                            using (var cmd = new SqlCommand(logQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Nộp hồ sơ thành công." };
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetCandidateHistoryAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetMaUngVienAsync(connection, maTaiKhoan);

                    string query = @"
                        SELECT u.*, t.TieuDe, d.TenCongTy
                        FROM HoSoUngTuyen u
                        JOIN TinTuyenDung t ON u.MaTinTuyenDung = t.MaTinTuyenDung
                        JOIN HoSoDoanhNghiep d ON t.MaDoanhNghiep = d.MaDoanhNghiep
                        WHERE u.MaUngVien = @MaUngVien
                        ORDER BY u.NgayNop DESC";

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
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công", Data = results };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetJobApplicationsAsync(int maTaiKhoan, int maTinTuyenDung)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);

                    // Validate job belongs to company
                    string checkQuery = "SELECT COUNT(1) FROM TinTuyenDung WHERE MaTinTuyenDung = @MaTin AND MaDoanhNghiep = @MaDoanhNghiep";
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 403, Message = "Không có quyền xem tin này." };
                    }

                    string query = @"
                        SELECT u.*, v.HoTen, v.DuongDanAvatar, c.DuongDanFile
                        FROM HoSoUngTuyen u
                        JOIN HoSoUngVien v ON u.MaUngVien = v.MaUngVien
                        LEFT JOIN CVUngVien c ON u.MaCV = c.MaCV
                        WHERE u.MaTinTuyenDung = @MaTin
                        ORDER BY u.NgayNop DESC";

                    var results = new List<Dictionary<string, object>>();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
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
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công", Data = results };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UpdateApplicationStatusAsync(int maTaiKhoan, int maUngTuyen, UpdateTrangThaiUngTuyenDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);

                    // Check permission
                    string checkQuery = @"
                        SELECT COUNT(1) 
                        FROM HoSoUngTuyen u
                        JOIN TinTuyenDung t ON u.MaTinTuyenDung = t.MaTinTuyenDung
                        WHERE u.MaUngTuyen = @MaUngTuyen AND t.MaDoanhNghiep = @MaDoanhNghiep";
                        
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 403, Message = "Không có quyền cập nhật." };
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateQuery = "UPDATE HoSoUngTuyen SET TrangThaiUngTuyen = @Status WHERE MaUngTuyen = @MaUngTuyen";
                            using (var cmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Status", dto.TrangThaiUngTuyen);
                                cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            string logQuery = @"
                                INSERT INTO LichSuTrangThaiUngTuyen (MaUngTuyen, TrangThaiChuyen, NgayCapNhat, NguoiCapNhat, GhiChu)
                                VALUES (@MaUngTuyen, @Status, GETDATE(), @MaTaiKhoan, @GhiChu)";
                            using (var cmd = new SqlCommand(logQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                cmd.Parameters.AddWithValue("@Status", dto.TrangThaiUngTuyen);
                                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                cmd.Parameters.AddWithValue("@GhiChu", (object?)dto.GhiChu ?? DBNull.Value);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật trạng thái thành công." };
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }
    }
}
