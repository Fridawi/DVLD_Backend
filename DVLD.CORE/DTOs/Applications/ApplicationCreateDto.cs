using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Applications
{
    public class ApplicationCreateDto
    {
        [Required]
        public int ApplicantPersonID { get; set; }

        [Required]
        public int ApplicationTypeID { get; set; }
    }
}
