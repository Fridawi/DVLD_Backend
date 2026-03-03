using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses.InternationalLicenses;

namespace DVLD.CORE.Interfaces
{
    public interface IInternationalLicenseService
    {
        Task<PagedResultDto<InternationalLicenseDto>> GetAllInternationalLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<PagedResultDto<InternationalLicenseDto>> GetInternationalLicensesByDriverIdAsync(int driverId, int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<InternationalLicenseDto?> GetInternationalLicenseByIdAsync(int internationalLicenseId);
        Task<DriverInternationalLicenseDto?> GetDriverInternationalLicenseByIdAsync(int internationalLicenseId);
        Task<InternationalLicenseDto?> IssueInternationalLicenseAsync(InternationalLicenseCreateDto createDto, int createdByUserId);
        Task<bool> DeactivateInternationalLicenseAsync(int internationalLicenseId);
        Task<int?> GetActiveInternationalLicenseIdByDriverIdAsync(int driverId);
        Task<bool> IsDriverEligibleForInternationalLicenseAsync(int localLicenseId);
    }
}
