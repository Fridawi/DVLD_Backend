using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses.DetainedLicenses;

namespace DVLD.CORE.Interfaces
{
    public interface IDetainedLicenseService
    {
        Task<PagedResultDto<DetainedLicenseDto>> GetAllDetainedLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<DetainedLicenseDto?> GetDetainedLicenseByIdAsync(int id);
        Task<DetainedLicenseDto?> GetDetainedLicenseByLicenseIdAsync(int licenseId);
        Task<DetainedLicenseDto?> DetainLicenseAsync(DetainLicenseCreateDto detainDto, int userId);
        Task<DetainedLicenseDto?> ReleaseLicenseAsync(int licenseId, int userId);
        Task<bool> IsLicenseDetainedAsync(int licenseId);
    }

}
