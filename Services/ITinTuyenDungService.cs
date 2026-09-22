using RecruitmentSystem.DTOs.TinTuyenDung;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface ITinTuyenDungService
    {
        Task<ApiResponse<object>> GetTinTuyenDungsAsync(FilterTinTuyenDungDto filter);
        Task<ApiResponse<object>> GetJobByIdAsync(int maTinTuyenDung);
        Task<ApiResponse<object>> CreateJobAsync(int maTaiKhoan, CreateTinTuyenDungDto dto);
        Task<ApiResponse<object>> UpdateJobStatusAsync(int maTaiKhoan, int maTinTuyenDung, string newStatus);
    }
}
