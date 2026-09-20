using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.CV
{
    public class CreateCvTrucTuyenDto
    {
        [Required(ErrorMessage = "Tiêu đề CV là bắt buộc.")]
        public string TieuDeCV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung CV trực tuyến là bắt buộc.")]
        public string CVTrucTuyen { get; set; } = string.Empty;
    }
}
