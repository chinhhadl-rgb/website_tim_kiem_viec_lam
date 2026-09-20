using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.Admin
{
    public class ThamDinhDoanhNghiepDto
    {
        [Required]
        public string TrangThaiXacThuc { get; set; } = string.Empty; // 'DaDuyet' | 'TuChoi'
        
        public string? LyDoTuChoi { get; set; }
    }
}
