using Microsoft.Data.SqlClient;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public class ThongBaoService : IThongBaoService
    {
        private readonly string _connectionString;

        public ThongBaoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<ApiResponse<object>> GetNotificationsAsync(int maTaiKhoan)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM ThongBao WHERE MaTaiKhoan = @MaTaiKhoan ORDER BY NgayTao DESC";
                    
                    var results = new List<Dictionary<string, object>>();
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
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

        public async Task<ApiResponse<object>> MarkAsReadAsync(int maTaiKhoan, int maThongBao)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE ThongBao SET DaDoc = 1 WHERE MaThongBao = @MaThongBao AND MaTaiKhoan = @MaTaiKhoan";
                    
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaThongBao", maThongBao);
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return new ApiResponse<object> { Success = false, StatusCode = 404, Message = "Thông báo không tồn tại hoặc không có quyền." };
                    }
                    return new ApiResponse<object> { Success = true, StatusCode = 200, Message = "Đã đánh dấu đọc." };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, StatusCode = 500, Message = "Lỗi hệ thống", Errors = new[] { ex.Message } };
            }
        }
    }
}
