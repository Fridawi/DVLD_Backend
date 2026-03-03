using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.TestAppointments
{
    public class TestAppointmentCreateDto
    {
        [Required]
        public byte TestTypeID { get; set; }

        [Required]
        public int LocalDrivingLicenseApplicationID { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Range(0, 500)]
        public float PaidFees { get; set; }
    }
}
