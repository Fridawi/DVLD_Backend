using DVLD.CORE.Enums;
using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Licenses
{
    public class LicenseCreateDto
    {
        [Required]
        public int LocalDrivingLicenseApplicationID { get; set; }

        public string? Notes { get; set; }

        [Required]
        public EnIssueReason IssueReason { get; set; }

    }
}
