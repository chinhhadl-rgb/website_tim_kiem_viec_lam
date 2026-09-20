using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Services
{
    public class CVService : ICVService
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        public CVService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _env = env;
        }

        private async Task<int> GetOrCreateMaUngVienAsync(SqlConnection connection, int maTaiKhoan)
        {
            string checkQuery = "SELECT MaUngVien FROM HoSoUngVien WHERE MaTaiKhoan = @MaTaiKhoan";
            using (var cmd = new SqlCommand(checkQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value) return Convert.ToInt32(result);
            }

            string insertQuery = "INSERT INTO HoSoUngVien (MaTaiKhoan) OUTPUT INSERTED.MaUngVien VALUES (@MaTaiKhoan)";
            using (var cmd = new SqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                return Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task<ApiResponse<object>> GetMyCVsAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string query = "SELECT * FROM CVUngVien WHERE MaUngVien = @MaUngVien";
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
                                {
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                }
                                results.Add(row);
                            }
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Lấy danh sách CV thành công.", Data = results };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> UploadCVAsync(int maTaiKhoan, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Không có file được chọn." };

                if (file.Length > 5 * 1024 * 1024)
                    return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dung lượng file không được vượt quá 5MB." };

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(extension))
                    return new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Chỉ chấp nhận file PDF hoặc Word." };

                string loaiCV = extension == ".pdf" ? "PDF_TaiLen" : "Word_TaiLen";
                
                // Ensure upload directory exists
                string uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads", "cvs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                string dbFilePath = $"/uploads/cvs/{uniqueFileName}";

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string insertQuery = @"
                        INSERT INTO CVUngVien (MaUngVien, TieuDeCV, LoaiCV, DuongDanFile, NgayTao) 
                        VALUES (@MaUngVien, @TieuDeCV, @LoaiCV, @DuongDanFile, GETDATE())";

                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@TieuDeCV", file.FileName);
                        cmd.Parameters.AddWithValue("@LoaiCV", loaiCV);
                        cmd.Parameters.AddWithValue("@DuongDanFile", dbFilePath);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Upload CV thành công.", Data = new { FilePath = dbFilePath } };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống khi upload file.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> CreateCvTrucTuyenAsync(int maTaiKhoan, RecruitmentSystem.DTOs.CV.CreateCvTrucTuyenDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string insertQuery = @"
                        INSERT INTO CVUngVien (MaUngVien, TieuDeCV, LoaiCV, CVTrucTuyen, NgayTao) 
                        VALUES (@MaUngVien, @TieuDeCV, 'Mau_TrucTuyen', @CVTrucTuyen, GETDATE())";

                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        cmd.Parameters.AddWithValue("@TieuDeCV", dto.TieuDeCV);
                        cmd.Parameters.AddWithValue("@CVTrucTuyen", dto.CVTrucTuyen);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Tạo CV trực tuyến thành công." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống khi tạo CV trực tuyến.", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> DeleteCVAsync(int maTaiKhoan, int maCV)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    int maUngVien = await GetOrCreateMaUngVienAsync(connection, maTaiKhoan);

                    string selectQuery = "SELECT DuongDanFile FROM CVUngVien WHERE MaCV = @MaCV AND MaUngVien = @MaUngVien";
                    string? duongDanFile = null;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaCV", maCV);
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null) 
                            return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "CV không tồn tại hoặc không có quyền xóa." };
                        
                        duongDanFile = result.ToString();
                    }

                    // Delete DB record
                    string deleteQuery = "DELETE FROM CVUngVien WHERE MaCV = @MaCV AND MaUngVien = @MaUngVien";
                    using (var cmd = new SqlCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaCV", maCV);
                        cmd.Parameters.AddWithValue("@MaUngVien", maUngVien);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Delete physical file
                    if (!string.IsNullOrEmpty(duongDanFile))
                    {
                        // dbFilePath is like /uploads/cvs/filename.pdf
                        string relativePath = duongDanFile.TrimStart('/'); // uploads/cvs/filename.pdf
                        string physicalPath = Path.Combine(_env.ContentRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (File.Exists(physicalPath))
                        {
                            File.Delete(physicalPath);
                        }
                    }

                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Xóa CV thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống khi xóa CV.", Errors = new[] { ex.Message } };
            }
        }
    }
}
