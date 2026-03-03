using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Tests
{
    public class TestUpdateDto
    {
        [Required]
        public int TestID { set; get; }

        [Required]
        public bool TestResult { set; get; }
        
        [StringLength(500)]
        public string? Notes { set; get; }
    }
}
