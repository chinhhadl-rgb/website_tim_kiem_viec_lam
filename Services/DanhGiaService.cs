using Microsoft.Data.SqlClient;
using RecruitmentSystem.DTOs.DanhGia;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public class DanhGiaService : IDanhGiaService
    {
        private readonly string _connectionString;

        public DanhGiaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<ApiResponse<object>> CreateDanhGiaAsync(CreateDanhGiaDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string insertQuery = @"
                        INSERT INTO DanhGiaHaiChieu (MaDoanhNghiep, MaUngVien, MaUngTuyen, NguoiDanhGia, TieuDe, SoSao, NhanXet, NgayTao)
                        VALUES (@MaDoanhNghiep, @MaUngVien, @MaUngTuyen, @NguoiDanhGia, @TieuDe, @SoSao, @NhanXet, GETDATE())";
                        
                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", dto.MaDoanhNghiep);
                        cmd.Parameters.AddWithValue("@MaUngVien", dto.MaUngVien);
                        cmd.Parameters.AddWithValue("@MaUngTuyen", (object?)dto.MaUngTuyen ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@NguoiDanhGia", dto.NguoiDanhGia);
                        cmd.Parameters.AddWithValue("@TieuDe", (object?)dto.TieuDe ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SoSao", dto.SoSao);
                        cmd.Parameters.AddWithValue("@NhanXet", (object?)dto.NhanXet ?? DBNull.Value);
                        
                        await cmd.ExecuteNonQueryAsync();
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 201, Message = "Gửi đánh giá thành công." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }

        public async Task<ApiResponse<object>> GetDanhGiaByDoanhNghiepAsync(int maDoanhNghiep)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var list = new List<Dictionary<string, object>>();
                    decimal avgStar = 0;
                    int totalReviews = 0;
                    int totalStars = 0;

                    string query = "SELECT * FROM DanhGiaHaiChieu WHERE MaDoanhNghiep = @MaDoanhNghiep AND NguoiDanhGia = 'Candidate' ORDER BY NgayTao DESC";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaDoanhNghiep", maDoanhNghiep);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                                
                                int stars = Convert.ToInt32(row["SoSao"]);
                                totalStars += stars;
                                totalReviews++;
                                
                                list.Add(row);
                            }
                        }
                    }

                    if (totalReviews > 0)
                    {
                        avgStar = Math.Round((decimal)totalStars / totalReviews, 1);
                    }

                    return new ApiResponse<object> { 
                        Success = true, 
                        StatusCode = 200, 
                        Message = "Thành công", 
                        Data = new {
                            TrungBinhSao = avgStar,
                            TongDanhGia = totalReviews,
                            DanhSach = list
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }
    }
}
