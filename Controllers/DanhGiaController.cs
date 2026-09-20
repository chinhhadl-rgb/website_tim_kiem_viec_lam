using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.DanhGia;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/danh-gia")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly IDanhGiaService _service;

        public DanhGiaController(IDanhGiaService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateDanhGia([FromBody] CreateDanhGiaDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object> { Success = false, StatusCode = 400, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }
            var result = await _service.CreateDanhGiaAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("doanh-nghiep/{id}")]
        public async Task<IActionResult> GetDanhGiaByDoanhNghiep(int id)
        {
            var result = await _service.GetDanhGiaByDoanhNghiepAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
