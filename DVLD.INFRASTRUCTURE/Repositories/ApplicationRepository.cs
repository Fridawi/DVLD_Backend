using DVLD.CORE.Entities;
using DVLD.CORE.Enums;
using DVLD.CORE.Interfaces;
using DVLD.INFRASTRUCTURE.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace DVLD.INFRASTRUCTURE.Repositories
{
    public class ApplicationRepository : GenericRepository<Application>, IApplicationRepository
    {
        private readonly AppDbContext _context;

        public ApplicationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> DoesPersonHaveActiveApplicationAsync(int personID, int applicationTypeID)
        {
            return await _context.Applications
                .AnyAsync(a => a.ApplicantPersonID == personID &&
                               a.ApplicationTypeID == applicationTypeID &&
                               a.ApplicationStatus == EnApplicationStatus.New);
        }

        public async Task<int> GetActiveApplicationIdAsync(int personID, int applicationTypeID)
        {
            return await _context.Applications
                .Where(a => a.ApplicantPersonID == personID &&
                            a.ApplicationTypeID == applicationTypeID &&
                            a.ApplicationStatus == EnApplicationStatus.New)
                .Select(a => a.ApplicationID)
                .FirstOrDefaultAsync(); 
        }

        public async Task<int> GetActiveApplicationIdForLicenseClassAsync(int personID, int applicationTypeID, int licenseClassID)
        {
            return await _context.LocalDrivingLicenseApplications
                .Where(ldla => ldla.LicenseClassID == licenseClassID &&
                               ldla.ApplicationInfo.ApplicantPersonID == personID &&
                               ldla.ApplicationInfo.ApplicationTypeID == applicationTypeID &&
                               ldla.ApplicationInfo.ApplicationStatus == EnApplicationStatus.New)
                .Select(ldla => ldla.ApplicationID)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatusAsync(int applicationID, EnApplicationStatus newStatus)
        {
            var rowsAffected = await _context.Applications
                .Where(a => a.ApplicationID == applicationID)
                .ExecuteUpdateAsync(a => a
                    .SetProperty(app => app.ApplicationStatus, newStatus)
                    .SetProperty(app => app.LastStatusDate, DateTime.UtcNow));

            return rowsAffected > 0;
        }
    }
}
