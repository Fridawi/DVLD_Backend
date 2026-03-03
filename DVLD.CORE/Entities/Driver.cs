using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.Entities
{
    public class Driver
    {
        public int DriverID { set; get; }
        public int PersonID { set; get; }
        public Person PersonInfo { set; get; } = null!;
        public int CreatedByUserID { set; get; }
        public DateTime CreatedDate { get; set; }
    }
}
