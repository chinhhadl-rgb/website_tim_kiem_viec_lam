using RecruitmentSystem.Models;

namespace RecruitmentSystem.Services
{
    public interface IThongBaoService
    {
        Task<ApiResponse<object>> GetNotificationsAsync(int maTaiKhoan);
        Task<ApiResponse<object>> MarkAsReadAsync(int maTaiKhoan, int maThongBao);
    }
}
