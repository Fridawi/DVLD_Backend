using DVLD.CORE.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.DTOs.Licenses.InternationalLicenses
{
    public class InternationalLicenseDto
    {
        public int InternationalLicenseID { get; set; }
        public int ApplicationID { get; set; }
        public int DriverID { get; set; }
        public int IssuedUsingLocalLicenseID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserID { get; set; }
    }
}
