using DVLD.CORE.DTOs.Applications.LocalDrivingLicenseApplication;
using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DVLD.INFRASTRUCTURE.Repositories
{
    public class LocalDrivingLicenseApplicationRepository : GenericRepository<LocalDrivingLicenseApplication>, ILocalDrivingLicenseApplicationRepository
    {
        private readonly AppDbContext _context;
        public LocalDrivingLicenseApplicationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<LocalDrivingLicenseApplicationDto> Data, int TotalCount)> GetPagedApplicationsAsync(
            Expression<Func<LocalDrivingLicenseApplication, bool>> filter,
            int pageNumber,
            int pageSize)
        {
            var query = _context.LocalDrivingLicenseApplications
                .Where(filter);

            int totalCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(la => la.ApplicationInfo.ApplicationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(la => new LocalDrivingLicenseApplicationDto
                {
                    LocalDrivingLicenseApplicationID = la.LocalDrivingLicenseApplicationID,
                    ApplicationID = la.ApplicationInfo.ApplicationID,
                    ClassName = la.LicenseClassInfo.ClassName,
                    NationalNo = la.ApplicationInfo.PersonInfo.NationalNo,
                    FullName = la.ApplicationInfo.PersonInfo.FirstName + " " +
                        (la.ApplicationInfo.PersonInfo.SecondName != null ? la.ApplicationInfo.PersonInfo.SecondName + " " : "") +
                        la.ApplicationInfo.PersonInfo.LastName,
                    ApplicationDate = la.ApplicationInfo.ApplicationDate,
                    PassedTestCount = _context.Tests.Count(t =>
                        t.TestAppointmentInfo.LocalDrivingLicenseApplicationID == la.LocalDrivingLicenseApplicationID
                        && t.TestResult),
                    Status = la.ApplicationInfo.ApplicationStatus.ToString()
                })
                .ToListAsync(); 

            return (data, totalCount);
        }

        public async Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID)
        {
            return await _context.LocalDrivingLicenseApplications
                .Where(la => la.ApplicationInfo.ApplicantPersonID == personID &&
                             la.ApplicationInfo.ApplicationTypeID == applicationTypeID &&
                             la.ApplicationInfo.ApplicationStatus == EnApplicationStatus.New &&
                             la.LicenseClassID == licenseClassID)
                .Select(la => la.ApplicationID)
                .FirstOrDefaultAsync();
        }
    }
}
