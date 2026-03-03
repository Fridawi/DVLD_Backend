namespace DVLD.CORE.DTOs.TestAppointments
{
    public class TestAppointmentDto
    {
        public int TestAppointmentID { get; set; }
        public int LocalDrivingLicenseApplicationID { get; set; }
        public string TestTypeName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty; 
        public string FullName { get; set; } = string.Empty;  
        public DateTime AppointmentDate { get; set; }
        public float PaidFees { get; set; }
        public bool IsLocked { get; set; }
        public int? TestID { get; set; } 
    }
}
