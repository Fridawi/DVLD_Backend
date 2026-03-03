using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication
{
    public class LocalDrivingLicenseApplicationDto
    {
        public int LocalDrivingLicenseApplicationID { get; set; }
        public int ApplicationID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string NationalNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime ApplicationDate { get; set; }
        public int PassedTestCount { get; set; } 
        public string Status { get; set; } = string.Empty;
    }
}
