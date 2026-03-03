namespace DVLD.CORE.DTOs.Licenses.DetainedLicenses
{
    public class DetainedLicenseDto
    {
        public int DetainID { get; set; }
        public int LicenseID { get; set; }
        public DateTime DetainDate { get; set; }
        public float FineFees { get; set; }
        public bool IsReleased { get; set; }
        public string NationalNo { get; set; } = null!;
        public string CreatedByUserName { get; set; } = null!;
        public int CreatedByUserID { get; set; } 
        public DateTime? ReleaseDate { get; set; }
        public string? ReleasedByUserName { get; set; }
        public int? ReleasedByUserID { get; set; }
        public int? ReleaseApplicationID { get; set; }
    }
}
