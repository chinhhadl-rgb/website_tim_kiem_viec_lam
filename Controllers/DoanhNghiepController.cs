using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.DoanhNghiep;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/doanh-nghiep")]
    [ApiController]
    public class DoanhNghiepController : ControllerBase
    {
        private readonly IDoanhNghiepService _service;

        public DoanhNghiepController(IDoanhNghiepService service)
        {
            _service = service;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var result = await _service.GetCompanyByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("profile")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _service.GetProfileAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateDoanhNghiepProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object>
                {
                    Success = false, StatusCode = 400, Message = "Dữ liệu đầu vào không hợp lệ", Errors = errors
                });
            }
            var result = await _service.UpdateProfileAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
