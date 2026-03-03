using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Drivers;

namespace DVLD.CORE.Interfaces
{
    public interface IDriverService
    {
        Task<PagedResultDto<DriverDto>> GetAllDriversAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);

        Task<DriverDto> GetDriverByIdAsync(int id);

        Task<DriverDto?> GetDriverByPersonIdAsync(int personId);

        Task<DriverDto?> AddDriverAsync(int personId,  int currentUserId, string currentUserRole);

        Task<bool> IsDriverExistByPersonIdAsync(int personId);
    }
}
