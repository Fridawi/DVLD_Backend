namespace DVLD.CORE.DTOs.Licenses
{
    public class DriverLicenseDto
    {
        public string LicenseClassName { get; set; } = null!;
        public string DriverFullName { get; set; } = null!;
        public int LicenseID { get; set; }
        public string NationalNo { get; set; } = null!;
        public int ApplicationID { get; set; }
        public byte Gender { get; set; }
        public string GenderText { get; set; } = null!;
        public DateTime IssueDate { get; set; }
        public string IssueReasonText { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateOnly DriverBirthDate { get; set; }
        public int DriverID { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsExpired { get; set; }
        public string? DriverImageUrl { get; set; }
        public int CreatedByUserID { get; set; }
        public bool IsDetained { get; set; }
    }


}
