using DVLD.CORE.Enums;

namespace DVLD.CORE.Entities
{
    public class TestAppointment
    {
        public int TestAppointmentID { get; set; }
        public EnTestType TestTypeID { get; set; }
        public int LocalDrivingLicenseApplicationID { get; set; }
        public virtual LocalDrivingLicenseApplication LocalAppInfo { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public float PaidFees { get; set; }
        public int CreatedByUserID { get; set; }
        public bool IsLocked { get; set; }
        public int? RetakeTestApplicationID { get; set; }
        public virtual Application? RetakeTestAppInfo { get; set; }

        public virtual Test? TestRecord { get; set; }
        public int TestID => TestRecord?.TestID ?? -1;
    }
}
