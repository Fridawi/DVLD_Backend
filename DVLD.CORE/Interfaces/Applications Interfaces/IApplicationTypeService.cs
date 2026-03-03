using DVLD.CORE.DTOs.Applications;
using DVLD.CORE.DTOs.Common;

namespace DVLD.CORE.Interfaces
{
    public interface IApplicationTypeService
    {
        Task<PagedResultDto<ApplicationTypeDto>> GetAllApplicationTypesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<ApplicationTypeDto> GetApplicationTypeByIdAsync(int id);
        Task<ApplicationTypeDto?> AddApplicationTypeAsync(ApplicationTypeDto testTypeDto);
        Task<ApplicationTypeDto?> UpdateApplicationTypeAsync(ApplicationTypeDto testTypeDto);
    }
}
