using RecruitmentSystem.DTOs.UngVien;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IUngVienService
    {
        Task<ApiResponse<object>> GetProfileAsync(int maTaiKhoan);
        Task<ApiResponse<object>> UpdateProfileAsync(int maTaiKhoan, UpdateHoSoUngVienDto dto);
        Task<ApiResponse<object>> AddKinhNghiemAsync(int maTaiKhoan, KinhNghiemLamViecDto dto);
        Task<ApiResponse<object>> DeleteKinhNghiemAsync(int maTaiKhoan, int idKinhNghiem);
        Task<ApiResponse<object>> UpdateKyNangAsync(int maTaiKhoan, KyNangUngVienDto dto);

        // Saved Jobs
        Task<ApiResponse<object>> SaveJobAsync(int maTaiKhoan, int maTinTuyenDung);
        Task<ApiResponse<object>> UnsaveJobAsync(int maTaiKhoan, int maTinTuyenDung);
        Task<ApiResponse<object>> GetSavedJobsAsync(int maTaiKhoan);

        // Follow Companies
        Task<ApiResponse<object>> FollowCompanyAsync(int maTaiKhoan, int maDoanhNghiep);
        Task<ApiResponse<object>> UnfollowCompanyAsync(int maTaiKhoan, int maDoanhNghiep);
        Task<ApiResponse<object>> GetFollowedCompaniesAsync(int maTaiKhoan);
    }
}
