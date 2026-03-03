using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.Enums;

namespace DVLD.CORE.Interfaces
{
    public interface IApplicationService
    {
        Task<ApplicationDto> GetByIdAsync(int applicationID);
        Task<ApplicationDto?> AddApplicationAsync(ApplicationCreateDto applicationDto, int currentUserId);
        Task<ApplicationDto?> UpdateStatusAsync( ApplicationUpdateDto applicationDto);
        Task<bool> DeleteApplicationAsync(int applicationID);
        Task<bool> IsApplicationExistAsync(int applicationID);
        Task<bool> DoesPersonHaveActiveApplicationAsync(int personID, int applicationTypeID);
        Task<int> GetActiveApplicationIdAsync(int personID, int applicationTypeID);
        Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID);
    }
}
