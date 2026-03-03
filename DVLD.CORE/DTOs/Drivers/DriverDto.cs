using DVLD.CORE.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.DTOs.Drivers
{
    public class DriverDto
    {
        public int DriverID { set; get; }
        public int PersonID { set; get; }
        public string NationalNo { set; get; } = null!;
        public string FullName { set; get; } = null!;
        public int CreatedByUserID { set; get; }
        public DateTime CreatedDate { get; set; }
    }
}
