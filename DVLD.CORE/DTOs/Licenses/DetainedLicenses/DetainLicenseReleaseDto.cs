using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Licenses.DetainedLicenses
{
    public class DetainLicenseReleaseDto
    {
        [Required]
        public int ReleaseApplicationID { get; set; }
    }
}
