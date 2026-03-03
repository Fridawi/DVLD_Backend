using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Tests
{
    public class TestCreateDto
    {
        [Required]
        public int TestAppointmentID { set; get; }
        
        [Required]
        public bool TestResult { set; get; }
        
        [StringLength(500)]
        public string? Notes { set; get; }      
    }
}
