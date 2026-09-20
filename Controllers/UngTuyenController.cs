using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.UngTuyen;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/ung-tuyen")]
    [ApiController]
    public class UngTuyenController : ControllerBase
    {
        private readonly IUngTuyenService _service;

        public UngTuyenController(IUngTuyenService service)
        {
            _service = service;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpPost]
        [Authorize(Roles = "UngVien")]
        public async Task<IActionResult> ApplyJob([FromBody] ApplyJobDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.ApplyJobAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("lich-su")]
        [Authorize(Roles = "UngVien")]
        public async Task<IActionResult> GetCandidateHistory()
        {
            var result = await _service.GetCandidateHistoryAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("tin/{maTin}")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> GetJobApplications(int maTin)
        {
            var result = await _service.GetJobApplicationsAsync(GetMaTaiKhoan(), maTin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/trang-thai")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateTrangThaiUngTuyenDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.UpdateApplicationStatusAsync(GetMaTaiKhoan(), id, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
