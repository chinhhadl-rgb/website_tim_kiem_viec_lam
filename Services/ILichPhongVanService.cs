using RecruitmentSystem.DTOs.LichPhongVan;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface ILichPhongVanService
    {
        Task<ApiResponse<object>> CreateScheduleAsync(int maTaiKhoan, CreateLichPhongVanDto dto);
        Task<ApiResponse<object>> RespondToScheduleAsync(int maTaiKhoan, int maLichPhongVan, PhanHoiLichDto dto);
        Task<ApiResponse<object>> InputInterviewResultAsync(int maTaiKhoan, int maLichPhongVan, KetQuaPhongVanDto dto);
    }
}
