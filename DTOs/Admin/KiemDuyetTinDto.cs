using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Admin
{
    public class KiemDuyetTinDto
    {
        [Required]
        public string TrangThaiTin { get; set; } = string.Empty; // 'DaDuyet' | 'TuChoi'
        
        public string? LyDoTuChoi { get; set; }
    }
}
