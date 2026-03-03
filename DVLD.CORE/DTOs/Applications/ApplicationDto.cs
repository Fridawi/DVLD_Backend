namespace DVLD.CORE.DTOs.Applications
{
    public class ApplicationDto
    {
        public int ApplicationID { get; set; }
        public int ApplicantPersonID { get; set; }

        public string ApplicantFullName { get; set; } = string.Empty;

        public DateTime ApplicationDate { get; set; }
        public int ApplicationTypeID { get; set; }

        public string ApplicationTypeTitle { get; set; } = string.Empty;

        public byte ApplicationStatus { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public DateTime LastStatusDate { get; set; }
        public float PaidFees { get; set; }

        public int CreatedByUserID { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
    }
}
