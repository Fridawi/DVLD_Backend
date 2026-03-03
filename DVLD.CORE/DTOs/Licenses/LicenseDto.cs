namespace DVLD.CORE.DTOs.Licenses
{
    public class LicenseDto
    {
        public int LicenseID { get; set; }
        public int ApplicationID { get; set; }
        public int DriverID { get; set; }
        public string LicenseClassName { get; set; } = null!;       
        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsExpired { get; set; }
        public string? Notes { get; set; }
        public float PaidFees { get; set; }
        public bool IsActive { get; set; }
        public string IssueReasonText { get; set; } = null!;
        public int CreatedByUserID { get; set; }
        public bool IsDetained { get; set; }
    }
}
