using System;
using System.Collections.Generic;
using System.Text;

namespace DVLD.CORE.DTOs.Tests
{
    public class TestDto
    {
        public int TestID { set; get; }
        public int TestAppointmentID { set; get; }
        public bool TestResult { set; get; }
        public string Notes { set; get; }
        public int CreatedByUserID { set; get; }
    }
}
