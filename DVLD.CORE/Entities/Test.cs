using DVLD.CORE.Enums;

namespace DVLD.CORE.Entities
{
    public class Test
    {
        public int TestID { set; get; }
        public int TestAppointmentID { set; get; }
        public TestAppointment TestAppointmentInfo { set; get; } = null!;
        public bool TestResult { set; get; }
        public string? Notes { set; get; }
        public int CreatedByUserID { set; get; }
    }
}