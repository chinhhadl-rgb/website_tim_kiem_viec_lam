using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.UngTuyen
{
    public class UpdateTrangThaiUngTuyenDto
    {
        [Required(ErrorMessage = "Trạng thái ứng tuyển là bắt buộc.")]
        public string TrangThaiUngTuyen { get; set; } = string.Empty;
        
        public string? GhiChu { get; set; }
    }
}
