using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.UngVien;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;
using System.Security.Claims;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/ung-vien")]
    [ApiController]
    [Authorize(Roles = "UngVien")]
    public class UngVienController : ControllerBase
    {
        private readonly IUngVienService _ungVienService;

        public UngVienController(IUngVienService ungVienService)
        {
            _ungVienService = ungVienService;
        }

        private int GetMaTaiKhoan()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        private IActionResult InvalidModelStateResult()
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

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _ungVienService.GetProfileAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateHoSoUngVienDto dto)
        {
            if (!ModelState.IsValid) return InvalidModelStateResult();
            var result = await _ungVienService.UpdateProfileAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("kinh-nghiem")]
        public async Task<IActionResult> AddKinhNghiem([FromBody] KinhNghiemLamViecDto dto)
        {
            if (!ModelState.IsValid) return InvalidModelStateResult();
            var result = await _ungVienService.AddKinhNghiemAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("kinh-nghiem/{id}")]
        public async Task<IActionResult> DeleteKinhNghiem(int id)
        {
            var result = await _ungVienService.DeleteKinhNghiemAsync(GetMaTaiKhoan(), id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("ky-nang")]
        public async Task<IActionResult> UpdateKyNang([FromBody] KyNangUngVienDto dto)
        {
            if (!ModelState.IsValid) return InvalidModelStateResult();
            var result = await _ungVienService.UpdateKyNangAsync(GetMaTaiKhoan(), dto);
            return StatusCode(result.StatusCode, result);
        }

        #region ViecLamDaLuu
        [HttpPost("viec-lam-da-luu/{maTin}")]
        public async Task<IActionResult> SaveJob(int maTin)
        {
            var result = await _ungVienService.SaveJobAsync(GetMaTaiKhoan(), maTin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("viec-lam-da-luu/{maTin}")]
        public async Task<IActionResult> UnsaveJob(int maTin)
        {
            var result = await _ungVienService.UnsaveJobAsync(GetMaTaiKhoan(), maTin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("viec-lam-da-luu")]
        public async Task<IActionResult> GetSavedJobs()
        {
            var result = await _ungVienService.GetSavedJobsAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }
        #endregion

        #region TheoDoiDoanhNghiep
        [HttpPost("theo-doi-doanh-nghiep/{maDN}")]
        public async Task<IActionResult> FollowCompany(int maDN)
        {
            var result = await _ungVienService.FollowCompanyAsync(GetMaTaiKhoan(), maDN);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("theo-doi-doanh-nghiep/{maDN}")]
        public async Task<IActionResult> UnfollowCompany(int maDN)
        {
            var result = await _ungVienService.UnfollowCompanyAsync(GetMaTaiKhoan(), maDN);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("theo-doi-doanh-nghiep")]
        public async Task<IActionResult> GetFollowedCompanies()
        {
            var result = await _ungVienService.GetFollowedCompaniesAsync(GetMaTaiKhoan());
            return StatusCode(result.StatusCode, result);
        }
        #endregion
    }
}
