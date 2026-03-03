using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.TestAppointments
{
    public class TestAppointmentUpdateDto
    {
        [Required]
        public int TestAppointmentID { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
    }
}
