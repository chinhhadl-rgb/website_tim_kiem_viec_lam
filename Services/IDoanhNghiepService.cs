using RecruitmentSystem.DTOs.DoanhNghiep;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IDoanhNghiepService
    {
        Task<ApiResponse<object>> GetCompanyByIdAsync(int maDoanhNghiep);
        Task<ApiResponse<object>> GetProfileAsync(int maTaiKhoan);
        Task<ApiResponse<object>> UpdateProfileAsync(int maTaiKhoan, UpdateDoanhNghiepProfileDto dto);
    }
}
