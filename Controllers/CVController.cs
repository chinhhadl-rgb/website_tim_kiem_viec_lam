using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.Services;
using System.Security.Claims;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/cv")]
    [ApiController]
    [Authorize(Roles = "UngVien")]
    public class CVController : ControllerBase
    {
        private readonly ICVService _cvService;

        public CVController(ICVService cvService)
        {
            _cvService = cvService;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCVs()
        {
            var result = await _cvService.GetMyCVsAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCV(IFormFile file)
        {
            var result = await _cvService.UploadCVAsync(GetMaTaiKhoan(), file);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("truc-tuyen")]
        public async Task<IActionResult> CreateCvTrucTuyen([FromBody] RecruitmentSystem.DTOs.CV.CreateCvTrucTuyenDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu đầu vào không hợp lệ.",
                    Errors = errors
                });
            }
            var result = await _cvService.CreateCvTrucTuyenAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCV(int id)
        {
            var result = await _cvService.DeleteCVAsync(GetMaTaiKhoan(), id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
