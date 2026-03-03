using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.TestTypes;
using DVLD.CORE.Entities;

namespace DVLD.CORE.Interfaces.Tests
{
    public interface ITestTypeService
    {
        Task<PagedResultDto<TestTypeDto>> GetAllTestTypesAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<TestTypeDto> GetTestTypeByIdAsync(int id);
        Task<TestTypeDto?> AddTestTypeAsync(TestTypeDto testTypeDto);
        Task<TestTypeDto?> UpdateTestTypeAsync(TestTypeDto testTypeDto);
    }
}
