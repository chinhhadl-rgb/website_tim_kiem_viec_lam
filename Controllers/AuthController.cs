using Microsoft.AspNetCore.Mvc;
using RecruitmentSystem.DTOs.Auth;
using RecruitmentSystem.Models;
using RecruitmentSystem.Services;

namespace RecruitmentSystem.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 1. Đăng ký tài khoản mới (NhaTuyenDung | UngVien) và sinh mã OTP xác thực
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu đăng ký không hợp lệ.",
                    Data = null,
                    Errors = errors
                });
            }

            var result = await _authService.RegisterAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// 2. Xác thực mã OTP 6 số để kích hoạt tài khoản
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu xác thực OTP không hợp lệ.",
                    Data = null,
                    Errors = errors
                });
            }

            var result = await _authService.VerifyOtpAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// 3. Đăng nhập và nhận JWT Bearer Token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu đăng nhập không hợp lệ.",
                    Data = null,
                    Errors = errors
                });
            }

            var result = await _authService.LoginAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// 4. Gửi lại mã OTP qua Email
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Email không được để trống."
                });
            }

            var result = await _authService.ResendOtpAsync(email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("quen-mat-khau")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu không hợp lệ.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
                });
            }

            var result = await _authService.ForgotPasswordAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("dat-lai-mat-khau")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Dữ liệu không hợp lệ.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray()
                });
            }

            var result = await _authService.ResetPasswordAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
