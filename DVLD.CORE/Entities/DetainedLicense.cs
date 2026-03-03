namespace DVLD.CORE.Entities
{
    public class DetainedLicense
    {
        public int DetainID { set; get; }
        public int LicenseID { set; get; }
        public License LicenseInfo { set; get; } = null!;
        public DateTime DetainDate { set; get; }    
        public float FineFees { set; get; }
        public int CreatedByUserID { set; get; }
        public User CreatedByUserInfo { set; get; } = null!;
        public bool IsReleased { set; get; }
        public DateTime? ReleaseDate { set; get; }
        public int? ReleasedByUserID { set; get; }
        public User? ReleasedByUserInfo { set; get; } 
        public int? ReleaseApplicationID { set; get; }

    }
}