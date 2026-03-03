using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.Entities;
using System.Linq.Expressions;

namespace DVLD.CORE.Interfaces
{
    public interface ILocalDrivingLicenseApplicationRepository : IGenericRepository<LocalDrivingLicenseApplication>
    {
        Task<(IEnumerable<LocalDrivingLicenseApplicationDto> Data, int TotalCount)> GetPagedApplicationsAsync(
            Expression<Func<LocalDrivingLicenseApplication, bool>> filter,
            int pageNumber,
            int pageSize);
        Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID);
    }
}
