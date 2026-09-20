using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.Admin;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public class AdminService : IAdminService
    {
        private readonly string _connectionString;

        public AdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<ApiResponse<object>> GetPendingCompaniesAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM HoSoDoanhNghiep WHERE TrangThaiXacThuc = 'ChoDuyet'";
                    
                    var results = new List<Dictionary<string, object>>();
                    using (var cmd = new SqlCommand(query, connection))
                    {
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

        public async Task<ApiResponse<object>> ThamDinhDoanhNghiepAsync(int maTaiKhoanAdmin, int maDoanhNghiep, ThamDinhDoanhNghiepDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string updateQuery = @"
                        UPDATE HoSoDoanhNghiep 
                        SET TrangThaiXacThuc = @Status, LyDoTuChoi = @LyDo, NgayXacThuc = GETDATE(), NguoiXacThuc = @Admin
                        WHERE MaDoanhNghiep = @MaDoanhNghiep";
                    
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Status", dto.TrangThaiXacThuc);
                        cmd.Parameters.AddWithValue("@LyDo", (object?)dto.LyDoTuChoi ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Admin", maTaiKhoanAdmin);
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Không tìm thấy doanh nghiệp." };
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật thẩm định thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> KiemDuyetTinAsync(int maTaiKhoanAdmin, int maTinTuyenDung, KiemDuyetTinDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string updateQuery = @"
                        UPDATE TinTuyenDung 
                        SET TrangThaiTin = @Status, LyDoTuChoi = @LyDo, NgayDuyet = GETDATE(), NguoiDuyet = @Admin
                        WHERE MaTinTuyenDung = @MaTin";
                    
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Status", dto.TrangThaiTin);
                        cmd.Parameters.AddWithValue("@LyDo", (object?)dto.LyDoTuChoi ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Admin", maTaiKhoanAdmin);
                        cmd.Parameters.AddWithValue("@MaTin", maTinTuyenDung);
                        
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Không tìm thấy tin tuyển dụng." };
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Kiểm duyệt tin thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetStatisticsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int totalDN = 0, totalUV = 0, totalTin = 0, totalUngTuyen = 0;

                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM HoSoDoanhNghiep", connection))
                        totalDN = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM HoSoUngVien", connection))
                        totalUV = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM TinTuyenDung", connection))
                        totalTin = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM HoSoUngTuyen", connection))
                        totalUngTuyen = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    return new ApiResponse<object> {
                        Success = true,
                        StatusCode = 200,
                        Message = "Thành công",
                        Data = new {
                            TongDoanhNghiep = totalDN,
                            TongUngVien = totalUV,
                            TongTinTuyenDung = totalTin,
                            TongLuotUngTuyen = totalUngTuyen
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> BackupDatabaseAsync(SaoLuuDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Construct Backup File path
                    string fileName = $"He_Backup_{DateTime.Now:yyyyMMddHHmmss}.bak";
                    string fullPath = Path.Combine(dto.DuongDanThuMuc, fileName);

                    // Must use master or current db to trigger backup
                    string backupQuery = string.Empty;
                    if (dto.LoaiSaoLuu == "FULL")
                    {
                        backupQuery = $"BACKUP DATABASE He TO DISK = '{fullPath}'";
                    }
                    else if (dto.LoaiSaoLuu == "DIFFERENTIAL")
                    {
                        backupQuery = $"BACKUP DATABASE He TO DISK = '{fullPath}' WITH DIFFERENTIAL";
                    }
                    else if (dto.LoaiSaoLuu == "LOG")
                    {
                        fullPath = fullPath.Replace(".bak", ".trn");
                        backupQuery = $"BACKUP LOG He TO DISK = '{fullPath}'";
                    }
                    else
                    {
                        return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Loại sao lưu không hợp lệ." };
                    }

                    bool success = true;
                    string errorMsg = string.Empty;

                    try
                    {
                        using (var cmd = new SqlCommand(backupQuery, connection))
                        {
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (Exception dbEx)
                    {
                        success = false;
                        errorMsg = dbEx.Message;
                    }

                    // Save log to SaoLuuDuLieu table
                    string logQuery = @"
                        INSERT INTO SaoLuuDuLieu (TenFileSaoLuu, DuongDanFile, LoaiSaoLuu, NgaySaoLuu, ThanhCong, MoTaLoi)
                        VALUES (@TenFile, @DuongDan, @Loai, GETDATE(), @ThanhCong, @MoTaLoi)";
                        
                    using (var cmd = new SqlCommand(logQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@TenFile", fileName);
                        cmd.Parameters.AddWithValue("@DuongDan", fullPath);
                        cmd.Parameters.AddWithValue("@Loai", dto.LoaiSaoLuu);
                        cmd.Parameters.AddWithValue("@ThanhCong", success);
                        cmd.Parameters.AddWithValue("@MoTaLoi", (object?)errorMsg ?? DBNull.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    if (success)
                    {
                        return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Sao lưu thành công.", Data = fullPath };
                    }
                    else
                    {
                        return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi sao lưu.", Errors = new[] { errorMsg } };
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
