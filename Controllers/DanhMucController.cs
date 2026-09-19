using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RecruitmentSystem.Models;
using System.Data;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly string _connectionString;

        public DanhMucController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpGet("tinh")]
        public IActionResult GetDanhSachTinh()
        {
            var resultList = new List<object>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_LayDanhSachTinh", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultList.Add(new
                                {
                                    MaDanhMuc = reader["MaDanhMuc"],
                                    TenDanhMuc = reader["TenDanhMuc"]?.ToString()
                                });
                            }
                        }
                    }
                }

                var response = new ApiResponse<List<object>>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Lấy danh sách tỉnh thành công",
                    Data = resultList,
                    Errors = null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi lấy danh sách tỉnh",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("tinh/{maTinh}/quan-huyen")]
        public IActionResult GetQuanHuyenTheoTinh(int maTinh)
        {
            var resultList = new List<object>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("sp_LayQuanHuyenTheoTinh", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@MaTinh", maTinh);
                        
                        connection.Open();
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultList.Add(new
                                {
                                    MaDanhMuc = reader["MaDanhMuc"],
                                    TenDanhMuc = reader["TenDanhMuc"]?.ToString()
                                });
                            }
                        }
                    }
                }

                var response = new ApiResponse<List<object>>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Lấy danh sách quận huyện thành công",
                    Data = resultList,
                    Errors = null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi lấy danh sách quận huyện",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
