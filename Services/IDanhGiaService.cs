using RecruitmentSystem.DTOs.DanhGia;
using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IDanhGiaService
    {
        Task<ApiResponse<object>> CreateDanhGiaAsync(CreateDanhGiaDto dto);
        Task<ApiResponse<object>> GetDanhGiaByDoanhNghiepAsync(int maDoanhNghiep);
    }
}
