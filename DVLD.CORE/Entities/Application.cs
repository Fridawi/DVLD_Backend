using DVLD.CORE.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.Entities
{
    public class Application
    {
        public int ApplicationID { set; get; }
        public int ApplicantPersonID { set; get; }
        public Person PersonInfo { set; get; } = null!;
        public string ApplicantFullName
        {
            get
            {
                return PersonInfo.FullName;
            }
        }
        public DateTime ApplicationDate { set; get; }
        public int ApplicationTypeID { set; get; }
        public ApplicationType ApplicationTypeInfo { set; get; } = null!;
        public EnApplicationStatus ApplicationStatus { set; get; }
        public string StatusText
        {
            get
            {
                switch (ApplicationStatus)
                {
                    case EnApplicationStatus.New:
                        return "New";
                    case EnApplicationStatus.Cancelled:
                        return "Cancelled";
                    case EnApplicationStatus.Completed:
                        return "Completed";
                    default:
                        return "Unknown";
                }
            }

        }
        public DateTime LastStatusDate { set; get; }
        public float PaidFees { set; get; }
        public int CreatedByUserID { set; get; }
        public User CreatedByUserInfo { set; get; } = null!;
    }
}
