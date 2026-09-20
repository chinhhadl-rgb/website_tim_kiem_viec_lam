using Microsoft.AspNetCore.Http;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface ICVService
    {
        Task<ApiResponse<object>> GetMyCVsAsync(int maTaiKhoan);
        Task<ApiResponse<object>> UploadCVAsync(int maTaiKhoan, IFormFile file);
        Task<ApiResponse<object>> CreateCvTrucTuyenAsync(int maTaiKhoan, RecruitmentSystem.DTOs.CV.CreateCvTrucTuyenDto dto);
        Task<ApiResponse<object>> DeleteCVAsync(int maTaiKhoan, int maCV);
    }
}
