using DVLD.CORE.DTOs.Common;
using DVLD.CORE.DTOs.Tests;

namespace DVLD.CORE.Interfaces.Tests
{
    public interface ITestService
    {
        Task<PagedResultDto<TestDto>> GetAllTestsAsync(int pageNumber, int pageSize, string? filterColumn = null, string? filterValue = null);
        Task<TestDto?> GetTestByIdAsync(int id);
        Task<TestDto?> GetLastTestPerPersonAndLicenseClassAsync(int personId, int licenseClassId, int testTypeID);
        Task<byte> GetPassedTestCountAsync(int localAppID);
        Task<bool> PassedAllTestsAsync(int localAppID);
        Task<TestDto?> AddTestAsync(TestCreateDto testCreateDto, int currentUserId);
        Task<TestDto?> UpdateTestAsync(TestUpdateDto testUpdateDto);
        Task<bool> HasPassedAsync(int localAppID, int testTypeID);
        Task<bool> RequiresRetakeAsync(int localAppID, int testTypeId);
    }
}
