using System.ComponentModel.DataAnnotations;

namespace RecruitmentSystem.DTOs.UngTuyen
{
    public class ApplyJobDto
    {
        [Required]
        public int MaTinTuyenDung { get; set; }
        
        [Required]
        public int MaCV { get; set; }
        
        public string? ThuGioiThieu { get; set; }
    }
}
