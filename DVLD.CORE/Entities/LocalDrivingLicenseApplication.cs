namespace DVLD.CORE.Entities
{
    public class LocalDrivingLicenseApplication
    {
        public int LocalDrivingLicenseApplicationID { get; set; }

        public int ApplicationID { get; set; }
        public virtual Application ApplicationInfo { get; set; } = null!;

        public int LicenseClassID { get; set; }
        public virtual LicenseClass LicenseClassInfo { get; set; } = null!;
    }
}