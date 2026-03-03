using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestAppointments;

namespace DVLD.CORE.Interfaces.Tests
{
    public interface ITestAppointmentService
    {
        Task<PagedResultDto<TestAppointmentDto>> GetAllTestAppointmentsAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<PagedResultDto<TestAppointmentDto>> GetApplicationTestAppointmentsPerTestTypeAsync(int localAppID, int testTypeID, int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<TestAppointmentDto?> GetTestAppointmentByIdAsync(int id);
        Task<TestAppointmentDto?> GetLastTestAppointmentAsync(int localAppID, int testTypeID);
        Task<TestAppointmentDto?> AddTestAppointmentAsync(TestAppointmentCreateDto testAppointmentCreateDto, int currentUserId);
        Task<TestAppointmentDto?> UpdateTestAppointmentAsync(TestAppointmentUpdateDto testAppointmentUpdateDto);
        Task<bool> HasActiveAppointmentAsync(int localAppID, int testTypeID);
        Task<bool> HasAnyAppointmentsAsync(int localAppID);
    }
}
