using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.TinTuyenDung;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/tin-tuyen-dung")]
    [ApiController]
    public class TinTuyenDungController : ControllerBase
    {
        private readonly ITinTuyenDungService _service;

        public TinTuyenDungController(ITinTuyenDungService service)
        {
            _service = service;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> FilterJobs([FromQuery] FilterTinTuyenDungDto filter)
        {
            var result = await _service.FilterJobsAsync(filter);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var result = await _service.GetJobByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> CreateJob([FromBody] CreateTinTuyenDungDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object>
                {
                    Success = false, StatusCode = 400, Message = "Dữ liệu đầu vào không hợp lệ", Errors = errors
                });
            }
            var result = await _service.CreateJobAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        public class UpdateStatusDto
        {
            public string Status { get; set; } = string.Empty;
        }

        [HttpPut("{id}/trang-thai")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> UpdateJobStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var result = await _service.UpdateJobStatusAsync(GetMaTaiKhoan(), id, dto.Status);
            return StatusCode(result.StatusCode, result);
        }
    }
}
