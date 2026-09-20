using RecruitmentSystem.DTOs.UngTuyen;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IUngTuyenService
    {
        Task<ApiResponse<object>> ApplyJobAsync(int maTaiKhoan, ApplyJobDto dto);
        Task<ApiResponse<object>> GetCandidateHistoryAsync(int maTaiKhoan);
        Task<ApiResponse<object>> GetJobApplicationsAsync(int maTaiKhoan, int maTinTuyenDung);
        Task<ApiResponse<object>> UpdateApplicationStatusAsync(int maTaiKhoan, int maUngTuyen, UpdateTrangThaiUngTuyenDto dto);
    }
}
