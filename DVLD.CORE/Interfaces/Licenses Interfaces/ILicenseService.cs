using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;
using DVLD.CORE.Enums;

namespace DVLD.CORE.Interfaces
{
    public interface ILicenseService
    {
        Task<PagedResultDto<LicenseDto>> GetAllLicensesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<PagedResultDto<DriverLicenseDto>> GetLicensesByDriverIdAsync(int driverId, int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<DriverLicenseDto?> GetDriverLicensesByIdAsync(int licenseId);
        Task<LicenseDto?> GetLicenseByIdAsync(int id);
        Task<LicenseDto?> GetActiveLicenseByPersonIDAndLicenseClassID(int personID, int licenseClassID);
        Task<bool> DeactivateLicense(int licenseID);
        Task<LicenseDto?> IssueFirstTimeLicenseAsync(LicenseCreateDto dto, int currentUserID);
        Task<LicenseDto?> RenewLicenseAsync(int oldLicenseId, string? notes, int currentUserID);
        Task<LicenseDto?> ReplaceLicenseAsync(int oldLicenseId, EnIssueReason issueReason, int currentUserID);
    }
}
