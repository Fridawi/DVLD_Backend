using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.DTOs.Common;
using DVLD.CORE.Entities;

namespace DVLD.CORE.Interfaces
{
    public interface ILocalDrivingLicenseApplicationService
    {
        Task<PagedResultDto<LocalDrivingLicenseApplicationDto>> GetAllLocalDrivingLicenseApplicationsAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<LocalDrivingLicenseApplicationDto?> GetByIdAsync(int id);
        Task<LocalDrivingLicenseApplicationDto?> GetByApplicationIdAsync(int id);
        Task<LocalDrivingLicenseApplicationDto?> AddLocalDrivingLicenseApplicationAsync(LocalDrivingLicenseApplicationCreateDto createDto, int currentUserID);
        Task<LocalDrivingLicenseApplicationDto?> UpdateLocalDrivingLicenseApplicationAsync(LocalDrivingLicenseApplicationUpdateDto localAppDto);
        Task<bool> DeleteLocalDrivingLicenseApplicationAsync(int id);
        Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID);
    }
}
