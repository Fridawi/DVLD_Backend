using DVLD.CORE.Enums;

namespace DVLD.CORE.Entities
{
    public class License
    {
        public int LicenseID { set; get; }
        public int ApplicationID { set; get; }
        public Application ApplicationInfo { set; get; } = null!;
        public int DriverID { set; get; }
        public Driver DriverInfo { set; get; } = null!;
        public int LicenseClassID { set; get; }
        public LicenseClass LicenseClassInfo { set; get; } = null!;
        public DateTime IssueDate { set; get; }
        public DateTime ExpirationDate { set; get; }
        public string? Notes { set; get; }
        public float PaidFees { set; get; }
        public bool IsActive { set; get; }
        public EnIssueReason IssueReason { set; get; }
        public int CreatedByUserID { set; get; }
        public bool IsExpired => ExpirationDate < DateTime.UtcNow;
        public virtual ICollection<DetainedLicense> DetainedRecords { set; get; } = new HashSet<DetainedLicense>();
        public bool IsDetained => DetainedRecords.Any(d => !d.IsReleased);
    }
}