using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.LichPhongVan;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public class LichPhongVanService : ILichPhongVanService
    {
        private readonly string _connectionString;

        public LichPhongVanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
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

        public async Task<ApiResponse<object>> CreateScheduleAsync(int maTaiKhoan, CreateLichPhongVanDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);

                    // Check if Employer owns this application
                    string checkQuery = @"
                        SELECT COUNT(1) 
                        FROM HoSoUngTuyen u
                        JOIN TinTuyenDung t ON u.MaTinTuyenDung = t.MaTinTuyenDung
                        WHERE u.MaUngTuyen = @MaUngTuyen AND t.MaDoanhNghiep = @MaDoanhNghiep";
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngTuyen", dto.MaUngTuyen);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 403, Message = "Không có quyền tạo lịch cho ứng viên này." };
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string insertQuery = @"
                                INSERT INTO LichPhongVan (MaUngTuyen, ThoiGianPhongVan, HinhThuc, DiaDiemHoacDuongDan, TrangThaiLich, NgayTao)
                                VALUES (@MaUngTuyen, @ThoiGian, @HinhThuc, @DiaDiem, 'DaLenLich', GETDATE())";
                            
                            using (var cmd = new SqlCommand(insertQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaUngTuyen", dto.MaUngTuyen);
                                cmd.Parameters.AddWithValue("@ThoiGian", dto.ThoiGianPhongVan);
                                cmd.Parameters.AddWithValue("@HinhThuc", dto.HinhThuc);
                                cmd.Parameters.AddWithValue("@DiaDiem", dto.DiaDiemHoacDuongDan);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            string updateStatus = "UPDATE HoSoUngTuyen SET TrangThaiUngTuyen = 'MoiPhongVan' WHERE MaUngTuyen = @MaUngTuyen";
                            using (var cmd = new SqlCommand(updateStatus, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaUngTuyen", dto.MaUngTuyen);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            string logQuery = @"
                                INSERT INTO LichSuTrangThaiUngTuyen (MaUngTuyen, TrangThaiChuyen, NgayCapNhat, NguoiCapNhat, GhiChu)
                                VALUES (@MaUngTuyen, 'MoiPhongVan', GETDATE(), @MaTaiKhoan, N'Nhà tuyển dụng đã lên lịch phỏng vấn')";
                            using (var cmd = new SqlCommand(logQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@MaUngTuyen", dto.MaUngTuyen);
                                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Tạo lịch phỏng vấn thành công." };
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

        public async Task<ApiResponse<object>> RespondToScheduleAsync(int maTaiKhoan, int maLichPhongVan, PhanHoiLichDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetMaUngVienAsync(connection, maTaiKhoan);

                    // Check if Candidate owns this schedule
                    string checkQuery = @"
                        SELECT COUNT(1) 
                        FROM LichPhongVan l
                        JOIN HoSoUngTuyen u ON l.MaUngTuyen = u.MaUngTuyen
                        WHERE l.MaLichPhongVan = @MaLich AND u.MaUngVien = @MaUngVien";
                    
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaLich", maLichPhongVan);
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        if (Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0)
                            return new ApiResponse<object> { Success = false, StatusCode = 403, Message = "Không tìm thấy lịch hoặc không có quyền." };
                    }

                    string updateQuery = @"
                        UPDATE LichPhongVan 
                        SET PhanHoiUngVien = @PhanHoi, LyDoDoiLich = @LyDo 
                        WHERE MaLichPhongVan = @MaLich";
                    
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@PhanHoi", dto.PhanHoiUngVien);
                        cmd.Parameters.AddWithValue("@LyDo", (object?)dto.LyDoDoiLich ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaLich", maLichPhongVan);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Phản hồi lịch phỏng vấn thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> InputInterviewResultAsync(int maTaiKhoan, int maLichPhongVan, KetQuaPhongVanDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetMaDoanhNghiepAsync(connection, maTaiKhoan);

                    // Check ownership
                    string checkQuery = @"
                        SELECT l.MaUngTuyen 
                        FROM LichPhongVan l
                        JOIN HoSoUngTuyen u ON l.MaUngTuyen = u.MaUngTuyen
                        JOIN TinTuyenDung t ON u.MaTinTuyenDung = t.MaTinTuyenDung
                        WHERE l.MaLichPhongVan = @MaLich AND t.MaDoanhNghiep = @MaDoanhNghiep";
                    
                    int maUngTuyen = 0;
                    using (var cmd = new SqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaLich", maLichPhongVan);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null) 
                            return new ApiResponse<object> { Success = false, StatusCode = 403, Message = "Không tìm thấy lịch hoặc không có quyền." };
                        maUngTuyen = Convert.ToInt32(result);
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateQuery = @"
                                UPDATE LichPhongVan 
                                SET DiemChuyenMon = @Diem, NhanXetNoiBo = @NhanXet, KetQuaNoiBo = @KetQua
                                WHERE MaLichPhongVan = @MaLich";
                            
                            using (var cmd = new SqlCommand(updateQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Diem", (object?)dto.DiemChuyenMon ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@NhanXet", (object?)dto.NhanXetNoiBo ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@KetQua", dto.KetQuaNoiBo);
                                cmd.Parameters.AddWithValue("@MaLich", maLichPhongVan);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            // Update HoSoUngTuyen Status based on Result if Dat/KhongDat
                            if (dto.KetQuaNoiBo == "Dat")
                            {
                                string updateStatus = "UPDATE HoSoUngTuyen SET TrangThaiUngTuyen = 'TrungTuyen' WHERE MaUngTuyen = @MaUngTuyen";
                                using (var cmd = new SqlCommand(updateStatus, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                                
                                string logQuery = @"
                                INSERT INTO LichSuTrangThaiUngTuyen (MaUngTuyen, TrangThaiChuyen, NgayCapNhat, NguoiCapNhat, GhiChu)
                                VALUES (@MaUngTuyen, 'TrungTuyen', GETDATE(), @MaTaiKhoan, N'Đã trúng tuyển sau phỏng vấn')";
                                using (var cmd = new SqlCommand(logQuery, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                    cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                            else if (dto.KetQuaNoiBo == "KhongDat")
                            {
                                string updateStatus = "UPDATE HoSoUngTuyen SET TrangThaiUngTuyen = 'TuChoi' WHERE MaUngTuyen = @MaUngTuyen";
                                using (var cmd = new SqlCommand(updateStatus, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                    await cmd.ExecuteNonQueryAsync();
                                }

                                string logQuery = @"
                                INSERT INTO LichSuTrangThaiUngTuyen (MaUngTuyen, TrangThaiChuyen, NgayCapNhat, NguoiCapNhat, GhiChu)
                                VALUES (@MaUngTuyen, 'TuChoi', GETDATE(), @MaTaiKhoan, N'Không trúng tuyển sau phỏng vấn')";
                                using (var cmd = new SqlCommand(logQuery, connection, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@MaUngTuyen", maUngTuyen);
                                    cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();
                            return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật kết quả phỏng vấn thành công." };
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
