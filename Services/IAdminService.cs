using RecruitmentSystem.DTOs.Admin;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IAdminService
    {
        Task<ApiResponse<object>> GetPendingCompaniesAsync();
        Task<ApiResponse<object>> ThamDinhDoanhNghiepAsync(int maTaiKhoanAdmin, int maDoanhNghiep, ThamDinhDoanhNghiepDto dto);
        Task<ApiResponse<object>> KiemDuyetTinAsync(int maTaiKhoanAdmin, int maTinTuyenDung, KiemDuyetTinDto dto);
        Task<ApiResponse<object>> GetStatisticsAsync();
        Task<ApiResponse<object>> BackupDatabaseAsync(SaoLuuDto dto);
    }
}
