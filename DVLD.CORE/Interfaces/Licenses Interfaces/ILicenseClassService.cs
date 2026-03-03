using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Licenses;

namespace DVLD.CORE.Interfaces.Licenses
{
    public interface ILicenseClassService
    {
        Task<PagedResultDto<LicenseClassDto>> GetAllLicenseClassesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<LicenseClassDto> GetLicenseClassByIdAsync(int id);
        Task<LicenseClassDto> GetLicenseClassByClassNameAsync(string className);
        Task<LicenseClassDto?> AddLicenseClassAsync(LicenseClassDto licenseClassDto);
        Task<LicenseClassDto?> UpdateLicenseClassAsync(LicenseClassDto licenseClassDto);
    }
}
