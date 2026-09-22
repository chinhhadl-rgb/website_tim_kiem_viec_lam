using RecruitmentSystem.DTOs.Auth;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<object>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<object>> VerifyOtpAsync(VerifyOtpDto dto);
        Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<object>> ResendOtpAsync(string email);
        Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
