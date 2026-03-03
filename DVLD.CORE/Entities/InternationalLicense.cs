namespace DVLD.CORE.Entities
{
    public class InternationalLicense
    {
        public int InternationalLicenseID { get; set; }

        public int ApplicationID { get; set; } 
        public Application ApplicationInfo { get; set; } = null!; 

        public int DriverID { get; set; }
        public Driver DriverInfo { get; set; } = null!;

        public int IssuedUsingLocalLicenseID { get; set; }
        public License LocalLicenseInfo { get; set; } = null!;

        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }

        public int CreatedByUserID { get; set; }
        public User CreatedByUserInfo { get; set; } = null!;
    }
}