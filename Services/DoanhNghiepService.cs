using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.DoanhNghiep;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Services
{
    public class DoanhNghiepService : IDoanhNghiepService
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        public DoanhNghiepService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _env = env;
        }

        private async Task<int> GetOrCreateMaDoanhNghiepAsync(SqlConnection connection, int maTaiKhoan)
        {
            string checkQuery = "SELECT MaDoanhNghiep FROM HoSoDoanhNghiep WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(checkQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value) return Convert.ToInt32(result);
            }

            string insertQuery = "INSERT INTO HoSoDoanhNghiep (MaTaiKhoan) OUTPUT INSERTED.MaDoanhNghiep VALUES (@MaTaiKhoan)";
            using (var cmd = new SqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task<ApiResponse<object>> GetCompanyByIdAsync(int maDoanhNghiep)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var congTy = new Dictionary<string, object>();
                    string queryHoSo = "SELECT * FROM HoSoDoanhNghiep WHERE MaDoanhNghiep = @MaDoanhNghiep";
                    using (var cmd = new SqlCommand(queryHoSo, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    congTy[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                            }
                            else return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Không tìm thấy doanh nghiệp" };
                        }
                    }

                    var tinTuyenDung = new List<Dictionary<string, object>>();
                    string queryTin = "SELECT * FROM TinTuyenDung WHERE MaDoanhNghiep = @MaDoanhNghiep AND TrangThaiTin = 'DaDuyet' AND (HanNopHoSo >= GETDATE() OR HanNopHoSo IS NULL)";
                    using (var cmd = new SqlCommand(queryTin, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                tinTuyenDung.Add(row);
                            }
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công", Data = new { CongTy = congTy, TinTuyenDung = tinTuyenDung } };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetProfileAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetOrCreateMaDoanhNghiepAsync(connection, maTaiKhoan);

                    var hoSo = new Dictionary<string, object>();
                    string query = "SELECT * FROM HoSoDoanhNghiep WHERE MaDoanhNghiep = @MaDoanhNghiep";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                    hoSo[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                            }
                        }
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Thành công", Data = hoSo };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        private async Task<string?> SaveFileAsync(IFormFile? file, string folderName)
        {
            if (file == null || file.Length == 0) return null;
            string uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public async Task<ApiResponse<object>> UpdateProfileAsync(int maTaiKhoan, UpdateDoanhNghiepProfileDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maDoanhNghiep = await GetOrCreateMaDoanhNghiepAsync(connection, maTaiKhoan);

                    string? logoPath = await SaveFileAsync(dto.Logo, "logos");
                    string? licensePath = await SaveFileAsync(dto.GiayPhepKinhDoanh, "licenses");

                    string updateQuery = @"
                        UPDATE HoSoDoanhNghiep 
                        SET TenCongTy = @TenCongTy, MaSoThue = @MaSoThue, QuyMo = @QuyMo, MoTa = @MoTa, 
                            MaDiaDiem = @MaDiaDiem, DiaChiChiTiet = @DiaChiChiTiet, DuongDanWebsite = @DuongDanWebsite,
                            TrangThaiXacThuc = 'ChoDuyet'";

                    if (logoPath != null) updateQuery += ", DuongDanLogo = @DuongDanLogo";
                    if (licensePath != null) updateQuery += ", GiayPhepKinhDoanh = @GiayPhepKinhDoanh";
                    
                    updateQuery += " WHERE MaDoanhNghiep = @MaDoanhNghiep";

                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        cmd.Parameters.AddWithValue("@TenCongTy", dto.TenCongTy);
                        cmd.Parameters.AddWithValue("@MaSoThue", (object?)dto.MaSoThue ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@QuyMo", (object?)dto.QuyMo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MoTa", (object?)dto.MoTa ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaDiaDiem", (object?)dto.MaDiaDiem ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DiaChiChiTiet", (object?)dto.DiaChiChiTiet ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DuongDanWebsite", (object?)dto.DuongDanWebsite ?? DBNull.Value);
                        
                        if (logoPath != null) cmd.Parameters.AddWithValue("@DuongDanLogo", logoPath);
                        if (licensePath != null) cmd.Parameters.AddWithValue("@GiayPhepKinhDoanh", licensePath);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Cập nhật thành công, đang chờ duyệt." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }
    }
}
