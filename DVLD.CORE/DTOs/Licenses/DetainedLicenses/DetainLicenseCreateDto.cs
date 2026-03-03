using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Licenses.DetainedLicenses
{
    public class DetainLicenseCreateDto
    {
        [Required]
        public int LicenseID { get; set; }

        [Required]
        [Range(0, float.MaxValue, ErrorMessage = "Fine fees must be a positive number")]
        public float FineFees { get; set; } 
    }
}
