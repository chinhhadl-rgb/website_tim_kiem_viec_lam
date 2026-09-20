using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.LichPhongVan;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/lich-phong-van")]
    [ApiController]
    public class LichPhongVanController : ControllerBase
    {
        private readonly ILichPhongVanService _service;

        public LichPhongVanController(ILichPhongVanService service)
        {
            _service = service;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpPost]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateLichPhongVanDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.CreateScheduleAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/phan-hoi")]
        [Authorize(Roles = "UngVien")]
        public async Task<IActionResult> RespondToSchedule(int id, [FromBody] PhanHoiLichDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.RespondToScheduleAsync(GetMaTaiKhoan(), id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/ket-qua")]
        [Authorize(Roles = "NhaTuyenDung")]
        public async Task<IActionResult> InputInterviewResult(int id, [FromBody] KetQuaPhongVanDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.InputInterviewResultAsync(GetMaTaiKhoan(), id, dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
