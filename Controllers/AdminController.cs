using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.Admin;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;

        public AdminController(IAdminService service)
        {
            _service = service;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpGet("doanh-nghiep/cho-duyet")]
        public async Task<IActionResult> GetPendingCompanies()
        {
            var result = await _service.GetPendingCompaniesAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("doanh-nghiep/{id}/tham-dinh")]
        public async Task<IActionResult> ThamDinhDoanhNghiep(int id, [FromBody] ThamDinhDoanhNghiepDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.ThamDinhDoanhNghiepAsync(GetMaTaiKhoan(), id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("tin-tuyen-dung/{id}/kiem-duyet")]
        public async Task<IActionResult> KiemDuyetTinTuyenDung(int id, [FromBody] KiemDuyetTinDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.KiemDuyetTinAsync(GetMaTaiKhoan(), id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("thong-ke/tong-quan")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _service.GetStatisticsAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("sao-luu")]
        public async Task<IActionResult> BackupDatabase([FromBody] SaoLuuDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.BackupDatabaseAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
